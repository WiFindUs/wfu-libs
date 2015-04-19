using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
	public class Map : Tile
	{
		public event Action<Map> ElevationStateChanged;
		public event Action<Map> CompositeImageChanged;



		//elevation
		public const double ELEV_MIN = -50.0;
		public const double ELEV_MAX = 2500.0;
		public const double ELEV_RANGE = ELEV_MAX - ELEV_MIN;
		private const int ELEV_SAMPLE_COUNT = 128;
		private const int ELEV_SAMPLE_SIZE = 8;
		private const int ELEV_SIZE = ELEV_SAMPLE_COUNT * ELEV_SAMPLE_SIZE;
		private static readonly string ELEV_URL
			= "https://maps.googleapis.com/maps/api/elevation/json?path=enc:{0}&samples={1}&key={2}";
		private static readonly Regex PATTERN_ELEV_STATUS
			= new Regex("\"status\"\\s+[:]\\s+\"OK\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex PATTERN_ELEV_DATA = new Regex("[{]\\s*"
				+ "\"elevation\"\\s*[:]\\s*([+-]?[0-9]+(?:[.][0-9]+)?)\\s*[,]\\s*"
				+ "\"location\"\\s*[:]\\s*[{]\\s*"
				+ "\"lat\"\\s*[:]\\s*([+-]?[0-9]+(?:[.][0-9]+)?)\\s*[,]\\s*"
				+ "\"lng\"\\s*[:]\\s*([+-]?[0-9]+(?:[.][0-9]+)?)\\s*[}]\\s*[,]\\s*"
				+ "\"resolution\"\\s*[:]\\s*([+-]?[0-9]+(?:[.][0-9]+)?)\\s*[}]",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private Bitmap elevation = null;
		private LoadingState elevationState = LoadingState.Waiting;
		private int elevationDownloadRetries = 0;
		private object elevationLock = new object();

		//image
		public const int COMPOSITE_SIZE = 2048;
		private readonly List<Tile> allTiles = new List<Tile>();
		private TilePaintLayer[] paintLayers;
		private TileCompositeLayer composite;
		private object compositeLock = new object();
		private readonly int[] totalTiles;
		private readonly int[] visibleTiles;

		//threads
		private readonly List<Thread> imageThreads = new List<Thread>();
		private Thread elevationThread = null;		

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public override bool ThreadlessState
		{
			get { return imageThreads.Count == 0 && elevationThread == null; }
		}

		public LoadingState ElevationState
		{
			get { return elevationState; }
			private set
			{
				if (elevationState == value)
					return;
				elevationState = value;
				if (ElevationStateChanged != null)
					ElevationStateChanged(this);
			}
		}

		private Bitmap ElevationImage
		{
			get { return elevation; }
			set
			{
				if (value == elevation)
					return;
				DisposeElevation();
				elevation = value;
			}
		}

		internal string ElevationPath
		{
			get { return System.IO.Path.Combine(RootDirectory, ElevationFilename); }
		}

		internal string ElevationFilename
		{
			get { return String.Format("{0}_elevation.png", Root.FilenameLatLong); }
		}

		internal List<Tile> AllTiles
		{
			get { return allTiles; }
		}

		public Bitmap Composite
		{
			get { return composite == null ? null : composite.bitmap; }
		}

		public Object CompositeLock
		{
			get { return compositeLock; }
		}

		public Object ElevationLock
		{
			get { return elevationLock; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public Map(uint levelCount = 5)
			: base(null, null, 0, levelCount)
		{
			totalTiles = new int[LevelCount];
			visibleTiles = new int[totalTiles.Length];
			for (uint i = 0; i < totalTiles.Length; i++ )
			{
				totalTiles[i] = ((int)RowMax(i) + 1) * ((int)ColumnMax(i) + 1);
				visibleTiles[i] = 0;
			}
			paintLayers = new TilePaintLayer[totalTiles.Length];
			for (int i = 0; i < paintLayers.Length; i++)
				paintLayers[i] = new TilePaintLayer();
			composite = new TileCompositeLayer();
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public override double Elevation(double lat, double lng)
		{
			if (elevation == null || Region == null)
				return 0.0;
			int x, y;
			Region.LocationToScreen(new Rectangle(0, 0, ELEV_SIZE, ELEV_SIZE), lat, lng, out x, out y);
			x = x.Clamp(0, ELEV_SIZE - 1);
			y = y.Clamp(0, ELEV_SIZE - 1);
			Color col = elevation.GetPixel(x, y);

			uint val = (uint)((col.A << 24) | (col.R << 16) |
					(col.G << 8) | (col.B << 0));

			return ELEV_MIN + ((double)val / (double)0xFFFFFFFF) * ELEV_RANGE;
		}

		public override string ToString()
		{
			return String.Format("BaseTile[{0},{1},{2} I:{3}, E:{4}]",
				Level, Row, Column, ImageState, ElevationState);
		}

		public void Load()
		{
			if (!ThreadlessState)
				throw new InvalidOperationException("You may not start loading operations - the tile is currently in a threaded state.");

			Debugger.V("Map: starting load operations");
			
			var orderedTiles = allTiles
				.OrderBy((tile) => tile.Level)
				.Where((tile) => tile.ImageState == LoadingState.Waiting);
			List<Tile> downloads = orderedTiles
				.Where((tile) => !File.Exists(tile.ImagePath))
				.ToList();
			List<Tile> loads = orderedTiles
				.Where((tile) => File.Exists(tile.ImagePath))
				.Reverse()
				.ToList();

			//loads
			while (loads.Count > 0)
			{
				int count = Math.Min(100, loads.Count);
				List<Tile> subLoads = loads.GetRange(0, count);
				loads.RemoveRange(0, count);

				Thread thread = new Thread(new ParameterizedThreadStart(LoadImagesThread));
				thread.IsBackground = true;
				thread.Priority = ThreadPriority.BelowNormal;
				imageThreads.Add(thread);
				thread.Start(subLoads);
			}

			//downloads
			while (downloads.Count > 0)
			{
				int count = Math.Min(50, downloads.Count);
				List<Tile> subLoads = downloads.GetRange(0, count);
				downloads.RemoveRange(0, count);
				
				Thread thread = new Thread(new ParameterizedThreadStart(DownloadImagesThread));
				thread.IsBackground = true;
				thread.Priority = ThreadPriority.BelowNormal;
				imageThreads.Add(thread);
				thread.Start(subLoads);
			}

			//elevation
			if (ElevationState != LoadingState.Waiting)
				return;

			elevationThread = new Thread(new ThreadStart(ElevationThread));
			elevationThread.IsBackground = true;
			elevationThread.Priority = ThreadPriority.BelowNormal;
			elevationThread.Start();
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Dispose(bool disposing)
		{
			if (Disposed)
				return;

			if (disposing)
			{
				//lock (CompositeLock)
				//{
					for (int i = 0; i < paintLayers.Length; i++)
					{
						if (paintLayers[i] != null)
							paintLayers[i].Dispose();
						paintLayers[i] = null;
					}
					if (composite != null)
						composite.Dispose();
					composite = null;
				//}
				allTiles.Clear();
				//lock (ElevationLock)
					DisposeElevation();
			}

			base.Dispose(disposing);
		}


		protected override void LocationChanged()
		{
			for (uint i = 0; i < visibleTiles.Length; i++)
				visibleTiles[i] = 0;
			
			lock (CompositeLock)
			{
				composite.Clear();
				for (int i = 0; i < paintLayers.Length; i++)
					paintLayers[i].CreateBitmap();
			}
			if (CompositeImageChanged != null)
				CompositeImageChanged(this);

			base.LocationChanged();

			
			lock (ElevationLock)
			{
				//check data directory
				if (Region != null && !Directory.Exists(RootDirectory))
				{
					ElevationState = LoadingState.Error;
					return;
				}

				//clear data and set states
				ElevationImage = null;
				ElevationState = (Region == null ? LoadingState.NoLocation : LoadingState.Waiting);
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void DisposeElevation()
		{
			if (elevation != null)
				elevation.Dispose();
			elevation = null;
		}

		private void DownloadImagesThread(object listObj)
		{
#if DEBUG
			Debugger.T("enter");
#endif
			//wait for a little while to allow more existing tiles to load
			//(allows for more obscuring, skipping unnecessary downloads)

			int timer = 0;
			while (timer < 7500)
			{
				Thread.Sleep(250);
				timer += 250;
				if (WiFindUs.Forms.MainForm.HasClosed)
					return;
			}

			List<Tile> tileList = listObj as List<Tile>;
			foreach (Tile tile in tileList)
			{
				tile.downloadRetries = 0;
				DownloadImage(tile);
			}


			lock (imageThreads)
			{
				imageThreads.Remove(Thread.CurrentThread);
				if (imageThreads.Count == 0)
					ImageThreadsFinished();
			}
#if DEBUG
			Debugger.T("exit");
#endif
		}

		private void LoadImagesThread(object listObj)
		{
#if DEBUG
			Debugger.T("enter");
#endif

			List<Tile> tileList = listObj as List<Tile>;
			foreach (Tile tile in tileList)
			{
				tile.downloadRetries = 0;
				LoadImage(tile);
			}

			lock (imageThreads)
			{
				imageThreads.Remove(Thread.CurrentThread);
				if (imageThreads.Count == 0)
					ImageThreadsFinished();
			}
#if DEBUG
			Debugger.T("exit");
#endif
		}

		private void ElevationThread()
		{
#if DEBUG
			Debugger.T("enter");
#endif
			elevationDownloadRetries = 0;
			ElevationState = File.Exists(ElevationPath)
				? LoadingState.Loading : LoadingState.Downloading;
			if (ElevationState == LoadingState.Downloading)
				DownloadElevation();
			else
				LoadElevation();

			elevationThread = null;
#if DEBUG
			Debugger.T("exit");
#endif
		}

		private void ImageThreadsFinished()
		{
			lock (CompositeLock)
			{
				for (int i = 0; i < paintLayers.Length; i++)
					paintLayers[i].DestroyBitmap();
			}
			Debugger.V("Map: image load operations completed.");
		}

		private void DownloadImage(Tile tile)
		{
			//if it's already being obscured by a higher-level tile, don't bother downloading it
			if (tile.ObscuredByChildren)
			{
				tile.ImageState = LoadingState.Finished;
				return;
			}

			bool ok = false;
			int tries = 0;
			while (!ok && tries < 3)
			{
				try
				{
					using (WebClient imageClient = new WebClient())
						imageClient.DownloadFile(new Uri(tile.ImageDownloadURL), tile.ImagePath);
					ok = true;
				}
				catch (Exception)
				{
					ok = false;
					tries++;
				}
				if (WiFindUs.Forms.MainForm.HasClosed)
					break;
			}

			if (WiFindUs.Forms.MainForm.HasClosed)
			{
				try { File.Delete(tile.ImagePath); }
				catch { }
				return;
			}

			if (ok)
			{
				tile.ImageState = LoadingState.Loading;
				LoadImage(tile);
			}
			else
			{
				Debugger.E("Error downloading map tile texture {0}", tile.ImageFilename);
				tile.ImageState = LoadingState.ErrorDownloading;
				try { File.Delete(tile.ImagePath); }
				catch { }
			}
		}

		private void LoadImage(Tile tile)
		{
			//if it's already being obscured by a higher-level tile, don't bother loading it
			if (tile.ObscuredByChildren)
			{
				tile.ImageState = LoadingState.Finished;
				return;
			}

			try
			{
				//get source image
				System.Drawing.Bitmap image;
				try
				{
					image = new System.Drawing.Bitmap(tile.ImagePath);
				}
				catch (ArgumentException) //corrupt file
				{
					if (tile.downloadRetries >= 3)
					{
						Debugger.E("Error loading map tile texture {0}: file was corrupt.", tile.ImageFilename);
						tile.ImageState = LoadingState.ErrorLoading;
						return;
					}

					if (File.Exists(tile.ImagePath))
					{
						try { File.Delete(tile.ImagePath); }
						catch
						{
							Debugger.E("Error deleting corrupt map tile texture {0}", tile.ImageFilename);
							tile.ImageState = LoadingState.ErrorLoading;
							return;
						}
					}
					tile.downloadRetries++;
					tile.ImageState = LoadingState.Downloading;
					DownloadImage(tile);
					return;
				}

				bool tileWasVisible = tile.VisibleInComposite;
				tile.ImageState = LoadingState.Finished;

				//check if the composite has changed
				if (tileWasVisible != tile.VisibleInComposite)
				{
					lock (CompositeLock)
					{
						//update the tile layers
						visibleTiles[tile.Level] += (tileWasVisible ? -1 : 1);
						paintLayers[tile.Level].visible = visibleTiles[tile.Level] > 0;
						if (paintLayers[tile.Level].visible)
							paintLayers[tile.Level].Paint(tile, image);

						//repaint the composite
						if (composite != null)
							composite.Paint(paintLayers);

						//fire event handler
						if (CompositeImageChanged != null)
							CompositeImageChanged(this);
					}
				}
				image.Dispose();
				image = null;


				return;
			}
			catch (FileNotFoundException)
			{
				Debugger.E("Error loading map tile texture {0}: file not found", tile.ImageFilename);
			}
			catch (OutOfMemoryException)
			{
				Debugger.E("Error loading map tile texture {0}: out of system memory", tile.ImageFilename);
			}
			tile.ImageState = LoadingState.ErrorLoading;
		}

		private void DownloadElevation()
		{
			//generate zigzag list of points for polyline
			double latStep = LatitudinalSpan / (double)(ELEV_SAMPLE_COUNT);
			double longStep = LongitudinalSpan / (double)(ELEV_SAMPLE_COUNT);
			List<ILocation> points = new List<ILocation>();
			for (int row = 0; row < ELEV_SAMPLE_COUNT; row++)
			{
				for (int column = (row % 2 == 0 ? 0 : ELEV_SAMPLE_COUNT - 1);
					(row % 2 == 0 ? column < ELEV_SAMPLE_COUNT : column >= 0);
						column += (row % 2 == 0 ? 1 : -1))
				{
					double lat = NorthWest.Latitude.Value - (latStep * 0.5) - (latStep * row);
					double lng = NorthWest.Longitude.Value + (longStep * 0.5) + (longStep * column);
					points.Add(new Location(lat, lng));
				}
			}

			//download and check data
			StringBuilder sb = new StringBuilder();
			using (WebClient elevationClient = new WebClient())
			{
				while (points.Count > 0)
				{
					int count = Math.Min(200, points.Count);
					List<ILocation> pts = points.GetRange(0, count);
					points.RemoveRange(0, count);

					string url = String.Format(ELEV_URL, Location.ToPolyline(pts), count, WFUApplication.GoogleAPIKey);
					string data = null;
					int tries = 0;
					while (data == null && tries < 3)
					{
						try
						{
							data = elevationClient.DownloadString(url);
						}
						catch (Exception)
						{
							data = null;
							tries++;
						}
						if (WiFindUs.Forms.MainForm.HasClosed)
							break;
					}
					if (WiFindUs.Forms.MainForm.HasClosed)
						break;

					if (data != null && PATTERN_ELEV_STATUS.IsMatch(data))
						sb.Append(data + "\n");
					if (points.Count > 0)
						Thread.Sleep(WFUApplication.Random.Next(300, 500));
				}
			}
			if (WiFindUs.Forms.MainForm.HasClosed)
				return;

			//check data
			MatchCollection matches = PATTERN_ELEV_DATA.Matches(sb.ToString());
			if (matches.Count == 0)
			{
				Debugger.E("Error downloading elevation data: no data?");
				ElevationState = LoadingState.ErrorDownloading;
				return;
			}

			//create bitmap
			Bitmap bmp = new Bitmap(ELEV_SIZE, ELEV_SIZE, PixelFormat.Format32bppArgb);
			Rectangle bmpRect = new Rectangle(0, 0, ELEV_SIZE, ELEV_SIZE);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SetQuality(GraphicsExtensions.GraphicsQuality.Low);
				g.PixelOffsetMode = PixelOffsetMode.None;
				g.CompositingMode = CompositingMode.SourceOver;
				for (int i = 0; i < matches.Count; i++)
				{
					//parse data
					double lat = Double.Parse(matches[i].Groups[2].Value);
					double lng = Double.Parse(matches[i].Groups[3].Value);
					double elev = Double.Parse(matches[i].Groups[1].Value);

					//get coords
					int x, y;
					Region.LocationToScreen(bmpRect, lat, lng, out x, out y);
					x = (x / ELEV_SAMPLE_SIZE) * ELEV_SAMPLE_SIZE; //rounding
					y = (y / ELEV_SAMPLE_SIZE) * ELEV_SAMPLE_SIZE; //rounding
					Rectangle radRect = new Rectangle(x, y, ELEV_SAMPLE_SIZE, ELEV_SAMPLE_SIZE);

					//get colour values
					uint cval = (uint)(((elev - ELEV_MIN) / ELEV_RANGE) * (double)0xFFFFFFFF);
					Color col = Color.FromArgb((byte)(cval >> 24), (byte)(cval >> 16), (byte)(cval >> 8), (byte)(cval >> 0));

					//paint heightmap
					using (SolidBrush b = new SolidBrush(col))
						g.FillRectangle(b, radRect);

				}
			}
			bmp.Save(ElevationPath, System.Drawing.Imaging.ImageFormat.Png);
			lock (ElevationLock)
			{
				ElevationImage = bmp;
				ElevationState = LoadingState.Finished;
			}
		}

		private void LoadElevation()
		{
			try
			{
				//get source image
				System.Drawing.Bitmap image;
				try
				{
					image = new System.Drawing.Bitmap(ElevationPath);
				}
				catch (ArgumentException) //corrupt file
				{
					if (elevationDownloadRetries >= 3)
					{
						Debugger.E("Error loading map elevation data {0}: file was corrupt.", ElevationFilename);
						ElevationState = LoadingState.ErrorLoading;
						return;
					}

					image = null;
					if (File.Exists(ElevationPath))
					{
						try { File.Delete(ElevationPath); }
						catch
						{
							Debugger.E("Error deleting corrupt map elevation data {0}", ElevationFilename);
							ElevationState = LoadingState.ErrorLoading;
							return;
						}
					}
					elevationDownloadRetries++;
					ElevationState = LoadingState.Downloading;
					DownloadElevation(); //already in a thread, call this directly
					return;
				}
				lock (ElevationLock)
				{
					ElevationImage = image;
					ElevationState = LoadingState.Finished;
				}
				return;

			}
			catch (FileNotFoundException)
			{
				Debugger.E("Error loading map elevation data {0}: file not found", ElevationFilename);
			}
			catch (OutOfMemoryException)
			{
				Debugger.E("Error loading  map elevation data {0}: out of system memory", ElevationFilename);
			}
			ElevationState = LoadingState.ErrorLoading;
		}

		private abstract class PaintLayerBase : IDisposable
		{
			public Bitmap bitmap = null;
			public static Rectangle rectangle = new Rectangle(0, 0, COMPOSITE_SIZE, COMPOSITE_SIZE);
			protected readonly Color clearColor;

			public PaintLayerBase(Color clearColor)
			{
				this.clearColor = clearColor;
			}

			public void Dispose()
			{
				DestroyBitmap();
			}

			public virtual void Clear(Graphics graphics = null)
			{
				if (graphics != null)
					graphics.Clear(clearColor);
				else
					bitmap.G((g) => g.Clear(clearColor));				
			}

			public void CreateBitmap()
			{
				if (bitmap == null)
					bitmap = new Bitmap(COMPOSITE_SIZE, COMPOSITE_SIZE, PixelFormat.Format32bppPArgb);
				Clear();
			}

			public void DestroyBitmap()
			{
				if (bitmap != null)
					bitmap.Dispose();
				bitmap = null;
			}
		}

		private class TilePaintLayer : PaintLayerBase
		{
			public bool visible = false;

			public TilePaintLayer() : base(Color.Transparent) { }

			public override void Clear(Graphics graphics = null)
			{
				base.Clear(graphics);
				visible = false;
			}

			public void Paint(Tile tile, Bitmap image)
			{
				if (tile == null)
					return;
				bitmap.G((g) => tile.PaintComposite(image, g, ref rectangle));
			}
		}

		private class TileCompositeLayer : PaintLayerBase
		{
			public TileCompositeLayer(): base(Color.Peru)
			{
				CreateBitmap();
			}

			public void Paint(TilePaintLayer[] layers)
			{
				if (layers == null || layers.Length == 0)
					return;
				bitmap.G((g) =>
				{
					Clear(g);
					for (int i = 0; i < layers.Length; i++)
					{
						if (layers[i] == null || !layers[i].visible || layers[i].bitmap == null)
							continue;
						try
						{ g.DrawImageUnscaled(layers[i].bitmap, 0, 0); }
						catch { }
					}
				});
			}
		}
	}
}

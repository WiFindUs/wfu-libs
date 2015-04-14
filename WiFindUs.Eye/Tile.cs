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
using System.Xml.Serialization;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
	public class Tile : IRegion, IDisposable
	{
		public enum LoadingState : int
		{
			/// <summary>
			/// A general/unknown error occurred.
			/// </summary>
			Error = -3,
			
			/// <summary>
			/// An error occurred during the loading phase.
			/// </summary>
			ErrorLoading = -2,

			/// <summary>
			/// An error occurred during the data download phase.
			/// </summary>
			ErrorDownloading = -1,

			/// <summary>
			/// The tile is waiting to be assigned a valid center location.
			/// </summary>
			NoLocation = 0,

			/// <summary>
			/// The tile has a valid location but LoadOnlyOnRequest is true.
			/// </summary>
			Waiting = 1,

			/// <summary>
			/// The tile is currently downloading data.
			/// </summary>
			Downloading = 2,

			/// <summary>
			/// The tile is currently loading the data.
			/// </summary>
			Loading = 3,

			/// <summary>
			/// The tile has successfully downloaded the data and it is ready for use.
			/// </summary>
			Finished = 4
		}

		public event Action<Tile> ImageStateChanged;
		public event Action<Tile> ElevationStateChanged;
		public event Action<Tile> RegionChanged;
		public event Action<Tile> ImageChanged;
		public event Action<Tile> ElevationChanged;

		private const int ELEV_SAMPLE_COUNT = 128;
		private const int ELEV_SAMPLE_SIZE = 8;
		private const int ELEV_SIZE = ELEV_SAMPLE_COUNT * ELEV_SAMPLE_SIZE;
		private const double ELEV_MIN = -50.0;
		private const double ELEV_MAX = 2500.0;
		private const int TILE_IMAGE_SIZE = 640;
		private static readonly string MAPS_DIR = "maps" + Path.DirectorySeparatorChar;
		private static readonly string ELEV_URL
			= "https://maps.googleapis.com/maps/api/elevation/json?path=enc:{0}&samples={1}&key={2}";
		private readonly Tile[] children;
		private readonly Tile parent;
		private readonly uint childIndex; //which sub-tile this one is on the parent
		private Region region;
		private readonly uint zoomLevel, level, row, column;
		private volatile Image image = null;
		private volatile Bitmap elevation = null;
		private volatile LoadingState imageState = LoadingState.Waiting;
		private volatile LoadingState elevationState = LoadingState.Waiting;
		private bool loadImageAutomatically = true;
		private bool loadElevationAutomatically = true;
		private readonly Tile[][,] tileMap;
		private volatile bool disposed = false;
		private volatile int downloadRetries = 0;
		private const uint MAX_IMAGE_DOWNLOAD_THREADS = 4;
		private static volatile uint currentImageDownloadThreads = 0;
		private static readonly Regex PATTERN_ELEV_STATUS
			= new Regex("\"status\"\\s+[:]\\s+\"OK\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex PATTERN_ELEV_DATA = new Regex("[{]\\s*"
				+ "\"elevation\"\\s*[:]\\s*([+-]?[0-9]+(?:[.][0-9]+)?)\\s*[,]\\s*"
				+ "\"location\"\\s*[:]\\s*[{]\\s*"
				+ "\"lat\"\\s*[:]\\s*([+-]?[0-9]+(?:[.][0-9]+)?)\\s*[,]\\s*"
				+ "\"lng\"\\s*[:]\\s*([+-]?[0-9]+(?:[.][0-9]+)?)\\s*[}]\\s*[,]\\s*"
				+ "\"resolution\"\\s*[:]\\s*([+-]?[0-9]+(?:[.][0-9]+)?)\\s*[}]",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public bool LoadImageAutomatically
		{
			get { return loadImageAutomatically; }
			set
			{
				if (value == loadImageAutomatically)
					return;
				loadImageAutomatically = value;
				if (loadImageAutomatically && image == null && ImageState == LoadingState.Waiting)
					LoadImage();

				if (children != null)
				{
					children[0].LoadImageAutomatically
					= children[1].LoadImageAutomatically
					= children[2].LoadImageAutomatically
					= children[3].LoadImageAutomatically = value;
				}
			}
		}

		public bool LoadElevationAutomatically
		{
			get { return parent == null ? loadElevationAutomatically : parent.LoadElevationAutomatically; }
			set
			{
				if (parent == null)
				{
					if (value == loadElevationAutomatically)
						return;
					loadElevationAutomatically = value;
					if (loadElevationAutomatically && elevation == null && ElevationState == LoadingState.Waiting)
						LoadElevation();
				}
				else
					parent.LoadElevationAutomatically = value;
			}
		}

		public bool ThreadlessState
		{
			get
			{
				return (ImageState <= LoadingState.Waiting || ImageState == LoadingState.Finished)
						&& (ElevationState <= LoadingState.Waiting || ElevationState == LoadingState.Finished)
						&& (children == null || (children[0].ThreadlessState
								&& children[1].ThreadlessState
								&& children[2].ThreadlessState
								&& children[3].ThreadlessState));
			}
		}

		public LoadingState ImageState
		{
			get { return imageState; }
			private set
			{
				if (imageState == value)
					return;
				imageState = value;
				if (ImageStateChanged != null)
					ImageStateChanged(this);
			}
		}

		public Image Image
		{
			get { return image; }
			private set
			{
				if (value == image)
					return;
				DisposeImage();
				image = value;
				if (ImageChanged != null)
					ImageChanged(this);
			}
		}

		public LoadingState ElevationState
		{
			get { return parent == null ? elevationState : parent.ElevationState; }
			private set
			{
				if (parent != null)
					return;
				if (elevationState == value)
					return;
				elevationState = value;
				if (ElevationStateChanged != null)
					ElevationStateChanged(this);
			}
		}

		private Bitmap ElevationImage
		{
			get { return parent == null ? elevation : parent.ElevationImage; }
			set
			{
				if (parent != null)
					return;
				if (value == elevation)
					return;
				DisposeElevation();
				elevation = value;
				if (ElevationChanged != null)
					ElevationChanged(this);
			}
		}

		public ILocation Center
		{
			get { return region == null ? null : region.Center; }
			set
			{
				if ((region == null && value == null) || Location.Equals(value,region))
					return;

				//check for existing threads
				if (!ThreadlessState)
					throw new InvalidOperationException("You may not change the Center location - it is currently in a threaded state.");

				//change region
				region = value == null ? null : new Region(value, zoomLevel); //throws an exception on invalid value
				if (RegionChanged != null)
					RegionChanged(this);
				
				//check data directory
				if (region != null && !Directory.Exists(RootDirectory))
				{
					try { Directory.CreateDirectory(RootDirectory); }
					catch
					{
						Debugger.E("Error creating maps directory {0}", RootDirectory);
						ImageState = LoadingState.Error;
						if (parent == null)
							ElevationState = LoadingState.Error;
						return;
					}
				}

				//clear data and set states
				LoadingState state = (region == null ? LoadingState.NoLocation : LoadingState.Waiting);;
				Image = null;
				ImageState = state;
				if (parent == null)
				{
					ElevationImage = null;
					ElevationState = state;
				}

				//start threads
				if (loadImageAutomatically)
					LoadImage();
				if (parent == null && loadElevationAutomatically)
					LoadElevation();

				//do children
				if (children != null)
				{
					if (region == null)
					{
						children[0].Center = null;
						children[1].Center = null;
						children[2].Center = null;
						children[3].Center = null;
					}
					else
					{
						double lat = LatitudinalSpan * 0.25;
						double lng = LongitudinalSpan * 0.25;
						children[0].Center
							= new Location(Center.Latitude + lat, Center.Longitude - lng);
						children[1].Center
							= new Location(Center.Latitude + lat, Center.Longitude + lng);
						children[2].Center
							= new Location(Center.Latitude - lat, Center.Longitude - lng);
						children[3].Center
							= new Location(Center.Latitude - lat, Center.Longitude + lng);
					}
				}
			}
		}

		public double Height
		{
			get { return region == null ? 0.0 : region.Height; }
		}

		public double LatitudinalSpan
		{
			get { return region == null ? 0.0 : region.LatitudinalSpan; }
		}

		public double LongitudinalSpan
		{
			get { return region == null ? 0.0 : region.LongitudinalSpan; }
		}

		public ILocation NorthEast
		{
			get { return region == null ? Location.EMPTY : region.NorthEast; }
		}

		public ILocation NorthWest
		{
			get { return region == null ? Location.EMPTY : region.NorthWest; }
		}

		public ILocation SouthEast
		{
			get { return region == null ? Location.EMPTY : region.SouthEast; }
		}

		public ILocation SouthWest
		{
			get { return region == null ? Location.EMPTY : region.SouthWest; }
		}

		public double Width
		{
			get { return region == null ? 0.0 : region.Width; }
		}

		internal Tile Root
		{
			get { return parent == null ? this : parent.Root; }
		}

		internal Tile Parent
		{
			get { return parent; }
		}

		internal Tile[] Children
		{
			get { return children; }
		}

		public static uint ZoomLevelMin
		{
			get { return WiFindUs.Eye.Region.GOOGLE_MAPS_TILE_MIN_ZOOM + 1; }
		}

		public static uint ZoomLevelMax
		{
			get { return WiFindUs.Eye.Region.GOOGLE_MAPS_TILE_MAX_ZOOM; }
		}

		public static uint ZoomLevelCount
		{
			get { return ZoomLevelMax - ZoomLevelMin + 1; }
		}

		public uint ZoomLevel
		{
			get { return zoomLevel; }
		}

		public static uint LevelMin
		{
			get { return 0; }
		}

		public static uint LevelMax
		{
			get { return ZoomLevelMax - ZoomLevelMin; }
		}

		public uint Level
		{
			get { return level; }
		}

		public uint Row
		{
			get { return row; }
		}

		public uint Column
		{
			get { return column; }
		}

		internal string FilenameLatLong
		{
			get { return String.Format("{0:0.000000}_{1:0.000000}", region.Latitude.Value, region.Longitude.Value); }
		}

		internal string ImageFilename
		{
			get { return String.Format("{0}_{1}_satellite.png", FilenameLatLong, zoomLevel); }
		}

		internal string ElevationFilename
		{
			get { return parent == null ? String.Format("{0}_elevation.png", FilenameLatLong) : parent.ElevationFilename; }
		}

		internal string RootDirectory
		{
			get { return System.IO.Path.Combine(MAPS_DIR, Root.FilenameLatLong + Path.DirectorySeparatorChar); }
		}

		internal string ImagePath
		{
			get { return System.IO.Path.Combine(RootDirectory, ImageFilename); }
		}

		internal string ElevationPath
		{
			get { return System.IO.Path.Combine(RootDirectory, ElevationFilename); }
		}

		internal string ImageDownloadURL
		{
			get
			{
				return String.Format(
					"https://maps.googleapis.com/maps/api/staticmap?center={0:0.######},{1:0.######}&zoom={2}&scale={3}&size={4}x{5}&key={6}&maptype=satellite&format=png", 
					region.Latitude.Value, region.Longitude.Value, zoomLevel,
					2, TILE_IMAGE_SIZE, TILE_IMAGE_SIZE,
					WFUApplication.GoogleAPIKey);
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		internal Tile(Tile parent, uint childIndex)
		{
			this.parent = parent;
			this.childIndex = childIndex;
			zoomLevel = parent == null ? ZoomLevelMin : parent.zoomLevel + 1;
			level = zoomLevel - ZoomLevelMin;
			row = parent == null ? 0 : (parent.Row * 2) + (childIndex / 2);
			column = parent == null ? 0 : (parent.Column * 2) + (childIndex % 2);

			if (parent == null)
			{
				tileMap = new Tile[ZoomLevelCount][,];
				for (uint i = 0; i < ZoomLevelCount; i++)
					tileMap[i] = new Tile[RowMax(i) + 1, ColumnMax(i) + 1];
			}
			else
				tileMap = null;
			Root.tileMap[level][row, column] = this;

			if (zoomLevel >= ZoomLevelMax)
			{
				children = null;
				return;
			}

			children = new Tile[4] { null, null, null, null };
			children[0] = new Tile(this, 0);
			children[1] = new Tile(this, 1);
			children[2] = new Tile(this, 2);
			children[3] = new Tile(this, 3);
		}

		public Tile() : this(null, 0) { }

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public double Elevation(double lat, double lng)
		{
			if (parent != null)
				return parent.Elevation(lat, lng);
			if (elevation == null)
				return ELEV_MIN;
			int x, y;
			region.LocationToScreen(new Rectangle(0,0,ELEV_SIZE,ELEV_SIZE),lat,lng, out x, out y);
			Color col = elevation.GetPixel(x, y);

			uint val = (uint)((col.A << 24) | (col.R << 16) |
					(col.G << 8) | (col.B << 0));

			return ELEV_MIN + ((double)val / (double)0xFFFFFFFF) * (ELEV_MAX - ELEV_MIN);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public override string ToString()
		{
			return String.Format("Tile[{0},{1},{2} I:{3} E:{4}]",
				level, row, column, imageState, elevationState);
		}

		public bool Contains(double latitude, double longitude)
		{
			return WiFindUs.Eye.Region.Contains(region, latitude, longitude);
		}

		public bool Contains(ILocation location)
		{
			return WiFindUs.Eye.Region.Contains(region, location);
		}

		public Point LocationToScreen(Rectangle screenBounds, double latitude, double longitude)
		{
			return region == null ? Point.Empty : region.LocationToScreen(screenBounds, latitude, longitude);
		}

		public Point LocationToScreen(Rectangle screenBounds, ILocation location)
		{
			return region == null ? Point.Empty : region.LocationToScreen(screenBounds, location);
		}

		public ILocation Clamp(ILocation location)
		{
			return region == null ? Location.EMPTY : region.Clamp(location);
		}

		public Tile FindChild(uint l, uint r, uint c)
		{
			return tileMap[l][r,c];
		}

		public void LoadImage()
		{
			if (image != null || ImageState != LoadingState.Waiting)
				return;

			if (!File.Exists(ImagePath))
			{
				if (currentImageDownloadThreads >= MAX_IMAGE_DOWNLOAD_THREADS)
					return;
				currentImageDownloadThreads++;
				ImageState = LoadingState.Downloading;
				ThreadPool.QueueUserWorkItem(DownloadImageThread);
			}
			else
			{
				ImageState = LoadingState.Loading;
				ThreadPool.QueueUserWorkItem(LoadImageThread);
			}
		}

		public void LoadElevation()
		{
			if (parent != null || elevation != null || ElevationState != LoadingState.Waiting)
				return;

			if (!File.Exists(ElevationPath))
			{
				ElevationState = LoadingState.Downloading;
				ThreadPool.QueueUserWorkItem(DownloadElevationThread);
			}
			else
			{
				ElevationState = LoadingState.Loading;
				ThreadPool.QueueUserWorkItem(LoadElevationThread);
			}
		}

		public void DisposeImage()
		{
			try
			{
				if (image != null)
					image.Dispose();
			}
			catch { }
			image = null;
		}

		public void DisposeElevation()
		{
			try
			{
				if (elevation != null)
					elevation.Dispose();
			}
			catch { }
			elevation = null;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				if (children != null)
				{
					children[0].Dispose();
					children[1].Dispose();
					children[2].Dispose();
					children[3].Dispose();
				}
				
				DisposeImage();
				DisposeElevation();
			}

			disposed = true;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private static uint RowMax(uint level)
		{
			return level == 0 ? 0 : (uint)(1 << (int)(level)) - 1;
		}

		private static uint ColumnMax(uint level)
		{
			return level == 0 ? 0 : (uint)(1 << (int)(level)) - 1;
		}

		private void DownloadElevationThread(object args)
		{
#if DEBUG
			Debugger.T("enter");
#endif
			//generate zigzag list of points for polyline
			double latStep = LatitudinalSpan / (double)(ELEV_SAMPLE_COUNT - 1);
			double longStep = LongitudinalSpan / (double)(ELEV_SAMPLE_COUNT - 1);
			List<ILocation> points = new List<ILocation>();
			for (int row = 0; row < ELEV_SAMPLE_COUNT; row++)
			{
				for (int column = (row % 2 == 0 ? 0 : ELEV_SAMPLE_COUNT - 1);
					(row % 2 == 0 ? column < ELEV_SAMPLE_COUNT : column >= 0);
						column += (row % 2 == 0 ? 1 : -1))
				{
					double lat = NorthWest.Latitude.Value - (latStep * row);
					double lng = NorthWest.Longitude.Value + (longStep * column);
					points.Add(new Location(lat, lng));
				}
			}
			
			//download and check data
			StringBuilder sb = new StringBuilder();
			while (points.Count > 0)
			{
				int count = Math.Min(250, points.Count);
				List<ILocation> pts = new List<ILocation>(points.GetRange(0, count));
				points.RemoveRange(0, count);

				string url = String.Format(ELEV_URL, Location.ToPolyline(pts), count, WFUApplication.GoogleAPIKey);
				string data = "";
				try
				{
					using (WebClient elevationClient = new WebClient())
						data = elevationClient.DownloadString(url);
				}
				catch (Exception e) { continue; }

				if (PATTERN_ELEV_STATUS.IsMatch(data))
					sb.Append(data + "\n");
				if (points.Count > 0)
					Thread.Sleep(WFUApplication.Random.Next(250, 500));
			}
				

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
					region.LocationToScreen(bmpRect, lat, lng, out x, out y);
					x = (x / ELEV_SAMPLE_SIZE) * ELEV_SAMPLE_SIZE; //rounding
					y = (y / ELEV_SAMPLE_SIZE) * ELEV_SAMPLE_SIZE; //rounding
					Rectangle radRect = new Rectangle(x, y, ELEV_SAMPLE_SIZE, ELEV_SAMPLE_SIZE);

					//get colour values
					uint cval = (uint)(((elev - ELEV_MIN) / (ELEV_MAX - ELEV_MIN)) * (double)0xFFFFFFFF);
					Color col = Color.FromArgb((byte)(cval >> 24), (byte)(cval >> 16), (byte)(cval >> 8), (byte)(cval >> 0));

					//paint heightmap
					using (SolidBrush b = new SolidBrush(col))
						g.FillRectangle(b, radRect);

				}
			}
			bmp.Save(ElevationPath,System.Drawing.Imaging.ImageFormat.Png);
			bmp.Dispose();

			ElevationImage = bmp;
			ElevationState = LoadingState.Finished;

#if DEBUG
			Debugger.T("exit");
#endif
		}

		private void LoadElevationThread(Object args)
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
					if (downloadRetries >= 3)
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
					downloadRetries++;
					ElevationState = LoadingState.Downloading;
					DownloadElevationThread(null); //already in a thread, call this directly
					return;
				}

				ElevationImage = image;
				ElevationState = LoadingState.Finished;
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

		private void DownloadImageThread(Object args)
		{
#if DEBUG
			Debugger.T("enter");
#endif
			WebClient imageClient = new WebClient();
			imageClient.DownloadFileCompleted += imageClient_DownloadFileCompleted;
			imageClient.DownloadProgressChanged += imageClient_DownloadProgressChanged;
			imageClient.DownloadFileAsync(new Uri(ImageDownloadURL), ImagePath, null);
		}

		private void imageClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			WebClient imageClient = sender as WebClient;

			if (e.Cancelled || e.Error != null)
			{
				if (e.Error != null)
					Debugger.E("Error downloading map tile texture {0}", ImageFilename);
				ImageState = LoadingState.ErrorDownloading;

				if (File.Exists(ImagePath))
				{
					try { File.Delete(ImagePath); }
					catch { }
				}
			}
			else //advance to the image loading state
			{
				ImageState = LoadingState.Loading;
				ThreadPool.QueueUserWorkItem(LoadImageThread);
			}
			currentImageDownloadThreads--;
			imageClient.DownloadFileCompleted -= imageClient_DownloadFileCompleted;
			imageClient.DownloadProgressChanged -= imageClient_DownloadProgressChanged;
			imageClient.Dispose();
#if DEBUG
			Debugger.T("exit");
#endif
		}

		private void imageClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			WebClient imageClient = sender as WebClient;
			if (WiFindUs.Forms.MainForm.HasClosed)
			{
				imageClient.CancelAsync();
				return;
			}
		}

		private void LoadImageThread(Object args)
		{
			try
			{
				//get source image
				System.Drawing.Bitmap image;
				try
				{
					image = new System.Drawing.Bitmap(ImagePath);
				}
				catch (ArgumentException) //corrupt file
				{
					if (downloadRetries >= 3)
					{
						Debugger.E("Error loading map tile texture {0}: file was corrupt.", ImageFilename);
						ImageState = LoadingState.ErrorLoading;
						return;
					}
					
					image = null;
					if (File.Exists(ImagePath))
					{
						try { File.Delete(ImagePath); }
						catch
						{
							Debugger.E("Error deleting corrupt map tile texture {0}", ImageFilename);
							ImageState = LoadingState.ErrorLoading;
							return;
						}
					}
					downloadRetries++;
					ImageState = LoadingState.Downloading;
					DownloadImageThread(null); //already in a thread, call this directly
					return;
				}
				
				
				//determine what the image scale is
				float scale = (WFUApplication.Config == null ? 1.0f : WFUApplication.Config.Get("map.texture_scale", 1.0f))
					.Clamp(0.1f, 1.0f);

				//is scaling necessary?
				int w = (int)(image.Width * scale);
				int h = (int)(image.Height * scale);
				if (w != image.Width || h != image.Height)
				{
					System.Drawing.Image resizedImage = image.Resize(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
					image.Dispose();
					image = (System.Drawing.Bitmap)resizedImage;
				}

				/*
				System.Drawing.Bitmap image = new System.Drawing.Bitmap(
					initial.Width, initial.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
				using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(image))
				{
					gr.SetQuality(GraphicsExtensions.GraphicsQuality.High);
					gr.DrawImage((System.Drawing.Image)initial, new System.Drawing.Rectangle(0, 0, image.Width, image.Height));
				}
				 * */

				Image = image;
				ImageState = LoadingState.Finished;
				return;

			}
			catch (FileNotFoundException)
			{
				Debugger.E("Error loading map tile texture {0}: file not found", ImageFilename);
			}
			catch (OutOfMemoryException)
			{
				Debugger.E("Error loading map tile texture {0}: out of system memory", ImageFilename);
			}
			ImageState = LoadingState.ErrorLoading;
		}
	}
}

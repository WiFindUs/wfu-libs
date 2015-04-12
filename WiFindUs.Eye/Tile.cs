using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

		private const uint ELEV_DENSITY = 8;
		private const int TILE_IMAGE_SIZE = 640;
		private static readonly string MAPS_DIR = "maps" + Path.DirectorySeparatorChar;
		private static readonly string ELEV_URL
			= "https://maps.googleapis.com/maps/api/elevation/json?locations={0:0.######},{1:0.######}&key={2}";
		private double[,] elevation;
		private readonly Tile[] children;
		private readonly Tile parent;
		private readonly uint childIndex; //which sub-tile this one is on the parent
		private Region region;
		private readonly uint zoomLevel, level, row, column;
		private Image image = null;
		private volatile LoadingState imageState = LoadingState.Waiting;
		private volatile LoadingState elevationState = LoadingState.Waiting;
		private bool loadImageAutomatically = true;
		private bool loadElevationAutomatically = true;
		private readonly Tile[][,] tileMap;
		private bool disposed = false;
		private volatile int downloadRetries = 0;

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
			get { return loadElevationAutomatically; }
			set
			{
				if (value == loadElevationAutomatically)
					return;
				loadElevationAutomatically = value;
				if (loadElevationAutomatically && elevation == null && ElevationState == LoadingState.Waiting)
					LoadElevation();

				if (children != null)
				{
					children[0].LoadElevationAutomatically
					= children[1].LoadElevationAutomatically
					= children[2].LoadElevationAutomatically
					= children[3].LoadElevationAutomatically = value;
				}
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

		public double[,] Elevation
		{
			get { return elevation; }
			private set
			{
				if (value == elevation)
					return;
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
						ImageState = ElevationState = LoadingState.Error;
						return;
					}
				}

				//clear data
				Image = null;
				Elevation = null;

				//set states
				ImageState = ElevationState = (region == null ? LoadingState.NoLocation : LoadingState.Waiting);
				if (loadImageAutomatically)
					LoadImage();
				if (loadElevationAutomatically)
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
			//get { return ZoomLevelMin + 2; }
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
			get { return String.Format("{0}_{1}_elevation.xml", FilenameLatLong, zoomLevel); }
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
			if (elevation != null || ElevationState != LoadingState.Waiting)
				return;

			if (!File.Exists(ElevationPath))
			{
				ElevationState = LoadingState.Downloading;
				//ThreadPool.QueueUserWorkItem(DownloadElevationThread);
			}
			else
			{
				ElevationState = LoadingState.Loading;
				//ThreadPool.QueueUserWorkItem(LoadElevationThread);
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

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				DisposeImage();
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

		private void DownloadImageThread(Object args)
		{
			WebClient imageClient = new WebClient();
			imageClient.DownloadFileCompleted += imageClient_DownloadFileCompleted;
			imageClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
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

			imageClient.DownloadFileCompleted -= imageClient_DownloadFileCompleted;
			imageClient.DownloadProgressChanged -= webClient_DownloadProgressChanged;
			imageClient.Dispose();
		}

		private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			WebClient webClient = sender as WebClient;
			if (WiFindUs.Forms.MainForm.HasClosed)
			{
				webClient.CancelAsync();
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
			//catch (Exception ex)
			//{
			//	Debugger.E("Error loading map tile texture {0}: {1}",ImageFilename,ex.Message);
			//}
			ImageState = LoadingState.ErrorLoading;
		}
	}
}

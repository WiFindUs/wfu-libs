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

		public event Action<Tile> RegionChanged;

		private static readonly string MAPS_DIR = "maps" + Path.DirectorySeparatorChar;
		private readonly Tile[] children;
		private readonly Tile parent;
		private readonly BaseTile root;
		private readonly uint childIndex; //which sub-tile this one is on the parent
		private Region region;
		private readonly uint zoomLevel, level, row, column;
		private static readonly Color[] debugColours = new Color[]
		{
			Color.FromArgb(50, Color.Red),
			Color.FromArgb(50, Color.Orange),
			Color.FromArgb(50, Color.Yellow),
			Color.FromArgb(50, Color.Green),
			Color.FromArgb(50, Color.Blue),
			Color.FromArgb(50, Color.Indigo),
			Color.FromArgb(50, Color.Violet),
		};

		//image
		private const int IMAGE_SIZE = 640;
		private const int IMAGE_SCALE = 2;
		private static readonly string IMAGE_FORMAT = "jpg";
		private static readonly string IMAGE_TYPE = "satellite";
		private static readonly string IMAGE_URL
			= "https://maps.googleapis.com/maps/api/staticmap?center={0:0.######},{1:0.######}&zoom={2}&scale={3}&size={4}x{5}&maptype={6}&format={7}&key={8}";
		private volatile LoadingState imageState = LoadingState.Waiting;
		private volatile bool disposed = false;
		internal volatile int downloadRetries = 0;
		

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public bool Disposed
		{
			get { return disposed; }
		}

		public virtual bool ThreadlessState
		{
			get { return root.ThreadlessState; }
		}

		public LoadingState ImageState
		{
			get { return imageState; }
			internal set
			{
				if (imageState == value)
					return;
				imageState = value;
			}
		}

		internal bool ObscuredByChildren
		{
			get
			{
				if (children == null)
					return false;

				return (children[0].imageState == LoadingState.Finished || children[0].ObscuredByChildren)
						&& (children[1].imageState == LoadingState.Finished || children[1].ObscuredByChildren)
						&& (children[2].imageState == LoadingState.Finished || children[2].ObscuredByChildren)
						&& (children[3].imageState == LoadingState.Finished || children[3].ObscuredByChildren);
			}
		}

		internal bool VisibleInComposite
		{
			get
			{
				return ImageState == LoadingState.Finished
					&& !ObscuredByChildren;
			}
		}

		public ILocation Center
		{
			get { return region == null ? null : region.Center; }
			set
			{
				if ((region == null && value == null) || Location.Equals(value, region))
					return;

				//check for existing threads
				if (!ThreadlessState)
					throw new InvalidOperationException("You may not change the Center location - the tile is currently in a threaded state.");

				region = value == null ? null : new Region(value, zoomLevel); //throws an exception on invalid value

				//change location
				LocationChanged();

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

		public virtual Region Region
		{
			get { return region; }
			internal set
			{
				region = value;
				if (RegionChanged != null)
					RegionChanged(this);
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

		internal BaseTile Root
		{
			get { return root; }
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
			//get { return ZoomLevelMin; }
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
			get { return String.Format("{0}_{1}_satellite.{2}", FilenameLatLong, zoomLevel, IMAGE_FORMAT); }
		}

		internal string RootDirectory
		{
			get { return System.IO.Path.Combine(MAPS_DIR, Root.FilenameLatLong + Path.DirectorySeparatorChar); }
		}

		internal string ImagePath
		{
			get { return System.IO.Path.Combine(RootDirectory, ImageFilename); }
		}

		internal string ImageDownloadURL
		{
			get
			{
				return String.Format(
					IMAGE_URL, //url
					region.Latitude.Value, //lat {0}
					region.Longitude.Value, //long {1}
					zoomLevel, //zoom {2}
					IMAGE_SCALE, //scale {3}
					IMAGE_SIZE, //width {4}
					IMAGE_SIZE, //height {5}
					IMAGE_TYPE, //map type {6}
					IMAGE_FORMAT, //format {7}
					WFUApplication.GoogleAPIKey //api key {8}
					);
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		internal Tile(BaseTile root, Tile parent, uint childIndex)
		{
			this.parent = parent;
			bool isRoot = parent == null;
			this.root = isRoot ? this as BaseTile : root;
			this.root.AllTiles.Add(this);
			this.childIndex = childIndex;
			zoomLevel = isRoot ? ZoomLevelMin : parent.zoomLevel + 1;
			level = zoomLevel - ZoomLevelMin;
			row = isRoot ? 0 : (parent.Row * 2) + (childIndex / 2);
			column = isRoot ? 0 : (parent.Column * 2) + (childIndex % 2);
			

			if (zoomLevel >= ZoomLevelMax)
			{
				children = null;
				return;
			}

			children = new Tile[4] { null, null, null, null };
			children[0] = new Tile(this.root, this, 0);
			children[1] = new Tile(this.root, this, 1);
			children[2] = new Tile(this.root, this, 2);
			children[3] = new Tile(this.root, this, 3);
		}


		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public virtual double Elevation(double lat, double lng)
		{
			return root.Elevation(lat, lng);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public override string ToString()
		{
			return String.Format("Tile[{0},{1},{2} I:{3}]",
				level, row, column, imageState);
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
			}

			disposed = true;
		}

		protected virtual void LocationChanged()
		{
			//check data directory
			if (region != null && !Directory.Exists(RootDirectory))
			{
				try { Directory.CreateDirectory(RootDirectory); }
				catch
				{
					Debugger.E("Error creating maps directory {0}", RootDirectory);
					ImageState = LoadingState.Error;
					return;
				}
			}

			//clear data and set states
			ImageState = (region == null ? LoadingState.NoLocation : LoadingState.Waiting);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		internal void PaintComposite(Bitmap image, Graphics g, ref Rectangle outputBounds)
		{
			//determine bounds
			int xStep = (int)((float)outputBounds.Width / (float)(ColumnMax(level) + 1));
			int yStep = (int)((float)outputBounds.Height / (float)(RowMax(level) + 1));
			Rectangle rect = new Rectangle(
				outputBounds.X + (int)column * xStep,
				outputBounds.Y + (int)row * yStep,
				xStep, yStep);
			
			//paint image
			g.CompositingMode = CompositingMode.SourceCopy;
			g.DrawImage(image, rect);
#if DEBUG
			//paint some debug stuff
			g.CompositingMode = CompositingMode.SourceOver;
			using (Brush b = new SolidBrush(debugColours[level % debugColours.Length]))
				g.FillRectangle(b, rect);

			using (StringFormat sf = new StringFormat(StringFormat.GenericTypographic))
			{
				sf.Alignment = StringAlignment.Center;
				sf.LineAlignment = StringAlignment.Center;
				g.DrawString(String.Format("Level\n{0}", level),
				WiFindUs.Themes.Theme.Current.Titles.Large.Bold,
				Brushes.White, rect.X + rect.Width / 2, rect.Y + rect.Height / 2,
				sf);
			}
#endif
		}

		internal static uint RowMax(uint level)
		{
			return level == 0 ? 0 : (uint)(1 << (int)(level)) - 1;
		}

		internal static uint ColumnMax(uint level)
		{
			return level == 0 ? 0 : (uint)(1 << (int)(level)) - 1;
		}

	}
}

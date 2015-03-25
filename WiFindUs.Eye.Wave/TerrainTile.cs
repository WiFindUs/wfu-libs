using System;
using System.IO;
using System.Net;
using System.Threading;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Materials;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Extensions;

namespace WiFindUs.Eye.Wave
{
	public class TerrainTile : Behavior
	{
		public event Action<TerrainTile> TextureLoadingFinished, TextureImageLoadingFinished, TextureError, CenterLocationChanged;

		private const float UNTEXTURED_SCALE = 0.4f;
		private const float SCALE_SPEED = 2.0f;
		private const int MAX_CONCURRENT_TEXTURE_CREATIONS = 1;
		private readonly int MAX_CONCURRENT_LOADS = Environment.ProcessorCount;
		private const int MAX_CONCURRENT_DOWNLOADS = 1;
		private const int TILE_IMAGE_SIZE = 640;
		private static readonly string IMAGE_FORMAT = "png";
		private static readonly string MAPS_URL_BASE = "https://maps.googleapis.com/maps/api/staticmap?";
		private static readonly string MAPS_DIR = "maps" + System.IO.Path.DirectorySeparatorChar;
		private static int currentDownloads = 0;
		private static int currentLoads = 0;
		private static int currentTextureCreations = 0;

		private Transform3D transform3D;
		private MaterialsMap materialsMap;
		private BoxCollider boxCollider;

		private BasicMaterial placeholderMaterial;
		private bool errorState = false;
		private Region region;
		private uint googleMapsZoomLevel;
		private uint row, column;
		private TerrainTile baseTile;
		private float size;
		private Vector3 topLeft, bottomRight;
		private bool textured = false;
		private int lastDownloadPercentage = 0;
		private bool mapImageFileExists = false;
		private System.Drawing.Image tileImage = null; //only used on base tile
		private object threadObject = null;
		private float horizontalScale;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public ILocation CenterLocation
		{
			get
			{
				return region == null ? null : region.Center;
			}
			set
			{
				if (value == null || (region != null && value.Equals(region.Center)))
					return;

				CancelThreads();
				placeholderMaterial.DiffuseColor = (row + column) % 2 == 0 ? Color.Peru : Color.Sienna;
				materialsMap.DefaultMaterial = placeholderMaterial;
				region = new Region(value, googleMapsZoomLevel);
				errorState = false;
				textured = false;
				horizontalScale = UNTEXTURED_SCALE;
				transform3D.Scale = new Vector3(horizontalScale, 1.0f, horizontalScale);
				if (TileImage != null)
				{
					TileImage.Dispose();
					TileImage = null;
				}
				if (!Directory.Exists(ImageDirectory))
				{
					mapImageFileExists = false;
					try
					{
						Directory.CreateDirectory(ImageDirectory);
					}
					catch
					{
						ErrorState("Error creating directory '" + ImageDirectory + "'");
						return;
					}
				}
				else
					mapImageFileExists = File.Exists(ImagePath);
				if (CenterLocationChanged != null)
					CenterLocationChanged(this);
				Owner.IsActive = true;
			}
		}

		public IRegion Region
		{
			get { return region; }
		}

		public uint Row
		{
			get { return row; }
		}

		public uint Column
		{
			get { return column; }
		}

		public uint Layer
		{
			get { return googleMapsZoomLevel - MapScene.MIN_LEVEL; }
		}

		public float Size
		{
			get { return size; }
		}

		public Vector3 TopLeft
		{
			get { return topLeft; }
		}

		public Vector3 BottomRight
		{
			get { return bottomRight; }
		}

		private string ImageFilenameLatLong
		{
			get
			{
				return String.Format("{0:0.000000}_{1:0.000000}",
					region.Latitude.Value, region.Longitude.Value);
			}
		}

		private string ImageFilename
		{
			get
			{
				return String.Format("{0}_{1}_{2}.{3}",
					ImageFilenameLatLong, googleMapsZoomLevel, "satellite", IMAGE_FORMAT);
			}
		}

		private string ImageDirectory
		{
			get { return baseTile == null ? System.IO.Path.Combine(MAPS_DIR, ImageFilenameLatLong + System.IO.Path.DirectorySeparatorChar) : baseTile.ImageDirectory; }
		}

		private string ImagePath
		{
			get { return System.IO.Path.Combine(ImageDirectory, ImageFilename); }
		}

		private string ImageDownloadURL
		{
			get
			{
				return String.Format(
					"{0}center={1:0.######},{2:0.######}&zoom={3}&scale={4}&size={5}x{6}&key={7}&maptype={8}&format={9}",
					MAPS_URL_BASE, region.Latitude.Value, region.Longitude.Value, googleMapsZoomLevel,
					2, TILE_IMAGE_SIZE, TILE_IMAGE_SIZE,
					WFUApplication.GoogleAPIKey, "satellite", IMAGE_FORMAT);
			}
		}

		public System.Drawing.Image TileImage
		{
			get { return tileImage; }
			protected set
			{
				if (value == tileImage)
					return;
				if (tileImage != null)
				{
					tileImage.Dispose();
					tileImage = null;
				}
				tileImage = value;
			}
		}

		public bool Error
		{
			get { return errorState; }
			private set
			{
				if (value == errorState)
					return;
				errorState = value;
				if (errorState)
				{
					Owner.IsActive = false;
					placeholderMaterial.DiffuseColor = Color.Red;
					if (TextureError != null)
						TextureError(this);
				}
			}
		}

		public bool Textured
		{
			get { return textured; }
			private set
			{
				if (value == textured)
					return;
				textured = value;
				if (textured)
				{
					if (TextureLoadingFinished != null)
						TextureLoadingFinished(this);
				}
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		private TerrainTile(TerrainTile baseTile, uint googleMapsZoomLevel, uint row, uint column, float size)
		{
			if (googleMapsZoomLevel < WiFindUs.Eye.Region.GOOGLE_MAPS_TILE_MIN_ZOOM
				|| googleMapsZoomLevel > WiFindUs.Eye.Region.GOOGLE_MAPS_TILE_MAX_ZOOM)
				throw new ArgumentOutOfRangeException("googleMapsZoomLevel", "Zoom level must be between "
					+ MapScene.MIN_LEVEL + " and " + WiFindUs.Eye.Region.GOOGLE_MAPS_TILE_MAX_ZOOM + " (inclusive).");

			this.googleMapsZoomLevel = googleMapsZoomLevel;
			this.row = row;
			this.column = column;
			this.baseTile = baseTile;
			this.size = size;
		}

		public static Entity Create(uint layer, uint row, uint column, TerrainTile baseTile)
		{
			float size = (float)Math.Pow(2.0,
				8.0 + (MapScene.MIN_LEVEL - WiFindUs.Eye.Region.GOOGLE_MAPS_TILE_MIN_ZOOM)//smallest chunks will be sized at this power of two
				+ (WiFindUs.Eye.Region.GOOGLE_MAPS_TILE_MAX_ZOOM - WiFindUs.Eye.Region.GOOGLE_MAPS_TILE_MIN_ZOOM)
				- layer) / 10.0f;

			BasicMaterial mat = new BasicMaterial((row + column) % 2 == 0 ? Color.Peru : Color.Sienna);
			Entity tileEntity = new Entity() { IsActive = false }
			.AddComponent(new Transform3D())
			.AddComponent(new MaterialsMap(mat))
			.AddComponent(Model.CreatePlane(Vector3.UnitY, size))
			.AddComponent(new ModelRenderer())
			.AddComponent(new BoxCollider() { DebugLineColor = Color.Brown })
			.AddComponent(new TerrainTile(
				layer == 0 ? null : baseTile,
				MapScene.MIN_LEVEL + layer,
				row, column,
				size)
				{
					placeholderMaterial = mat
				});

			return tileEntity;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public void CalculatePosition()
		{
			if (baseTile == null)
				transform3D.LocalPosition = new Vector3(0f, 0f, 0f);
			else
			{
				float start = (baseTile.Size / -2.0f) + (size / 2.0f);
				transform3D.LocalPosition = new Vector3(
					start + (column * size),
					0.0f,
					start + (row * size));
			}

			topLeft = new Vector3(transform3D.Position.X - size / 2.0f, 0.0f, transform3D.Position.Z - size / 2.0f);
			bottomRight = new Vector3(transform3D.Position.X + size / 2.0f, 0.0f, transform3D.Position.Z + size / 2.0f);
		}

		public void CancelThreads()
		{
			if (threadObject != null && (threadObject is WebClient))
				(threadObject as WebClient).CancelAsync();
			threadObject = null;
		}

		public Vector3 LocationToVector(ILocation loc)
		{
			if (region == null)
				return Vector3.Zero;
			return region.LocationToVector(topLeft, bottomRight, loc);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			transform3D = Owner.FindComponent<Transform3D>();
			materialsMap = Owner.FindComponent<MaterialsMap>();
			boxCollider = Owner.FindComponent<BoxCollider>();
		}

		private void CheckTextureState()
		{
			//check for map tile, download if necessary
			if (!mapImageFileExists)
			{
				if (currentDownloads >= MAX_CONCURRENT_DOWNLOADS)
					return;

				currentDownloads++;
				Debugger.V("Downloading map tile texture " + ImageFilename + "...");
				placeholderMaterial.DiffuseColor = Color.Orange;
				WebClient downloadClient = new WebClient();
				downloadClient.DownloadFileCompleted += DownloadFileCompleted;
				downloadClient.DownloadProgressChanged += DownloadProgressChanged;
				downloadClient.DownloadFileAsync(new Uri(ImageDownloadURL), ImagePath, null);
				threadObject = downloadClient;
				return;
			}

			//the image has already been loaded, create a texture from it
			if (TileImage != null)
			{
				if (currentTextureCreations >= MAX_CONCURRENT_TEXTURE_CREATIONS)
					return;

				currentTextureCreations++;
				placeholderMaterial.DiffuseColor = Color.Green;
				Thread textureThread = new Thread(new ThreadStart(TextureCreationThread));
				threadObject = textureThread;
				textureThread.Start();
			}
			else //load it
			{
				if (currentLoads >= Math.Max(1, Environment.ProcessorCount / 2))
					return;

				currentLoads++;
				placeholderMaterial.DiffuseColor = Color.Yellow;
				Thread loadThread = new Thread(new ThreadStart(LoadThread));
				threadObject = loadThread;
				loadThread.Start();
			}
		}

		protected override void Update(TimeSpan gameTime)
		{
			if (!Error)
			{
				if (Textured)
				{
					bool done = false;
					horizontalScale += (float)gameTime.TotalSeconds * SCALE_SPEED;
					if (horizontalScale > 1.0f)
					{
						horizontalScale = 1.0f;
						done = true;
					}
					transform3D.Scale = new Vector3(horizontalScale, 1.0f, horizontalScale);
					if (done)
						Owner.IsActive = false;
				}
				else if (threadObject == null
					&& Owner.Parent.IsVisible //tile layer
					&& Owner.Scene.RenderManager.ActiveCamera3D.Contains(boxCollider))
					CheckTextureState();
			}
		}

		protected override void DeleteDependencies()
		{
			TileImage = null;
			base.DeleteDependencies();
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			WebClient downloadClient = sender as WebClient;

			if (e.Cancelled || e.Error != null)
			{
				if (threadObject == downloadClient && e.Error != null)
					ErrorState("Error downloading map tile texture " + ImageFilename + ".");

				if (File.Exists(ImagePath))
				{
					try { File.Delete(ImagePath); }
					catch { }
				}
			}
			else
				mapImageFileExists = true;

			downloadClient.DownloadFileCompleted -= DownloadFileCompleted;
			downloadClient.DownloadProgressChanged -= DownloadProgressChanged;
			downloadClient.Dispose();
			currentDownloads--;
			if (threadObject == downloadClient)
				threadObject = null;
		}

		private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			WebClient downloadClient = sender as WebClient;
			if (threadObject != downloadClient)
				return;

			if ((lastDownloadPercentage < 25 && e.ProgressPercentage >= 25)
				|| (lastDownloadPercentage < 50 && e.ProgressPercentage >= 50)
				|| (lastDownloadPercentage < 75 && e.ProgressPercentage >= 75))
				Debugger.V("Downloaded " + e.ProgressPercentage.ToString() + "%");
			lastDownloadPercentage = e.ProgressPercentage;
		}

		private void LoadThread()
		{
			object initialThreadObject = threadObject; //should be the loading thread

			try
			{
				//determine what the image scale is
				float scale = (WFUApplication.Config == null ? 1.0f : WFUApplication.Config.Get("map.texture_scale", 1.0f))
					.Clamp(0.1f, 1.0f);

				//get source image
				System.Drawing.Image image = System.Drawing.Image.FromFile(ImagePath);
				if (initialThreadObject == threadObject) //cancelled?
				{
					//scale is necessary
					int w = (int)(image.Width * scale);
					int h = (int)(image.Height * scale);
					if (w != image.Width || h != image.Height)
					{
						System.Drawing.Image resizedImage = image.Resize(w, h);
						image.Dispose();
						image = resizedImage;
					}

					if (initialThreadObject == threadObject) //cancelled?
					{
						//store image
						TileImage = image;
					}
				}
			}
			catch (FileNotFoundException)
			{
				ErrorState("Error loading map tile texture " + ImageFilename + ": file not found");
			}
			catch (OutOfMemoryException)
			{
				ErrorState("Error loading map tile texture " + ImageFilename + ": out of system memory");
			}
			catch (Exception ex)
			{
				ErrorState("Error loading map tile texture " + ImageFilename + ": " + ex.Message);
			}

			if (Error)
				TileImage = null;
			if (initialThreadObject == threadObject) //cancelled?
			{
				threadObject = null;
				if (TileImage != null)
					placeholderMaterial.DiffuseColor = Color.GreenYellow;

				if (TextureImageLoadingFinished != null)
					TextureImageLoadingFinished(this);
			}
			currentLoads--;
		}

		private void TextureCreationThread()
		{
			object initialThreadObject = threadObject; //should be the texture thread

			if (TileImage != null)
			{
				using (Stream stream = TileImage.GetStream())
				{
					try
					{
						materialsMap.DefaultMaterial = new BasicMaterial(Texture2D.FromFile(RenderManager.GraphicsDevice, stream));
					}
					catch
					{
						ErrorState("Error creating texture for " + ImageFilename + ": exception occurred.");
					}
				}
				if (baseTile != null)
					TileImage = null; //don't hang on to the texture for child tiles
			}
			else
				ErrorState("Error creating texture for " + ImageFilename + ": image not loaded.");

			if (initialThreadObject == threadObject) //cancelled?
			{
				threadObject = null;
				Textured = true;
			}
			currentTextureCreations--;
		}

		private void ErrorState(string message)
		{
			Debugger.E(message);
			Error = true;
		}
	}
}
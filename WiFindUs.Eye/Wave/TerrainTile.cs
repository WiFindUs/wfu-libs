using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Materials;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class TerrainTile : Behavior
    {
        private static Material placeHolderMaterial, placeHolderMaterialAlt,
            loadingMaterial, downloadingMaterial, errorMaterial;
        private const int MAX_CONCURRENT_LOADS = 1;
        private const int MAX_CONCURRENT_DOWNLOADS = 1;
        private const int TILE_IMAGE_SIZE = 640;
        private static readonly string IMAGE_FORMAT = "png";
        private static readonly string MAPS_URL_BASE = "https://maps.googleapis.com/maps/api/staticmap?";
        private static readonly string MAPS_DIR = "maps/";
        private static int currentDownloads = 0;
        private static int currentLoads = 0;
        private bool cancelThreads = false;

        [RequiredComponent]
        private Transform3D transform3D;
        [RequiredComponent]
        private MaterialsMap materialsMap;

        private Region region;
        private uint googleMapsZoomLevel;
        private uint row, column;
        private TerrainTile baseTile;
        private float size;
        private Vector3 topLeft, bottomRight;
        private Thread loadThread;
        private bool mapTextured = true;
        private WebClient downloadClient = null;
        private int lastDownloadPercentage = 0;
        private static bool mapsDirectoryExists = false;
        private bool mapImageFileExists = false;

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
                region = new Region(value, googleMapsZoomLevel);
                mapImageFileExists = File.Exists(ImagePath);
                mapTextured = false;
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

        public static Material PlaceHolderMaterial
        {
            get
            {
                if (placeHolderMaterial == null)
                    placeHolderMaterial = new BasicMaterial(Color.Peru);
                return placeHolderMaterial;
            }
        }
        public static Material PlaceHolderMaterialAlt
        {
            get
            {
                if (placeHolderMaterialAlt == null)
                    placeHolderMaterialAlt = new BasicMaterial(Color.Sienna);
                return placeHolderMaterialAlt;
            }
        }

        public static Material LoadingMaterial
        {
            get
            {
                if (loadingMaterial == null)
                    loadingMaterial = new BasicMaterial(Color.Yellow);
                return loadingMaterial;
            }
        }

        public static Material DownloadingMaterial
        {
            get
            {
                if (downloadingMaterial == null)
                    downloadingMaterial = new BasicMaterial(Color.Orange);
                return downloadingMaterial;
            }
        }

        public static Material ErrorMaterial
        {
            get
            {
                if (errorMaterial == null)
                    errorMaterial = new BasicMaterial(Color.Red);
                return errorMaterial;
            }
        }

        private string ImageFilename
        {
            get
            {
                return String.Format("{0:0.000000}_{1:0.000000}_{2}_{3}.{4}",
                    region.Latitude.Value, region.Longitude.Value, googleMapsZoomLevel, "satellite", IMAGE_FORMAT);
            }
        }

        private string ImagePath
        {
            get { return System.IO.Path.Combine(MAPS_DIR, ImageFilename); }
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

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public TerrainTile(TerrainTile baseTile, uint googleMapsZoomLevel, uint row, uint column, float size)
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

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public void CalculatePosition()
        {
            if (baseTile == null)
                transform3D.Position = new Vector3(0f, 0f, 0f);
            else
            {
                float start = (baseTile.Size / -2.0f) + (size / 2.0f);
                transform3D.Position = new Vector3(
                    start + (column * size),
                    0.0f,
                    start + (row * size));
            }

            topLeft = new Vector3(transform3D.Position.X - size/2.0f, 0.0f, transform3D.Position.Z - size/2.0f);
            bottomRight = new Vector3(transform3D.Position.X + size / 2.0f, 0.0f, transform3D.Position.Z + size / 2.0f);
        }

        public void CancelThreads()
        {
            cancelThreads = true;
            if (downloadClient != null)
                downloadClient.CancelAsync();
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


        private void CheckTextureState()
        {
            if (mapTextured || loadThread != null || downloadClient != null || cancelThreads)
                return;

            //check maps directory exists, create if necessary
            if (!mapsDirectoryExists)
            {
                if (!Directory.Exists(MAPS_DIR))
                {
                    try
                    {
                        Directory.CreateDirectory(MAPS_DIR);
                    }
                    catch
                    {
                        Debugger.E("Error creating maps directory!");
                        materialsMap.DefaultMaterial = ErrorMaterial;
                        mapTextured = true;
                        return;
                    }
                }
                mapsDirectoryExists = true;
            }

            //check for map tile, download if necessary
            if (!mapImageFileExists)
            {
                if (currentDownloads >= MAX_CONCURRENT_DOWNLOADS)
                    return;

                currentDownloads++;
                Debugger.V("Downloading map tile texture " + ImageFilename + "...");
                downloadClient = new WebClient();
                downloadClient.DownloadFileCompleted += DownloadFileCompleted;
                downloadClient.DownloadProgressChanged += DownloadProgressChanged;
                downloadClient.DownloadFileAsync(new Uri(ImageDownloadURL), ImagePath, null);
                materialsMap.DefaultMaterial = DownloadingMaterial;
            }
            else
            {
                if (!Owner.IsVisible || currentLoads >= MAX_CONCURRENT_LOADS)
                    return;

                currentLoads++;
                materialsMap.DefaultMaterial = LoadingMaterial;
                loadThread = new Thread(new ThreadStart(LoadThread));
                loadThread.Start();
            }
        }

        protected override void Update(TimeSpan gameTime)
        {
            CheckTextureState();
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                if (e.Error != null)
                {
                    Debugger.E("Error downloading map tile texture " + ImageFilename + ".");
                    materialsMap.DefaultMaterial = ErrorMaterial;
                    mapTextured = true;
                }

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
            downloadClient = null;
            currentDownloads--;
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if ((lastDownloadPercentage < 25 && e.ProgressPercentage >= 25)
                || (lastDownloadPercentage < 50 && e.ProgressPercentage >= 50)
                || (lastDownloadPercentage < 75 && e.ProgressPercentage >= 75))
                Debugger.V("Downloaded " + e.ProgressPercentage.ToString() + "%");
            lastDownloadPercentage = e.ProgressPercentage;
        }
        
        private void LoadThread()
        {
            try
            {
                //load image file
                Texture2D tex2D = null;
                using (FileStream file = new FileStream(ImagePath, FileMode.Open))
                    tex2D = Texture2D.FromFile(RenderManager.GraphicsDevice, file);

                //swap out texture
                materialsMap.DefaultMaterial = new BasicMaterial(tex2D);
            }
            catch (Exception e)
            {
                Debugger.E("Error loading map tile texture " + ImageFilename + ".");
                materialsMap.DefaultMaterial = ErrorMaterial;
            }
            mapTextured = true;
            loadThread = null;
            currentLoads--;
        }
    }
}
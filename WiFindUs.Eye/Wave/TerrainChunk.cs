using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Materials;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class TerrainChunk : Behavior
    {
        private static readonly int CHUNK_IMAGE_SIZE = 640;
        private static readonly string IMAGE_FORMAT = "png";
        private static readonly string MAPS_URL_BASE = "https://maps.googleapis.com/maps/api/staticmap?";
        private static readonly string MAPS_DIR = "maps/";
        private static int currentDownloads = 0;
        private static int currentLoads = 0;

        [RequiredComponent]
        private Transform3D transform3D;

        private Region region;
        private uint googleMapsZoomLevel;
        private uint row, column;
        private TerrainChunk baseChunk;
        private float size;
        private Vector3 topLeft, bottomRight;
        private Thread loadThread, downloadThread;
        private bool badDownload = false;
        private bool badLoad = false;
        private bool mapTextured = false;

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
                    2, CHUNK_IMAGE_SIZE, CHUNK_IMAGE_SIZE,
                    WFUApplication.GoogleAPIKey, "satellite", IMAGE_FORMAT);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public TerrainChunk(TerrainChunk baseChunk, uint googleMapsZoomLevel, uint row, uint column, float size)
        {
            if (googleMapsZoomLevel < WiFindUs.Eye.Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM
                || googleMapsZoomLevel > WiFindUs.Eye.Region.GOOGLE_MAPS_CHUNK_MAX_ZOOM)
                throw new ArgumentOutOfRangeException("googleMapsZoomLevel", "Zoom level must be between "
                    + MapScene.MIN_LEVEL + " and " + WiFindUs.Eye.Region.GOOGLE_MAPS_CHUNK_MAX_ZOOM + " (inclusive).");

            this.googleMapsZoomLevel = googleMapsZoomLevel;
            this.row = row;
            this.column = column;
            this.baseChunk = baseChunk;
            this.size = size;
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public void CalculatePosition()
        {
            if (baseChunk == null)
                transform3D.Position = new Vector3(0f, 0f, 0f);
            else
            {
                float start = (baseChunk.Size / -2.0f) + (size / 2.0f);
                transform3D.Position = new Vector3(
                    start + (column * size),
                    0.0f,
                    start + (row * size));
            }

            topLeft = new Vector3(transform3D.Position.X - size/2.0f, 0.0f, transform3D.Position.Z - size/2.0f);
            bottomRight = new Vector3(transform3D.Position.X + size / 2.0f, 0.0f, transform3D.Position.Z + size / 2.0f);
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void Update(TimeSpan gameTime)
        {
            if (this.Owner.IsVisible && !mapTextured && loadThread == null && downloadThread == null)
            {
                if (!badLoad && System.IO.File.Exists(ImagePath))
                {
                    if (currentLoads < 10)
                    {
                        loadThread = new Thread(new ThreadStart(LoadThread));
                        loadThread.Start();
                    }
                }
                else if (!badDownload)
                {
                    if (currentDownloads < 3)
                    {
                        downloadThread = new Thread(new ThreadStart(DownloadThread));
                        downloadThread.Start();
                    }
                }


            }
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void LoadThread()
        {
            //load image file
            currentLoads++;
            System.Drawing.Image image = null;
            try
            {
                image = System.Drawing.Image.FromFile(ImagePath);
            }
            catch
            {
                Debugger.E("Error loading '" + ImagePath + "'.");
                badLoad = true;
                loadThread = null;
                currentLoads--;
                return;
            }

            //check pixel formats to eliminate non-32bit ones
            switch (image.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Alpha:
                case System.Drawing.Imaging.PixelFormat.DontCare:
                case System.Drawing.Imaging.PixelFormat.PAlpha:
                case System.Drawing.Imaging.PixelFormat.Max:
                case System.Drawing.Imaging.PixelFormat.Gdi:
                case System.Drawing.Imaging.PixelFormat.Extended:
                    Debugger.E("Invalid pixel format in '" + ImagePath + "' ("+image.PixelFormat+").");
                    badLoad = true;
                    loadThread = null;
                    currentLoads--;
                    return;
            }

            //generate texture
            Texture2D tex2D = TxdFromBitmap(image);
            image.Dispose();

            //swap out texture
            try
            {
                Owner.FindComponent<MaterialsMap>().DefaultMaterial = new BasicMaterial(tex2D);
            }
            catch (Exception e)
            {
                Debugger.Ex(e);
            }

            mapTextured = true;
            loadThread = null;
            currentLoads--;
        }

        private void DownloadThread()
        {
            currentDownloads++;

            if (!Directory.Exists(MAPS_DIR))
            {
                try
                {
                    Directory.CreateDirectory(MAPS_DIR);
                }
                catch
                {
                    Debugger.E("Error creating maps directory!");
                }
            }

            using (WebClient Client = new WebClient())
            {
                try
                {
                    Debugger.V("Downloading map chunk texture " + ImageFilename + "...");
                    Client.DownloadFile(ImageDownloadURL, ImagePath);
                }
                catch
                {
                    badDownload = true;
                    Debugger.E("Error downloading map chunk texture " + ImageFilename + ".");
                }
            }

            downloadThread = null;
            currentDownloads--;
        }

        private static byte[] Array1DFromBitmap(System.Drawing.Bitmap bmp)
        {
            // get total locked pixels count
            int PixelCount = bmp.Width * bmp.Height;

            // Create rectangle to lock
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

            // get source bitmap pixel format size
            int depth = System.Drawing.Bitmap.GetPixelFormatSize(bmp.PixelFormat);

            // Check if bpp (Bits Per Pixel) is 8, 24, or 32
            if (depth != 8 && depth != 24 && depth != 32)
            {
                throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
            }

            // Lock bitmap and return bitmap data
            System.Drawing.Imaging.BitmapData bitmapData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
            bmp.PixelFormat);

            //declare an array to hold the bytes of the bitmap
            int numBytes = bitmapData.Stride * bmp.Height;
            byte[] pixels = new byte[numBytes];

            //copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixels, 0, numBytes);
            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte bb = pixels[i];
                byte br = pixels[i + 1];
                byte bg = pixels[i + 2];
                byte ba = pixels[i + 3];

                pixels[i] = bg;
                pixels[i + 1] = br;
                pixels[i + 2] = bb;
                pixels[i + 3] = ba;
            }

            return pixels;
        }

        private Texture2D TxdFromBitmap(System.Drawing.Image b)
        {
            Texture2D tex2D = new Texture2D()
            {
                Format = WaveEngine.Common.Graphics.PixelFormat.R8G8B8A8,
                Height = b.Height,
                Width = b.Width,
                Levels = 1
            };

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(b);
            tex2D.Data = new byte[1][][];
            tex2D.Data[0] = new byte[1][];
            tex2D.Data[0][0] = Array1DFromBitmap(bmp);
            RenderManager.GraphicsDevice.Textures.UploadTexture(tex2D);
            bmp.Dispose();

            return tex2D;
        }

    }
}
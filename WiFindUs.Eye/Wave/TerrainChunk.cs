using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class TerrainChunk : Behavior
    {
        private static readonly int CHUNK_IMAGE_SIZE = 640;
        private static readonly string IMAGE_FORMAT = "png";
        private static readonly string MAPS_URL_BASE = "https://maps.googleapis.com/maps/api/staticmap?";
        private static readonly string MAPS_DIR = "maps/";

        [RequiredComponent]
        private Transform3D transform3D;

        private Region region;
        private uint googleMapsZoomLevel;
        private uint row, column;
        private TerrainChunk baseChunk;
        private float size;

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
                if (value == null || value.Equals(region.Center))
                    return;
                region = new Region(value, googleMapsZoomLevel);
            }
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
            get { return googleMapsZoomLevel - Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM; }
        }

        public float Size
        {
            get { return size; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public TerrainChunk(TerrainChunk baseChunk, uint googleMapsZoomLevel, uint row, uint column, float size)
        {
            if (googleMapsZoomLevel < Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM || googleMapsZoomLevel > Region.GOOGLE_MAPS_CHUNK_MAX_ZOOM)
                throw new ArgumentOutOfRangeException("googleMapsZoomLevel", "Zoom level must be between "
                    + Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM + " and " + Region.GOOGLE_MAPS_CHUNK_MAX_ZOOM + " (inclusive).");

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
        }

        public void CalculateLocation()
        {
            if (baseChunk == null)
                return;


        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void Update(TimeSpan gameTime)
        {
            
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

    }
}

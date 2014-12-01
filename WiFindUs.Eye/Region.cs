using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    /// <summary>
    /// An immutable packet of data describing a rectangular area bound by GPS coordinates, with the primary purpose of mapping those coordinates to a screen space.
    /// </summary>
    public class Region : ILocation, IEquatable<IRegion>, IRegion
    {
        public static readonly uint GOOGLE_MAPS_CHUNK_MIN_ZOOM = 15;
        public static readonly uint GOOGLE_MAPS_CHUNK_MAX_ZOOM = 21;

        private static readonly double GOOGLE_MAPS_CHUNK_RADIUS = 0.01126;
        private static readonly double GOOGLE_MAPS_CHUNK_LONG_SCALE = 1.22;
        private ILocation northWest, northEast, southWest, southEast, center;
        private double latSpan, longSpan, width, height;
        private uint googleMapsZoomLevel = 0;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// The north-west corner of this region.
        /// </summary>
        public ILocation NorthWest
        {
            get { return northWest;  }
        }

        /// <summary>
        /// The north-east corner of this region.
        /// </summary>
        public ILocation NorthEast
        {
            get { return northEast; }
        }

        /// <summary>
        /// The south-east corner of this region.
        /// </summary>
        public ILocation SouthEast
        {
            get { return southEast; }
        }

        /// <summary>
        /// The south-west corner of this region.
        /// </summary>
        public ILocation SouthWest
        {
            get { return southWest; }
        }

        /// <summary>
        /// The latitude of the region's exact center.
        /// </summary>
        public double Latitude
        {
            get { return center.Latitude; }
        }

        /// <summary>
        /// The longitude of the region's exact center.
        /// </summary>
        public double Longitude
        {
            get { return center.Longitude; }
        }

        /// <summary>
        /// The mean Accuracy of the region's bounding points, if the data is present.
        /// </summary>
        public double? Accuracy
        {
            get
            {
                return (!northWest.Accuracy.HasValue && !southEast.Accuracy.HasValue ? null :
                    new double?(((northWest.Accuracy.HasValue ? northWest.Accuracy.Value : 0.0)
                        + (southEast.Accuracy.HasValue ? southEast.Accuracy.Value : 0.0)) / 2.0)
                    );
            }
        }

        /// <summary>
        /// The mean Altitude of the region's bounding points, if the data is present.
        /// </summary>
        public double? Altitude
        {
            get
            {
                return (!northWest.Altitude.HasValue && !southEast.Altitude.HasValue ? null :
                    new double?(((northWest.Altitude.HasValue ? northWest.Altitude.Value : 0.0)
                        + (southEast.Altitude.HasValue ? southEast.Altitude.Value : 0.0)) / 2.0)
                    );
            }
        }

        /// <summary>
        /// The center coordinates of this region.
        /// </summary>
        public ILocation Center
        {
            get { return center; }
        }

        /// <summary>
        /// The longitude range spanned by this region (from west-to-east / left-to-right).
        /// </summary>
        public double LongitudinalSpan
        {
            get { return longSpan; }
        }

        /// <summary>
        /// The latitude range spanned by this region (from north-to-south / top-to-bottom).
        /// </summary>
        public double LatitudinalSpan
        {
            get { return latSpan; }
        }

        /// <summary>
        /// The 'horizontal' distance covered by this region, in meters (as per the haversine formula). 
        /// </summary>
        public double Width
        {
            get { return width; }
        }

        /// <summary>
        /// The 'vertical' distance covered by this region, in meters (as per the haversine formula). 
        /// </summary>
        public double Height
        {
            get { return height; }
        }

        /// <summary>
        /// The google maps zoom level represented by this region, if it was constructed as one. Meaningless otherwise.
        /// </summary>
        public uint GoogleMapsZoomLevel
        {
            get { return googleMapsZoomLevel; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public Region(ILocation northWest, ILocation southEast)
        {
            if (northWest == null)
                throw new ArgumentNullException("northWest");
            if (southEast == null)
                throw new ArgumentNullException("southEast");
            if (southEast.Longitude < northWest.Longitude)
                throw new ArgumentException("SE longitude must be greater than NW longitude (left-to-right).");
            if (northWest.Latitude < southEast.Latitude)
                throw new ArgumentException("SE latitude must be lower than NW latitude (top-to-bottom).");

            this.northWest = northWest;
            this.southEast = southEast;
            northEast = new Location(northWest.Latitude, southEast.Longitude);
            southWest = new Location(southEast.Latitude, northWest.Longitude);
            latSpan = northWest.Latitude - southEast.Latitude;
            longSpan = southEast.Longitude - northWest.Longitude;
            width = northWest.DistanceTo(northEast);
            height = northWest.DistanceTo(southWest);
            center = new Location(northWest.Latitude - latSpan / 2.0, northWest.Longitude + longSpan / 2.0);
        }

        public Region(ILocation center, double latSpan, double longSpan)
        {
            if (center == null)
                throw new ArgumentNullException("center");
            if (latSpan < 0.0)
                throw new ArgumentOutOfRangeException("latSpan", "latSpan cannot be negative");
            if (longSpan < 0.0)
                throw new ArgumentOutOfRangeException("longSpan", "longSpan cannot be negative");

            this.center = center;
            this.latSpan = latSpan;
            this.longSpan = longSpan;
            northWest = new Location(center.Latitude + latSpan / 2.0, center.Longitude - longSpan / 2.0);
            southEast = new Location(center.Latitude - latSpan / 2.0, center.Longitude + longSpan / 2.0);
            northEast = new Location(northWest.Latitude, southEast.Longitude);
            southWest = new Location(southEast.Latitude, northWest.Longitude);
            width = northWest.DistanceTo(northEast);
            height = northWest.DistanceTo(southWest);
        }

        public Region(ILocation center, uint googleMapsZoomLevel)
        {
            if (center == null)
                throw new ArgumentNullException("center");
            if (googleMapsZoomLevel < GOOGLE_MAPS_CHUNK_MIN_ZOOM || googleMapsZoomLevel > GOOGLE_MAPS_CHUNK_MAX_ZOOM)
                throw new ArgumentOutOfRangeException("googleMapsZoomLevel", "Zoom level must be between "
                    + GOOGLE_MAPS_CHUNK_MIN_ZOOM + " and " + GOOGLE_MAPS_CHUNK_MAX_ZOOM + " (inclusive).");

            this.googleMapsZoomLevel = googleMapsZoomLevel;
            double scaledRadius = GOOGLE_MAPS_CHUNK_RADIUS / Math.Pow(2.0, (googleMapsZoomLevel - GOOGLE_MAPS_CHUNK_MIN_ZOOM));
            this.center = center;
            northWest = new Location(center.Latitude + scaledRadius, center.Longitude - (scaledRadius * GOOGLE_MAPS_CHUNK_LONG_SCALE));
            southEast = new Location(center.Latitude - scaledRadius,  center.Longitude + (scaledRadius * GOOGLE_MAPS_CHUNK_LONG_SCALE));
            northEast = new Location(northWest.Latitude, southEast.Longitude);
            southWest = new Location(southEast.Latitude, northWest.Longitude);
            latSpan = northWest.Latitude - southEast.Latitude;
            longSpan = southEast.Longitude - northWest.Longitude;
            width = northWest.DistanceTo(northEast);
            height = northWest.DistanceTo(southWest);
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public bool Equals(IRegion other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return northEast.Equals(other.NorthEast) && southWest.Equals(other.SouthWest);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IRegion))
                return false;
            return Equals((IRegion)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                    .Hash(northEast)
                    .Hash(southWest);
        }

        public bool Contains(double latitude, double longitude)
        {
            return latitude <= northWest.Latitude
                && latitude >= southEast.Latitude
                && longitude >= northWest.Longitude
                && longitude <= southEast.Longitude;
        }

        public bool Contains(ILocation location)
        {
            if (location == null)
                return false;
            return Contains(location.Latitude, location.Longitude);
        }

        public Point LocationToScreen(Rectangle screenBounds, double latitude, double longitude)
        {
            return new Point(
                    screenBounds.X + (int)(((longitude - northWest.Longitude) / longSpan) * (double)screenBounds.Width),
                    screenBounds.Y + (int)(((northWest.Latitude - latitude) / latSpan) * (double)screenBounds.Height)
                );
        }

        public Point LocationToScreen(Rectangle screenBounds, ILocation location)
        {
            return LocationToScreen(screenBounds, location.Latitude, location.Longitude);
        }

        public double DistanceTo(ILocation other)
        {
            return Location.Distance(this, other);
        }
    }
}

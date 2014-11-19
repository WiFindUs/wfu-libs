using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace WiFindUs
{
    /// <summary>
    /// An immutable packet of data describing a rectangular area bound by GPS coordinates, with the primary purpose of mapping those coordinates to a screen space.
    /// </summary>
    public class Region : IEquatable<Region>
    {
        private Location northWest, southWest, northEast, southEast, center;
        private double latSpan, longSpan, width, height;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////
        public Location NorthWest
        {
            get { return northWest;  }
        }
        public Location NorthEast
        {
            get { return northEast; }
        }
        public Location SouthWest
        {
            get { return southWest; }
        }
        public Location SouthEast
        {
            get { return southEast; }
        }
        public Location Center
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

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public Region(Location northWest, Location southEast)
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
            center = new Location(northWest.Latitude - latSpan / 2.0, northWest.Longitude + longSpan/2.0);
        }

        public Region(Location center, double latSpan, double longSpan)
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

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public bool Equals(Region other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return northEast.Equals(other.northEast) && southWest.Equals(other.southWest);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Region))
                return false;
            return Equals((Region)obj);
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

        public bool Contains(Location location)
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

        public Point LocationToScreen(Rectangle screenBounds, Location location)
        {
            return LocationToScreen(screenBounds, location.Latitude, location.Longitude);
        }
    }
}

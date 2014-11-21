using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    /// <summary>
    /// An immutable packet of data describing an object's location.
    /// </summary>
    public class Location : ILocation, IEquatable<ILocation>
    {
        private const double EARTH_RADIUS_MEAN = 6378.1370;
        private const double EPSILON_HORIZONTAL = 0.000001;
        private const double EPSILON_ACCURACY = 0.5;
        private const double EPSILON_ALTITUDE = 1.0;
        private double latitude;
        private double longitude;
        private double? altitude = null;
        private double? accuracy = null;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public double Latitude
        {
            get { return latitude; }
        }
        public double Longitude
        {
            get { return longitude; }
        }
        public double? Accuracy
        {
            get { return accuracy; }
        }
        public double? Altitude
        {
            get { return altitude; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new Location object.
        /// </summary>
        /// <param name="latitude">The latitude of the coordinate.</param>
        /// <param name="longitude">The longitude of the coordinate.</param>
        /// <param name="accuracy">The horizontal radius of 68% confidence as reported by the device. Use null for 'no data'.</param>
        /// <param name="altitude">The altitude of of the coordinate, as meters above sea level. Use null for 'no data'.</param>
        public Location(double latitude, double longitude, double? accuracy = null, double? altitude = null)
        {
            if (latitude > 90.0 || latitude < -90.0)
                throw new ArgumentOutOfRangeException("latitude", "Latitude must be null or between -90.0 and 90.0 (inclusive).");
            if (longitude > 180.0 || longitude < -180.0)
                throw new ArgumentOutOfRangeException("longitude", "Longitude must be null or between -180.0 and 180.0 (inclusive).");
            if (accuracy != null && accuracy <= 0.0)
                throw new ArgumentOutOfRangeException("Accuracy must be null or greater than 0m.");

            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
            this.accuracy = accuracy;
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public bool Equals(ILocation other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (!Latitude.Tolerance(other.Latitude, EPSILON_HORIZONTAL)
                || !Longitude.Tolerance(other.Longitude, EPSILON_HORIZONTAL)
                || !Accuracy.Tolerance(other.Accuracy, EPSILON_ACCURACY)
                || !Altitude.Tolerance(other.Altitude, EPSILON_ALTITUDE))
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ILocation))
                return false;
            return Equals((ILocation)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                    .Hash(Latitude)
                    .Hash(Longitude)
                    .Hash(Accuracy)
                    .Hash(Altitude);
        }

        /// <summary>
        /// Returns the horizontal (spherical) distance between two GPS coordinates in meters, according to the haversine formula.
        /// </summary>
        /// <returns>Distance 'as the crow flies' between the two points, in meters.</returns>
        public static double Distance(double latA, double longA, double latB, double longB)
        {
            double latitudeDistance = (latA - latB).ToRadians();
            double longitudeDistance = (longA - longB).ToRadians();
            double d = Math.Sin(latitudeDistance / 2.0) * Math.Sin(latitudeDistance / 2.0) +
                       Math.Cos(latB.ToRadians()) * Math.Cos(latA.ToRadians()) *
                       Math.Sin(longitudeDistance / 2.0) * Math.Sin(longitudeDistance / 2.0);
            return (2.0 * Math.Atan2(Math.Sqrt(d), Math.Sqrt(1.0 - d))) * EARTH_RADIUS_MEAN * 1000.0;
        }

        /// <summary>
        /// Returns the horizontal (spherical) distance between two GPS coordinates in meters, according to the haversine formula.
        /// </summary>
        /// <returns>Distance 'as the crow flies' between the two points, in meters.</returns>
        public static double Distance(ILocation A, ILocation B)
        {
            if (A == null)
                throw new ArgumentNullException("A");
            if (B == null)
                throw new ArgumentNullException("B");
            return Distance(A.Latitude, A.Longitude, B.Latitude, B.Longitude);
        }

        /// <summary>
        /// Returns the horizontal (spherical) distance between two GPS coordinates in meters, according to the haversine formula.
        /// </summary>
        /// <param name="other">The point to measure to, starting from the current one.</param>
        /// <returns>Distance 'as the crow flies' between the two points, in meters.</returns>
        public double DistanceTo(ILocation other)
        {
            return Distance(this, other);
        }
    }
}

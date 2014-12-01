using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    /// <summary>
    /// An immutable packet of data describing an object's location.
    /// </summary>
    public class Location : ILocation, IEquatable<ILocation>
    {
        private static readonly double EARTH_RADIUS_MEAN = 6378.1370;
        private static readonly double EPSILON_HORIZONTAL = 0.000001;
        private static readonly double EPSILON_ACCURACY = 0.5;
        private static readonly double EPSILON_ALTITUDE = 1.0;
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
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
            this.accuracy = accuracy;

            CheckAssignedValues();
        }

        /// <summary>
        /// Creates a new Location object.
        /// </summary>
        /// <param name="values">A double[] parameter list in the following order: Latitude, Longitude, Accuracy, Altutide. Must have at least two elements.</param>
        public Location(params double[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length < 2)
                throw new ArgumentOutOfRangeException("values", "Must provide at least to parameters (lat and long).");

            this.latitude = values[0];
            this.longitude = values[1];
            if (values.Length > 2)
                this.altitude = values[2];
            if (values.Length > 3)
                this.accuracy = values[3];

            CheckAssignedValues();
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public static bool Equals(ILocation A, ILocation B)
        {
            if (A == null || B == null)
                return false;
            if (ReferenceEquals(A, B))
                return true;

            if (!A.Latitude.Tolerance(B.Latitude, EPSILON_HORIZONTAL)
                || !A.Longitude.Tolerance(B.Longitude, EPSILON_HORIZONTAL)
                || !A.Accuracy.Tolerance(B.Accuracy, EPSILON_ACCURACY)
                || !A.Altitude.Tolerance(B.Altitude, EPSILON_ALTITUDE))
                return false;

            return true;
        }

        public bool Equals(ILocation other)
        {
            return Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ILocation);
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

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void CheckAssignedValues()
        {
            if (latitude > 90.0 || latitude < -90.0)
                throw new ArgumentOutOfRangeException("latitude", "Latitude must be null or between -90.0 and 90.0 (inclusive).");
            if (longitude > 180.0 || longitude < -180.0)
                throw new ArgumentOutOfRangeException("longitude", "Longitude must be null or between -180.0 and 180.0 (inclusive).");
            if (accuracy.HasValue && accuracy.Value <= 0.0)
                throw new ArgumentOutOfRangeException("Accuracy must be null or greater than 0m.");
        }
    }
}

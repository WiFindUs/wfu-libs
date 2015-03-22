using System;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    /// <summary>
    /// An immutable packet of data describing an object's location.
    /// </summary>
    public class Location : ILocation, IEquatable<ILocation>
    {
        public static readonly Location EMPTY = new Location();
        private static readonly Location GPS_MARKS_HOUSE = new Location(-35.025435, 138.561954);
        private static readonly Location GPS_PARKSIDE = new Location(-34.951551, 138.623063);
        private static readonly Location GPS_BONYTHON_PARK = new Location(-34.9165, 138.581479);
        private static readonly Location GPS_MORPHETTVILLE = new Location(-34.977575, 138.54267);
        private static readonly Location GPS_WAYVILLE = new Location(-34.945508, 138.5866207);
        
        private static readonly double EARTH_RADIUS_MEAN = 6378.1370;
        private static readonly double EPSILON_HORIZONTAL = 0.00000001;
        private static readonly double EPSILON_ACCURACY = 0.1;
        private static readonly double EPSILON_ALTITUDE = 0.5;
        private double? latitude = null;
        private double? longitude = null;
        private double? altitude = null;
        private double? accuracy = null;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public double? Latitude
        {
            get { return latitude; }
        }
        public double? Longitude
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

        public bool HasLatLong
        {
            get
            {
                return latitude.HasValue
                    && longitude.HasValue;
            }
        }

        public bool EmptyLocation
        {
            get
            {
                return !latitude.HasValue
                    && !longitude.HasValue
                    && !accuracy.HasValue
                    && !altitude.HasValue;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new Location object.
        /// </summary>
        /// <param name="latitude">The latitude of the coordinate. Use null for 'no data'.</param>
        /// <param name="longitude">The longitude of the coordinate. Use null for 'no data'.</param>
        /// <param name="accuracy">The horizontal radius of 68% confidence as reported by the device. Use null for 'no data'.</param>
        /// <param name="altitude">The altitude of of the coordinate, as meters above sea level. Use null for 'no data'.</param>
        public Location(double? latitude = null, double? longitude = null, double? accuracy = null, double? altitude = null)
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
                throw new ArgumentOutOfRangeException("values", "Must provide at least two parameters (lat and long).");

            this.latitude = values[0];
            this.longitude = values[1];
            if (values.Length > 2)
                this.altitude = values[2];
            if (values.Length > 3)
                this.accuracy = values[3];

            CheckAssignedValues();
        }

        private Location()
            : this (null,null,null,null)
        {

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
            if (A.EmptyLocation != B.EmptyLocation || A.HasLatLong != B.HasLatLong)
                return false;

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
            return Distance(A.Latitude.GetValueOrDefault(), A.Longitude.GetValueOrDefault(),
                B.Latitude.GetValueOrDefault(), B.Longitude.GetValueOrDefault());
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

        public static ILocation FromName(string locationName)
        {
            if (locationName == null)
                throw new ArgumentNullException("locationName");
            if ((locationName = locationName.Trim().ToLower()).Length == 0)
                throw new ArgumentOutOfRangeException("locationName", "Location name was blank.");

            switch (locationName)
            {
                case "mark":
                    return GPS_MARKS_HOUSE;
                case "morphettville":
                    return GPS_MORPHETTVILLE;
                case "wayville":
                    return GPS_WAYVILLE;
                case "parkside":
                    return GPS_PARKSIDE;
                case "bonython":
                    return GPS_BONYTHON_PARK;
            }

            return null;
        }

        public static string ToString(ILocation location)
        {
            return "{" + location.Latitude.GetValueOrDefault()
                + ", " + location.Longitude.GetValueOrDefault() + "}";
        }

        public override string ToString()
        {
            return ToString(this);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void CheckAssignedValues()
        {
            if (latitude.HasValue && (latitude > 90.0 || latitude < -90.0))
                throw new ArgumentOutOfRangeException("latitude", "Latitude must be null or between -90.0 and 90.0 (inclusive).");
            if (longitude.HasValue && (longitude > 180.0 || longitude < -180.0))
                throw new ArgumentOutOfRangeException("longitude", "Longitude must be null or between -180.0 and 180.0 (inclusive).");
            if (accuracy.HasValue && accuracy.Value <= 0.0)
                throw new ArgumentOutOfRangeException("Accuracy must be null or greater than 0m.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs
{
    /// <summary>
    /// An immutable packet of data describing an object's location.
    /// </summary>
    public abstract class Location : ILocation, IEquatable<ILocation>
    {
        private const double EARTH_RADIUS_MEAN = 6378.1370;
        private const double EPSILON_HORIZONTAL = 0.000001;
        private const double EPSILON_ACCURACY = 0.5;
        private const double EPSILON_ALTITUDE = 1.0;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public abstract double Latitude { get; }
        public abstract double Longitude { get; }
        public abstract double? Accuracy { get; }
        public abstract double? Altitude { get; }

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
        public virtual double DistanceTo(ILocation other)
        {
            return Distance(this, other);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs
{
    public class StaticLocation : Location
    {
        private double latitude;
        private double longitude;
        private double? altitude = null;
        private double? accuracy = null;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public override double Latitude
        {
            get { return latitude; }
        }
        public override double Longitude
        {
            get { return longitude; }
        }
        public override double? Accuracy
        {
            get { return accuracy; }
        }
        public override double? Altitude
        {
            get { return altitude; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new StaticLocation object.
        /// </summary>
        /// <param name="latitude">The latitude of the coordinate.</param>
        /// <param name="longitude">The longitude of the coordinate.</param>
        /// <param name="accuracy">The horizontal radius of 68% confidence as reported by the device. Use null for 'no data'.</param>
        /// <param name="altitude">The altitude of of the coordinate, as meters above sea level. Use null for 'no data'.</param>
        public StaticLocation(double latitude, double longitude, double? accuracy = null, double? altitude = null)
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
    }
}

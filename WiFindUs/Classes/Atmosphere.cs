using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs
{
    /// <summary>
    /// An immutable packet of data describing the atmospheric condition at an object's location.
    /// </summary>
    public class Atmosphere : IEquatable<Atmosphere>
    {
        private const double EPSILON_HUMIDITY = 0.01;
        private const double EPSILON_AIR_PRESSURE = 0.01;
        private const double EPSILON_TEMPERATURE = 0.5;
        private const double EPSILON_LIGHT_LEVEL = 1.0;
        private double? humidity = null;
        private double? airPressure = null;
        private double? temperature = null;
        private double? lightLevel = null;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public double? Humidity
        {
            get { return humidity; }
        }
        public double? AirPressure
        {
            get { return airPressure; }
        }
        public double? Temperature
        {
            get { return temperature; }
        }
        public double? LightLevel
        {
            get { return lightLevel; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new Atmosphere object.
        /// </summary>
        /// <param name="humidity">The relative humidity (as a percentage) recorded by the device's hygrometer (if present). Use null for 'no data'.</param>
        /// <param name="airPressure">Barometric pressure (in mbar) recorded by the device's pressure sensor (if present). Use null for 'no data'.</param>
        /// <param name="temperature">Temperature (in degrees celcius) recorded by the device's temperature sensor (if present). Use null for 'no data'.</param>
        /// <param name="lightLevel">Ambient light level (in lux) recorded by the device's light sensor (if present). Use null for 'no data'.</param>
        public Atmosphere(double? humidity = null, double? airPressure = null, double? temperature = null, double? lightLevel = null)
        {
            this.humidity = humidity;
            this.airPressure = airPressure;
            this.temperature = temperature;
            this.lightLevel = lightLevel;
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public bool Equals(Atmosphere other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (!humidity.Tolerance(other.humidity, EPSILON_HUMIDITY)
                || !airPressure.Tolerance(other.airPressure, EPSILON_AIR_PRESSURE)
                || !temperature.Tolerance(other.temperature, EPSILON_TEMPERATURE)
                || !lightLevel.Tolerance(other.lightLevel, EPSILON_LIGHT_LEVEL))
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Atmosphere))
                return false;
            return Equals((Atmosphere)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                    .Hash(humidity)
                    .Hash(airPressure)
                    .Hash(temperature)
                    .Hash(lightLevel);
        }
    }
}

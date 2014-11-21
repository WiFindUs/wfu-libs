using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    /// <summary>
    /// An immutable packet of data describing the atmospheric condition at an object's location.
    /// </summary>
    public class Atmosphere : IAtmosphere, IEquatable<IAtmosphere>
    {
        private const double EPSILON_HUMIDITY = 0.01;
        private const double EPSILON_AIR_PRESSURE = 0.01;
        private const double EPSILON_TEMPERATURE = 0.5;
        private const double EPSILON_LIGHT_LEVEL = 1.0;
        private const double ABSOLUTE_ZERO = -273.15;
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
        /// <param name="humidity">The relative humidity (as a percentage, 0.0-1.0) recorded by the device's hygrometer (if present). Use null for 'no data'.</param>
        /// <param name="airPressure">Barometric pressure (in mbar) recorded by the device's pressure sensor (if present). Use null for 'no data'.</param>
        /// <param name="temperature">Temperature (in degrees celcius) recorded by the device's temperature sensor (if present). Use null for 'no data'.</param>
        /// <param name="lightLevel">mbient light level (in lux) recorded by the device's light sensor (if present). Use null for 'no data'.</param>
        public Atmosphere(double? humidity = null, double? airPressure = null, double? temperature = null, double? lightLevel = null)
        {
            if (humidity.HasValue && (humidity.Value > 1.0 || humidity.Value < -0.0))
                throw new ArgumentOutOfRangeException("humidity", "Humidity must be null or between 0.0 and 1.0 (inclusive).");
            if (airPressure.HasValue && airPressure.Value <= 0.0)
                throw new ArgumentOutOfRangeException("airPressure", "Air Pressure must be null or greater than 0.0 mbar.");
            if (temperature.HasValue && temperature.Value <= ABSOLUTE_ZERO)
                throw new ArgumentOutOfRangeException("temperature", "Temperature must be null or greater than absolute zero (-273.15 degrees celcius).");
            if (lightLevel.HasValue && lightLevel.Value <= 0.0)
                throw new ArgumentOutOfRangeException("lightLevel", "Light Level must be null or greater than 0.0 lux.");

            this.humidity = humidity;
            this.airPressure = airPressure;
            this.temperature = temperature;
            this.lightLevel = lightLevel;
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public virtual bool Equals(IAtmosphere other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (!Humidity.Tolerance(other.Humidity, EPSILON_HUMIDITY)
                || !AirPressure.Tolerance(other.AirPressure, EPSILON_AIR_PRESSURE)
                || !Temperature.Tolerance(other.Temperature, EPSILON_TEMPERATURE)
                || !LightLevel.Tolerance(other.LightLevel, EPSILON_LIGHT_LEVEL))
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IAtmosphere))
                return false;
            return Equals((IAtmosphere)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                    .Hash(Humidity)
                    .Hash(AirPressure)
                    .Hash(Temperature)
                    .Hash(LightLevel);
        }
    }
}

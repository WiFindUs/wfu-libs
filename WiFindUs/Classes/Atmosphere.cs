using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs
{
    /// <summary>
    /// An immutable packet of data describing the atmospheric condition at an object's location.
    /// </summary>
    public abstract class Atmosphere : IAtmosphere, IEquatable<Atmosphere>
    {
        private const double EPSILON_HUMIDITY = 0.01;
        private const double EPSILON_AIR_PRESSURE = 0.01;
        private const double EPSILON_TEMPERATURE = 0.5;
        private const double EPSILON_LIGHT_LEVEL = 1.0;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public abstract double? Humidity { get; }
        public abstract double? AirPressure { get; }
        public abstract double? Temperature { get; }
        public abstract double? LightLevel { get; }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public virtual bool Equals(Atmosphere other)
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
            if (!(obj is Atmosphere))
                return false;
            return Equals((Atmosphere)obj);
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

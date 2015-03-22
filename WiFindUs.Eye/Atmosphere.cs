using System;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
	/// <summary>
	/// An immutable packet of data describing the atmospheric condition at an object's location.
	/// </summary>
	public class Atmosphere : IAtmosphere, IEquatable<IAtmosphere>
	{
		private static readonly double EPSILON_HUMIDITY = 0.01;
		private static readonly double EPSILON_AIR_PRESSURE = 0.01;
		private static readonly double EPSILON_TEMPERATURE = 0.5;
		private static readonly double EPSILON_LIGHT_LEVEL = 1.0;
		private static readonly double ABSOLUTE_ZERO = -273.15;
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
		public bool EmptyAtmosphere
		{
			get
			{
				return !humidity.HasValue
					&& !temperature.HasValue
					&& !airPressure.HasValue
					&& !lightLevel.HasValue;
			}
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

		public static bool Equals(IAtmosphere A, IAtmosphere B)
		{
			if (A == null || B == null)
				return false;
			if (ReferenceEquals(A, B))
				return true;

			if (!A.Humidity.Tolerance(B.Humidity, EPSILON_HUMIDITY)
				|| !A.AirPressure.Tolerance(B.AirPressure, EPSILON_AIR_PRESSURE)
				|| !A.Temperature.Tolerance(B.Temperature, EPSILON_TEMPERATURE)
				|| !A.LightLevel.Tolerance(B.LightLevel, EPSILON_LIGHT_LEVEL))
				return false;

			return true;
		}

		public bool Equals(IAtmosphere other)
		{
			return Equals(this, other);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as IAtmosphere);
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

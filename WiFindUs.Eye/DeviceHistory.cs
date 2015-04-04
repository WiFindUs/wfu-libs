using System;

namespace WiFindUs.Eye
{
	public partial class DeviceHistory : ILocation, IAtmosphere
	{
		public static event Action<DeviceHistory> OnDeviceHistoryLoaded;
		private bool loaded = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public bool EmptyAtmosphere
		{
			get
			{
				return !Humidity.HasValue
					&& !Temperature.HasValue
					&& !AirPressure.HasValue
					&& !LightLevel.HasValue;
			}
		}

		public bool HasLatLong
		{
			get
			{
				return Latitude.HasValue
					&& Longitude.HasValue;
			}
		}

		public bool EmptyLocation
		{
			get
			{
				return !Latitude.HasValue
					&& !Longitude.HasValue
					&& !Accuracy.HasValue
					&& !Altitude.HasValue;
			}
		}

		public double DistanceTo(ILocation other)
		{
			return WiFindUs.Eye.Location.Distance(this, other);
		}

		public bool Loaded
		{
			get { return loaded; }
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		partial void OnLoaded()
		{
			if (!loaded)
			{
				loaded = true;
				Debugger.V(this.ToString() + " loaded.");
				if (OnDeviceHistoryLoaded != null)
					OnDeviceHistoryLoaded(this);
			}
		}
	}
}

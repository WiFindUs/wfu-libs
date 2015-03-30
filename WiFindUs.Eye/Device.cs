using System;
using System.Drawing;
using System.Net;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
	public partial class Device
		: SelectableEntity, ILocatable, ILocation, IAtmospheric, IAtmosphere, IBatteryStats, IUpdateable, IActionSubscriber
	{
		public const ulong TIMEOUT = 60;
		public const long MIN_MESH_IP_ADDRESS = 2886729985L;
		public const long MAX_MESH_IP_ADDRESS = 2886795006L;
		public static event Action<Device> OnDeviceLoaded;
		public event Action<Device> OnDeviceTypeChanged;
		public event Action<Device> OnDeviceAtmosphereChanged;
		public event Action<Device> OnDeviceBatteryChanged;
		public event Action<Device> OnDeviceIPAddressChanged;
		public event Action<Device> OnDeviceUserChanged;
		public event Action<Device> OnDeviceAssignedWaypointChanged;
		public event Action<Device> OnDeviceNodeChanged;
		public event Action<IUpdateable> WhenUpdated;
		public event Action<IUpdateable> TimedOutChanged;
		public event Action<ILocatable> LocationChanged;
		public event Action<Device> OnGPSEnabledChanged;
		public event Action<Device> OnGPSFixedChanged;

		private bool timedOut = false, loaded = false;
		private bool? gpsEnabled = null, gpsHasFix = null;
		private StackedLock locationEventLock = new StackedLock();
		private bool fireLocationEvents = false;
		private IPAddress ipAddress = null;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public ILocation Location
		{
			get
			{
				return this;
			}
			set
			{
				locationEventLock.Lock();
				if (value == null)
				{
					Altitude = null;
					Accuracy = null;
					Longitude = null;
					Latitude = null;
				}
				else
				{
					Altitude = value.Altitude;
					Accuracy = value.Accuracy;
					Longitude = value.Longitude;
					Latitude = value.Latitude;
				}
				locationEventLock.Unlock();
			}
		}

		public IAtmosphere Atmosphere
		{
			get
			{
				return this;
			}
			set
			{
				if (value == null)
				{
					Temperature = null;
					Humidity = null;
					AirPressure = null;
					LightLevel = null;
				}
				else
				{
					Temperature = value.Temperature;
					Humidity = value.Humidity;
					AirPressure = value.AirPressure;
					LightLevel = value.LightLevel;
				}

				if (OnDeviceAtmosphereChanged != null)
					OnDeviceAtmosphereChanged(this);
			}
		}

		public bool? IsGPSEnabled
		{
			get { return gpsEnabled; }
			set
			{
				if (gpsEnabled != value)
				{
					gpsEnabled = value;
					if (OnGPSEnabledChanged != null)
						OnGPSEnabledChanged(this);

					if (!gpsEnabled.GetValueOrDefault())
						IsGPSFixed = false;
				}
			}
		}

		public bool? IsGPSFixed
		{
			get { return gpsHasFix; }
			set
			{
				if (gpsHasFix != value)
				{
					gpsHasFix = value;
					if (OnGPSFixedChanged != null)
						OnGPSFixedChanged(this);

					if (!gpsHasFix.GetValueOrDefault())
						Location = null;
				}
			}
		}

		public IBatteryStats BatteryStats
		{
			get
			{
				return this;
			}
			set
			{
				if (value == null)
				{
					Charging = null;
					BatteryLevel = null;
				}
				else
				{
					Charging = value.Charging;
					BatteryLevel = value.BatteryLevel;
				}
			}
		}

		public bool EmptyBatteryStats
		{
			get
			{
				return !Charging.HasValue && !BatteryLevel.HasValue;
			}
		}

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

		public IPAddress IPAddress
		{
			get
			{
				return ipAddress;
			}
			set
			{
#pragma warning disable 0618
				IPAddressRaw = value == null ? null : new Nullable<long>(value.Address);
#pragma warning restore 0618
			}
		}

		public ulong UpdateAge
		{
			get
			{
				return (DateTime.UtcNow.ToUnixTimestamp() - Updated);
			}
		}

		public bool TimedOut
		{
			get
			{
				return timedOut;
			}
			private set
			{
				if (value == timedOut)
					return;

				timedOut = value;
				if (timedOut)
				{
					BatteryStats = null;
					Atmosphere = null;
					Location = null;
					IPAddress = null;
					IsGPSEnabled = null;
					IsGPSFixed = null;
				}
				if (TimedOutChanged != null)
					TimedOutChanged(this);
			}
		}

		public ulong TimeoutLength
		{
			get { return TIMEOUT; }
		}

		public String ActionDescription
		{
			get { return this.ToString(); }
		}

		public bool Loaded
		{
			get { return loaded; }
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public static uint NodeNumberFromIPAddress(long ipRaw)
		{
			if (ipRaw == 0)
				return 0;
			long ipRawHost = (ipRaw & 0x000000FFU) << 24
				| (ipRaw & 0x0000FF00U) << 8
				| (ipRaw & 0x00FF0000U) >> 8
				| (ipRaw & 0xFF000000U) >> 24;
			if (ipRawHost < MIN_MESH_IP_ADDRESS
				|| ipRawHost > MAX_MESH_IP_ADDRESS)
				return 0;
			return ((uint)ipRawHost & 0xFF00) >> 8;
		}

		public static uint NodeNumberFromIPAddress(IPAddress ipv4)
		{
			if (ipv4 == null)
				return 0;
#pragma warning disable 0618
			return NodeNumberFromIPAddress(ipv4.Address);
#pragma warning restore 0618
		}

		public override string ToString()
		{
			return String.Format("Device[{0:X}]", ID);
		}

		public double DistanceTo(ILocation other)
		{
			return WiFindUs.Eye.Location.Distance(this, other);
		}

		public void CheckTimeout()
		{
			TimedOut = UpdateAge > TIMEOUT;
		}

		public bool ActionEnabled(uint index)
		{
			switch (index)
			{
				case 0: return true;
				case 1: return true;
				case 2: return true;
				case 9: return true;
			}
			return false;
		}

		public Image ActionImage(uint index)
		{
			return null;
		}

		public string ActionText(uint index)
		{
			switch (index)
			{
				case 0: return "Dispatch";
				case 1: return "Zoom To";
				case 2: return "Track";
				case 8: return "Cancel";
			}
			return "";
		}

		public void ActionTriggered(uint index)
		{

		}

		public void LockLocationEvents()
		{
			locationEventLock.Lock();
		}

		public void UnlockLocationEvents()
		{
			locationEventLock.Unlock();
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		partial void OnCreated()
		{
			locationEventLock.OnLocked += OnLocationLocked;
			locationEventLock.OnUnlocked += OnLocationUnlocked;
		}

		private void OnLocationLocked(StackedLock obj)
		{
			fireLocationEvents = false;
		}

		private void OnLocationUnlocked(StackedLock obj)
		{
			if (fireLocationEvents && LocationChanged != null)
				LocationChanged(this);
			fireLocationEvents = false;
		}

		private void FireLocationChangedEvent()
		{
			if (locationEventLock.IsLocked)
				fireLocationEvents = true;
			else if (LocationChanged != null)
				LocationChanged(this);
		}

		partial void OnLatitudeChanged()
		{
			FireLocationChangedEvent();
		}

		partial void OnLongitudeChanged()
		{
			FireLocationChangedEvent();
		}

		partial void OnAltitudeChanged()
		{
			FireLocationChangedEvent();
		}

		partial void OnAccuracyChanged()
		{
			FireLocationChangedEvent();
		}

		partial void OnLoaded()
		{
			ipAddress = IPAddressRaw.HasValue ? new IPAddress(IPAddressRaw.Value) : null;
			loaded = true;
			Debugger.V(this.ToString() + " loaded.");
			if (OnDeviceLoaded != null)
				OnDeviceLoaded(this);
		}

		partial void OnTypeChanged()
		{
			if (OnDeviceTypeChanged != null)
				OnDeviceTypeChanged(this);
		}

		partial void OnWaypointIDChanged()
		{
			if (OnDeviceAssignedWaypointChanged != null)
				OnDeviceAssignedWaypointChanged(this);
		}

		partial void OnIPAddressRawChanged()
		{
			ipAddress = IPAddressRaw.HasValue ? new IPAddress(IPAddressRaw.Value) : null;
			if (OnDeviceIPAddressChanged != null)
				OnDeviceIPAddressChanged(this);
		}

		partial void OnBatteryLevelChanged()
		{
			if (OnDeviceBatteryChanged != null)
				OnDeviceBatteryChanged(this);
		}

		partial void OnChargingChanged()
		{
			if (OnDeviceBatteryChanged != null)
				OnDeviceBatteryChanged(this);
		}

		partial void OnUserIDChanged()
		{
			if (OnDeviceUserChanged != null)
				OnDeviceUserChanged(this);
		}

		partial void OnUpdatedChanged()
		{
			if (WhenUpdated != null)
				WhenUpdated(this);
			CheckTimeout();
		}

		partial void OnNodeIDChanged()
		{
			if (OnDeviceNodeChanged != null)
				OnDeviceNodeChanged(this);
		}
	}
}

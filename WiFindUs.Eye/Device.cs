using System;
using System.Drawing;
using System.Net;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
	public partial class Device
		: SelectableEntity, ILocatable, ILocation, IAtmospheric, IAtmosphere, IBatteryStats, IUpdateable, IActionSubscriber
	{
		private const ulong TIMEOUT = 60;
		public static event Action<Device> OnDeviceLoaded;
		public event Action<Device> OnDeviceTypeChanged;
		public event Action<Device> OnDeviceAtmosphereChanged;
		public event Action<Device> OnDeviceBatteryChanged;
		public event Action<Device> OnDeviceUserChanged;
		public event Action<Device> OnDeviceAssignedWaypointChanged;
		public event Action<Device> OnDeviceNodeChanged;
		public event Action<IUpdateable> ActiveChanged;
		public event Action<IUpdateable> Updated;
		public event Action<ILocatable> LocationChanged;
		public event Action<Device> OnDeviceGPSEnabledChanged;
		public event Action<Device> OnDeviceGPSHasFixChanged;

		private bool loaded = false;
		private ReferenceCountedLock locationEventLock = new ReferenceCountedLock();
		private bool fireLocationEvents = false;

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

		public String ActionDescription
		{
			get { return this.ToString(); }
		}

		public bool Loaded
		{
			get { return loaded; }
		}

		public ulong LastUpdatedSecondsAgo
		{
			get { return DateTime.UtcNow.ToUnixTimestamp() - LastUpdated; }
		}

		public ulong TimeoutLength
		{
			get { return TIMEOUT; }
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return String.Format("Device[{0:X}]", ID);
		}

		public double DistanceTo(ILocation other)
		{
			return WiFindUs.Eye.Location.Distance(this, other);
		}

		public void CheckActive()
		{
			Active = LastUpdatedSecondsAgo <= TimeoutLength;
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

		internal void ProcessPacket(DevicePacket packet)
		{
			Active = true;
			LastUpdated = DateTime.UtcNow.ToUnixTimestamp();
			if (packet.DeviceType != null)
				Type = packet.DeviceType;
			if (packet.Charging.HasValue)
				Charging = packet.Charging;
			if (packet.BatteryLevel.HasValue)
				BatteryLevel = packet.BatteryLevel;

			if (packet.GPSEnabled.HasValue)
				GPSEnabled = packet.GPSEnabled;
			if (packet.GPSHasFix.HasValue)
				GPSHasFix = packet.GPSHasFix;

			locationEventLock.Lock();
			if (packet.Accuracy.HasValue)
				Accuracy = packet.Accuracy;
			if (packet.Latitude.HasValue)
				Latitude = packet.Latitude;
			if (packet.Longitude.HasValue)
				Longitude = packet.Longitude;
			if (packet.Altitude.HasValue)
				Altitude = packet.Altitude;
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

		private void OnLocationLocked(ReferenceCountedLock obj)
		{
			fireLocationEvents = false;
		}

		private void OnLocationUnlocked(ReferenceCountedLock obj)
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
			if (!loaded)
			{
				loaded = true;
				PropertyChanged += DevicePropertyChanged;
				Debugger.V(this.ToString() + " loaded.");
				if (OnDeviceLoaded != null)
					OnDeviceLoaded(this);
				CheckActive();
			}
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

		partial void OnLastUpdatedChanged()
		{
			if (Updated != null)
				Updated(this);
			CheckActive();
		}

		partial void OnActiveChanged()
		{
			if (ActiveChanged != null)
				ActiveChanged(this);
		}

		partial void OnNodeIDChanged()
		{
			if (OnDeviceNodeChanged != null)
				OnDeviceNodeChanged(this);
		}

		partial void OnGPSEnabledChanged()
		{
			if (OnDeviceGPSEnabledChanged != null)
				OnDeviceGPSEnabledChanged(this);
		}

		partial void OnGPSHasFixChanged()
		{
			if (OnDeviceGPSHasFixChanged != null)
				OnDeviceGPSHasFixChanged(this);
		}

		private void DevicePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "AssignedWaypoint":
					if (OnDeviceAssignedWaypointChanged != null)
						OnDeviceAssignedWaypointChanged(this);
					break;

				case "User":
					if (OnDeviceUserChanged != null)
						OnDeviceUserChanged(this);
					break;

				case "Node":
					if (OnDeviceNodeChanged != null)
						OnDeviceNodeChanged(this);
					break;
			}
		}
	}
}

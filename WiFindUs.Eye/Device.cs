using Devart.Data.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    public partial class Device
        : ILocatable, ILocation, IAtmospheric, IAtmosphere, IBatteryStats, IUpdateable
    {
        public const long TIMEOUT = 60;
        
        public static event Action<Device> OnDeviceLoaded;
        public event Action<Device> OnDeviceUpdated;
        public event Action<Device> OnDeviceTypeChanged;
        public event Action<Device> OnDeviceLocationChanged;
        public event Action<Device> OnDeviceAtmosphereChanged;
        public event Action<Device> OnDeviceBatteryChanged;
        public event Action<Device> OnDeviceIPAddressChanged;
        public event Action<Device> OnDeviceUserChanged;
        public event Action<Device> OnDeviceAssignedWaypointChanged;
        public event Action<Device> OnDeviceTimedOutChanged;
        private bool timedOut = false, loaded = false;

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

                if (OnDeviceLocationChanged != null)
                    OnDeviceLocationChanged(this);
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
                return IPAddressRaw.HasValue ? new IPAddress(IPAddressRaw.Value) : null;
            }
            set
            {
                IPAddressRaw = value == null ? null : new Nullable<long>(value.Address);
            }
        }

        public long UpdateAge
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
                if (OnDeviceTimedOutChanged != null)
                    OnDeviceTimedOutChanged(this);
            }
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

        public void SetBatteryStats(IBatteryStats stats)
        {
            if (stats == null)
            {
                Charging = null;
                BatteryLevel = null;
            }
            else
            {
                Charging = stats.Charging;
                BatteryLevel = stats.BatteryLevel;
            }
        }

        public void CheckTimeout()
        {
            TimedOut = UpdateAge > TIMEOUT;
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
            if (OnDeviceUpdated != null)
                OnDeviceUpdated(this);
            CheckTimeout();
        }
    }
}

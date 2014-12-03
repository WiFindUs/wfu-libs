using Devart.Data.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    public partial class Device
        : ILocatable, ILocation, IAtmospheric, IAtmosphere, IBatteryStats
    {
        public static event Action<Device> OnDeviceCreated;
        public event Action<Device> OnDeviceUpdated;
        public event Action<Device> OnDeviceLoaded;
        public event Action<Device> OnDeviceTypeChanged;
        public event Action<Device> OnDeviceLocationChanged;
        public event Action<Device> OnDeviceAtmosphereChanged;
        public event Action<Device> OnDeviceBatteryChanged;
        public event Action<Device> OnDeviceIPAddressChanged;
        public event Action<Device> OnDeviceUserChanged;
        public event Action<Device> OnDeviceAssignedWaypointChanged;
        
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public ILocation Location
        {
            get
            {
                return this;
            }
        }

        public IAtmosphere Atmosphere
        {
            get
            {
                return this;
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

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return String.Format("Device[{0}]", ID);
        }

        public double DistanceTo(ILocation other)
        {
            return WiFindUs.Eye.Location.Distance(this, other);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnCreated()
        {
            if (OnDeviceCreated != null)
                OnDeviceCreated(this);
        }

        partial void OnLoaded()
        {
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

        partial void OnAccuracyChanged()
        {
            if (OnDeviceLocationChanged != null)
                OnDeviceLocationChanged(this);
        }

        partial void OnAltitudeChanged()
        {
            if (OnDeviceLocationChanged != null)
                OnDeviceLocationChanged(this);
        }

        partial void OnLatitudeChanged()
        {
            if (OnDeviceLocationChanged != null)
                OnDeviceLocationChanged(this);
        }

        partial void OnLongitudeChanged()
        {
            if (OnDeviceLocationChanged != null)
                OnDeviceLocationChanged(this);
        }

        partial void OnHumidityChanged()
        {
            if (OnDeviceAtmosphereChanged != null)
                OnDeviceAtmosphereChanged(this);
        }

        partial void OnLightLevelChanged()
        {
            if (OnDeviceAtmosphereChanged != null)
                OnDeviceAtmosphereChanged(this);
        }

        partial void OnAirPressureChanged()
        {
            if (OnDeviceAtmosphereChanged != null)
                OnDeviceAtmosphereChanged(this);
        }

        partial void OnTemperatureChanged()
        {
            if (OnDeviceAtmosphereChanged != null)
                OnDeviceAtmosphereChanged(this);
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
        }
    }
}

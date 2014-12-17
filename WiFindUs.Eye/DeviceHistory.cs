using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public partial class DeviceHistory : ILocation, ILocatable, IAtmosphere, IAtmospheric
    {
        public static event Action<DeviceHistory> OnDeviceHistoryLoaded;

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
        
        public ILocation Location
        {
            get { return this; }
        }

        public IAtmosphere Atmosphere
        {
            get { return this; }
        }

        public double DistanceTo(ILocation other)
        {
            return WiFindUs.Eye.Location.Distance(this, other);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnLoaded()
        {
            Debugger.V(this.ToString() + " loaded.");
            if (OnDeviceHistoryLoaded != null)
                OnDeviceHistoryLoaded(this);
        }
    }
}

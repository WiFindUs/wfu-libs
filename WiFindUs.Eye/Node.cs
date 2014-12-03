using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    public partial class Node : ILocatable, ILocation
    {
        public static event Action<Node> OnNodeCreated;
        public event Action<Node> OnNodeUpdated;
        public event Action<Node> OnNodeLoaded;
        public event Action<Node> OnNodeNumberChanged;
        public event Action<Node> OnNodeIPAddressChanged;
        public event Action<Node> OnNodeLocationChanged;
        public event Action<Node> OnNodeVoltageChanged;

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

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public double DistanceTo(ILocation other)
        {
            return WiFindUs.Eye.Location.Distance(this, other);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnCreated()
        {
            if (OnNodeCreated != null)
                OnNodeCreated(this);
        }

        partial void OnLoaded()
        {
            if (OnNodeLoaded != null)
                OnNodeLoaded(this);
        }

        partial void OnIPAddressRawChanged()
        {
            if (OnNodeIPAddressChanged != null)
                OnNodeIPAddressChanged(this);
        }

        partial void OnAccuracyChanged()
        {
            if (OnNodeLocationChanged != null)
                OnNodeLocationChanged(this);
        }

        partial void OnAltitudeChanged()
        {
            if (OnNodeLocationChanged != null)
                OnNodeLocationChanged(this);
        }

        partial void OnLatitudeChanged()
        {
            if (OnNodeLocationChanged != null)
                OnNodeLocationChanged(this);
        }

        partial void OnLongitudeChanged()
        {
            if (OnNodeLocationChanged != null)
                OnNodeLocationChanged(this);
        }

        partial void OnNumberChanged()
        {
            if (OnNodeNumberChanged != null)
                OnNodeNumberChanged(this);
        }

        partial void OnUpdatedChanged()
        {
            if (OnNodeUpdated != null)
                OnNodeUpdated(this);
        }

        partial void OnVoltageChanged()
        {
            if (OnNodeVoltageChanged != null)
                OnNodeVoltageChanged(this);
        }
    }
}

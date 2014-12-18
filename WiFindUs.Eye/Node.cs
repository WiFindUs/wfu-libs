using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    public partial class Node : ILocatable, ILocation, IUpdateable, ISelectableEntity
    {
        public static event Action<Node> OnNodeLoaded;
        public event Action<Node> OnNodeUpdated;
        public event Action<Node> OnNodeNumberChanged;
        public event Action<Node> OnNodeIPAddressChanged;
        public event Action<Node> OnNodeLocationChanged;
        public event Action<Node> OnNodeVoltageChanged;
        public event Action<Node> OnNodeTimedOutChanged;
        public event Action<ISelectableEntity> SelectedChanged;
        private bool timedOut = false, loaded = false, selected = false;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                    return;
                selected = value;
                if (SelectedChanged != null)
                    SelectedChanged(this);
            }
        }

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
                if (OnNodeTimedOutChanged != null)
                    OnNodeTimedOutChanged(this);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public double DistanceTo(ILocation other)
        {
            return WiFindUs.Eye.Location.Distance(this, other);
        }

        public void CheckTimeout()
        {
            TimedOut = UpdateAge > 300;
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
            CheckTimeout();
        }

        partial void OnVoltageChanged()
        {
            if (OnNodeVoltageChanged != null)
                OnNodeVoltageChanged(this);
        }
    }
}

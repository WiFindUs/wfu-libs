using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    public partial class Node : SelectableEntity, ILocatable, ILocation, IUpdateable, IActionSubscriber
    {
        public const long TIMEOUT = 300;
        public static event Action<Node> OnNodeLoaded;
        public event Action<Node> OnNodeUpdated;
        public event Action<Node> OnNodeNumberChanged;
        public event Action<Node> OnNodeIPAddressChanged;
        public event Action<Node> OnNodeLocationChanged;
        public event Action<Node> OnNodeVoltageChanged;
        public event Action<Node> OnNodeTimedOutChanged;
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
        public override string ToString()
        {
            return String.Format("Node[{0:X}]", ID);
        }

        public double DistanceTo(ILocation other)
        {
            return WiFindUs.Eye.Location.Distance(this, other);
        }

        public void CheckTimeout()
        {
            TimedOut = UpdateAge > TIMEOUT;
        }

        public bool Loaded
        {
            get { return loaded; }
        }

        public bool ActionEnabled(uint index)
        {
            switch (index)
            {
                case 1: return true;
                case 2: return true;
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
                case 1: return "Zoom To";
                case 2: return "Track";
            }
            return "";
        }

        public void ActionTriggered(uint index)
        {

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

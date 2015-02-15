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
        public event Action<IUpdateable> WhenUpdated;
        public event Action<IUpdateable> TimedOutChanged;
        public event Action<Node> OnNodeNumberChanged;
        public event Action<Node> OnNodeIPAddressChanged;
        public event Action<ILocatable> LocationChanged;
        public event Action<Node> OnNodeVoltageChanged;

        public event Action<Node> OnMeshPointChanged;
        public event Action<Node> OnAPDaemonRunningChanged;
        public event Action<Node> OnDHCPDaemonRunningChanged;
        public event Action<Node> OnGPSDaemonRunningChanged;
        public event Action<Node> OnVisibleSatellitesChanged;
        public event Action<Node> OnMeshPeersChanged;

        private bool timedOut = false, loaded = false;
        private bool? meshPoint = null, apDaemon = null, dhcpDaemon = null, gpsDaemon = null;
        private int? satellites = null;
        private readonly List<Node> meshPeers = new List<Node>();

        //mp:1 ap:1 dhcp:1 gps:1 sats:10 mpl:1,2,3

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        
        //private readonly List<Node> meshPeers = new List<Node>();
        public List<Node> MeshPeers
        {
            get { return new List<Node>(meshPeers); }
            set
            {
                if (value == null || value.Count == 0)
                {
                    if (meshPeers.Count > 0)
                    {
                        meshPeers.Clear();
                        if (OnMeshPeersChanged != null)
                            OnMeshPeersChanged(this);
                    }
                    return;
                }

                if (meshPeers.Count == 0)
                {
                    meshPeers.AddRange(value);
                    if (OnMeshPeersChanged != null)
                        OnMeshPeersChanged(this);
                    return;
                }

                bool areEquivalent = (value.Count == meshPeers.Count) && !value.Except(meshPeers).Any();
                if (!areEquivalent)
                {
                    meshPeers.Clear();
                    meshPeers.AddRange(value);
                    if (OnMeshPeersChanged != null)
                        OnMeshPeersChanged(this);
                }
            }
        }

        public int? VisibleSatellites
        {
            get { return satellites; }
            set
            {
                if (satellites != value)
                {
                    satellites = value;
                    if (OnVisibleSatellitesChanged != null)
                        OnVisibleSatellitesChanged(this);
                }
            }
        }

        public bool? IsGPSDaemonRunning
        {
            get { return gpsDaemon; }
            set
            {
                if (gpsDaemon != value)
                {
                    gpsDaemon = value;
                    if (OnGPSDaemonRunningChanged != null)
                        OnGPSDaemonRunningChanged(this);
                }
            }
        }

        public bool? IsDHCPDaemonRunning
        {
            get { return dhcpDaemon; }
            set
            {
                if (dhcpDaemon != value)
                {
                    dhcpDaemon = value;
                    if (OnDHCPDaemonRunningChanged != null)
                        OnDHCPDaemonRunningChanged(this);
                }
            }
        }

        public bool? IsMeshPoint
        {
            get { return meshPoint; }
            set
            {
                if (meshPoint != value)
                {
                    meshPoint = value;
                    if (OnMeshPointChanged != null)
                        OnMeshPointChanged(this);
                }
            }
        }

        public bool? IsAPDaemonRunning
        {
            get { return apDaemon; }
            set
            {
                if (apDaemon != value)
                {
                    apDaemon = value;
                    if (OnAPDaemonRunningChanged != null)
                        OnAPDaemonRunningChanged(this);
                }
            }
        }

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

                if (LocationChanged != null)
                    LocationChanged(this);
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
                if (TimedOutChanged != null)
                    TimedOutChanged(this);
            }
        }

        public long TimeoutLength
        {
            get { return TIMEOUT; }
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

        public String ActionDescription
        {
            get { return this.ToString(); }
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
            if (LocationChanged != null)
                LocationChanged(this);
        }

        partial void OnAltitudeChanged()
        {
            if (LocationChanged != null)
                LocationChanged(this);
        }

        partial void OnLatitudeChanged()
        {
            if (LocationChanged != null)
                LocationChanged(this);
        }

        partial void OnLongitudeChanged()
        {
            if (LocationChanged != null)
                LocationChanged(this);
        }

        partial void OnNumberChanged()
        {
            if (OnNodeNumberChanged != null)
                OnNodeNumberChanged(this);
        }

        partial void OnUpdatedChanged()
        {
            if (WhenUpdated != null)
                WhenUpdated(this);
            CheckTimeout();
        }

        partial void OnVoltageChanged()
        {
            if (OnNodeVoltageChanged != null)
                OnNodeVoltageChanged(this);
        }


    }
}

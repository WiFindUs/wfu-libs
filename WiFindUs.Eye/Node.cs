using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
	public partial class Node : SelectableEntity, ILocatable, ILocation, IUpdateable, IActionSubscriber
	{
		public const ulong TIMEOUT = 60;
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
		public event Action<Node> OnGPSFakeChanged;

		private bool timedOut = false, loaded = false;
		private bool? meshPoint = null, apDaemon = null, dhcpDaemon = null, gpsDaemon = null, gpsFake = null;
		private uint? satellites = null;
		private readonly List<Node> meshPeers = new List<Node>();
		private StackedLock locationEventLock = new StackedLock();
		private bool fireLocationEvents = false;
		private IPAddress ipAddress = null;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public int MeshPeerCount
		{
			get { return meshPeers.Count; }
		}

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

		public uint? VisibleSatellites
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

					if (!gpsDaemon.GetValueOrDefault())
					{
						Location = null;
						IsGPSFake = null;
					}
				}
			}
		}

		public bool? IsGPSFake
		{
			get { return gpsFake; }
			set
			{
				if (gpsFake != value)
				{
					gpsFake = value;
					if (OnGPSFakeChanged != null)
						OnGPSFakeChanged(this);
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
					MeshPeers = null;
					VisibleSatellites = null;
					IsGPSDaemonRunning = null;
					IsDHCPDaemonRunning = null;
					IsMeshPoint = null;
					IsAPDaemonRunning = null;
					IPAddress = null;
					Voltage = null;
					Location = null;
				}
				if (TimedOutChanged != null)
					TimedOutChanged(this);
			}
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

		partial void OnLoaded()
		{
			ipAddress = IPAddressRaw.HasValue ? new IPAddress(IPAddressRaw.Value) : null;
			loaded = true;
			Debugger.V(this.ToString() + " loaded.");
			if (OnNodeLoaded != null)
				OnNodeLoaded(this);
		}

		partial void OnIPAddressRawChanged()
		{
			ipAddress = IPAddressRaw.HasValue ? new IPAddress(IPAddressRaw.Value) : null;
			if (OnNodeIPAddressChanged != null)
				OnNodeIPAddressChanged(this);
		}

		partial void OnAccuracyChanged()
		{
			FireLocationChangedEvent();
		}

		partial void OnAltitudeChanged()
		{
			FireLocationChangedEvent();
		}

		partial void OnLatitudeChanged()
		{
			FireLocationChangedEvent();
		}

		partial void OnLongitudeChanged()
		{
			FireLocationChangedEvent();
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

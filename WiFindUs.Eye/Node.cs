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
		private const ulong TIMEOUT = 60;
		public static event Action<Node> OnNodeLoaded;
		public event Action<IUpdateable> ActiveChanged;
		public event Action<IUpdateable> Updated;
		public event Action<Node> OnNodeNumberChanged;
		public event Action<ILocatable> LocationChanged;
		public event Action<Node> OnNodeVoltageChanged;

		public event Action<Node> OnNodeMeshPointChanged;
		public event Action<Node> OnNodeAccessPointChanged;
		public event Action<Node> OnNodeDHCPDChanged;
		public event Action<Node> OnNodeGPSDChanged;
		public event Action<Node> OnNodeSatelliteCountChanged;
		public event Action<Node> OnNodeMockLocationChanged;

		private bool loaded = false;
		private StackedLock locationEventLock = new StackedLock();
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

		public bool Loaded
		{
			get { return loaded; }
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

		public void CheckActive()
		{
			Active = (DateTime.UtcNow.ToUnixTimestamp() - LastUpdated) <= TIMEOUT;
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
			if (!loaded)
			{
				loaded = true;
				Debugger.V(this.ToString() + " loaded.");
				if (OnNodeLoaded != null)
					OnNodeLoaded(this);
				CheckActive();
			}
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

		partial void OnVoltageChanged()
		{
			if (OnNodeVoltageChanged != null)
				OnNodeVoltageChanged(this);
		}

		partial void OnAccessPointChanged()
		{
			if (OnNodeAccessPointChanged != null)
				OnNodeAccessPointChanged(this);
		}

		partial void OnDHCPDChanged()
		{
			if (OnNodeDHCPDChanged != null)
				OnNodeDHCPDChanged(this);
		}

		partial void OnGPSDChanged()
		{
			if (OnNodeGPSDChanged != null)
				OnNodeGPSDChanged(this);
		}

		partial void OnMeshPointChanged()
		{
			if (OnNodeMeshPointChanged != null)
				OnNodeMeshPointChanged(this);
		}

		partial void OnSatelliteCountChanged()
		{
			if (OnNodeSatelliteCountChanged != null)
				OnNodeSatelliteCountChanged(this);
		}

		partial void OnMockLocationChanged()
		{
			if (OnNodeMockLocationChanged != null)
				OnNodeMockLocationChanged(this);
		}

	}
}

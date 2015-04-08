using System;
using System.Collections.Generic;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
	public partial class Waypoint : SelectableEntity, ILocation, ILocatable
	{
		public static event Action<Waypoint> OnWaypointLoaded;
		public event Action<ILocatable> LocationChanged;
		public event Action<Waypoint> OnWaypointTypeChanged;
		public event Action<Waypoint> OnWaypointCategoryChanged;
		public event Action<Waypoint> OnWaypointDescriptionChanged;
		public event Action<Waypoint> OnWaypointSeverityChanged;
		public event Action<Waypoint> OnWaypointCodeChanged;
		public event Action<Waypoint> OnWaypointReportingUserChanged;
		public event Action<Waypoint> OnWaypointArchivedChanged;
		public event Action<Waypoint> OnWaypointNextWaypointChanged;
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
					//Accuracy = null;
					Longitude = null;
					Latitude = null;
				}
				else
				{
					Altitude = value.Altitude;
					//Accuracy = value.Accuracy;
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

		public double? Accuracy
		{
			get { return 0.0; }
		}


		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public double DistanceTo(ILocation other)
		{
			return WiFindUs.Eye.Location.Distance(this, other);
		}

		public void Archive()
		{
			if (Archived)
				return;

			//store users
			List<Device> assignedDevices = new List<Device>();
			foreach (Device device in AssignedDevices)
			{
				if (device.User != null)
					ArchivedResponders.Add(device.User);
				assignedDevices.Add(device);
			}

			//unassign devices
			foreach (Device device in assignedDevices)
				device.AssignedWaypoint = null;

			//store time and set flag
			ArchivedTime = DateTime.UtcNow.ToUnixTimestamp();
			Archived = true;
		}

		public bool Loaded
		{
			get { return loaded; }
		}

		public bool IsIncident
		{
			get { return Type.ToLower().CompareTo("incident") == 0; }
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

		partial void OnLoaded()
		{
			if (!loaded)
			{
				loaded = true;
				Debugger.V(this.ToString() + " loaded.");
				if (OnWaypointLoaded != null)
					OnWaypointLoaded(this);
			}
		}

		partial void OnNextWaypointIDChanged()
		{
			if (OnWaypointNextWaypointChanged != null)
				OnWaypointNextWaypointChanged(this);
		}

		partial void OnReportedByIDChanged()
		{
			if (OnWaypointReportingUserChanged != null)
				OnWaypointReportingUserChanged(this);
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

		partial void OnCategoryChanged()
		{
			if (OnWaypointCategoryChanged != null)
				OnWaypointCategoryChanged(this);
		}

		partial void OnCodeChanged()
		{
			if (OnWaypointCodeChanged != null)
				OnWaypointCodeChanged(this);
		}

		partial void OnDescriptionChanged()
		{
			if (OnWaypointDescriptionChanged != null)
				OnWaypointDescriptionChanged(this);
		}

		partial void OnTypeChanged()
		{
			if (OnWaypointTypeChanged != null)
				OnWaypointTypeChanged(this);
		}

		partial void OnSeverityChanged()
		{
			if (OnWaypointSeverityChanged != null)
				OnWaypointSeverityChanged(this);
		}

		partial void OnArchivedChanged()
		{
			if (OnWaypointArchivedChanged != null)
				OnWaypointArchivedChanged(this);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    public partial class Waypoint : ILocation, ILocatable, ThemedListBoxItem
    {
        public static event Action<Waypoint> OnWaypointCreated;
        public event Action<Waypoint> OnWaypointLocationChanged;
        public event Action<Waypoint> OnWaypointTypeChanged;
        public event Action<Waypoint> OnWaypointCategoryChanged;
        public event Action<Waypoint> OnWaypointDescriptionChanged;
        public event Action<Waypoint> OnWaypointSeverityChanged;
        public event Action<Waypoint> OnWaypointCodeChanged;
        public event Action<Waypoint> OnWaypointReportingUserChanged;
        public event Action<Waypoint> OnWaypointArchivedChanged;
        public event Action<Waypoint> OnWaypointNextWaypointChanged;

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

        public int MeasureItemHeight(ThemedListBox host, System.Windows.Forms.MeasureItemEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void DrawListboxItem(System.Windows.Forms.DrawItemEventArgs e)
        {
            throw new NotImplementedException();
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////
        partial void OnCreated()
        {
            if (OnWaypointCreated != null)
                OnWaypointCreated(this);
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
            if (OnWaypointLocationChanged != null)
                OnWaypointLocationChanged(this);
        }

        partial void OnLongitudeChanged()
        {
            if (OnWaypointLocationChanged != null)
                OnWaypointLocationChanged(this);
        }

        partial void OnAltitudeChanged()
        {
            if (OnWaypointLocationChanged != null)
                OnWaypointLocationChanged(this);
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

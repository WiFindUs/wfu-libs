using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public partial class Waypoint : ILocation, ILocatable, IIndentifiable, ICreationTimestamped
    {
        public delegate void WaypointEvent(Waypoint sender);
        public static event WaypointEvent OnWaypointCreated;
        public event WaypointEvent OnLocationChanged;
        public event WaypointEvent OnWaypointTypeChanged;
        public event WaypointEvent OnWaypointCategoryChanged;
        public event WaypointEvent OnWaypointDescriptionChanged;
        public event WaypointEvent OnWaypointSeverityChanged;
        public event WaypointEvent OnWaypointCodeChanged;
        public event WaypointEvent OnReportingUserChanged;
        public event WaypointEvent OnWaypointArchivedChanged;
        public event WaypointEvent OnNextWaypointChanged;
        public event WaypointEvent OnAssignedDevicesChanged;

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

        public double? Accuracy
        {
            get { return 0.0; }
        }

        public bool IsArchived
        {
            get { return ArchivedInternal; }
            set
            {
                if (ArchivedInternal || !value)
                    return;

                //store users
                List<Device> assignedDevices = new List<Device>();
                foreach (Device device in AssignedDevices)
                {
                    if (device.User != null)
                        ArchivedRespondersInternal.Add(device.User);
                    assignedDevices.Add(device);
                }

                //unassign devices
                foreach (Device device in assignedDevices)
                    device.AssignedWaypoint = null;

                //store time and set flag
                ArchivedTime = DateTime.UtcNow;
                ArchivedInternal = true;
            }
        }

        public List<User> ArchivedResponders
        {
            get { return IsArchived ? new List<User>() : new List<User>(ArchivedRespondersInternal); }
        }

        public DateTime? Archived
        {
            get { return IsArchived ? ArchivedTime : null; }
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
            AssignedDevices.ListChanged += AssignedDevices_ListChanged;
            if (OnWaypointCreated != null)
                OnWaypointCreated(this);
        }

        private void AssignedDevices_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (OnAssignedDevicesChanged != null)
                OnAssignedDevicesChanged(this);
        }

        partial void OnNextWaypointIDChanged()
        {
            if (OnNextWaypointChanged != null)
                OnNextWaypointChanged(this);
        }

        partial void OnArchivedInternalChanged()
        {
            if (OnWaypointArchivedChanged != null)
                OnWaypointArchivedChanged(this);
        }

        partial void OnReportedByIDChanged()
        {
            if (OnReportingUserChanged != null)
                OnReportingUserChanged(this);
        }

        partial void OnLatitudeChanged()
        {
            if (OnLocationChanged != null)
                OnLocationChanged(this);
        }

        partial void OnLongitudeChanged()
        {
            if (OnLocationChanged != null)
                OnLocationChanged(this);
        }

        partial void OnAltitudeChanged()
        {
            if (OnLocationChanged != null)
                OnLocationChanged(this);
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

        /*


        public List<IUser> ArchivedResponders
        {
            get
            {
                return new List<IUser>(ArchivedResponders);
            }
        }

        public List<IDevice> AssignedDevices
        {
            get
            {
                return new List<IDevice>(AssignedDevices);
            }
        }

        public IWaypoint NextWaypoint
        {
            get
            {
                return NextWaypoint;
            }
            set
            {
                if (value == null)
                    NextWaypoint = null;
                else
                {
                    Waypoint waypoint = value as Waypoint;
                    if (waypoint == null)
                        throw new InvalidOperationException("You must use the database type when making this assignment!");
                    NextWaypoint = waypoint;
                }
            }
        }

        public IUser ReportingUser
        {
            get
            {
                return ReportingUser;
            }
            set
            {
                if (value == null)
                    ReportingUser = null;
                else
                {
                    DBUser user = value as DBUser;
                    if (user == null)
                        throw new InvalidOperationException("You must use the database type when making this assignment!");
                    ReportingUser = user;
                }
            }
        }
         * */

        /*
        
        public bool Archived
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<IUser> ArchivedResponders
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime? ArchivedTime
        {
            get { throw new NotImplementedException(); }
        }

        public List<IDevice> AssignedDevices
        {
            get { throw new NotImplementedException(); }
        }

        public string Category
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Code
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime Created
        {
            get { throw new NotImplementedException(); }
        }

        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IWaypoint NextWaypoint
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IUser ReportingUser
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Severity
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Type
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double Latitude
        {
            get { throw new NotImplementedException(); }
        }

        public double Longitude
        {
            get { throw new NotImplementedException(); }
        }

        public double? Accuracy
        {
            get { throw new NotImplementedException(); }
        }

        public double? Altitude
        {
            get { throw new NotImplementedException(); }
        }

        public double DistanceTo(ILocation other)
        {
            return Location.Distance(this, other);
        }

        */
    }
}

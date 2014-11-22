using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye.Context
{
    public partial class DBWaypoint : ILocation, IWaypoint, IIndentifiable, ICreationTimestamped
    {
        public double DistanceTo(ILocation other)
        {
            return Location.Distance(this, other);
        }

        public List<IUser> ArchivedResponders
        {
            get
            {
                return new List<IUser>(DBArchivedResponders);
            }
        }

        public List<IDevice> AssignedDevices
        {
            get
            {
                return new List<IDevice>(DBAssignedDevices);
            }
        }

        public IWaypoint NextWaypoint
        {
            get
            {
                return DBNextWaypoint;
            }
            set
            {
                if (value == null)
                    DBNextWaypoint = null;
                else
                {
                    DBWaypoint waypoint = value as DBWaypoint;
                    if (waypoint == null)
                        throw new InvalidOperationException("You must use the database type when making this assignment!");
                    DBNextWaypoint = waypoint;
                }
            }
        }

        public IUser ReportingUser
        {
            get
            {
                return DBReportingUser;
            }
            set
            {
                if (value == null)
                    DBReportingUser = null;
                else
                {
                    DBUser user = value as DBUser;
                    if (user == null)
                        throw new InvalidOperationException("You must use the database type when making this assignment!");
                    DBReportingUser = user;
                }
            }
        }
    }
}

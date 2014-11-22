using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public class Waypoint : ILocation, IWaypoint, IIndentifiable, ICreationTimestamped
    {
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

        public long ID
        {
            get { throw new NotImplementedException(); }
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
            throw new NotImplementedException();
        }
    }
}

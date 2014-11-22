using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public class Device : IDevice, IIndentifiable, ILocatable, ICreationTimestamped
    {
        public IWaypoint AssignedWaypoint
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

        public List<IAtmosphere> Atmospheres
        {
            get { throw new NotImplementedException(); }
        }

        public IAtmosphere CurrentAtmosphere
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime Created
        {
            get { throw new NotImplementedException(); }
        }

        public List<ILocation> Locations
        {
            get { throw new NotImplementedException(); }
        }

        public ILocation CurrentLocation
        {
            get { throw new NotImplementedException(); }
        }

        public List<IDeviceLogin> Logins
        {
            get { throw new NotImplementedException(); }
        }

        public IUser CurrentUser
        {
            get { throw new NotImplementedException(); }
        }

        public string Type
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime Updated
        {
            get { throw new NotImplementedException(); }
        }

        public long ID
        {
            get { throw new NotImplementedException(); }
        }
    }
}

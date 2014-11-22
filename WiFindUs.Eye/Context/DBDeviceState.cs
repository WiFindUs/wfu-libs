using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye.Context
{
    public partial class DBDeviceState : ILocation, IAtmosphere, IDeviceLogin, IDeviceClient, IUserClient, ICreationTimestamped
    {
        public double DistanceTo(ILocation other)
        {
            return Location.Distance(this, other);
        }

        public IDevice Device
        {
            get { return DBDevice; }
        }

        public IUser User
        {
            get { return DBUser; }
        }
    }
}

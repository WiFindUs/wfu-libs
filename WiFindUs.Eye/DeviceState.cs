using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public partial class DeviceState : ILocation, IAtmosphere, IDeviceLogin, IDeviceClient, IUserClient, ICreationTimestamped
    {
        public double DistanceTo(ILocation other)
        {
            return Location.Distance(this, other);
        }
    }
}

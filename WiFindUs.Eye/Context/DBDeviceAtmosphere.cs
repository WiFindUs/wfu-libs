using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye.Context
{
    public partial class DBDeviceAtmosphere : IAtmosphere, IDeviceClient, ICreationTimestamped
    {
        public IDevice Device
        {
            get { return DeviceDB; }
        }
    }
}

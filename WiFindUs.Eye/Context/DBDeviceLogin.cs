using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye.Context
{
    public partial class DBDeviceLogin : IDeviceLogin, IDeviceClient, IUserClient, ICreationTimestamped
    {
        public IDevice Device
        {
            get { return DeviceDB; }
        }

        public IUser User
        {
            get { return UserDB; }
        }
    }
}

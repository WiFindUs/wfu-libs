﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public interface IDeviceClient
    {
        IDevice Device { get; }
    }
}
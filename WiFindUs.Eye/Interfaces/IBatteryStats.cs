using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public interface IBatteryStats
    {
        bool? Charging { get; }
        double? BatteryLevel { get; }
    }
}

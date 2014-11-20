﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs
{
    public interface IAtmosphere
    {
        double? Humidity { get; }
        double? AirPressure { get; }
        double? Temperature { get; }
        double? LightLevel { get; }
    }
}

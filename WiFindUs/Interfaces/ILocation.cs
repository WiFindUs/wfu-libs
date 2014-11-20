using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs
{
    public interface ILocation
    {
        double Latitude { get; }
        double Longitude { get; }
        double? Accuracy { get; }
        double? Altitude { get; }
        double DistanceTo(ILocation other);
    }
}

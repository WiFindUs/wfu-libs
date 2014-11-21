using System;

namespace WiFindUs.Eye
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

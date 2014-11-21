using System;
using System.Drawing;

namespace WiFindUs.Eye
{
    public interface IRegion
    {
        ILocation Center { get; }
        bool Contains(double latitude, double longitude);
        bool Contains(ILocation location);
        double Height { get; }
        double LatitudinalSpan { get; }
        Point LocationToScreen(Rectangle screenBounds, double latitude, double longitude);
        Point LocationToScreen(Rectangle screenBounds, ILocation location);
        double LongitudinalSpan { get; }
        ILocation NorthEast { get; }
        ILocation NorthWest { get; }
        ILocation SouthEast { get; }
        ILocation SouthWest { get; }
        double Width { get; }
    }
}

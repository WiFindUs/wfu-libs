using System;
namespace WiFindUs
{
    interface IRegion
    {
        WiFindUs.ILocation Center { get; }
        bool Contains(double latitude, double longitude);
        bool Contains(WiFindUs.Location location);
        double Height { get; }
        double LatitudinalSpan { get; }
        System.Drawing.Point LocationToScreen(System.Drawing.Rectangle screenBounds, double latitude, double longitude);
        System.Drawing.Point LocationToScreen(System.Drawing.Rectangle screenBounds, WiFindUs.Location location);
        double LongitudinalSpan { get; }
        WiFindUs.ILocation NorthEast { get; }
        WiFindUs.ILocation NorthWest { get; }
        WiFindUs.ILocation SouthEast { get; }
        WiFindUs.ILocation SouthWest { get; }
        double Width { get; }
    }
}

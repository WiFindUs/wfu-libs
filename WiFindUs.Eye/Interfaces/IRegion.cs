
namespace WiFindUs.Eye
{
	public interface IRegion
	{
		ILocation Center { get; }
		bool Contains(double latitude, double longitude);
		bool Contains(ILocation location);
		double Height { get; }
		double LatitudinalSpan { get; }
		System.Drawing.Point LocationToScreen(System.Drawing.Rectangle screenBounds, double latitude, double longitude);
		System.Drawing.Point LocationToScreen(System.Drawing.Rectangle screenBounds, ILocation location);
		double LongitudinalSpan { get; }
		ILocation NorthEast { get; }
		ILocation NorthWest { get; }
		ILocation SouthEast { get; }
		ILocation SouthWest { get; }
		double Width { get; }
		ILocation Clamp(ILocation location);
	}
}

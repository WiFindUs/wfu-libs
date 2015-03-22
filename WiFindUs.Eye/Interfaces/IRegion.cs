
namespace WiFindUs.Eye
{
	public interface IRegion
	{
		ILocation Center { get; }
		bool Contains(double latitude, double longitude);
		bool Contains(ILocation location);
		double Height { get; }
		double LatitudinalSpan { get; }
		WaveEngine.Common.Math.Vector3 LocationToVector(WaveEngine.Common.Math.Vector3 tl, WaveEngine.Common.Math.Vector3 br, double latitude, double longitude);
		WaveEngine.Common.Math.Vector3 LocationToVector(WaveEngine.Common.Math.Vector3 tl, WaveEngine.Common.Math.Vector3 br, ILocation location);
		System.Drawing.Point LocationToScreen(System.Drawing.Rectangle screenBounds, double latitude, double longitude);
		System.Drawing.Point LocationToScreen(System.Drawing.Rectangle screenBounds, ILocation location);
		double LongitudinalSpan { get; }
		ILocation NorthEast { get; }
		ILocation NorthWest { get; }
		ILocation SouthEast { get; }
		ILocation SouthWest { get; }
		double Width { get; }
	}
}

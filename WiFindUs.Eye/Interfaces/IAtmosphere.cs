
namespace WiFindUs.Eye
{
	public interface IAtmosphere
	{
		double? Humidity { get; }
		double? AirPressure { get; }
		double? Temperature { get; }
		double? LightLevel { get; }
		bool EmptyAtmosphere { get; }
	}
}

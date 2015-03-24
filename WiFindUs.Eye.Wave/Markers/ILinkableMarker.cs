using WaveEngine.Common.Math;

namespace WiFindUs.Eye.Wave.Markers
{
	public interface ILinkableMarker
	{
		Vector3 LinkPointPrimary { get; }
		Vector3 LinkPointSecondary { get; }
	}
}

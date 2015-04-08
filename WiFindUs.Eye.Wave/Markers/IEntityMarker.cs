using System;
using WaveEngine.Common.Math;

namespace WiFindUs.Eye.Wave.Markers
{
	public interface IEntityMarker
	{
		ISelectable Selectable { get; }
		ILocatable Locatable { get; }
		IUpdateable Updateable { get; }
	}
}

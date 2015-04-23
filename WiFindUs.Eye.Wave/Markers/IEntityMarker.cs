using System;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WiFindUs.Eye.Wave.Markers
{
	public interface IEntityMarker
	{
		ISelectable Selectable { get; }
		ILocatable Locatable { get; }
		IUpdateable Updateable { get; }
		bool EntityActive { get; }
		bool EntityWaiting { get; }
		bool EntitySelected { get; }
		bool CursorOver { get; }
		bool IsOwnerVisible { get; }
		bool IsOwnerActive { get; }
		Transform2D UITransform { get; }
		Entity UIEntity { get; }
		Transform3D Transform3D { get; }
	}
}

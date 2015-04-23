using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Services;
using WiFindUs.Eye.Wave.Markers;

namespace WiFindUs.Eye.Wave
{
	public class MapUISorter : MapSceneBehavior
	{
		protected override void Update(TimeSpan gameTime)
		{
			IEntityMarker[] entityMarkers = MapScene.Markers
				.OfType<IEntityMarker>()
				.Where(mk => mk.IsOwnerVisible && mk.UIEntity != null
					&& (mk.EntitySelected || mk.CursorOver || mk.EntityWaiting))
				.OrderBy(mk => { return Vector3.DistanceSquared(mk.Transform3D.Position,
					MapScene.RenderManager.ActiveCamera3D.Position); })
				.ToArray();

			if (entityMarkers.Length > 1)
			{
				float step = 1.0f / (entityMarkers.Length - 1);
				for (int i = 0; i < entityMarkers.Length; i++)
					entityMarkers[i].UITransform.DrawOrder = step * i;
			}

			WaveServices.Layout.PerformLayout();
		}
	}
}

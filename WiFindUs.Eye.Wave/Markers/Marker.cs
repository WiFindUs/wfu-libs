using System;
using System.Collections.Generic;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Materials;

namespace WiFindUs.Eye.Wave.Markers
{
	public abstract class Marker : MapBehavior
	{
		private CylindricalCollider cylindricalCollider;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public abstract bool Selected { get; set; }

		protected virtual float ScaleMultiplier
		{
			get { return 1.0f; }
		}

		public CylindricalCollider CylindricalCollider
		{
			get { return cylindricalCollider; }
			internal set { cylindricalCollider = value; }
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			float scale = MapScene.MarkerScale * ScaleMultiplier;
			Transform3D.Scale = Vector3.Lerp(Transform3D.Scale, new Vector3(scale, scale, scale),
				(float)gameTime.TotalSeconds * SCALE_SPEED);
		}
	}
}

using System;
using System.Collections.Generic;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Materials;

namespace WiFindUs.Eye.Wave.Markers
{
	public abstract class Marker : MapSceneEntityBehavior
	{
		protected const float FADE_SPEED = 15f;
		protected const float SCALE_SPEED = 15f;
		private List<BoxCollider> colliders = new List<BoxCollider>();

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public abstract bool Selected { get; set; }

		protected virtual float ScaleMultiplier
		{
			get { return 1.0f; }
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public float? Intersects(ref Ray ray)
		{
			if (ray == null || colliders.Count == 0)
				return null;
			foreach (BoxCollider collider in colliders)
			{
				float? result = collider.Intersects(ref ray);
				if (result.HasValue)
					return result;
			}
			return null;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected BoxCollider AddCollider(BoxCollider collider)
		{
			if (collider == null)
				return null;
			if (colliders.Contains(collider))
				return collider;
			colliders.Add(collider);
			collider.DebugLineColor = Color.Cyan;
			return collider;
		}

		protected BoxCollider RemoveCollider(BoxCollider collider)
		{
			if (collider == null)
				return null;
			colliders.Remove(collider);
			return collider;
		}

		protected override void Update(TimeSpan gameTime)
		{
			float scale = Scene.MarkerScale * ScaleMultiplier;
			Transform3D.Scale = Vector3.Lerp(Transform3D.Scale, new Vector3(scale, scale, scale),
				(float)gameTime.TotalSeconds * SCALE_SPEED);
		}
	}
}

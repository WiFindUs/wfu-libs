using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WiFindUs.Eye.Wave
{
	public class CylindricalCollider : Component
	{
		[RequiredComponent]
		public Transform3D Transform3D;
		public Color DebugLineColor = Color.Cyan;

		public readonly float Height;
		public readonly float Radius;
		public readonly float Diameter;
		public readonly float Offset;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public Vector3 Position
		{
			get
			{
				Vector3 position = Transform3D.Position;
				position.Y += Offset * Transform3D.Scale.Y;
				return position;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public CylindricalCollider(float h = 1.0f, float r = 1.0f, float o = 0.0f)
		{
			Height = Math.Max(1.0f, h);
			Radius = Math.Max(1.0f, r);
			Diameter = Radius * 2.0f;
			Offset = o;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public bool Intersects(CylindricalCollider collider)
		{
			if (collider == null || collider == this)
				return false;

			//get center points, make them co-planar on the Y axis
			Vector3 a = Position;
			Vector3 b = collider.Position;
			float bY = b.Y;
			b.Y = a.Y;
			
			//check if distance is too great for an intersection
			float distance;
			Vector3.Distance(ref a, ref b, out distance);
			if (distance > (Radius * Math.Max(Transform3D.Scale.X, Transform3D.Scale.Z)
				+ collider.Radius * Math.Max(collider.Transform3D.Scale.X, collider.Transform3D.Scale.Z)))
				return false;
			
			//check limits
			float aTop = a.Y + (Height / 2.0f) * Transform3D.Scale.Y;
			float aBottom = a.Y - (Height / 2.0f) * Transform3D.Scale.Y;
			float bTop = bY + (collider.Height / 2.0f) * collider.Transform3D.Scale.Y;
			float bBottom = bY - (collider.Height / 2.0f) * collider.Transform3D.Scale.Y;

			return (aTop >= bBottom && aBottom <= bTop);
		}
	}
}



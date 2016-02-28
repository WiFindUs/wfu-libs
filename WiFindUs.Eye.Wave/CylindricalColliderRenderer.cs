using System;

namespace WiFindUs.Eye.Wave
{
    public class CylindricalColliderRenderer : Drawable3D
	{
		[RequiredComponent]
		public CylindricalCollider CylindricalCollider;
		[RequiredComponent]
		public Transform3D Transform3D;
		private static int instances;
		private bool disposed;
		public readonly uint Rings;
		public readonly uint Tesselation;



		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public CylindricalColliderRenderer(uint r = 4, uint t = 8)
            : this("CursorColliderRenderer" + instances, r, t)
        {
        }

		public CylindricalColliderRenderer(string name, uint r = 4, uint t = 8)
            : base(name)
        {
            instances++;
			Rings = Math.Min(Math.Max(r, 2), 10);
			Tesselation = Math.Min(Math.Max(t, 3), 24);
        }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		public override void Draw(TimeSpan gameTime)
		{
			if (CylindricalCollider == null || !RenderManager.DebugLines)
				return;

			float radius = CylindricalCollider.Radius * Math.Max(Transform3D.Scale.X, Transform3D.Scale.Z);
			float height = CylindricalCollider.Height * Transform3D.Scale.Y;
			Vector3 position = CylindricalCollider.Position;

			//build rings
			Vector3[][] rings = new Vector3[Rings][];
			for (uint r = 0; r < Rings; r++)
			{
				rings[r] = new Vector3[Tesselation];
				float ry = r * (height / Rings);
				for (uint p = 0; p < Tesselation; p++)
				{
					double rot = (Math.PI * 2.0) * ((double)p / Tesselation);
					rings[r][p] = new Vector3(
						position.X + radius * (float)Math.Cos(rot),
						position.Y - height / 2.0f + ry,
						position.Z + radius * (float)Math.Sin(rot));
				}
			}

			//draw rings
			for (uint r = 0; r < Rings; r++)
				for (uint p = 0; p < Tesselation; p++)
					RenderManager.LineBatch3D.DrawLine(ref rings[r][p], ref rings[r][(p + 1) % Tesselation], ref CylindricalCollider.DebugLineColor);

			//draw lines
			for (uint p = 0; p < Tesselation; p++)
				RenderManager.LineBatch3D.DrawLine(ref rings[0][p], ref rings[Rings - 1][p], ref CylindricalCollider.DebugLineColor);
		}

		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.disposed = true;
				}
			}
		}
	}
}

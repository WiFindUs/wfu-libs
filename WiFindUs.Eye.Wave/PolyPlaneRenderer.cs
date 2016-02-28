using System;
using System.Collections.Generic;

namespace WiFindUs.Eye.Wave
{
    public class PolyPlaneRenderer : Drawable3D
	{
		[RequiredComponent]
		public PolyPlane PolyPlane;
		[RequiredComponent]
		public MaterialsMap MaterialMap;
		[RequiredComponent]
		public Transform3D Transform3D;

		private Color diffuseColor;
		private float alpha;
		private static int instances;
		private bool disposed;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public Color DiffuseColor
		{
			get { return this.diffuseColor; }

			set
			{
				if (value != this.diffuseColor)
				{
					this.diffuseColor = value;

					if (this.MaterialMap != null)
					{
						foreach (KeyValuePair<string, Material> kv in this.MaterialMap.Materials)
						{
							var material = kv.Value as BasicMaterial;
							if (material != null)
								material.DiffuseColor = this.diffuseColor;
						}
					}
				}
			}
		}

		public float Alpha
		{
			get { return this.alpha; }

			set
			{
				if (value != this.alpha)
				{
					this.alpha = value;

					if (this.MaterialMap != null)
					{
						foreach (KeyValuePair<string, Material> kv in this.MaterialMap.Materials)
						{
							var material = kv.Value as BasicMaterial;
							if (material != null)
								material.Alpha = this.alpha;
						}
					}
				}
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public PolyPlaneRenderer()
            : this("PolyPlaneRenderer" + instances)
        {
        }

		public PolyPlaneRenderer(string name)
            : base(name)
        {
            this.DiffuseColor = Color.White;
            this.Alpha = 1.0f;
            instances++;
        }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		public override void Draw(TimeSpan gameTime)
		{
			if (PolyPlane == null || PolyPlane.Mesh == null || MaterialMap.DefaultMaterial == null)
				return;

			PolyPlane.Mesh.ZOrder = Vector3.DistanceSquared(RenderManager.CurrentDrawingCamera3D.Position, Transform3D.Position);
			Matrix l2w = Transform3D.WorldTransform;
			RenderManager.DrawMesh(PolyPlane.Mesh, MaterialMap.DefaultMaterial, ref l2w);

			if (RenderManager.DebugLines)
				PolyPlane.DrawDebugLines();
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

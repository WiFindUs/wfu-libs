using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Materials;
using WiFindUs.Eye.Wave.Layers;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
	public class SelectionRing : MapSceneObject
	{
		protected readonly ISelectable selectable;
		private Entity[] points;
		private BasicMaterial matte;
		protected const float ROTATE_SPEED = 5f;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public ISelectable Selectable
		{
			get { return selectable; }
		}
		
		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public SelectionRing(ISelectable selectable)
		{
			if (selectable == null)
				throw new ArgumentNullException("selectable", "Selectable cannot be null!");
			this.selectable = selectable;
		}

		public static Entity Create(ISelectable selectable, float yOffset = 10.0f, uint points = 6, float radius = 12.0f, float thickness = 2.5f)
		{
			SelectionRing ring = new SelectionRing(selectable);
			ring.points = new Entity[points];
			ring.matte = new BasicMaterial(MapScene.WhiteTexture)
			{
				LayerType = typeof(Overlays),
				LightingEnabled = true,
				AmbientLightColor = Color.White * 0.75f,
				//DiffuseColor = Color.White,
				Alpha = 0.0f
			};

			Entity entity = new Entity()
				.AddComponent(new Transform3D())
				.AddComponent(ring);

			
			for (uint i = 0; i < points; i++)
			{
				float pc = (float)i / (float)points;
				entity.AddChild
				(
					ring.points[i] = new Entity()
						.AddComponent(new Transform3D()
						{
							LocalPosition = new Vector3((float)Math.Cos(Math.PI * 2.0f * pc) * radius,
								yOffset, (float)Math.Sin(Math.PI * 2.0f * pc) * radius)
						})
						.AddComponent(new MaterialsMap(ring.matte))
						.AddComponent(Model.CreateSphere(thickness, 3))
						.AddComponent(new ModelRenderer())
					);
			}
			return entity;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			Transform3D.LocalRotation = new Vector3(
				Transform3D.LocalRotation.X,
				Transform3D.LocalRotation.Y + ROTATE_SPEED * (float)gameTime.TotalSeconds,
				Transform3D.LocalRotation.Z);

			matte.Alpha = matte.Alpha.Lerp(selectable.Selected ? 0.8f : 0.0f,
				(float)gameTime.TotalSeconds * FADE_SPEED);
		}
	}
}

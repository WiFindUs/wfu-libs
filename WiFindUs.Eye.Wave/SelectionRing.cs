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
	public class SelectionRing : MapBehavior
	{
		protected readonly ISelectable selectable;
		private Entity[] points;
		private BasicMaterial matte;
		protected const float ROTATE_SPEED = 2f;
		private float fader = 0.0f;

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

		public static Entity Create(ISelectable selectable,
			float yOffset = 10.0f, uint points = 16, float radius = 10.0f, float thickness = 1f)
		{
			SelectionRing ring = new SelectionRing(selectable);
			ring.points = new Entity[points];
			ring.matte = new BasicMaterial(MapScene.WhiteTexture)
			{
				LayerType = typeof(Overlays),
				LightingEnabled = true,
				AmbientLightColor = Color.White * 0.75f,
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
			float secs = (float)gameTime.TotalSeconds;
			Transform3D.LocalRotation = new Vector3(
				Transform3D.LocalRotation.X,
				Transform3D.LocalRotation.Y + ROTATE_SPEED * secs,
				Transform3D.LocalRotation.Z);

			matte.Alpha = matte.Alpha.Lerp(selectable.Selected ? 0.6f : 0.0f, secs * FADE_SPEED);
			if (selectable.Selected)
				matte.DiffuseColor = Color.Wheat.Coserp(Color.Gold, fader += secs);
		}
	}
}

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

namespace WiFindUs.Eye.Wave.Markers
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
				AmbientLightColor = Color.White,
				DiffuseColor = Color.White,
				SpecularPower = 3
			};

			Entity entity = new Entity() { IsActive = false, IsVisible = false }
				.AddComponent(new Transform3D())
				.AddComponent(ring);

			
			for (uint i = 0; i < points; i++)
			{
				float pc = (float)i / (float)points;
				entity.AddChild
				(
					ring.points[i] = new Entity() { IsActive = false }
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

		protected override void Initialize()
		{
			base.Initialize();

			SelectedChanged(selectable);
			selectable.SelectedChanged += SelectedChanged;
		}

		protected override void Update(TimeSpan gameTime)
		{
			if (!IsOwnerVisible)
				return;

			Transform3D.LocalRotation = new Vector3(
				Transform3D.LocalRotation.X,
				Transform3D.LocalRotation.Y + ROTATE_SPEED * (float)gameTime.TotalSeconds,
				Transform3D.LocalRotation.Z);
		}

		protected virtual void SelectedChanged(ISelectable obj)
		{
			if (obj != selectable)
				return;
			IsOwnerActive = IsOwnerVisible = selectable.Selected;
		}
	}
}

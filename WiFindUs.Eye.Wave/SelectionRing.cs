﻿using System;
using WiFindUs.Eye.Wave.Layers;

namespace WiFindUs.Eye.Wave
{
    public class SelectionRing : MapBehavior
	{
		protected readonly ISelectable selectable;
		private Entity[] points;
		private BasicMaterial matte;

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
			selectable.SelectedChanged += selectable_SelectedChanged;
		}

		public static SelectionRing Create(ISelectable selectable,
			float yOffset = 10.0f, uint points = 16, float radius = 10.0f, float thickness = 1.5f)
		{
			SelectionRing ring = new SelectionRing(selectable);
			ring.points = new Entity[points];
			ring.matte = new BasicMaterial(MapScene.WhiteTexture)
			{
				LayerType = typeof(NonPremultipliedAlpha),
				LightingEnabled = true,
				AmbientLightColor = Color.White * 0.75f,
				Alpha = 0.6f,
				DiffuseColor = Color.Gold
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

			return ring;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			selectable_SelectedChanged(selectable);
		}

		protected override void Update(TimeSpan gameTime)
		{
			if (!IsOwnerVisible)
				return;

			float secs = (float)gameTime.TotalSeconds;
			Transform3D.LocalRotation = new Vector3(
				Transform3D.LocalRotation.X,
				Transform3D.LocalRotation.Y + ROTATE_SPEED * secs * 0.5f,
				Transform3D.LocalRotation.Z);
		}

		private void selectable_SelectedChanged(ISelectable obj)
		{
			if (selectable != obj)
				return;
			IsOwnerActive = IsOwnerVisible = selectable.Selected;
		}
	}
}

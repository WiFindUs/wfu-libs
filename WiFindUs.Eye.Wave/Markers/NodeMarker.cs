using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Materials;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Layers;

namespace WiFindUs.Eye.Wave.Markers
{
	public class NodeMarker : EntityMarker<Node>, ILinkableMarker
	{
		protected const float ROTATE_SPEED = 5f;
		private Transform3D ringTransform, coreTransform;
		private BasicMaterial spikeMat, ringMat, coreMat;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public Vector3 LinkPoint
		{
			get { return coreTransform.Position; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public static Entity Create(Node node)
		{
			NodeMarker marker = new NodeMarker(node);
			return new Entity()
				//base
				.AddComponent(new Transform3D())
				.AddComponent(marker)
				//spike
				.AddChild
				(
					new Entity("spike")
					.AddComponent(new Transform3D()
					{
						LocalPosition = new Vector3(0.0f, 7.0f, 0.0f),
						Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f)
					})
					.AddComponent(new MaterialsMap(marker.spikeMat = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(NonPremultipliedAlpha),
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						DiffuseColor = new Color(0, 175, 255),
						Alpha = 0.75f
					}))
					.AddComponent(Model.CreateCone(14f, 8f, 8))
					.AddComponent(new ModelRenderer())
					.AddComponent(marker.AddCollider(new BoxCollider()))
				)
				//core
				.AddChild
				(
					new Entity("core")
					.AddComponent(marker.coreTransform = new Transform3D()
					{
						LocalPosition = new Vector3(0.0f, 21.0f, 0.0f)
					})
					.AddComponent(new MaterialsMap(marker.coreMat = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(NonPremultipliedAlpha),
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						DiffuseColor = new Color(220, 220, 220),
						Alpha = 0.75f
					}))
					.AddComponent(Model.CreateSphere(4f, 4))
					.AddComponent(new ModelRenderer())
					//ring
					.AddChild
					(
						new Entity("ring")
						.AddComponent(marker.ringTransform = new Transform3D()
						{
							LocalPosition = new Vector3(0.0f, 0.0f, 0.0f),
							Rotation = new Vector3(90.0f.ToRadians(), 0f, 0f)
						})
						.AddComponent(new MaterialsMap(marker.ringMat = new BasicMaterial(MapScene.WhiteTexture)
						{
							LayerType = typeof(NonPremultipliedAlpha),
							LightingEnabled = true,
							AmbientLightColor = Color.White * 0.75f,
							DiffuseColor = new Color(0, 200, 255),
							Alpha = 0.25f
						}))
						.AddComponent(Model.CreateTorus(14f, 2, 12))
						.AddComponent(new ModelRenderer())
						.AddComponent(marker.AddCollider(new BoxCollider()))
					)
				)
				//selection
				.AddChild(SelectionRing.Create(node));
		}

		internal NodeMarker(Node n) : base(n) { }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!Owner.IsVisible)
				return;

			float secs = (float)gameTime.TotalSeconds;
			float targetAlpha = !Entity.Active ? 0.5f : (Entity.Selected || MapScene.Camera.TrackingTarget == this.Entity ? 1.0f : 0.75f);
			spikeMat.Alpha = spikeMat.Alpha.Lerp(targetAlpha, secs * FADE_SPEED);
			coreMat.Alpha = spikeMat.Alpha;
			ringMat.Alpha = spikeMat.Alpha * 0.5f;
			if (Entity.Active)
			{
				ringTransform.Rotation = new Vector3(
				   ringTransform.Rotation.X,
				   ringTransform.Rotation.Y + ROTATE_SPEED * secs,
				   ringTransform.Rotation.Z
					);
			}
		}
	}
}

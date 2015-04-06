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
		private Transform3D orbTransform;
		private BasicMaterial spikeMat, orbMat, coreMat;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public Vector3 LinkPoint
		{
			get
			{
				return new Vector3(
					this.Transform3D.Position.X,
					this.Transform3D.Position.Y + 21.0f * this.Transform3D.Scale.Y,
					this.Transform3D.Position.Z
					);
			}
		}

		protected override float RotationSpeed
		{
			get { return base.RotationSpeed * (Entity.Selected ? 10.0f : 5.0f); }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public NodeMarker(Node n)
			: base(n)
		{

		}

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
					.AddComponent(new Transform3D()
					{
						LocalPosition = new Vector3(0.0f, 21.0f, 0.0f)
					})
					.AddComponent(new MaterialsMap(marker.coreMat = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(Overlays),
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						DiffuseColor = new Color(180, 180, 180),
						Alpha = 0.5f
					}))
					.AddComponent(Model.CreateSphere(4f, 4))
					.AddComponent(new ModelRenderer())
					//orb
					.AddChild
					(
						new Entity("orb")
						.AddComponent(marker.orbTransform = new Transform3D()
						{
							LocalPosition = new Vector3(0.0f, 0.0f, 0.0f),
							Rotation = new Vector3(90.0f.ToRadians(), 0f, 0f)
						})
						.AddComponent(new MaterialsMap(marker.orbMat = new BasicMaterial(MapScene.WhiteTexture)
						{
							LayerType = typeof(Overlays),
							LightingEnabled = true,
							AmbientLightColor = Color.White * 0.75f,
							DiffuseColor = new Color(0, 200, 255),
							Alpha = 0.25f
						}))
						.AddComponent(Model.CreateTorus(14f, 2, 8))
						.AddComponent(new ModelRenderer())
						.AddComponent(marker.AddCollider(new BoxCollider()))
					)
				)
				//selection
				.AddChild(SelectionRing.Create(node));
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!Owner.IsVisible)
				return;

			spikeMat.Alpha = spikeMat.Alpha.Lerp(Entity.Selected ? 1.0f : 0.75f,
				(float)gameTime.TotalSeconds * FADE_SPEED);
			coreMat.Alpha = coreMat.Alpha.Lerp(Entity.Selected ? 1.0f : 0.5f,
				(float)gameTime.TotalSeconds * FADE_SPEED);
			orbMat.Alpha = orbMat.Alpha.Lerp(Entity.Selected ? 0.7f : 0.25f,
				(float)gameTime.TotalSeconds * FADE_SPEED);
			orbTransform.Rotation = new Vector3(
				orbTransform.Rotation.X,
				orbTransform.Rotation.Y + RotationSpeed * (float)gameTime.TotalSeconds,
				orbTransform.Rotation.Z);
		}
	}
}

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

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public Vector3 LinkPointPrimary
		{
			get
			{
				return new Vector3(
					this.Transform3D.Position.X,
					this.Transform3D.Position.Y + 28.0f * Scene.MarkerScale,
					this.Transform3D.Position.Z
					);
			}
		}

		public Vector3 LinkPointSecondary
		{
			get
			{
				return new Vector3(
					this.Transform3D.Position.X,
					this.Transform3D.Position.Y + 14.0f * Scene.MarkerScale,
					this.Transform3D.Position.Z
					);
			}
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
			return new Entity() { IsActive = false, IsVisible = false }
				//base
				.AddComponent(new Transform3D())
				.AddComponent(marker)
				//spike
				.AddChild
				(
					new Entity("spike") { IsActive = false }
					.AddComponent(new Transform3D()
					{
						Position = new Vector3(0.0f, 7.0f, 0.0f),
						Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f)
					})
					.AddComponent(new MaterialsMap(new BasicMaterial(new Color(0, 191, 255))
					{
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						SpecularPower = 2
					}))
					.AddComponent(Model.CreateCone(14f, 6f, 8))
					.AddComponent(new ModelRenderer())
					.AddComponent(marker.AddCollider(new BoxCollider()))
				)
				//orb
				.AddChild
				(
					new Entity("orb") { IsActive = false }
					.AddComponent(marker.orbTransform = new Transform3D()
					{
						Position = new Vector3(0.0f, 21.0f, 0.0f),
						Rotation = new Vector3(90.0f.ToRadians(), 0f, 0f)
					})
					.AddComponent(new MaterialsMap(new BasicMaterial(new Color(0, 191, 255), typeof(WireframeObjectsLayer))
					{
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						SpecularPower = 2
					}))
					.AddComponent(Model.CreateTorus(14f, 3, 8))
					.AddComponent(new ModelRenderer())
					.AddComponent(marker.AddCollider(new BoxCollider()))
				);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);

			if (orbTransform != null)
			{
				float rot = RotationSpeed * (float)gameTime.TotalSeconds * 0.5f;
				if (!rot.Tolerance(0.0f, 0.0001f))
				{
					orbTransform.Rotation = new Vector3(
						orbTransform.Rotation.X,
						orbTransform.Rotation.Y + rot,
						orbTransform.Rotation.Z);
				}
			}
		}
	}
}

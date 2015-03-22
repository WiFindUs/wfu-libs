using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave.Markers
{
	public class NodeMarker : EntityMarker<Node>
	{
		private BoxCollider collider;
		private Transform3D orbTransform;

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
			return new Entity() { IsActive = true, IsVisible = true }
				//base
				.AddComponent(new Transform3D())
				.AddComponent(marker)
				.AddComponent(new NodeLineBatch())
				//spike
				.AddChild
				(
					new Entity("spike") { IsActive = false }
					.AddComponent(new Transform3D()
					{
						Position = new Vector3(0.0f, 10.0f, 0.0f),
						Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f)
					})
					.AddComponent(new MaterialsMap(PlaceHolderMaterial))
					.AddComponent(Model.CreateCone(20f, 12f, 5))
					.AddComponent(new ModelRenderer())
					.AddComponent(marker.AddCollider(new BoxCollider()))
				)
				//orb
				.AddChild
				(
					new Entity("orb") { IsActive = false }
					.AddComponent(marker.orbTransform = new Transform3D()
					{
						Position = new Vector3(0.0f, 30.0f, 0.0f),
					})
					.AddComponent(new MaterialsMap(PlaceHolderMaterial))
					.AddComponent(Model.CreateSphere(20f, 3))
					.AddComponent(new ModelRenderer())
					.AddComponent(marker.AddCollider(new BoxCollider()))
				);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			if (orbTransform != null)
			{
				float rot = RotationSpeed * (float)gameTime.TotalSeconds;
				if (!rot.Tolerance(0.0f, 0.0001f))
				{
					orbTransform.Rotation = new Vector3(
						orbTransform.Rotation.X,
						orbTransform.Rotation.Y + rot,
						orbTransform.Rotation.Z + rot);
				}
			}
		}
	}
}

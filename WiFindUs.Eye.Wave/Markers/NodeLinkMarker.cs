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
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Layers;

namespace WiFindUs.Eye.Wave.Markers
{
	public class NodeLinkMarker : Marker
	{
		private NodeMarker nodeA, nodeB;
		private Transform3D linkTransform;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public override bool Selected
		{
			get { return nodeA.Selected || nodeB.Selected; }
			set
			{
				;
			}
		}

		public NodeMarker NodeA
		{
			get { return nodeA; }
		}

		public NodeMarker NodeB
		{
			get { return nodeB; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public NodeLinkMarker(NodeMarker nodeA, NodeMarker nodeB)
		{
			if (nodeA == null)
				throw new ArgumentNullException("nodeA", "nodeA cannot be null!");
			if (nodeB == null)
				throw new ArgumentNullException("nodeB", "nodeB cannot be null!");
			if (nodeA == nodeB)
				throw new ArgumentOutOfRangeException("nodeB", "nodeB cannot be the same as nodeA!");
			this.nodeA = nodeA;
			this.nodeB = nodeB;
		}

		public static Entity Create(NodeMarker nodeA, NodeMarker nodeB)
		{
			NodeLinkMarker marker = new NodeLinkMarker(nodeA, nodeB);
			return new Entity() { IsActive = true, IsVisible = true }
				//base
				.AddComponent(new Transform3D())
				.AddComponent(marker)
				//link
				.AddChild
				(
					new Entity("link") { IsActive = false }
					.AddComponent(marker.linkTransform = new Transform3D())
					.AddComponent(new MaterialsMap(new BasicMaterial(Color.Lime, typeof(WireframeObjectsLayer))
					{
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						SpecularPower = 2
					}))
					.AddComponent(Model.CreateCylinder(1f, 2f, 8))
					.AddComponent(new ModelRenderer())
				);
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public bool LinksNode(NodeMarker node)
		{
			return nodeA == node || nodeB == node;
		}

		public bool LinksNodes(NodeMarker nodeA, NodeMarker nodeB)
		{
			return (this.nodeA == nodeA && this.nodeB == nodeB)
				|| (this.nodeB == nodeA && this.nodeA == nodeB);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();

			UpdateMarkerState();

			nodeA.VisibleChanged += NodeMarkerVisibleChanged;
			nodeB.VisibleChanged += NodeMarkerVisibleChanged;
			nodeA.Entity.OnMeshPointChanged += NodeOnMeshPointChanged;
			nodeB.Entity.OnMeshPointChanged += NodeOnMeshPointChanged;
			nodeA.Entity.OnMeshPeersChanged += NodeOnMeshPeersChanged;
			nodeB.Entity.OnMeshPeersChanged += NodeOnMeshPeersChanged;
		}

		protected override void Update(TimeSpan gameTime)
		{
			Vector3 up = Vector3.Up;
			Vector3 start = nodeA.LinkPoint;
			Vector3 end = nodeB.LinkPoint;
			Vector3 direction = end - start;
			float scale = Scene.MarkerScale;
			float distance = direction.Length();
			direction.Normalize();

			Quaternion rot;
			Quaternion.CreateFromTwoVectors(ref up, ref direction, out rot);
			Vector3 rotation = Quaternion.ToEuler(rot);

			Transform3D.Position = start + (direction * (distance / 2.0f));
			linkTransform.Rotation = rotation;
			linkTransform.Scale = new Vector3(scale, distance, scale);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void NodeOnMeshPointChanged(Node obj)
		{
			UpdateMarkerState();
		}

		private void NodeMarkerVisibleChanged(EntityMarker<Node> obj)
		{
			UpdateMarkerState();
		}

		private void NodeOnMeshPeersChanged(Node obj)
		{
			UpdateMarkerState();
		}

		private void UpdateMarkerState()
		{
			Owner.IsActive = Owner.IsVisible =
			(
				nodeA.Owner.IsVisible
				&& nodeB.Owner.IsVisible
				&& nodeA.Entity.IsMeshPoint.GetValueOrDefault()
				&& nodeB.Entity.IsMeshPoint.GetValueOrDefault()
				&& nodeA.Entity.MeshPeerCount > 0
				&& nodeB.Entity.MeshPeerCount > 0
				&& nodeA.Entity.MeshPeers.Contains(nodeB.Entity)
				&& nodeB.Entity.MeshPeers.Contains(nodeA.Entity)
			);
		}
	}
}

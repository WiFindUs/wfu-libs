﻿using System;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave.Markers
{
    public class NodeLinkMarker : LinkMarker
	{
		private NodeMarker fromNode, toNode;
		private NodeLink link;
		private Color currentColor;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public NodeLink NodeLink
		{
			get { return link; }
			set
			{
				if (value == link)
					return;
				if (link != null)
				{
					link.OnNodeLinkActiveChanged -= NodeLinkChanged;
					link.OnNodeLinkSignalStrengthChanged -= NodeLinkChanged;
					link.OnNodeLinkSpeedChanged -= NodeLinkChanged;
				}

				link = value;

				if (link != null)
				{ 
					link.OnNodeLinkActiveChanged += NodeLinkChanged;
					link.OnNodeLinkSignalStrengthChanged += NodeLinkChanged;
					link.OnNodeLinkSpeedChanged += NodeLinkChanged;
				}
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		internal NodeLinkMarker(ILinkableMarker fromDevice, ILinkableMarker toNode)
			: base(fromDevice, toNode) { }

		public NodeLinkMarker() : base() { }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			Diameter = 5.0f;
			UpdateMarkerState();
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!Owner.IsVisible)
				return;

			float secs = (float)gameTime.TotalSeconds;
			Alpha = Alpha.Lerp(link.Start.Selected || link.End.Selected ? 0.35f : 0.0f, secs * FADE_SPEED).Clamp(0.0f, 1.0f);
			Colour = Color.Lerp(Colour,
				!toNode.Entity.Active || !fromNode.Entity.Active ? inactiveLinkColour : currentColor,
				secs * COLOUR_SPEED);
		}

		protected override void FromMarkerChanged(ILinkableMarker oldFromMarker)
		{
			if (oldFromMarker != null && oldFromMarker == fromNode)
			{
				fromNode.VisibleChanged -= NodeMarkerChanged;
				fromNode.Entity.OnNodeMeshPointChanged -= NodeChanged;
				fromNode.Entity.SelectedChanged -= EntitySelectedChanged;
			}

			fromNode = FromMarker as NodeMarker;

			if (fromNode != null)
			{
				fromNode.VisibleChanged += NodeMarkerChanged;
				fromNode.Entity.OnNodeMeshPointChanged += NodeChanged;
				fromNode.Entity.SelectedChanged += EntitySelectedChanged;
			}

			UpdateMarkerState();
		}

		protected override void ToMarkerChanged(ILinkableMarker oldToMarker)
		{
			if (oldToMarker != null && oldToMarker == toNode)
			{
				toNode.VisibleChanged -= NodeMarkerChanged;
				toNode.Entity.OnNodeMeshPointChanged -= NodeChanged;
				toNode.Entity.SelectedChanged -= EntitySelectedChanged;
			}

			toNode = ToMarker as NodeMarker;

			if (toNode != null)
			{
				toNode.VisibleChanged += NodeMarkerChanged;
				toNode.Entity.OnNodeMeshPointChanged += NodeChanged;
				toNode.Entity.SelectedChanged += EntitySelectedChanged;
			}

			UpdateMarkerState();
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void NodeChanged(Node node)
		{
			UpdateMarkerState();
		}

		private void NodeLinkChanged(NodeLink nodeLink)
		{
			UpdateMarkerState();
		}

		private void NodeMarkerChanged(EntityMarker<Node> nodeMarker)
		{
			UpdateMarkerState();
		}

		private void EntitySelectedChanged(ISelectable entity)
		{
			if (entity != link.Start && entity != link.End)
				return;
			UpdateMarkerState();
		}

		private void UpdateMarkerState()
		{
			if (Owner == null)
				return;
			bool newVisible = 
			(
				fromNode != null
				&& toNode != null
				&& toNode != fromNode
				&& fromNode.Entity != null
				&& toNode.Entity != null
				&& link != null
				&& link.Start != link.End
				&& (link.Start == fromNode.Entity || link.Start == toNode.Entity)
				&& (link.End == fromNode.Entity || link.End == toNode.Entity)
				&& link.Active
				&& fromNode.Owner.IsVisible
				&& toNode.Owner.IsVisible
				&& fromNode.Entity.MeshPoint.GetValueOrDefault()
				&& toNode.Entity.MeshPoint.GetValueOrDefault()
				&& (toNode.Selected || fromNode.Selected)
			);
			IsOwnerActive = newVisible;
			if (newVisible != IsOwnerVisible)
			{
				if (newVisible)
					Alpha = 0.0f;
				IsOwnerVisible = newVisible;
			}

			if (IsOwnerVisible)
			{
				if (!link.SignalStrength.HasValue || link.SignalStrength > -30)
					currentColor = Color.White;
				if (link.SignalStrength <= -30 && link.SignalStrength > -50)
					currentColor = Color.Lime;
				else if (link.SignalStrength > -65)
					currentColor = Color.LawnGreen;
				else if (link.SignalStrength > -70)
					currentColor = Color.Yellow;
				else if (link.SignalStrength > -80)
					currentColor = Color.Orange;
				else
					currentColor = Color.Red;
			}
		}


	}
}

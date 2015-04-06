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

namespace WiFindUs.Eye.Wave.Markers
{
	public class LinkMarker : Marker
	{
		private ILinkableMarker fromMarker, toMarker;
		private Transform3D linkTransform;
		private float diameter = 1.5f;
		private BasicMaterial matte;
		private bool toSecondary = false, fromSecondary = false;
		private Entity child;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public override bool Selected
		{
			get { return false; }
			set { ; }
		}

		public ILinkableMarker FromMarker
		{
			get { return fromMarker; }
			set
			{
				if (value == fromMarker)
					return;
				ILinkableMarker oldFromMarker = fromMarker;
				fromMarker = value;
				FromMarkerChanged(oldFromMarker);
			}
		}

		public ILinkableMarker ToMarker
		{
			get { return toMarker; }
			set
			{
				if (value == toMarker)
					return;
				ILinkableMarker oldToMarker = toMarker;
				toMarker = value;
				ToMarkerChanged(oldToMarker);
			}
		}

		public float Diameter
		{
			get { return diameter; }
			set { diameter = Math.Max(value, 0.1f); }
		}

		public Color Colour
		{
			get { return matte == null ? Color.Black : matte.DiffuseColor; }
			set { if (matte != null) matte.DiffuseColor = value; }
		}

		public float Alpha
		{
			get { return matte == null ? 1.0f : matte.Alpha; }
			set { if (matte != null) matte.Alpha = value.Clamp(0.0f,1.0f); }
		}

		public bool FromSecondaryPoint
		{
			get { return fromSecondary; }
			set { fromSecondary = value; }
		}

		public bool ToSecondaryPoint
		{
			get { return toSecondary; }
			set { toSecondary = value; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public LinkMarker(ILinkableMarker fromMarker, ILinkableMarker toMarker)
		{
			FromMarker = fromMarker;
			ToMarker = toMarker;
		}

		public static Entity Create(ILinkableMarker fromMarker, ILinkableMarker toMarker, Type linkMarkerType)
		{
			LinkMarker marker = (LinkMarker)linkMarkerType.GetConstructor(
				new Type[] { typeof(ILinkableMarker), typeof(ILinkableMarker) })
				.Invoke(new object[] { fromMarker, toMarker });
			return new Entity() { IsActive = true, IsVisible = true }
				//base
				.AddComponent(new Transform3D())
				.AddComponent(marker)
				//link
				.AddChild
				(
					marker.child = new Entity("link") { IsActive = false }
					.AddComponent(marker.linkTransform = new Transform3D())
					.AddComponent(new MaterialsMap(marker.matte = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(Overlays),
						LightingEnabled = true,
						AmbientLightColor = Color.White,
						DiffuseColor = new Color(200, 240, 255),
						Alpha = 1.0f
					}))
					.AddComponent(Model.CreateCylinder(1f, 1f, 6))
					.AddComponent(new ModelRenderer())
				);
		}

		public static Entity Create(ILinkableMarker fromMarker, ILinkableMarker markerB)
		{
			return Create(fromMarker, markerB, typeof(LinkMarker));
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public bool LinksMarker(ILinkableMarker marker)
		{
			if (marker == null)
				return false;
			return fromMarker == marker || toMarker == marker;
		}

		public bool LinksMarkers(ILinkableMarker fromMarker, ILinkableMarker toMarker)
		{
			if (fromMarker == null || toMarker == null)
				return false;

			return (this.fromMarker == fromMarker && this.toMarker == toMarker)
				|| (this.toMarker == fromMarker && this.fromMarker == toMarker);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			if (fromMarker == null || toMarker == null || fromMarker == toMarker || linkTransform == null)
				return;

			Vector3 up = Vector3.Up;
			Vector3 start = fromSecondary ? fromMarker.LinkPointSecondary : fromMarker.LinkPointPrimary;
			Vector3 end = toSecondary ? toMarker.LinkPointSecondary : toMarker.LinkPointPrimary;
			Vector3 direction = end - start;
			float scale = Scene.MarkerScale;
			float distance = direction.Length();
			direction.Normalize();

			Quaternion rot;
			Quaternion.CreateFromTwoVectors(ref up, ref direction, out rot);
			Vector3 rotation = Quaternion.ToEuler(rot);

			Transform3D.Position = start + (direction * (distance / 2.0f));
			linkTransform.Rotation = rotation;
			linkTransform.Scale = new Vector3(diameter * scale, distance, diameter * scale);
		}

		protected virtual void FromMarkerChanged(ILinkableMarker oldFromMarker)
		{
			;
		}

		protected virtual void ToMarkerChanged(ILinkableMarker oldToMarker)
		{
			;
		}
	}
}

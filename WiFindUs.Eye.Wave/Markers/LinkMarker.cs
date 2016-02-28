using System;
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
		private Entity child;
		protected static readonly Color inactiveLinkColour = new Color(0.5f, 0.5f, 0.5f);


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

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public static T Create<T>(ILinkableMarker fromMarker, ILinkableMarker toMarker)
			 where T : LinkMarker, new()
		{
			T marker = new T()
			{
				FromMarker = fromMarker,
				ToMarker = toMarker
			};
			Entity entity = new Entity()
				//base
				.AddComponent(new Transform3D())
				.AddComponent(marker)
				//link
				.AddChild
				(
					marker.child = new Entity("link")
					.AddComponent(marker.linkTransform = new Transform3D())
					.AddComponent(new MaterialsMap(marker.matte = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(Overlays),
						LightingEnabled = true,
						AmbientLightColor = Color.White,
						DiffuseColor = Color.Yellow,
						Alpha = 1.0f
					}))
					.AddComponent(Model.CreateCylinder(1f, 1f, 6))
					.AddComponent(new ModelRenderer())
				);

			return marker;
		}

		internal LinkMarker(ILinkableMarker fromMarker, ILinkableMarker toMarker)
		{
			FromMarker = fromMarker;
			ToMarker = toMarker;
		}

		public LinkMarker() { }

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
			Vector3 start = fromMarker.LinkPoint;
			Vector3 end = toMarker.LinkPoint;
			Vector3 direction = end - start;
			float scale = MapScene.MarkerScale;
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

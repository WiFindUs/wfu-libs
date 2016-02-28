using System;
using System.Collections.Generic;
using System.Linq;
using WiFindUs.Eye.Wave.Markers;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Layers;

namespace WiFindUs.Eye.Wave
{
    public class MapCursor : MapBehavior
	{
		protected const float BASE_SCALE = 1.5f;
		protected const float RING_DIAMETER = 16f;
		protected const float RAY_LENGTH = 50f;

		private Transform3D coreTransform, spikeTransform, ringTransform;
		private BasicMaterial matte;
		private Vector3 destination = Vector3.Zero;
		private float fader = 0.0f;
		internal TextBox PositionText;
		[RequiredComponent]
		public CylindricalCollider CylindricalCollider;
		internal readonly List<Marker> AllMarkersAtCursor = new List<Marker>();

		public Vector3 Normal
		{
			set
			{
				if (value.Equals(Vector3.Up))
				{
					ringTransform.Rotation = new Vector3(0f, 90.0f.ToRadians(), 0f);
				}
				else
				{
					Vector3 up = Vector3.Up;
					Quaternion rot;
					Quaternion.CreateFromTwoVectors(ref up, ref value, out rot);
					Vector3 euler;
					Quaternion.ToEuler(ref rot, out euler);
					ringTransform.Rotation = euler;
				}
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public static MapCursor Create()
		{
			MapCursor cursor = new MapCursor();
			cursor.matte = new BasicMaterial(MapScene.WhiteTexture)
			{
				LayerType = typeof(NonPremultipliedAlpha),
				LightingEnabled = true,
				AmbientLightColor = Color.White * 0.75f,
				DiffuseColor = Color.Gold
			};

			Entity entity = new Entity()
				.AddComponent(new Transform3D()
				{
					Position = Vector3.Zero
				})
				.AddComponent(cursor)
				//collider
				.AddComponent(new CylindricalCollider(40.0f, RING_DIAMETER / 2.0f, 20.0f))
				.AddComponent(new CylindricalColliderRenderer())
				//spike
				.AddChild
				(
					new Entity("spike")
					.AddComponent(cursor.spikeTransform = new Transform3D()
					{
						LocalPosition = new Vector3(0.0f, 30.0f, 0.0f),
						LocalRotation = new Vector3(180.0f.ToRadians(), 0f, 0f)
					})
					.AddComponent(new MaterialsMap(cursor.matte))
					.AddComponent(Model.CreateCone(4f, 2f, 4))
					.AddComponent(new ModelRenderer())
				)
				//core
				.AddChild
				(
					new Entity("core")
					.AddComponent(cursor.coreTransform = new Transform3D()
					{
						LocalPosition = new Vector3(0.0f, 1.0f, 0.0f)
					})
					.AddComponent(new MaterialsMap(cursor.matte))
					.AddComponent(Model.CreateSphere(1f, 3))
					.AddComponent(new ModelRenderer())
					//ring
					.AddChild
					(
						new Entity("ring")
						.AddComponent(cursor.ringTransform = new Transform3D()
						{
							LocalPosition = new Vector3(0.0f, 0.0f, 0.0f),
							LocalRotation = new Vector3(0f, 90.0f.ToRadians(), 0f)
						})
						.AddComponent(new MaterialsMap(cursor.matte))
						.AddComponent(Model.CreateTorus(RING_DIAMETER, 0.5f, 24))
						.AddComponent(new ModelRenderer())
					)
				);

			//position text
			cursor.UIEntity = (cursor.PositionText = new TextBox("position")
			{
				HorizontalAlignment = WaveEngine.Framework.UI.HorizontalAlignment.Left,
				VerticalAlignment = WaveEngine.Framework.UI.VerticalAlignment.Bottom,
				TextWrapping = false,
				IsBorder = false,
				Text = "",
				IsReadOnly = true,
				Foreground = Themes.Theme.Current.Foreground.Light.Colour.Wave(),
				Background = Themes.Theme.Current.Background.Dark.Colour.Wave(200),
				TextAlignment = TextAlignment.Left,
				Width = 290.0f,
				Height = 24.0f,
				IsVisible = false
			}).Entity;
			cursor.UIEntity.FindComponent<Transform2D>().LocalScale = new Vector2(UI_SCALE);

			return cursor;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public IEnumerable<T> MarkersAtCursor<T>() where T : Marker
		{
			if (CylindricalCollider == null || MapScene.Markers.Count == 0)
				return new T[0];

			return MapScene.Markers.Where(m => m.Transform3D != null && m.CylindricalCollider != null && m.IsOwnerVisible)
				.OfType<T>()
				.Where(m => m.CylindricalCollider.Intersects(CylindricalCollider))
				.OrderBy(m => { return Vector3.DistanceSquared(m.CylindricalCollider.Position, CylindricalCollider.Position); });
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			bool visible = MapScene.Input.MouseInsideHost && !MapScene.Input.MousePanning;
			float secs = (float)gameTime.TotalSeconds;

			//get markers under cursor
			AllMarkersAtCursor.Clear();
			if (visible)
				AllMarkersAtCursor.AddRange(MarkersAtCursor<Marker>());
			
			//alpha
			matte.Alpha = matte.Alpha.Lerp(visible ? 0.8f : 0.0f, secs * FADE_SPEED).Clamp(0.0f, 1.0f);
			PositionText.IsVisible = visible;
			
			//scale
			float scale = MapScene.MarkerScale * BASE_SCALE;
			Transform3D.Scale = Vector3.Lerp(Transform3D.Scale, new Vector3(scale, scale, scale),
				secs * SCALE_SPEED);

			//spike movement
			spikeTransform.Rotation = new Vector3(
				spikeTransform.Rotation.X,
				spikeTransform.Rotation.Y + MOVE_SPEED * secs,
				spikeTransform.Rotation.Z);
			spikeTransform.LocalPosition = new Vector3(
				spikeTransform.LocalPosition.X,
				(25.0f).Coserp(35.0f, fader += (secs * MOVE_SPEED * 0.25f)),
				spikeTransform.LocalPosition.Z);
		}
	}
}

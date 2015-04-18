using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Materials;
using WiFindUs.Eye.Wave.Markers;
using WiFindUs.Extensions;
using WaveEngine.Components.Graphics3D;
using WiFindUs.Eye.Wave.Layers;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework.Physics3D;

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
		private Ray ray;
		private float[] xOffsets = new float[12], zOffsets = new float[12];
		private float fader = 0.0f;

		public Vector3 Normal
		{
			set
			{
				Vector3 up = Vector3.Up;
				Quaternion rot;
				Quaternion.CreateFromTwoVectors(ref up, ref value, out rot);
				Vector3 euler;
				Quaternion.ToEuler(ref rot, out euler);

				ringTransform.Rotation = euler;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public static Entity Create()
		{
			MapCursor cursor = new MapCursor();
			cursor.matte = new BasicMaterial(MapScene.WhiteTexture)
			{
				LayerType = typeof(Overlays),
				LightingEnabled = true,
				AmbientLightColor = Color.White * 0.75f,
				DiffuseColor = Color.Gold
			};

			cursor.xOffsets[0] = 0.0f;
			cursor.zOffsets[0] = 0.0f;
			for (int i = 0; i < cursor.xOffsets.Length - 1; i++)
			{
				double pc = Math.PI * 2.0 * (i / (double)(cursor.xOffsets.Length - 1));
				cursor.xOffsets[i + 1] = (float)Math.Cos(pc) * (RING_DIAMETER/2.0f);
				cursor.zOffsets[i + 1] = (float)Math.Sin(pc) * (RING_DIAMETER / 2.0f);
			}

			return new Entity()
				.AddComponent(new Transform3D()
				{
					Position = Vector3.Zero
				})
				.AddComponent(cursor)
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
						LocalPosition = new Vector3(0.0f, 0.5f, 0.0f)
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
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public T[] MarkersAtCursor<T>(bool visibleOnly = true) where T : Marker
		{
			if (MapScene.AllMarkers.Count == 0)
				return new T[0];

			List<T> markers = new List<T>();

			//check nodes
			foreach (Marker marker in MapScene.AllMarkers)
			{
				if (marker.Transform3D == null || (!marker.Owner.IsVisible && visibleOnly))
					continue;

				T typedMarker = marker as T;
				if (typedMarker == null || markers.Contains(typedMarker))
					continue;

				float len = RAY_LENGTH * Transform3D.Scale.Y;
				for (int r = 1; r <= 2; r++)
				{
					bool found = false;
					for (int i = (r == 2 ? 1 : 0); i < xOffsets.Length; i++)
					{
						ConfigureCursorRay(xOffsets[i] / (float)r, zOffsets[i] / (float)r);
						float? val;
						if ((val = marker.Intersects(ref ray)).HasValue && val.Value >= 0.0f && val.Value <= len)
						{
							markers.Add(typedMarker);
							found = true;
							break;
						}
					}
					if (found)
						break;
				}

			}

			//sort based on distance
			return markers.OrderBy(o =>
			{
				return Vector3.DistanceSquared(o.Transform3D.Position, ray.Position);
			}).ToArray();
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			ray = new Ray();
			Vector3 dir = new Vector3(0.001f, -1.0f, 0.001f);
			dir.Normalize();
			ray.Direction = dir;
		}

		protected override void Update(TimeSpan gameTime)
		{
			float secs = (float)gameTime.TotalSeconds;

			//alpha
			matte.Alpha = matte.Alpha.Lerp(MapScene.Input.MouseInsideHost
				&& !MapScene.Input.MousePanning
				? 0.8f : 0.0f, secs * FADE_SPEED);
			
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

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////


		private void ConfigureCursorRay(float x = 0.0f, float z = 0.0f)
		{
			ray.Position = new Vector3(Transform3D.Position.X + x * Transform3D.Scale.X,
				Transform3D.Position.Y + RAY_LENGTH * Transform3D.Scale.Y,
				Transform3D.Position.Z + z * Transform3D.Scale.Z);
		}
	}
}

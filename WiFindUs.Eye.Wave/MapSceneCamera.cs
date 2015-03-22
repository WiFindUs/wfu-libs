using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Graphics;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Markers;

namespace WiFindUs.Eye.Wave
{
	public class MapSceneCamera : MapSceneEntityBehavior
	{
		private const float MIN_ZOOM = 100.0f;
		private const float MAX_ZOOM = 2000.0f;
#if DEBUG
		private const float MIN_ANGLE = (float)(Math.PI / 50.0);
#else
		private const float MIN_ANGLE = (float)(Math.PI / 10.0);
#endif
		private const float MAX_ANGLE = (float)(Math.PI / 2.0001);
		private const float MOVE_SPEED = 15f;
		private const float ROTATE_SPEED = MOVE_SPEED;

		private float zoom = 1.0f;
		private float tilt = 0.0f;
		float angle, distance;
		Vector3 direction, destination, targetVector;
		private ILocation target;
		private bool interpolate = true;
		private Camera3D camera;
		private Ray ray;

		public event Action<MapSceneCamera> Updated;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public float Zoom
		{
			get { return zoom; }
			set
			{
				float z = value.Clamp(0.0f, 1.0f);
				if (z.Tolerance(zoom, 0.001f))
					return;
				zoom = z;
				UpdateMetrics();
			}
		}

		public float Tilt
		{
			get { return tilt; }
			set
			{
				float t = value.Clamp(0.0f, 1.0f);
				if (t.Tolerance(tilt, 0.001f))
					return;
				tilt = t;
				UpdateMetrics();
			}
		}

		public ILocation Target
		{
			get { return target; }
			set
			{
				ILocation loc = value ?? Scene.CenterLocation;
				if (WiFindUs.Eye.Location.Equals(loc, target))
					return;
				target = loc;
				targetVector = Scene.LocationToVector(target);
				UpdateMetrics();
			}
		}

		public Vector3 TargetVector
		{
			get { return targetVector; }
		}

		public ILocation Location
		{
			get { return Scene.VectorToLocation(camera.Position); }
		}

		public ILocation FrustumNorthWest
		{
			get { return LocationFromScreenRay(0, 0); }
		}

		public ILocation FrustumNorthEast
		{
			get { return LocationFromScreenRay(Scene.HostApplication.Width, 0); }
		}

		public ILocation FrustumSouthEast
		{
			get { return LocationFromScreenRay(Scene.HostApplication.Width, Scene.HostApplication.Height); }
		}

		public ILocation FrustumSouthWest
		{
			get { return LocationFromScreenRay(0, Scene.HostApplication.Height); }
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public ILocation LocationFromScreenRay(int x, int y)
		{
			//set up ray
			ConfigureScreenRay(x, y);

			//test for collision with ground plane
			float? result = Scene.GroundPlane.Intersects(ref ray);
			if (!result.HasValue)
				return null;

			//return result
			return Scene.VectorToLocation(ray.Position + ray.Direction * result.Value);
		}

		public Marker[] MarkersFromScreenRay(int x, int y)
		{
			if (Scene.DeviceMarkers.Count == 0 && Scene.NodeMarkers.Count == 0)
				return new Marker[0];

			List<Marker> markers = new List<Marker>();

			//set up ray
			ConfigureScreenRay(x, y);

			//check nodes
			foreach (Marker marker in Scene.AllMarkers)
				if (marker.Transform3D != null && marker.Intersects(ref ray).HasValue)
					markers.Add(marker);

			//sort based on distance
			return markers.OrderBy(o =>
			{
				return Vector3.DistanceSquared(o.Transform3D.Position, ray.Position);
			}).ToArray();
		}

		public void SnapToDestination()
		{
			interpolate = false;
			UpdateCamera(TimeSpan.Zero);
			interpolate = true;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			ray = new Ray();
			camera = Owner.FindComponent<Camera3D>();

			UpdateMetrics();
		}

		protected override void Update(TimeSpan gameTime)
		{
			UpdateCamera(gameTime);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void UpdateMetrics()
		{
			distance = MIN_ZOOM + ((MAX_ZOOM - MIN_ZOOM) * zoom);
			angle = MIN_ANGLE + ((MAX_ANGLE - MIN_ANGLE) * tilt);
			direction = new Vector3(0f, (float)Math.Sin(angle), (float)Math.Cos(angle));
			direction.Normalize();
			destination = targetVector + (direction * distance);
		}

		private void UpdateCamera(TimeSpan gameTime)
		{
			if (interpolate)
			{
				camera.Position = Vector3.Lerp(camera.Position, destination,
					(float)gameTime.TotalSeconds * MOVE_SPEED);
				camera.LookAt = Vector3.Lerp(camera.LookAt, targetVector,
					(float)gameTime.TotalSeconds * ROTATE_SPEED);
			}
			else
			{
				camera.Position = destination;
				camera.LookAt = targetVector;
			}
			Scene.VisibleLayer = (uint)((1.0f - zoom) * (float)Scene.LayerCount);
			if (Updated != null)
				Updated(this);
		}

		private void ConfigureScreenRay(int x, int y)
		{
			//convert screen to world
			Vector3 screenCoords = new Vector3(x, y, 0.0f);
			Vector3 screenCoordsFar = new Vector3(x, y, 1.0f);
			screenCoords = camera.Unproject(ref screenCoords);
			screenCoordsFar = camera.Unproject(ref screenCoordsFar);

			//update ray
			Vector3 rayDirection = screenCoordsFar - screenCoords;
			rayDirection.Normalize();
			ray.Direction = rayDirection;
			ray.Position = screenCoords;
		}
	}
}

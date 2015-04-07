﻿using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Graphics;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Markers;
using WaveEngine.Common.System;
using WaveEngine.Framework.Services;

namespace WiFindUs.Eye.Wave
{
	public class MapCamera : MapBehavior
	{
		private const float MIN_ZOOM = 100.0f;
		private const float MAX_ZOOM = 2000.0f;
		private const float MIN_ANGLE = (float)(Math.PI / 10.0);
		private const float MAX_ANGLE = (float)(Math.PI / 2.0001);
		private const float MOVE_SPEED = 15f;
		private const float ROTATE_SPEED = MOVE_SPEED;
		private const float MIN_MARKER_SCALE = 0.5f;
		private const float MAX_MARKER_SCALE = 4.0f;

		private float zoom = 1.0f;
		private float tilt = 0.0f;
		float angle, distance;
		Vector3 direction, destination, targetVector;
		private ILocation target;
		private Camera3D camera;
		private Ray ray;

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
				ILocation loc = MapScene.BaseTile.Region.Clamp(value ?? MapScene.CenterLocation);
				if (WiFindUs.Eye.Location.Equals(loc, target))
					return;
				target = loc;
				targetVector = MapScene.LocationToVector(target);
				UpdateMetrics();
			}
		}

		public Vector3 TargetVector
		{
			get { return targetVector; }
		}

		public ILocation Location
		{
			get { return MapScene.VectorToLocation(camera.Position); }
		}

		public ILocation FrustumNorthWest
		{
			get { return LocationFromScreenRay(0, 0); }
		}

		public ILocation FrustumNorthEast
		{
			get { return LocationFromScreenRay(MapScene.BackBufferWidth, 0); }
		}

		public ILocation FrustumSouthEast
		{
			get { return LocationFromScreenRay(MapScene.BackBufferWidth, MapScene.BackBufferHeight); }
		}

		public ILocation FrustumSouthWest
		{
			get { return LocationFromScreenRay(0, MapScene.BackBufferHeight); }
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public Vector3? VectorFromScreenRay(int x, int y)
		{
			//set up ray
			ConfigureScreenRay(x, y);

			//test for collision with ground plane
			float? result = MapScene.GroundPlane.Intersects(ref ray);
			if (!result.HasValue)
				return null;

			//return result
			return ray.Position + ray.Direction * result.Value;
		}

		public ILocation LocationFromScreenRay(int x, int y)
		{
			Vector3? vec = VectorFromScreenRay(x, y);
			return vec == null ? null : MapScene.VectorToLocation(vec.Value);
		}

		public T[] MarkersFromScreenRay<T>(int x, int y, bool visibleOnly = true) where T : Marker
		{
			if (MapScene.AllMarkers.Count == 0)
				return new T[0];

			List<T> markers = new List<T>();

			//set up ray
			ConfigureScreenRay(x, y);

			//check nodes
			foreach (Marker marker in MapScene.AllMarkers)
			{
				if (marker.Transform3D == null || (!marker.Owner.IsVisible && visibleOnly))
					continue;

				T typedMarker = marker as T;
				if (typedMarker == null || markers.Contains(typedMarker))
					continue;

				float? val;
				if ((val = marker.Intersects(ref ray)).HasValue && val.Value >= 0.0f)
					markers.Add(typedMarker);
			}

			//sort based on distance
			return markers.OrderBy(o =>
			{
				return Vector3.DistanceSquared(o.Transform3D.Position, ray.Position);
			}).ToArray();
		}

		public void SnapToDestination()
		{
			camera.Position = destination;
			camera.LookAt = targetVector;
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
			//position
			camera.Position = Vector3.Lerp(camera.Position, destination,
				(float)gameTime.TotalSeconds * MOVE_SPEED);

			//look at target
			camera.LookAt = Vector3.Lerp(camera.LookAt, targetVector,
				(float)gameTime.TotalSeconds * ROTATE_SPEED);

			//tile layer
			MapScene.VisibleLayer = (uint)((1.0f - zoom) * (float)MapScene.LayerCount);

			//marker scale
			MapScene.MarkerScale = MIN_MARKER_SCALE + (MAX_MARKER_SCALE - MIN_MARKER_SCALE) * zoom;
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
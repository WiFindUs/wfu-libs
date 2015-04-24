using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Graphics;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Markers;
using WaveEngine.Common.System;
using WaveEngine.Framework.Services;
using WaveEngine.Components.UI;

namespace WiFindUs.Eye.Wave
{
	public class MapCamera : MapBehavior
	{
		private const float MIN_ZOOM = 128.0f;
		internal const float MAX_ZOOM = Terrain.SIZE * 1.5f;
		private const float MIN_ANGLE = (float)(Math.PI / 10.0);
		private const float MAX_ANGLE = (float)(Math.PI / 2.0001);
		private const float MIN_MARKER_SCALE = 0.5f;
		private const float MAX_MARKER_SCALE = 4.0f;

		private float zoom = 1.0f;
		private float tilt = 0.0f;
		float angle, distance;
		Vector3 direction, destination, targetVector;
		private ILocation target;
		private Camera3D camera;
		private Ray ray;
		private IEntityMarker trackingEntity = null;
		private TextBox trackingText;

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
				ILocation loc = MapScene.Terrain.Source.Clamp(value ?? MapScene.Terrain.Source.Center);
				if (WiFindUs.Location.Equals(loc, target))
					return;
				target = loc;
				targetVector = MapScene.LocationToVector(target);
				UpdateMetrics();
			}
		}

		public IEntityMarker TrackingEntity
		{
			get { return trackingEntity; }
			set
			{
				if (value == trackingEntity)
					return;
				if (trackingEntity != null)
					trackingEntity.Locatable.LocationChanged -= trackingTarget_LocationChanged;
				trackingEntity = value;
				if (trackingEntity != null)
				{
					trackingTarget_LocationChanged(trackingEntity.Locatable);
					trackingEntity.Locatable.LocationChanged += trackingTarget_LocationChanged;
					Tilt = 0.1f;
					Zoom = Math.Min(Zoom, 0.1f);
					trackingText.Text = String.Format("Tracking {0}", trackingEntity.ToString());
					trackingText.IsVisible = true;
				}
				else
					trackingText.IsVisible = false;
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

		internal bool LerpMovement
		{
			get { return trackingEntity == null; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		internal MapCamera()
		{
			UIEntity = (trackingText = new TextBox("tracking")
			{
				HorizontalAlignment = WaveEngine.Framework.UI.HorizontalAlignment.Center,
				VerticalAlignment = WaveEngine.Framework.UI.VerticalAlignment.Bottom,
				TextWrapping = false,
				IsBorder = false,
				Text = "",
				IsReadOnly = true,
				Foreground = Themes.Theme.Current.Foreground.Light.Colour.Wave(),
				Background = Themes.Theme.Current.Background.Dark.Colour.Wave(200),
				TextAlignment = TextAlignment.Center,
				Width = 200.0f,
				Height = 24.0f
			}).Entity;
			UIEntity.FindComponent<Transform2D>().LocalScale = new Vector2(UI_SCALE); 
		}


		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public Vector3? VectorFromScreenRay(int x, int y, out Vector3 normal)
		{
			//set up ray
			ConfigureScreenRay(x, y);
			
			//test for collision with terrain, then ground plane
			float? result = MapScene.Terrain.Intersects(ref ray, out normal);
			if (!result.HasValue)
			{
				Plane gp = MapScene.GroundPlane;
				ray.Intersects(ref gp, out result);
				if (result.HasValue)
					normal = Vector3.Up;
			}
			
			//no hit
			if (!result.HasValue)
			{
				normal = Vector3.Zero;
				return null;
			}

			//return result
			return ray.Position + ray.Direction * result.Value;
		}

		public ILocation LocationFromScreenRay(int x, int y)
		{
			Vector3 fn;
			Vector3? vec = VectorFromScreenRay(x, y, out fn);
			return vec == null ? null : MapScene.VectorToLocation(vec.Value);
		}

		public void TrackSelectedMarkers()
		{
			//get all active, selected entity markers which have a valid location
			IEntityMarker[] selectedMarkers = MapScene.Markers
				.OfType<IEntityMarker>()
				.Where(mk => (mk.EntityActive || mk.EntityWaiting)
					&& mk.EntitySelected
					&& mk.Locatable.Location.HasLatLong)
				.ToArray();

			//if none, clear selection
			if (selectedMarkers == null || selectedMarkers.Length == 0)
			{
				TrackingEntity = null;
				return;
			}

			//find the current tracked object in the selection
			int index = -1;
			if (TrackingEntity != null)
			{
				for (int i = 0; i < selectedMarkers.Length; i++)
				{
					if (TrackingEntity == selectedMarkers[i])
					{
						index = i;
						break;
					}
				}
			}

			//if we have no current tracked object, or there was only selected object found,
			//or the currently tracked object was not part of the selection,
			//set the first selected object as the tracked object
			if (TrackingEntity == null || selectedMarkers.Length == 1 || index == -1)
			{
				TrackingEntity = selectedMarkers[0];
				return;
			}

			//otherwise, we're a member of the set and progressing through a loop,
			//so advance to the next selected object
			TrackingEntity = selectedMarkers[(index + 1) % selectedMarkers.Length];
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
			float secs = (float)gameTime.TotalSeconds;
			
			//position and lookAt
			if (LerpMovement)
			{
				camera.Position = Vector3.Lerp(camera.Position, destination, secs * CAMERA_SPEED);
				camera.LookAt = Vector3.Lerp(camera.LookAt, targetVector, secs * CAMERA_SPEED);
			}
			else
			{
				camera.Position = destination;
				camera.LookAt = targetVector;
			}

			//marker scale
			MapScene.MarkerScale = MIN_MARKER_SCALE + (MAX_MARKER_SCALE - MIN_MARKER_SCALE) * zoom;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void trackingTarget_LocationChanged(ILocatable target)
		{
			Target = target.Location;
		}

		private void UpdateMetrics()
		{
			distance = MIN_ZOOM + ((MAX_ZOOM - MIN_ZOOM) * zoom);
			angle = MIN_ANGLE + ((MAX_ANGLE - MIN_ANGLE) * tilt);
			direction = new Vector3(0f, (float)Math.Sin(angle), (float)Math.Cos(angle));
			direction.Normalize();
			destination = targetVector + (direction * distance);
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

using System;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave.Markers
{
    public abstract class EntityMarker<T> : Marker, ISelectableProxy, ILocatableProxy, IUpdateableProxy, IEntityMarker
		where T : class, ILocatable, ISelectable, IUpdateable
	{
		public event Action<EntityMarker<T>> VisibleChanged;

		internal readonly T Entity;
		private ILocation lastLocation;
		private Vector3 destination;
		internal readonly StackPanel UIPanel;
		internal readonly TextBox UIText, UISubtext;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public ILocatable Locatable
		{
			get { return Entity; }
		}

		public IUpdateable Updateable
		{
			get { return Entity; }
		}

		public ISelectable Selectable
		{
			get { return Entity; }
		}

		public override bool Selected
		{
			get { return Entity.Selected; }
			set { Entity.Selected = value; }
		}

		protected override float ScaleMultiplier
		{
			get { return Entity.Selected || CursorOver ? 1.25f : 1.0f; }
		}

		public bool CameraTracking
		{
			get
			{
				return MapScene.Camera != null
					&& MapScene.Camera.TrackingEntity == this;
			}
			set
			{
				if (MapScene.Camera == null)
					return;
				
				if (value && !CameraTracking)
					MapScene.Camera.TrackingEntity = this;
				else if (!value && CameraTracking)
					MapScene.Camera.TrackingEntity = null;
			}
		}

		public bool EntityActive
		{
			get { return Entity.Active; }
		}

		public bool EntityWaiting
		{
			get { return !Entity.Active && Entity.LastUpdatedSecondsAgo < (Entity.TimeoutLength * 5); }
		}

		public bool EntitySelected
		{
			get { return Entity.Selected; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		internal EntityMarker(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException("entity", "Entity cannot be null!");
			Entity = entity;

			//ui
			UIEntity = (UIPanel = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				IsBorder = false,
				IsVisible = false,
				Width = 100.0f,
				Height = 40.0f,
			}).Entity;
			UIPanel.Add(UIText = new TextBox()
			{
				Text = "",
				Foreground = Themes.Theme.Current.Foreground.Light.Colour.Wave(),
				Width = UIPanel.Width,
				Height = 22.0f,
				TextAlignment = TextAlignment.Center,
				TextWrapping = false,
				IsBorder = false,
				Background = Themes.Theme.Current.Background.Dark.Colour.Wave(),
				IsReadOnly = true,
				IsVisible = false
			});
			UIPanel.Add(UISubtext = new TextBox()
			{
				Text = "",
				Foreground = Themes.Theme.Current.Foreground.Light.Colour.Wave(),
				Width = UIPanel.Width * (1.0f / SUBTEXT_SCALE),
				Height = 16.0f * (1.0f / SUBTEXT_SCALE),
				TextAlignment = TextAlignment.Center,
				TextWrapping = false,
				IsBorder = false,
				Background = Themes.Theme.Current.Background.Dark.Colour.Wave(),
				IsReadOnly = true,
				Margin = new WaveEngine.Framework.UI.Thickness(0.0f,-4.0f,0.0f,0.0f),
				IsVisible = false
			});
			UIEntity.WithComponent<Transform2D>(t => t.LocalScale = new Vector2(UI_SCALE));
			UISubtext.Entity.WithComponent<Transform2D>(t => t.LocalScale = new Vector2(SUBTEXT_SCALE));
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return Entity.ToString();
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();

			LocationChanged(Entity);
			UpdateMarkerState();

			Entity.SelectedChanged += SelectedChanged;
			Entity.LocationChanged += LocationChanged;
			Entity.ActiveChanged += ActiveChanged;
			Entity.Updated += Updated;
			MapScene.Terrain.Plane.BoundingBoxUpdated += Plane_BoundingBoxUpdated;
			MapScene.Terrain.Source.ElevationStateChanged += Source_ElevationStateChanged;
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!IsOwnerVisible)
				return;

			float secs = (float)gameTime.TotalSeconds;

			//position
			Transform3D.Position = Vector3.Lerp(Transform3D.Position, destination, secs * MOVE_SPEED);

			//ui
			UIPanel.IsVisible = UIText.IsVisible = UISubtext.IsVisible = EntitySelected || CursorOver;
			if (UITransform != null && UIPanel.IsVisible)
				UITransform.Position = ScreenPosition.Add(UIPanel.Width * UI_SCALE * -0.5f, 3.0f);
		}

		protected void Source_ElevationStateChanged(Map source)
		{
			UpdateDestination();
		}

		protected void Plane_BoundingBoxUpdated(PolyPlane plane)
		{
			UpdateDestination();
		}

		protected virtual void LocationChanged(ILocatable obj)
		{
			UpdateDestination();
			if (lastLocation == null || WiFindUs.Location.Distance(Entity.Location, lastLocation) > 50.0)
				Transform3D.Position = destination;
			lastLocation = new Location(Entity.Location);
		}

		protected virtual void SelectedChanged(ISelectable obj)
		{
			UpdateMarkerState();
			UpdateUI();
		}

		protected virtual void ActiveChanged(IUpdateable obj)
		{
			UpdateMarkerState();
			UpdateUI();
		}

		protected virtual void Updated(IUpdateable obj)
		{
			UpdateMarkerState();
		}

		protected override void CursorOverChanged(bool cursorOver)
		{
			base.CursorOverChanged(cursorOver);
			UpdateUI();
		}

		protected virtual void UpdateMarkerState()
		{
			bool active = MapScene.Terrain != null
				&& MapScene.Terrain.Source != null
				&& Entity.Location.HasLatLong
				&& MapScene.Terrain.Source.Contains(Entity.Location)
				&& (EntityActive || EntityWaiting);

			IsOwnerActive = active;
			if (IsOwnerVisible != active)
			{
				IsOwnerVisible = active;
				if (VisibleChanged != null)
					VisibleChanged(this);
			}
			if (!IsOwnerActive && CameraTracking)
				CameraTracking = false;
		}

		protected virtual void UpdateUI()
		{

		}

		private void UpdateDestination()
		{
			if (!Entity.Location.HasLatLong || !MapScene.Terrain.Source.Contains(Entity.Location))
				return;
			destination = MapScene.LocationToVector(Entity.Location);
		}
	}
}

using System;
using System.Collections.Generic;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Materials;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave.Markers
{
	public abstract class EntityMarker<T> : Marker, ISelectableProxy, ILocatableProxy, IUpdateableProxy, IEntityMarker
		where T : class, ILocatable, ISelectable, IUpdateable
	{
		public event Action<EntityMarker<T>> VisibleChanged;
		
		protected readonly T entity;
		protected const float MOVE_SPEED = 10f;
		private ILocation lastLocation;
		private Vector3 destination;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public T Entity
		{
			get { return entity; }
		}

		public ILocatable Locatable
		{
			get { return entity; }
		}

		public IUpdateable Updateable
		{
			get { return entity; }
		}

		public ISelectable Selectable
		{
			get { return entity; }
		}

		public override bool Selected
		{
			get { return entity.Selected; }
			set { entity.Selected = value; }
		}

		protected override float ScaleMultiplier
		{
			get { return entity.Selected ? 1.25f : 1.0f; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		internal EntityMarker(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException("entity", "Entity cannot be null!");
			this.entity = entity;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();

			LocationChanged(entity);
			UpdateMarkerState();

			entity.SelectedChanged += SelectedChanged;
			entity.LocationChanged += LocationChanged;
			entity.ActiveChanged += ActiveChanged;
			entity.Updated += Updated;
			MapScene.BaseTile.Source.RegionChanged += Source_RegionChanged;
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!Owner.IsVisible)
				return;
			
			Transform3D.Position = Vector3.Lerp(Transform3D.Position, destination,
				(float)gameTime.TotalSeconds * MOVE_SPEED);
		}

		protected virtual void Source_RegionChanged(Tile obj)
		{
			LocationChanged(entity);
		}

		protected virtual void LocationChanged(ILocatable obj)
		{
			if (!entity.Location.HasLatLong)
				return;

			destination = MapScene.LocationToVector(entity.Location);
			if (lastLocation == null || WiFindUs.Eye.Location.Distance(entity.Location, lastLocation) > 50.0)
				Transform3D.Position = destination;
			lastLocation = new Location(entity.Location);
		}

		protected virtual void SelectedChanged(ISelectable obj)
		{
			if (obj != entity)
				return;
			UpdateMarkerState();
		}

		protected virtual void ActiveChanged(IUpdateable obj)
		{
			UpdateMarkerState();
		}

		protected virtual void Updated(IUpdateable obj)
		{
			UpdateMarkerState();
		}

		protected virtual void UpdateMarkerState()
		{
			bool active = MapScene.BaseTile != null
				&& MapScene.BaseTile.Source != null
				&& entity.Location.HasLatLong
				&& (entity.Active || entity.LastUpdatedSecondsAgo < (entity.TimeoutLength * 5));

			IsOwnerActive = active;
			if (IsOwnerVisible != active)
			{
				IsOwnerVisible = active;
				if (VisibleChanged != null)
					VisibleChanged(this);
			}
		}
	}
}

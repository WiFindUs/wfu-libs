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
	public abstract class EntityMarker<T> : Marker, ISelectableProxy where T : class, ILocatable, ISelectable, IUpdateable
	{
		public event Action<EntityMarker<T>> VisibleChanged;
		
		protected readonly T entity;
		protected const float MOVE_SPEED = 10f;
		
		private ILocation lastLocation = null;
		private Vector3 destination;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public T Entity
		{
			get { return entity; }
		}

		public virtual bool VisibleWhileInactive
		{
			get { return false; }
		}

		protected virtual float RotationSpeed
		{
			get { return entity.Active ? 1.0f : 0.0f; }
		}

		public override bool Selected
		{
			get { return entity.Selected; }
			set { entity.Selected = value; }
		}

		public ISelectable Selectable
		{
			get { return entity; }
		}

		protected virtual bool VisibilityOverride
		{
			get { return true; }
		}

		protected override float ScaleMultiplier
		{
			get { return entity.Selected ? 1.25f : 1.0f; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public EntityMarker(T entity)
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
			Scene.BaseTile.CenterLocationChanged += BaseTileCenterLocationChanged;
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!Owner.IsVisible)
				return;
			
			Transform3D.Position = Vector3.Lerp(Transform3D.Position, destination,
				(float)gameTime.TotalSeconds * MOVE_SPEED);
		}

		protected virtual void BaseTileCenterLocationChanged(TerrainTile obj)
		{
			LocationChanged(entity);
		}

		protected virtual void LocationChanged(ILocatable obj)
		{
			if (!entity.Location.HasLatLong)
				return;

			destination = Scene.LocationToVector(entity.Location);
			if (lastLocation == null || Location.Distance(entity.Location, lastLocation) > 50.0)
				Transform3D.Position = destination;
			lastLocation = new Location(entity.Location);
		}

		protected virtual void SelectedChanged(ISelectable obj)
		{
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
			bool active = Scene.BaseTile != null
				&& Scene.BaseTile.Region != null
				&& (VisibleWhileInactive || entity.Active)
				&& entity.Location.HasLatLong
				&& VisibilityOverride;

			bool oldVisible = Owner.IsVisible;
			Owner.IsActive = Owner.IsVisible = active;

			if (oldVisible != Owner.IsVisible && VisibleChanged != null)
				VisibleChanged(this);
		}
	}
}

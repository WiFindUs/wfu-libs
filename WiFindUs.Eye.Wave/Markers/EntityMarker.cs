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
		protected readonly T entity;
		protected const float MAX_SPIN_RATE = 5.0f;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public T Entity
		{
			get { return entity; }
		}

		public virtual bool VisibleOnTimeout
		{
			get { return false; }
		}

		public virtual float RotationSpeed
		{
			get
			{
				if (entity.TimedOut)
					return 0.0f;

				ulong age = entity.UpdateAge;
				if (age == 0)
					return MAX_SPIN_RATE;
				else if (age >= entity.TimeoutLength)
					return 0.0f;
				else
					return MAX_SPIN_RATE * (1.0f - (entity.UpdateAge / (float)entity.TimeoutLength));
			}
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

			entity.SelectedChanged += SelectedChanged;
			entity.LocationChanged += LocationChanged;
			entity.TimedOutChanged += TimedOutChanged;
			Scene.BaseTile.CenterLocationChanged += BaseTileCenterLocationChanged;

			UpdateMarkerState();
		}

		protected override void Update(TimeSpan gameTime)
		{
			Transform3D.Scale = new Vector3(Scale, Scale, Scale);

			/*
			if (model != null && modelTransform != null)
			{
				float rot = RotationSpeed;
				if (!rot.Tolerance(0.0f, 0.0001f))
				{
					modelTransform.Rotation = new Vector3(
						modelTransform.Rotation.X,
						modelTransform.Rotation.Y + rot * (float)gameTime.TotalSeconds,
						modelTransform.Rotation.Z);
				}
			}
			 * */
		}

		protected virtual void BaseTileCenterLocationChanged(TerrainTile obj)
		{
			UpdateMarkerState();
		}

		protected virtual void LocationChanged(ILocatable obj)
		{
			UpdateMarkerState();
		}

		protected virtual void SelectedChanged(ISelectable obj)
		{
			UpdateMarkerState();
		}

		protected virtual void TimedOutChanged(IUpdateable obj)
		{
			UpdateMarkerState();
		}

		protected virtual bool UpdateVisibilityCheck()
		{
			return true;
		}

		protected virtual void UpdateMarkerState()
		{
			bool active = Scene.BaseTile != null
				&& Scene.BaseTile.Region != null
				&& (VisibleOnTimeout || !entity.TimedOut)
				&& entity.Location.HasLatLong
				&& Scene.BaseTile.Region.Contains(entity.Location)
				&& UpdateVisibilityCheck();

			Owner.IsActive = Owner.IsVisible = active;
			if (active)
			{
				if (Transform3D != null)
					Transform3D.Position = Scene.LocationToVector(entity.Location);
			}
		}
	}
}

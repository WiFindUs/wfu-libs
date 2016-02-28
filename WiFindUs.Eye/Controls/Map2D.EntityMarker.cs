using System;
using System.Drawing;

namespace WiFindUs.Eye.Controls
{
    public partial class Map2D
	{
		internal abstract class Marker
		{
			private Rectangle oldBounds;
			private bool paintedOnce = false;
			internal readonly Map2D HostControl;
			internal readonly Map Source;
			internal abstract Point Position { get; }
			internal abstract Rectangle Bounds { get; }

			internal Marker(Map2D host)
			{
				if (host == null)
					throw new ArgumentNullException("host", "HostControl cannot be null!");
				HostControl = host;
				Source = HostControl.source;
			}

			internal abstract void Paint(Graphics g);

			protected void Refresh()
			{
				if (HostControl.InvokeRequired)
				{
					HostControl.Invoke(new Action(Refresh));
					return;
				}
				Rectangle bounds = Bounds;
				if (paintedOnce && !oldBounds.Equals(bounds))
					HostControl.Invalidate(oldBounds);
				HostControl.Invalidate(oldBounds = bounds);
				paintedOnce = true;
			}
		}
		
		internal interface IEntityMarker
		{
			ISelectable Selectable { get; }
			ILocatable Locatable { get; }
			IUpdateable Updateable { get; }
			bool Active { get; }
			bool Waiting { get; }
			bool Selected { get; }
			bool Visible { get; }
		}

		internal abstract class EntityMarker<T> : Marker, ISelectableProxy, ILocatableProxy, IUpdateableProxy, IEntityMarker
			where T : class, ILocatable, ISelectable, IUpdateable
		{
			internal readonly T Entity;


			private bool visible;

			/////////////////////////////////////////////////////////////////////
			// PROPERTIES
			/////////////////////////////////////////////////////////////////////

			internal virtual float ScaleMultiplier
			{
				get { return 1.0f; }
			}

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

			public bool Selected
			{
				get { return Entity.Selected; }
				set { Entity.Selected = value; }
			}

			public bool Active
			{
				get { return Entity.Active; }
			}

			public bool Waiting
			{
				get { return !Entity.Active && Entity.LastUpdatedSecondsAgo < (Entity.TimeoutLength * 5); }
			}

			public bool Visible
			{
				get { return visible; }
				private set
				{
					if (visible == value)
						return;
					visible = value;
					Refresh();
				}
			}

			/////////////////////////////////////////////////////////////////////
			// CONSTRUCTORS
			/////////////////////////////////////////////////////////////////////

			internal EntityMarker(Map2D host, T entity)
				: base(host)
			{
				if (entity == null)
					throw new ArgumentNullException("entity", "Entity cannot be null!");
				Entity = entity;

				LocationChanged(Entity);
				UpdateMarkerState();

				Entity.SelectedChanged += SelectedChanged;
				Entity.LocationChanged += LocationChanged;
				Entity.ActiveChanged += ActiveChanged;
				Entity.Updated += Updated;
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
				Visible = Entity.Location.HasLatLong
					&& Source.Contains(Entity.Location)
					&& (Active || Waiting);
			}

			protected virtual void LocationChanged(ILocatable obj)
			{
				if (!Entity.Location.HasLatLong || !Source.Contains(Entity.Location))
					return;

				/*
				destination = MapScene.LocationToVector(Entity.Location);
				if (lastLocation == null || WiFindUs.Location.Distance(Entity.Location, lastLocation) > 50.0)
					Transform3D.Position = destination;
				lastLocation = new Location(Entity.Location);
				 * */
			}
		}
	}
}

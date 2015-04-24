using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WiFindUs.Eye.Wave.Markers;

namespace WiFindUs.Eye.Wave
{
	public partial class MapInput : MapSceneBehavior
	{
		private bool mousePanning = false;

		public bool MousePanning
		{
			get { return mousePanning; }
		}

		/////////////////////////////////////////////////////////////////////
		// DEFAULT HANDLERS
		// everything in this file is the 'fallback' input handling;
		// these methods are called after notifying any subscribed event
		// handlers, and only if their Args.Handled property is still False.
		//
		// Setting the Args.Handled property to True from these will prevent
		// the event from being passed back to Windows, where applicable.
		/////////////////////////////////////////////////////////////////////

		protected virtual void OnHostGainedFocus(InputEventArgs args)
		{

		}

		protected virtual void OnHostLostFocus(InputEventArgs args)
		{

		}

		protected virtual void OnKeyDown(KeyEventArgs args)
		{
			if (args.Key == System.Windows.Forms.Keys.Add
				|| args.Key == System.Windows.Forms.Keys.Subtract)
			{
				if (ControlOnly) //keyboard zoom
					MapScene.Camera.Zoom -= 0.10f * (args.Key == System.Windows.Forms.Keys.Add
						? 1.0f : -1.0f);
				else if (ShiftOnly) //keyboard tilt
					MapScene.Camera.Tilt -= 0.10f * (args.Key == System.Windows.Forms.Keys.Add
						? 1.0f : -1.0f);
				args.Handled = true;
			}
			else if (args.Key == System.Windows.Forms.Keys.T)
			{
				if (NoModifiers)
				{
					MapScene.Camera.TrackSelectedMarkers();
					args.Handled = true;
				}
			}
		}

		protected virtual void OnKeyUp(KeyEventArgs args)
		{
		
		}

		protected virtual void OnMouseDown(MouseButtonEventArgs args)
		{
			if (args.Button == System.Windows.Forms.MouseButtons.Middle)
			{
				if ((ShiftOnly || NoModifiers) && MouseInsideHost)
				{
					mousePanning = true;
					args.Handled = true;
				}
			}
		}

		protected virtual void OnMouseUp(MouseButtonEventArgs args)
		{
			if (args.Button == System.Windows.Forms.MouseButtons.Middle)
			{
				if (mousePanning)
				{
					mousePanning = false;
					args.Handled = true;

					if (hostControl.Bounds.Contains(args.MouseX, args.MouseY))
					{
						Vector3 normal;
						Vector3? pos = MapScene.Camera.VectorFromScreenRay(args.MouseX, args.MouseY, out normal);
						if (pos.HasValue)
						{
							MapScene.Cursor.Transform3D.Position = pos.Value;
							MapScene.Cursor.Normal = normal;
						}
					}
				}
			}
		}

		protected virtual void OnMouseWheel(MouseWheelEventArgs args)
		{
			//mouse zoom
			if (args.Delta != 0)
				MapScene.Camera.Zoom -= args.Delta * 0.05f;
			args.Handled = true;
		}

		protected virtual void OnMouseMove(MouseMoveEventArgs args)
		{
			if (mousePanning)
			{
				if (!args.DeltaZero)
				{
					//mouse tilt
					if (ShiftOnly)
						MapScene.Camera.Tilt -= args.DeltaY * 0.01f;
					else
					{
						//mouse pan
						MapScene.Camera.TrackingEntity = null;
						float diff = 1.0f + MapScene.Camera.Zoom;
						MapScene.Camera.Target = MapScene.VectorToLocation(
							MapScene.Camera.TargetVector - new Vector3(args.DeltaX * diff, 0.0f, args.DeltaY * diff));
					}
				}
			}
			else
			{
				Vector3 normal;
				Vector3? pos = MapScene.Camera.VectorFromScreenRay(args.MouseX, args.MouseY, out normal);
				if (pos.HasValue && pos.Value.Y < Scene.RenderManager.ActiveCamera3D.Position.Y)
				{
					MapScene.Cursor.Transform3D.Position = pos.Value;
					MapScene.Cursor.Normal = normal;
					ILocation loc = MapScene.VectorToLocation(pos.Value);
					if (loc != null && loc.HasLatLong)
					{
						MapScene.Cursor.PositionText.Text = String.Format(
							"{0:0.000000}, {1:0.000000}, {2:0.0}m",
							loc.Latitude.GetValueOrDefault(),
							loc.Longitude.GetValueOrDefault(),
							loc.Altitude.GetValueOrDefault(Map.ELEV_MIN));
					}
					else
						MapScene.Cursor.PositionText.Text = "";
				}
			}

			 args.Handled = true;
		}

		protected virtual void OnMouseClick(MouseButtonEventArgs args)
		{
			if (mousePanning)
				return;

			if (args.Button == System.Windows.Forms.MouseButtons.Left)
			{
				args.Handled = true;

				ISelectable[] selectables = MapScene.Cursor.AllMarkersAtCursor
					.OfType<IEntityMarker>()
					.Where(mk => mk.Locatable.Location.HasLatLong)
					.Select<IEntityMarker,ISelectable>(mk => mk.Selectable)
					.ToArray();

				if (selectables == null || selectables.Length == 0)
				{
					if (!Control)
						MapScene.SelectionGroup.ClearSelection();
					return;
				}

				if (Control || Shift)
				{
					
					if (selectables.Length == 1)
						MapScene.SelectionGroup.ToggleSelection(selectables);
					else
					{
						bool sel = selectables.First().Selected;
						if (selectables.All(s => s.Selected == sel))
							MapScene.SelectionGroup.ToggleSelection(selectables);
						else
							MapScene.SelectionGroup.AddToSelection(selectables);
					}
				}
				else
				{
					if (selectables.Length == 1 || MapScene.SelectionGroup.SelectedEntities.Length == 0)
						MapScene.SelectionGroup.SetSelection(selectables[0]);
					else
					{
						ISelectable[] intersection = MapScene.SelectionGroup.SelectedEntities.Intersect(selectables).ToArray();
						if (intersection.Length == 0)
							MapScene.SelectionGroup.SetSelection(selectables[0]);
						else
							MapScene.SelectionGroup.SetSelection(
								selectables[((Array.IndexOf(selectables, intersection[intersection.Length - 1]) + 1) % selectables.Length)]);
					}
				}

			}
		}

		protected virtual void OnMouseDoubleClick(MouseButtonEventArgs args)
		{

		}

		protected override void Update(TimeSpan gameTime)
		{
			if (!HostHasFocus)
				return;
			
			if (!NoArrows)
			{
				Vector3 moveDelta = new Vector3();
				if (Left)
					moveDelta.X -= 1.0f;
				if (Right)
					moveDelta.X += 1.0f;
				if (Up)
					moveDelta.Z -= 1.0f;
				if (Down)
					moveDelta.Z += 1.0f;
				moveDelta.Normalize();
				moveDelta *= (float)gameTime.TotalSeconds * 1000.0f;
				MapScene.Camera.TrackingEntity = null;
				MapScene.Camera.Target = MapScene.VectorToLocation(
					MapScene.Camera.TargetVector + moveDelta);

			}
		}
	}
}

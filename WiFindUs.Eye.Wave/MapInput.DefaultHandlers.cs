using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WiFindUs.Eye.Wave.Controls;

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

		protected virtual void OnKeyDown(KeyEventArgs args)
		{
			if (ControlOnly)
			{
				if (args.Key == System.Windows.Forms.Keys.Add
					|| args.Key == System.Windows.Forms.Keys.Subtract)
				{
					MapScene.Camera.Zoom -= 0.10f * (args.Key == System.Windows.Forms.Keys.Add
						? 1.0f : -1.0f);
					args.Handled = true;
				}
			}
			else if (ShiftOnly)
			{
				if (args.Key == System.Windows.Forms.Keys.Add
					|| args.Key == System.Windows.Forms.Keys.Subtract)
				{
					MapScene.Camera.Tilt -= 0.10f * (args.Key == System.Windows.Forms.Keys.Add
						? 1.0f : -1.0f);
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
				mousePanning = true;
				args.Handled = true;
			}
		}

		protected virtual void OnMouseUp(MouseButtonEventArgs args)
		{
			if (args.Button == System.Windows.Forms.MouseButtons.Middle)
			{
				mousePanning = false;
				args.Handled = true;
			}
		}

		protected virtual void OnMouseWheel(MouseWheelEventArgs args)
		{
			if (ShiftOnly)
			{
				MapScene.Camera.Tilt -= args.Delta * 0.05f;
				args.Handled = true;
			}
			else if (NoModifiers)
			{
				MapScene.Camera.Zoom -= args.Delta * 0.05f;
				args.Handled = true;
			}
		}

		protected virtual void OnMouseMove(MouseMoveEventArgs args)
		{
			if (mousePanning)
			{
				float diff = 1.0f + MapScene.Camera.Zoom;
				MapScene.Camera.Target = MapScene.VectorToLocation(
					MapScene.Camera.TargetVector - new Vector3(args.DeltaX * diff, 0.0f, args.DeltaY * diff));
			}

			Vector3? pos = MapScene.Camera.VectorFromScreenRay(args.MouseX, args.MouseY);
			if (pos.HasValue)
				MapScene.Cursor.Transform3D.Position = pos.Value;

			 args.Handled = true;
		}

		protected virtual void OnMouseClick(MouseButtonEventArgs args)
		{

		}

		protected virtual void OnMouseDoubleClick(MouseButtonEventArgs args)
		{

		}
	}
}

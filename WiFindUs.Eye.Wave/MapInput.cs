using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WiFindUs.Eye.Wave.Controls;

namespace WiFindUs.Eye.Wave
{
	public partial class MapInput : MapSceneBehavior
	{
		public class InputEventArgs : EventArgs
		{
			private bool handled = false;
			public bool Handled
			{
				get { return handled; }
				set { if (value == handled || handled) return; handled = value; }
			}
		}
		public class MouseEventArgs : InputEventArgs
		{
			public readonly int MouseX;
			public readonly int MouseY;
			public MouseEventArgs(int x, int y)
			{
				MouseX = x;
				MouseY = y;
			}
		}
		public class MouseMoveEventArgs : MouseEventArgs
		{
			public readonly int DeltaX;
			public readonly int DeltaY;
			public MouseMoveEventArgs(int x, int y, int dx, int dy)
				: base(x, y)
			{
				DeltaX = dx;
				DeltaY = dy;
			}
		}
		public class MouseWheelEventArgs : MouseEventArgs
		{
			public readonly int Delta;
			public MouseWheelEventArgs(int x, int y, int d)
				: base(x, y)
			{
				Delta = d;
			}
		}
		public class MouseButtonEventArgs : MouseEventArgs
		{
			public readonly System.Windows.Forms.MouseButtons Button;
			public readonly bool Down;
			public MouseButtonEventArgs(int x, int y, System.Windows.Forms.MouseButtons button, bool down)
				: base (x,y)
			{
				Button = button;
				Down = down;
			}
		}
		public class KeyEventArgs : InputEventArgs
		{
			public readonly System.Windows.Forms.Keys Key;
			public readonly bool Down;
			public KeyEventArgs(System.Windows.Forms.Keys key, bool down)
			{
				Key = key;
				Down = down;
			}
		}

		public event Action<MapInput, KeyEventArgs> KeyDown, KeyUp;
		public event Action<MapInput, MouseButtonEventArgs> MouseDown, MouseUp, MouseClick, MouseDoubleClick;
		public event Action<MapInput, MouseMoveEventArgs> MouseMove;
		public event Action<MapInput, MouseWheelEventArgs> MouseWheel;
		public event Action<MapInput, InputEventArgs> HostLostFocus, HostGainedFocus;
		public event Action<MapInput, InputEventArgs> MouseEnteredHost, MouseLeftHost;

		private const int WHEEL_DELTA = 120;
		private bool mouseEntered = false;
		private bool hostHasFocus = false;
		private Dictionary<System.Windows.Forms.Keys, bool> keyStates = new Dictionary<System.Windows.Forms.Keys, bool>();
		private Dictionary<System.Windows.Forms.MouseButtons, bool> mouseStates = new Dictionary<System.Windows.Forms.MouseButtons, bool>();
		private int mouseX = 0, mouseY = 0;
		private MapControl hostControl;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES 
		/////////////////////////////////////////////////////////////////////

		public bool HostHasFocus
		{
			get { return hostHasFocus; }
			private set
			{
				if (value == hostHasFocus)
					return;
				hostHasFocus = value;
				if (hostHasFocus && HostGainedFocus != null)
					HostGainedFocus(this, new InputEventArgs());
				else if (!hostHasFocus && HostLostFocus != null)
					HostLostFocus(this, new InputEventArgs());
			}
		}

		public bool MouseInsideHost
		{
			get { return mouseEntered; }
			private set
			{
				if (value == mouseEntered)
					return;
				mouseEntered = value;
				if (mouseEntered && MouseEnteredHost != null)
					MouseEnteredHost(this, new InputEventArgs());
				else if (!mouseEntered && MouseLeftHost != null)
					MouseLeftHost(this, new InputEventArgs());
			}
		}

		public int MouseX
		{
			get { return mouseX; }
		}

		public int MouseY
		{
			get { return mouseY; }
		}

		public bool LeftMouse
		{
			get { return IsMouseButtonDown(System.Windows.Forms.MouseButtons.Left); }
		}

		public bool MiddleMouse
		{
			get { return IsMouseButtonDown(System.Windows.Forms.MouseButtons.Middle); }
		}

		public bool RightMouse
		{
			get { return IsMouseButtonDown(System.Windows.Forms.MouseButtons.Right); }
		}

		public bool MouseButton4
		{
			get { return IsMouseButtonDown(System.Windows.Forms.MouseButtons.XButton1); }
		}

		public bool MouseButton5
		{
			get { return IsMouseButtonDown(System.Windows.Forms.MouseButtons.XButton2); }
		}

		public bool NoMouseButtons
		{
			get
			{
				return !LeftMouse
					&& !MiddleMouse
					&& !RightMouse
					&& !MouseButton4
					&& !MouseButton5;
			}
		}

		public bool Shift
		{
			get { return IsKeyDown(System.Windows.Forms.Keys.ShiftKey); }
		}

		public bool Control
		{
			get { return IsKeyDown(System.Windows.Forms.Keys.ControlKey); }
		}

		public bool Alt
		{
			get { return IsKeyDown(System.Windows.Forms.Keys.Menu); }
		}

		public bool ShiftOnly
		{
			get { return Shift && !Control && !Alt; }
		}

		public bool AltOnly
		{
			get { return !Shift && !Control && Alt; }
		}

		public bool ControlOnly
		{
			get { return !Shift && Control && !Alt; }
		}

		public bool NoModifiers
		{
			get { return !Shift && !Control && !Alt; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public MapInput(MapControl hostControl)
		{
			if (hostControl == null)
				throw new ArgumentNullException("hostControl", "MapSceneControlInput cannot be instantiated outside of a host MapControl.");
			this.hostControl = hostControl;
		}

		protected override void ResolveDependencies()
		{
			base.ResolveDependencies();

			hostControl.MouseEnter += control_MouseEnter;
			hostControl.MouseLeave += control_MouseLeave;
			hostControl.MouseMove += control_MouseMove;
			hostControl.MouseDown += control_MouseDown;
			hostControl.MouseUp += control_MouseUp;
			hostControl.MouseWheel += control_MouseWheel;
			hostControl.MouseClick += control_MouseClick;
			hostControl.MouseDoubleClick += control_MouseDoubleClick;

			hostControl.KeyDown += control_KeyDown;
			hostControl.KeyUp += control_KeyUp;

			hostControl.Enter += control_Enter;
			hostControl.Leave += control_Leave;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public bool IsKeyDown(System.Windows.Forms.Keys key)
		{
			bool down;
			if (!keyStates.TryGetValue(key, out down))
				down = false;
			return down;
		}

		public bool IsMouseButtonDown(System.Windows.Forms.MouseButtons button)
		{
			bool down;
			if (!mouseStates.TryGetValue(button, out down))
				down = false;
			return down;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void control_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			UpdateMousePosition(e);
			MouseButtonEventArgs args = new MouseButtonEventArgs(mouseX, mouseY, e.Button, IsMouseButtonDown(e.Button));
			if (MouseClick != null)
				MouseClick(this, args);
			if (!args.Handled)
				OnMouseClick(args);
		}

		private void control_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			UpdateMousePosition(e);
			MouseButtonEventArgs args = new MouseButtonEventArgs(mouseX, mouseY, e.Button, IsMouseButtonDown(e.Button));
			if (MouseDoubleClick != null)
				MouseDoubleClick(this, args);
			if (!args.Handled)
				OnMouseDoubleClick(args);
		}

		private void control_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			UpdateMousePosition(e);
			if (e.Delta == 0)
				return;
			MouseWheelEventArgs args = new MouseWheelEventArgs(mouseX, mouseY, e.Delta / WHEEL_DELTA);
			if (MouseWheel != null)
				MouseWheel(this, args);
			if (!args.Handled)
				OnMouseWheel(args);
		}

		private void control_Leave(object sender, EventArgs e)
		{
			HostHasFocus = false;
		}

		private void control_Enter(object sender, EventArgs e)
		{
			HostHasFocus = true;
		}

		private void control_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			UpdateKeyState(e, true);
		}

		private void control_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			UpdateKeyState(e, false);
		}

		private void control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			UpdateMousePosition(e);
			UpdateMouseButtonState(e.Button, true);
		}

		private void control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			UpdateMousePosition(e);
		}

		private void control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			UpdateMousePosition(e);
			UpdateMouseButtonState(e.Button, false);
		}

		private void control_MouseEnter(object sender, EventArgs e)
		{
			MouseInsideHost = true;
		}

		private void control_MouseLeave(object sender, EventArgs e)
		{
			MouseInsideHost = false;
		}

		private void UpdateKeyState(System.Windows.Forms.KeyEventArgs e, bool down)
		{
			if (e.Handled || IsKeyDown(e.KeyCode) == down)
				return;

			keyStates[e.KeyCode] = down;
			KeyEventArgs args = new KeyEventArgs(e.KeyCode, down);
			if (down)
			{
				if (KeyDown != null)
					KeyDown(this, args);
				if (!args.Handled)
					OnKeyDown(args);
			}
			else
			{
				if (KeyUp != null)
					KeyUp(this, args);
				if (!args.Handled)
					OnKeyUp(args);
			}
			if (args.Handled)
				e.Handled = true;
		}

		private void UpdateMouseButtonState(System.Windows.Forms.MouseButtons button, bool down)
		{
			if (IsMouseButtonDown(button) == down)
				return;

			mouseStates[button] = down;
			MouseButtonEventArgs args = new MouseButtonEventArgs(mouseX, mouseY, button, down);
			if (down)
			{
				if (MouseDown != null)
					MouseDown(this, args);
				if (!args.Handled)
					OnMouseDown(args);
			}
			else
			{
				if (MouseUp != null)
					MouseUp(this, args);
				if (!args.Handled)
					OnMouseUp(args);
			}
		}

		private void UpdateMousePosition(System.Windows.Forms.MouseEventArgs e)
		{
			if (mouseX == e.X && mouseY == e.Y)
				return;
			int deltaX = e.X - mouseX;
			int deltaY = e.Y - mouseY;
			mouseX = e.X;
			mouseY = e.Y;
			MouseMoveEventArgs args = new MouseMoveEventArgs(mouseX, mouseY, deltaX, deltaY);
			if (MouseMove != null)
				MouseMove(this, args);
			if (!args.Handled)
				OnMouseMove(args);
		}
	}
}

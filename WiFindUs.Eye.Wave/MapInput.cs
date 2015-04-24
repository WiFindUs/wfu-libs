using System;
using System.Collections.Generic;
using System.Linq;

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
			public readonly bool DeltaZero;
			public readonly float DeltaLength;
			public MouseMoveEventArgs(int x, int y, int dx, int dy)
				: base(x, y)
			{
				DeltaX = dx;
				DeltaY = dy;
				DeltaZero = dx == 0 && dy == 0;
				DeltaLength = DeltaZero ? 0.0f : (float)Math.Sqrt((dx * dx) + (dy * dy));
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
		private int mouseButtonCount = 0;
		private int modifierCount = 0;
		private int arrowCount = 0;
		private int keyCount = 0;
		private Map3D hostControl;

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
				InputEventArgs args = new InputEventArgs();
				if (hostHasFocus)
				{
					if (HostGainedFocus != null)
						HostGainedFocus(this, args);
					if (!args.Handled)
						OnHostGainedFocus(args);
				}
				else
				{
					ClearAllStates();
					if (HostLostFocus != null)
						HostLostFocus(this, args);
					if (!args.Handled)
						OnHostLostFocus(args);
				}
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
			get { return mouseButtonCount > 0 && IsMouseButtonDown(System.Windows.Forms.MouseButtons.Left); }
		}

		public bool LeftMouseOnly
		{
			get
			{
				return LeftMouse
					&& !MiddleMouse
					&& !RightMouse
					&& !MouseButton4
					&& !MouseButton5;
			}
		}

		public bool MiddleMouse
		{
			get { return mouseButtonCount > 0 && IsMouseButtonDown(System.Windows.Forms.MouseButtons.Middle); }
		}

		public bool MiddleMouseOnly
		{
			get
			{
				return MiddleMouse
					&& !LeftMouse
					&& !RightMouse
					&& !MouseButton4
					&& !MouseButton5;
			}
		}

		public bool RightMouse
		{
			get { return mouseButtonCount > 0 && IsMouseButtonDown(System.Windows.Forms.MouseButtons.Right); }
		}

		public bool RightMouseOnly
		{
			get
			{
				return RightMouse
					&& !LeftMouse
					&& !MiddleMouse
					&& !MouseButton4
					&& !MouseButton5;
			}
		}

		public bool MouseButton4
		{
			get { return mouseButtonCount > 0 && IsMouseButtonDown(System.Windows.Forms.MouseButtons.XButton1); }
		}

		public bool MouseButton4Only
		{
			get
			{
				return MouseButton4
					&& !LeftMouse
					&& !MiddleMouse
					&& !RightMouse
					&& !MouseButton5;
			}
		}

		public bool MouseButton5
		{
			get { return mouseButtonCount > 0 && IsMouseButtonDown(System.Windows.Forms.MouseButtons.XButton2); }
		}

		public bool MouseButton5Only
		{
			get
			{
				return MouseButton5
					&& !LeftMouse
					&& !MiddleMouse
					&& !RightMouse
					&& !MouseButton4;
			}
		}

		public bool NoMouseButtons
		{
			get { return mouseButtonCount == 0; }
		}

		public bool Shift
		{
			get { return modifierCount > 0 && IsKeyDown(System.Windows.Forms.Keys.ShiftKey); }
		}

		public bool ShiftOnly
		{
			get { return Shift && !Control && !Alt; }
		}

		public bool Control
		{
			get { return modifierCount > 0 && IsKeyDown(System.Windows.Forms.Keys.ControlKey); }
		}

		public bool ControlOnly
		{
			get { return Control && !Shift && !Alt; }
		}

		public bool Alt
		{
			get { return modifierCount > 0 && IsKeyDown(System.Windows.Forms.Keys.Menu); }
		}

		public bool AltOnly
		{
			get { return Alt && !Shift && !Control; }
		}

		public bool NoModifiers
		{
			get { return modifierCount == 0; }
		}

		public bool Left
		{
			get { return arrowCount > 0 && IsKeyDown(System.Windows.Forms.Keys.Left); }
		}

		public bool LeftOnly
		{
			get { return Left && !Right && !Up && !Down; }
		}

		public bool Right
		{
			get { return arrowCount > 0 && IsKeyDown(System.Windows.Forms.Keys.Right); }
		}

		public bool RightOnly
		{
			get { return Right && !Left && !Up && !Down; }
		}

		public bool Up
		{
			get { return arrowCount > 0 && IsKeyDown(System.Windows.Forms.Keys.Up); }
		}

		public bool UpOnly
		{
			get { return Up && !Right && !Left && !Down; }
		}

		public bool Down
		{
			get { return arrowCount > 0 && IsKeyDown(System.Windows.Forms.Keys.Down); }
		}

		public bool DownOnly
		{
			get { return Down && !Up && !Right && !Left; }
		}

		public bool NoArrows
		{
			get { return arrowCount == 0; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public MapInput(Map3D hostControl)
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

		public static bool IsModifierKey(System.Windows.Forms.Keys key)
		{
			return key == System.Windows.Forms.Keys.ShiftKey
				|| key == System.Windows.Forms.Keys.ControlKey
				|| key == System.Windows.Forms.Keys.Menu;
		}

		public static bool IsArrowKey(System.Windows.Forms.Keys key)
		{
			return key == System.Windows.Forms.Keys.Left
				|| key == System.Windows.Forms.Keys.Right
				|| key == System.Windows.Forms.Keys.Up
				|| key == System.Windows.Forms.Keys.Down;
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
			if (!MouseInsideHost && hostControl.Bounds.Contains(e.X, e.Y))
				MouseInsideHost = true;
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
			if (e.Handled)
				return;

			KeyEventArgs args = new KeyEventArgs(e.KeyCode, down);
			UpdateKeyState(args);
			if (args.Handled)
				e.Handled = true;
		}

		private void UpdateKeyState(KeyEventArgs args)
		{
			if (IsKeyDown(args.Key) == args.Down)
				return;

			keyStates[args.Key] = args.Down;

			if (args.Down)
			{
				keyCount++;
				if (IsModifierKey(args.Key))
					modifierCount++;
				else if (IsArrowKey(args.Key))
					arrowCount++;

				if (KeyDown != null)
					KeyDown(this, args);
				if (!args.Handled)
					OnKeyDown(args);
			}
			else
			{
				keyCount--;
				if (IsModifierKey(args.Key))
					modifierCount--;
				else if (IsArrowKey(args.Key))
					arrowCount--;

				if (KeyUp != null)
					KeyUp(this, args);
				if (!args.Handled)
					OnKeyUp(args);
			}
		}

		private void UpdateMouseButtonState(System.Windows.Forms.MouseButtons button, bool down)
		{
			if (IsMouseButtonDown(button) == down)
				return;

			mouseStates[button] = down;
			MouseButtonEventArgs args = new MouseButtonEventArgs(mouseX, mouseY, button, down);
			if (down)
			{
				mouseButtonCount++;
				if (MouseDown != null)
					MouseDown(this, args);
				if (!args.Handled)
					OnMouseDown(args);
			}
			else
			{
				mouseButtonCount--;
				if (MouseUp != null)
					MouseUp(this, args);
				if (!args.Handled)
					OnMouseUp(args);
			}
		}

		private void UpdateMousePosition(System.Windows.Forms.MouseEventArgs e)
		{
			float ratioX = (float)hostControl.BackBufferWidth / (float)hostControl.Width;
			float ratioY = (float)hostControl.BackBufferHeight / (float)hostControl.Height;

			int realX = (int)(e.X * ratioX);
			int realY = (int)(e.Y * ratioY);

			if (mouseX == realX && mouseY == realY)
				return;
			int deltaX = realX - mouseX;
			int deltaY = realY - mouseY;
			mouseX = realX;
			mouseY = realY;
			MouseMoveEventArgs args = new MouseMoveEventArgs(mouseX, mouseY, deltaX, deltaY);
			if (MouseMove != null)
				MouseMove(this, args);
			if (!args.Handled)
				OnMouseMove(args);
		}

		private void ClearAllStates()
		{
			System.Windows.Forms.Keys[] keys = keyStates.Keys.ToArray();
			foreach (System.Windows.Forms.Keys key in keys)
				UpdateKeyState(new KeyEventArgs(key, false));

			System.Windows.Forms.MouseButtons[] mbs = mouseStates.Keys.ToArray();
			foreach (System.Windows.Forms.MouseButtons mb in mbs)
				UpdateMouseButtonState(mb, false);
		}
	}
}

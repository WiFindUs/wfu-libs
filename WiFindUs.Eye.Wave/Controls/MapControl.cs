using System;
using System.Drawing;
using System.Windows.Forms;
using WaveEngine.Adapter.Win32;
using WaveEngine.Framework.Services;
using WiFindUs.Controls;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Adapter;
using WiFindUs.Eye.Extensions;
using System.ComponentModel;
using WiFindUs.Themes;

namespace WiFindUs.Eye.Wave.Controls
{
	public class MapControl : ThemedControl
	{
		public event Action<MapScene> SceneStarted;

		private MapApplication mapApp;
		private Input input;
		private float scaleFactor = 1.0f;
		private Form form = null;
		private bool started = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form Form
		{
			get { return form; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MapScene Scene
		{
			get { return mapApp == null ? null : mapApp.Scene; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public float BackBufferScale
		{
			get { return scaleFactor; }
			set
			{
				float newVal = value < 0.1f ? 0.1f : (value > 1.0f ? 1.0f : value);
				if (newVal.Tolerance(scaleFactor, 0.01f))
					return;
				scaleFactor = newVal;
				if (mapApp != null)
					mapApp.ResizeScreen(BackBufferWidth, BackBufferHeight);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int BackBufferWidth
		{
			get
			{
				int val = (int)((float)ClientRectangle.Width * scaleFactor);
				return val <= 0 ? 1 : val;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int BackBufferHeight
		{
			get
			{
				int val = (int)((float)ClientRectangle.Height * scaleFactor);
				return val <= 0 ? 1 : val;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DebugMode
		{
			get { return Scene == null ? false : Scene.DebugMode; }
			set
			{
				if (Scene == null)
					return;
				Scene.DebugMode = value;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MapControl()
		{
			TabStop = false;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public override void ApplyTheme(ITheme theme)
		{
			if (theme == null)
				return;
			BackColor = theme.Background.Dark.Colour;
			ForeColor = theme.Foreground.Light.Colour;
			Font = theme.Controls.Normal.Regular;
		}

		public void StartMapApplication()
		{
			if (mapApp != null || started)
				return;
			if (WFUApplication.Config != null)
				BackBufferScale = WFUApplication.Config.Get("map.resolution_scale", 1.0f);
			mapApp = new MapApplication(this, BackBufferWidth, BackBufferHeight);
			mapApp.SceneStarted += scene_SceneStarted;
			mapApp.Configure(this.Handle);
			started = true;
		}
		public void Render()
		{
			if (mapApp != null)
				mapApp.Render();
		}

		public void CancelThreads()
		{
			if (mapApp != null)
				mapApp.CancelThreads();
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnParentChanged(EventArgs e)
		{
			form = FindForm();
			base.OnParentChanged(e);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (mapApp != null)
			{
				mapApp.CancelThreads();
				mapApp.OnDeactivate();
				mapApp.Dispose();
				mapApp = null;
			}
			base.OnHandleDestroyed(e);
		}

		protected override void OnResize(EventArgs e)
		{
			if (!IsDesignMode && mapApp != null)
				mapApp.ResizeScreen(BackBufferWidth, BackBufferHeight);
			base.OnResize(e);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (Scene == null || IsDesignMode)
				base.OnPaintBackground(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			bool design = IsDesignMode;
			if (design || Scene == null)
			{
				e.Graphics.Clear(design ? SystemColors.InactiveCaption : Theme.Current.Background.Dark.Colour);
				string text = design ? "Wave Engine 3D Map Control" : "Waiting for map scene to initialize...";
				var sizeText = e.Graphics.MeasureString(text, Font);
				e.Graphics.DrawString(text,
					design ? SystemFonts.DefaultFont : Theme.Current.Controls.Large.Bold,
					design ? SystemBrushes.InactiveCaptionText : Theme.Current.Foreground.Mid.Brush,
					(Width - sizeText.Width) / 2,
					(Height - sizeText.Height) / 2,
					StringFormat.GenericTypographic);
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			Cursor.Hide();
			if (!CheckInputManager())
				return;
			UpdateMousePosition(PointToClient(System.Windows.Forms.Cursor.Position));
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			Cursor.Show();
			if (!CheckInputManager())
				return;
			UpdateMousePosition(PointToClient(System.Windows.Forms.Cursor.Position));
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!CheckInputManager())
				return;

			UpdateMousePosition(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (!CheckInputManager())
				return;

			UpdateMousePosition(e);
			SetMouseButtonState(e, true);
			Focus();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (!CheckInputManager())
				return;

			UpdateMousePosition(e);
			SetMouseButtonState(e, false);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (!CheckInputManager())
				return;

			UpdateMousePosition(e);
			input.MouseState.Wheel += e.Delta > 0 ? 1 : -1;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!e.Handled && Form != null && Form == System.Windows.Forms.Form.ActiveForm)
			{
				SetKeyboardButtonState(e, true);
				e.Handled = true;
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (!e.Handled && Form != null && Form == System.Windows.Forms.Form.ActiveForm)
			{
				SetKeyboardButtonState(e, false);
				e.Handled = true;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void UpdateMousePosition(int x, int y)
		{
			input.MouseState.X = x;
			input.MouseState.Y = y;
		}

		private void UpdateMousePosition(Point point)
		{
			UpdateMousePosition(point.X, point.Y);
		}

		private void UpdateMousePosition(MouseEventArgs args)
		{
			UpdateMousePosition(args.X, args.Y);
		}

		private bool CheckInputManager()
		{
			if (IsDesignMode)
				return false;
			
			if (input == null)
			{
				input = WaveServices.Input;
				if (input != null)
					input.IsEnabled = false;
			}

			return input != null;
		}

		private void SetMouseButtonState(MouseEventArgs e, bool pressed)
		{
			if (!CheckInputManager())
				return;

			WaveEngine.Common.Input.ButtonState state = pressed ?
				WaveEngine.Common.Input.ButtonState.Pressed : WaveEngine.Common.Input.ButtonState.Release;

			switch (e.Button)
			{
				case MouseButtons.Left:
					this.input.MouseState.LeftButton = state;
					break;

				case MouseButtons.Middle:
					this.input.MouseState.MiddleButton = state;
					break;

				case MouseButtons.Right:
					this.input.MouseState.RightButton = state;
					break;
			}
		}

		private void SetKeyboardButtonState(KeyEventArgs e, bool pressed)
		{
			if (!CheckInputManager())
				return;

			WaveEngine.Common.Input.ButtonState state = pressed ?
				WaveEngine.Common.Input.ButtonState.Pressed : WaveEngine.Common.Input.ButtonState.Release;

			switch (e.KeyCode)
			{
				case System.Windows.Forms.Keys.A:
					this.input.KeyboardState.A = state;
					break;
				case System.Windows.Forms.Keys.B:
					this.input.KeyboardState.B = state;
					break;
				case System.Windows.Forms.Keys.Back:
					this.input.KeyboardState.Back = state;
					break;
				case System.Windows.Forms.Keys.C:
					this.input.KeyboardState.C = state;
					break;
				case System.Windows.Forms.Keys.CapsLock:
					this.input.KeyboardState.CapsLock = state;
					break;
				case System.Windows.Forms.Keys.Crsel:
					this.input.KeyboardState.Crsel = state;
					break;
				case System.Windows.Forms.Keys.D:
					this.input.KeyboardState.D = state;
					break;
				case System.Windows.Forms.Keys.D0:
					this.input.KeyboardState.D0 = state;
					break;
				case System.Windows.Forms.Keys.D1:
					this.input.KeyboardState.D1 = state;
					break;
				case System.Windows.Forms.Keys.D2:
					this.input.KeyboardState.D2 = state;
					break;
				case System.Windows.Forms.Keys.D3:
					this.input.KeyboardState.D3 = state;
					break;
				case System.Windows.Forms.Keys.D4:
					this.input.KeyboardState.D4 = state;
					break;
				case System.Windows.Forms.Keys.D5:
					this.input.KeyboardState.D5 = state;
					break;
				case System.Windows.Forms.Keys.D6:
					this.input.KeyboardState.D6 = state;
					break;
				case System.Windows.Forms.Keys.D7:
					this.input.KeyboardState.D7 = state;
					break;
				case System.Windows.Forms.Keys.D8:
					this.input.KeyboardState.D8 = state;
					break;
				case System.Windows.Forms.Keys.D9:
					this.input.KeyboardState.D9 = state;
					break;
				case System.Windows.Forms.Keys.Delete:
					this.input.KeyboardState.Delete = state;
					break;
				case System.Windows.Forms.Keys.Down:
					this.input.KeyboardState.Down = state;
					break;
				case System.Windows.Forms.Keys.E:
					this.input.KeyboardState.E = state;
					break;
				case System.Windows.Forms.Keys.Enter:
					this.input.KeyboardState.Enter = state;
					break;
				case System.Windows.Forms.Keys.Escape:
					this.input.KeyboardState.Escape = state;
					break;
				case System.Windows.Forms.Keys.Execute:
					this.input.KeyboardState.Execute = state;
					break;
				case System.Windows.Forms.Keys.F:
					this.input.KeyboardState.F = state;
					break;
				case System.Windows.Forms.Keys.F1:
					this.input.KeyboardState.F = state;
					break;
				case System.Windows.Forms.Keys.F10:
					this.input.KeyboardState.F10 = state;
					break;
				case System.Windows.Forms.Keys.F11:
					this.input.KeyboardState.F11 = state;
					break;
				case System.Windows.Forms.Keys.F12:
					this.input.KeyboardState.F12 = state;
					break;
				case System.Windows.Forms.Keys.F2:
					this.input.KeyboardState.F2 = state;
					break;
				case System.Windows.Forms.Keys.F3:
					this.input.KeyboardState.F3 = state;
					break;
				case System.Windows.Forms.Keys.F4:
					this.input.KeyboardState.F4 = state;
					break;
				case System.Windows.Forms.Keys.F5:
					this.input.KeyboardState.F5 = state;
					break;
				case System.Windows.Forms.Keys.F6:
					this.input.KeyboardState.F6 = state;
					break;
				case System.Windows.Forms.Keys.F7:
					this.input.KeyboardState.F7 = state;
					break;
				case System.Windows.Forms.Keys.F8:
					this.input.KeyboardState.F8 = state;
					break;
				case System.Windows.Forms.Keys.F9:
					this.input.KeyboardState.F9 = state;
					break;
				case System.Windows.Forms.Keys.G:
					this.input.KeyboardState.G = state;
					break;
				case System.Windows.Forms.Keys.H:
					this.input.KeyboardState.H = state;
					break;
				case System.Windows.Forms.Keys.I:
					this.input.KeyboardState.I = state;
					break;
				case System.Windows.Forms.Keys.J:
					this.input.KeyboardState.J = state;
					break;
				case System.Windows.Forms.Keys.K:
					this.input.KeyboardState.K = state;
					break;
				case System.Windows.Forms.Keys.L:
					this.input.KeyboardState.L = state;
					break;
				case System.Windows.Forms.Keys.Left:
					this.input.KeyboardState.Left = state;
					break;
				case System.Windows.Forms.Keys.ControlKey:
					this.input.KeyboardState.LeftControl = state;
					this.input.KeyboardState.RightControl = state;
					break;
				case System.Windows.Forms.Keys.ShiftKey:
					this.input.KeyboardState.LeftShift = state;
					this.input.KeyboardState.RightShift = state;
					break;
				case System.Windows.Forms.Keys.Menu:
					this.input.KeyboardState.LeftAlt = state;
					this.input.KeyboardState.RightAlt = state;
					break;
				case System.Windows.Forms.Keys.LWin:
					this.input.KeyboardState.LeftWindows = state;
					break;
				case System.Windows.Forms.Keys.M:
					this.input.KeyboardState.M = state;
					break;
				case System.Windows.Forms.Keys.N:
					this.input.KeyboardState.N = state;
					break;
				case System.Windows.Forms.Keys.O:
					this.input.KeyboardState.O = state;
					break;
				case System.Windows.Forms.Keys.P:
					this.input.KeyboardState.P = state;
					break;
				case System.Windows.Forms.Keys.Q:
					this.input.KeyboardState.Q = state;
					break;
				case System.Windows.Forms.Keys.R:
					this.input.KeyboardState.R = state;
					break;
				case System.Windows.Forms.Keys.Right:
					this.input.KeyboardState.Right = state;
					break;
				case System.Windows.Forms.Keys.RWin:
					this.input.KeyboardState.RightWindows = state;
					break;
				case System.Windows.Forms.Keys.S:
					this.input.KeyboardState.S = state;
					break;
				case System.Windows.Forms.Keys.Space:
					this.input.KeyboardState.Space = state;
					break;
				case System.Windows.Forms.Keys.Subtract:
					this.input.KeyboardState.Subtract = state;
					break;
				case System.Windows.Forms.Keys.T:
					this.input.KeyboardState.T = state;
					break;
				case System.Windows.Forms.Keys.Tab:
					this.input.KeyboardState.Tab = state;
					break;
				case System.Windows.Forms.Keys.U:
					this.input.KeyboardState.U = state;
					break;
				case System.Windows.Forms.Keys.Up:
					this.input.KeyboardState.Up = state;
					break;
				case System.Windows.Forms.Keys.V:
					this.input.KeyboardState.V = state;
					break;
				case System.Windows.Forms.Keys.W:
					this.input.KeyboardState.W = state;
					break;
				case System.Windows.Forms.Keys.X:
					this.input.KeyboardState.X = state;
					break;
				case System.Windows.Forms.Keys.Y:
					this.input.KeyboardState.Y = state;
					break;
				case System.Windows.Forms.Keys.Z:
					this.input.KeyboardState.Z = state;
					break;
			}
		}

		private void scene_SceneStarted(MapScene scene)
		{
			SetStyle(ControlStyles.Opaque, true);
			UpdateStyles();

			if (WFUApplication.Config != null)
			{
				ILocation location = WFUApplication.Config.Get("map.center", (ILocation)null);
				if (location == null)
					Debugger.E("Could not parse map.center from config files!");
				else
					scene.CenterLocation = location;
			}

			if (SceneStarted != null)
				SceneStarted(scene);
		}
	}
}

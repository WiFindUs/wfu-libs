using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WiFindUs.Forms;
using WiFindUs.Extensions;
using System.Drawing;
using System.IO;
using WaveEngine.Framework.Services;
using WaveEngine.Adapter.Win32;
using WiFindUs.Controls;

namespace WiFindUs.Eye.Wave
{
    public class MapControl : Control, IThemeable
    {
        public event Action<MapScene> SceneStarted;
        public event Action<MapControl> ApplicationStarting;
        public event Action<MapControl> AltEnterPressed;

        private MapApplication mapApp;
        private Input input;
        private float scaleFactor = 1.0f;
        private Form form = null;
        private Theme theme;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public Form Form
        {
            get
            {
                if (form == null)
                    form = FindForm();
                return form;
            }
        }

        public MapScene Scene
        {
            get
            {
                return mapApp == null ? null : mapApp.Scene;
            }
        }


        public bool IsDesignMode
        {
            get
            {
                return DesignMode || this.IsDesignMode();
            }
        }

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

        public int BackBufferWidth
        {
            get
            {
                int val = (int)((float)ClientRectangle.Width * scaleFactor);
                return val <= 0 ? 1 : val;
            }
        }

        public int BackBufferHeight
        {
            get
            {
                int val = (int)((float)ClientRectangle.Height * scaleFactor);
                return val <= 0 ? 1 : val;
            }
        }

        public Theme Theme
        {
            get
            {
                return theme;
            }
            set
            {
                if (value == null || value == theme)
                    return;

                theme = value;
                BackColor = theme.ControlDarkColour;
                ForeColor = theme.TextLightColour;
                Font = theme.WindowFont;
                OnThemeChanged();

                if (Scene != null)
                    Scene.Theme = value;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public MapControl()
        {
            Margin = new Padding(0);
            TabStop = false;
            
            if (IsDesignMode)
                return;

            SetStyle( ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public virtual void OnThemeChanged()
        {

        }

        public void StartMapApplication()
        {
            if (ApplicationStarting != null)
                ApplicationStarting(this);
            mapApp = new MapApplication(this, BackBufferWidth, BackBufferHeight);
            mapApp.SceneStarted += scene_SceneStarted;
            mapApp.Configure(this.Handle);
        }

        public static void StartRenderLoop(WiFindUs.Forms.MainForm form)
        {
            IMapForm mapForm = form as IMapForm;
            if (mapForm == null)
            {
                String message = "The supplied MainForm type (" + form.GetType().FullName+ ") does not implement the IMapForm interface!";
                Debugger.E(message);
                MessageBox.Show(message + "\n\nThe application will now exit.", "IMapForm Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RenderLoop.Run(form, () =>
            {
                mapForm.RenderMap();
            });
        }

        public void Render()
        {
            if (mapApp != null)
            {
                mapApp.Render();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            mapApp.CancelThreads();
            mapApp.OnDeactivate();
            mapApp.Dispose();
            base.OnHandleDestroyed(e);
        }

        public void CancelThreads()
        {
            if (mapApp != null)
                mapApp.CancelThreads();
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
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
                e.Graphics.Clear(design ? Color.WhiteSmoke : theme.ControlDarkColour);
                string text = design ? "Wave Engine Map Renderer Control" : "Waiting for map scene to initialize...";
                var sizeText = e.Graphics.MeasureString(text, Font);
                using (Font f = new Font(Font.FontFamily, Font.Size + 6.0f, Font.Style | FontStyle.Bold))
                    e.Graphics.DrawString(text, f, design ? Brushes.Black : theme.TextMidBrush, (Width - sizeText.Width) / 2, (Height - sizeText.Height) / 2, StringFormat.GenericTypographic);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!CheckInputManager())
                return;
            input.MouseState.X = e.X;
            input.MouseState.Y = e.Y;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            SetMouseButtonState(e, true);
            this.Focus();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            SetMouseButtonState(e, false);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!CheckInputManager())
                return;
            this.input.MouseState.Wheel += e.Delta > 0 ? 1 : -1;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled && Form == System.Windows.Forms.Form.ActiveForm)
            {
                if (e.Modifiers == Keys.Alt)
                {
                    if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) && AltEnterPressed != null)
                    {
                        AltEnterPressed(this);
                        e.Handled = true;
                    }
                    return;
                }
                SetKeyboardButtonState(e, true);
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (!e.Handled && Form == System.Windows.Forms.Form.ActiveForm)
            {
                if (e.Modifiers == Keys.Alt)
                    return;
                SetKeyboardButtonState(e, false);
                e.Handled = true;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private bool CheckInputManager()
        {
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
                case System.Windows.Forms.Keys.LMenu:
                    this.input.KeyboardState.LeftAlt = state;
                    break;
                case System.Windows.Forms.Keys.LControlKey:
                    this.input.KeyboardState.LeftControl = state;
                    break;
                case System.Windows.Forms.Keys.LShiftKey:
                case System.Windows.Forms.Keys.Shift:
                case System.Windows.Forms.Keys.ShiftKey:
                    this.input.KeyboardState.LeftShift = state;
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
                case System.Windows.Forms.Keys.RMenu:
                    this.input.KeyboardState.RightAlt = state;
                    break;
                case System.Windows.Forms.Keys.RControlKey:
                    this.input.KeyboardState.RightControl = state;
                    break;
                case System.Windows.Forms.Keys.RShiftKey:
                    this.input.KeyboardState.RightShift = state;
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

        private void scene_SceneStarted(MapScene obj)
        {
            SetStyle(ControlStyles.Opaque, true);
            UpdateStyles();

            if (SceneStarted != null)
                SceneStarted(obj);
        }
    }
}

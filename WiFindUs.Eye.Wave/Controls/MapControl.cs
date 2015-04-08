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
		private float scaleFactor = 1.0f;
		private bool started = false;
		private ISelectableGroup selectionGroup = new SelectableEntityGroup();

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal MapScene Scene
		{
			get { return mapApp == null ? null : mapApp.Scene; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal float BackBufferScale
		{
			get { return scaleFactor; }
			set
			{
				float newVal = value < 0.1f ? 0.1f : (value > 1.0f ? 1.0f : value);
				if (newVal.Tolerance(scaleFactor, 0.01f))
					return;
				scaleFactor = newVal;
				ResizeBackBuffer();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal int BackBufferWidth
		{
			get { return mapApp != null ? mapApp.Width : 0; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal int BackBufferHeight
		{
			get { return mapApp != null ? mapApp.Height : 0; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal bool DebugMode
		{
			get { return Scene == null ? false : Scene.DebugMode; }
			set
			{
				if (Scene == null)
					return;
				Scene.DebugMode = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ISelectableGroup SelectionGroup
		{
			get { return selectionGroup; }
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

		internal void StartMapApplication()
		{
			if (mapApp != null || started)
				return;
#if DEBUG
			Debugger.T("entry");
#endif
			if (WFUApplication.Config != null)
				BackBufferScale = WFUApplication.Config.Get("map.resolution_scale", 1.0f);
			mapApp = new MapApplication(this,
				Math.Max((int)((float)ClientRectangle.Width * scaleFactor), 1),
				Math.Max((int)((float)ClientRectangle.Height * scaleFactor), 1)
				);
			mapApp.SceneStarted += scene_SceneStarted;
			mapApp.Configure(this.Handle);
			started = true;
#if DEBUG
			Debugger.T("exit");
#endif
		}

		internal void Render()
		{
			if (mapApp != null)
				mapApp.Render();
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

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
			if (!IsDesignMode)
				ResizeBackBuffer();
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
#if !DEBUG
			CursorManager.Hide();
#endif
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{

			base.OnMouseLeave(e);
#if !DEBUG
			CursorManager.Show();
#endif
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			Focus();
			base.OnMouseDown(e);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void ResizeBackBuffer()
		{
			if (mapApp == null)
				return;
			int w = Math.Max((int)((float)ClientRectangle.Width * scaleFactor), 1);
			int h = Math.Max((int)((float)ClientRectangle.Height * scaleFactor), 1);
			mapApp.ResizeScreen(w, h);
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

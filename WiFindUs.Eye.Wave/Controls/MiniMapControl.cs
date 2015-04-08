using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;
using WiFindUs.Themes;

namespace WiFindUs.Eye.Wave.Controls
{
	public class MiniMapControl : ThemedControl
	{
		private const int UPDATE_FPS = 15;
		private MapControl hostControl;
		private Rectangle mapArea = Rectangle.Empty;
		private bool mouseDown = false;
		private Image image;
		private long lastCameraUpdate = 0;
		private Timer timer;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MapControl Map
		{
			get { return hostControl; }
			set
			{
				if (value == hostControl)
					return;

				//suspend/release resources
				timer.Stop();
				DisposeImage();

				//unset previous
				if (hostControl != null)
					hostControl.SceneStarted -= hostControl_SceneStarted;

				//change value
				hostControl = value;

				//set new
				if (hostControl != null)
				{
					hostControl.SceneStarted += hostControl_SceneStarted;
					if (hostControl.Scene != null)
					{
						if (hostControl.Scene.IsStarted)
							hostControl_SceneStarted(hostControl.Scene);
					}
				}

				//update
				SetTimerState();
				Refresh();
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MiniMapControl()
		{
			TabStop = false;
			timer = new Timer() { Interval = (1000 / UPDATE_FPS) };
			timer.Tick += timer_Tick;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public Point LocationToScreen(ILocation loc)
		{
			if (loc == null
				|| !loc.HasLatLong
				|| hostControl == null
				|| hostControl.Scene == null
				|| hostControl.Scene.BaseTile == null
				|| hostControl.Scene.BaseTile.Region == null)
				return Point.Empty;
			return hostControl.Scene.BaseTile.Region.LocationToScreen(mapArea, loc);
		}

		public ILocation ScreenToLocation(Point point)
		{
			if (hostControl == null
				|| hostControl.Scene == null
				|| hostControl.Scene.BaseTile == null
				|| hostControl.Scene.BaseTile.Region == null)
				return WiFindUs.Eye.Location.EMPTY;

			return new WiFindUs.Eye.Location(
				hostControl.Scene.BaseTile.Region.NorthWest.Latitude - ((point.Y - mapArea.Top) / (float)mapArea.Height) * hostControl.Scene.BaseTile.Region.LatitudinalSpan,
				hostControl.Scene.BaseTile.Region.NorthWest.Longitude + ((point.X - mapArea.Left) / (float)mapArea.Width) * hostControl.Scene.BaseTile.Region.LongitudinalSpan
				);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			RecalculateMapArea();
			Refresh();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			SetTimerState();
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			SetTimerState();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			//design mode 
			if (IsDesignMode)
			{
				e.Graphics.Clear(SystemColors.InactiveCaption);
				string text = "Wave Engine 2D Map Control";
				var sizeText = e.Graphics.MeasureString(text, Font);
				e.Graphics.DrawString(text,
					SystemFonts.DefaultFont,
					SystemBrushes.InactiveCaptionText,
					(Width - sizeText.Width) / 2,
					(Height - sizeText.Height) / 2,
					StringFormat.GenericTypographic);
				return;
			}

			//initialize render state
			e.Graphics.SetQuality(GraphicsExtensions.GraphicsQuality.Low);
			e.Graphics.Clear(Theme.Current.Background.Dark.Colour);
			if (hostControl == null || hostControl.Scene == null || hostControl.Scene.BaseTile == null || hostControl.Scene.Camera == null)
			{
				e.Graphics.FillRectangle(Theme.Current.Background.Light.Brush, mapArea);
				return;
			}

			//draw base image
			CheckMapImage();
			e.Graphics.DrawImageSafe(image, mapArea, Brushes.White, CompositingMode.SourceCopy);

			//get frustum coords
			ILocation nw = hostControl.Scene.Camera.FrustumNorthWest;
			ILocation ne = hostControl.Scene.Camera.FrustumNorthEast;
			ILocation sw = hostControl.Scene.Camera.FrustumSouthWest;
			ILocation se = hostControl.Scene.Camera.FrustumSouthEast;

			//generate frustum poly
			Point[] points = new Point[4];
			points[0] = nw == null
				? new Point(mapArea.Left - 5000, mapArea.Top - 5000) : LocationToScreen(nw);
			points[1] = ne == null
				? new Point(mapArea.Right + 5000, mapArea.Top - 5000) : LocationToScreen(ne);
			points[2] = se == null
				? new Point(mapArea.Right, mapArea.Bottom) : LocationToScreen(se);
			points[3] = sw == null
				? new Point(mapArea.Left, mapArea.Bottom) : LocationToScreen(sw);

			//darken non-focal area
			GraphicsPath path = new GraphicsPath();
			path.AddPolygon(points);
			System.Drawing.Region region = new System.Drawing.Region(path);
			region.Xor(ClientRectangle);
			using (Brush b = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
				e.Graphics.FillRegion(b, region);

			//draw frustum
			using (Pen p = new Pen(Color.FromArgb(140, 255, 255, 255), 1f))
				e.Graphics.DrawPolygon(p, points);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				Focus();
				mouseDown = true;
				MoveByMouse(e);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (mouseDown)
				MoveByMouse(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
				mouseDown = false;
		}

		protected virtual void OnDisposing()
		{
			Map = null;
			DisposeImage();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				OnDisposing();
			base.Dispose(disposing);
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);

			if (!mouseDown || hostControl == null || hostControl.Scene == null || hostControl.Scene.Camera == null)
				return;
			hostControl.Scene.Camera.Zoom = Math.Min(hostControl.Scene.Camera.Zoom, 0.35f);
			hostControl.Scene.Camera.Tilt = 0.5f;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void hostControl_SceneStarted(MapScene scene)
		{
			if (scene.BaseTile != null)
				scene.BaseTile.TextureImageLoadingFinished += BaseTile_TextureImageLoadingFinished;
			scene.CenterLocationChanged += Scene_CenterLocationChanged;
			SetTimerState();
			Refresh();
		}

		private void SetTimerState()
		{
			timer.Enabled = Visible
				&& Enabled
				&& hostControl != null
				&& hostControl.Scene != null;
				//&& hostControl.Scene.IsStarted;
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (!Visible)
				return;
			long timer = DateTime.Now.Ticks;
			TimeSpan span = new TimeSpan(timer - lastCameraUpdate);
			if (span.TotalMilliseconds > 66.0) //15fps
			{
				lastCameraUpdate = timer;
				Refresh();
			}
		}

		private void CheckMapImage()
		{
			if (!hostControl.Scene.BaseTile.Textured || hostControl.Scene.BaseTile.Error)
				return;

			int targetSize = ((int)((float)Math.Min(ClientRectangle.Width, ClientRectangle.Height)
				/ 256.0f) + 1) * 256;
			targetSize /= 2;
			if (image != null && image.Width == targetSize)
				return;
			if (image != null)
				image.Dispose();

			//draw base image
			try
			{
				image = hostControl.Scene.BaseTile.TileImage.Resize(
					targetSize, targetSize, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
			}
			catch { image = null; } //just wait until next time
		}

		private void BaseTile_TextureImageLoadingFinished(TerrainTile obj)
		{
			if (Visible)
				this.RefreshThreadSafe();
		}

		private void MoveByMouse(MouseEventArgs e)
		{
			if (hostControl == null
				|| hostControl.Scene == null
				|| hostControl.Scene.BaseTile == null
				|| hostControl.Scene.BaseTile.Region == null)
				return;

			hostControl.Scene.Camera.TrackingTarget = null;
			hostControl.Scene.Camera.Target =
				ScreenToLocation(
				new Point(
					e.X < mapArea.Left ? mapArea.Left : (e.X > mapArea.Right ? mapArea.Right : e.X),
					e.Y < mapArea.Top ? mapArea.Top : (e.Y > mapArea.Bottom ? mapArea.Bottom : e.Y)
					));
		}

		private void RecalculateMapArea()
		{
			int size = Math.Min(ClientRectangle.Width, ClientRectangle.Height);
			mapArea = new Rectangle(ClientRectangle.Width / 2 - size / 2,
				ClientRectangle.Height / 2 - size / 2,
				size, size);
		}

		private void Scene_CenterLocationChanged(MapScene obj)
		{
			DisposeImage();
		}

		private void DisposeImage()
		{
			if (image != null)
				image.Dispose();
			image = null;
		}
	}
}

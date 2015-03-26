using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave.Controls
{
	public class MiniMapControl : Control, IThemeable
	{
		private Theme theme;
		private WiFindUs.Eye.Wave.MapScene scene;
		private Rectangle mapArea = Rectangle.Empty;
		private bool mouseDown = false;
		private Image image;
		private long lastCameraUpdate = 0;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public bool IsDesignMode
		{
			get
			{
				return DesignMode || this.IsDesignMode();
			}
		}

		public WiFindUs.Eye.Wave.MapScene Scene
		{
			get { return scene; }
			set
			{
				if (value == scene)
					return;
				if (scene != null)
				{
					if (scene.BaseTile != null)
						scene.BaseTile.TextureImageLoadingFinished -= BaseTile_TextureImageLoadingFinished;
					if (scene.CameraController != null)
						scene.CameraController.Moved -= CameraController_Moved;
					scene.CenterLocationChanged -= Scene_CenterLocationChanged;
					DisposeImage();
				}
				scene = value;
				if (scene != null)
				{
					if (scene.BaseTile != null)
						scene.BaseTile.TextureImageLoadingFinished += BaseTile_TextureImageLoadingFinished;
					if (scene.CameraController != null)
						scene.CameraController.Moved += CameraController_Moved;
					scene.CenterLocationChanged += Scene_CenterLocationChanged;
				}
				Refresh();
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
				BackColor = theme.ControlLightColour;
				Font = theme.WindowFont;
				OnThemeChanged();
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MiniMapControl()
		{
			Margin = new Padding(0);
			TabStop = false;

			if (IsDesignMode)
				return;

			ResizeRedraw = false;
			//DoubleBuffered = true;
			SetStyle(
				System.Windows.Forms.ControlStyles.UserPaint |
				System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
				System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
				true);
			UpdateStyles();
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public virtual void OnThemeChanged()
		{

		}

		public Point LocationToScreen(ILocation loc)
		{
			if (loc == null
				|| !loc.HasLatLong
				|| scene == null
				|| scene.BaseTile == null
				|| scene.BaseTile.Region == null)
				return Point.Empty;
			return Scene.BaseTile.Region.LocationToScreen(mapArea, loc);
		}

		public ILocation ScreenToLocation(Point point)
		{
			if (scene == null
				|| scene.BaseTile == null
				|| scene.BaseTile.Region == null)
				return WiFindUs.Eye.Location.EMPTY;

			return new WiFindUs.Eye.Location(
				scene.BaseTile.Region.NorthWest.Latitude - ((point.Y - mapArea.Top) / (float)mapArea.Height) * scene.BaseTile.Region.LatitudinalSpan,
				scene.BaseTile.Region.NorthWest.Longitude + ((point.X - mapArea.Left) / (float)mapArea.Width) * scene.BaseTile.Region.LongitudinalSpan
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

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (IsDesignMode || scene == null || scene.BaseTile == null)
				return;

			//initialize render state
			GraphicsExtensions.GraphicsQualitySettings settings = e.Graphics.GetQuality();
			e.Graphics.SetQuality(GraphicsExtensions.GraphicsQuality.Low);
			e.Graphics.Clear(theme.ControlDarkColour);

			//draw base image
			CheckMapImage();
			e.Graphics.DrawImageSafe(image, mapArea, Brushes.White, CompositingMode.SourceCopy);

			if (scene.CameraController != null)
			{
				//get frustum coords
				ILocation nw = scene.CameraController.FrustumNorthWest;
				ILocation ne = scene.CameraController.FrustumNorthEast;
				ILocation sw = scene.CameraController.FrustumSouthWest;
				ILocation se = scene.CameraController.FrustumSouthEast;

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

			//reset render state
			e.Graphics.SetQuality(settings);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
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
			Scene = null;
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

			if (!mouseDown || scene == null || scene.CameraController == null)
				return;
			scene.CameraController.Zoom = 0.35f;
			scene.CameraController.Tilt = 0.5f;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void CheckMapImage()
		{
			if (!scene.BaseTile.Textured || scene.BaseTile.Error)
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
				image = scene.BaseTile.TileImage.Resize(targetSize, targetSize, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
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
			if (scene == null
				|| scene.BaseTile == null
				|| scene.BaseTile.Region == null)
				return;

			scene.CameraController.Target =
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

		private void CameraController_Moved(MapSceneCamera obj)
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

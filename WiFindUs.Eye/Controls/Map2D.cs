using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;
using WiFindUs.Themes;

namespace WiFindUs.Eye.Controls
{
	public class Map2D : ThemedControl
	{
		private Rectangle mapArea = Rectangle.Empty;
		private Tile source;

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public Map2D()
		{
			TabStop = false;
			if (IsDesignMode)
				return;
			source = (WFUApplication.MainForm as EyeMainForm).BaseTile;
			source.ImageChanged += source_ImageChanged;
			source.ImageStateChanged += source_ImageChanged;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public Point LocationToScreen(ILocation loc)
		{
			if (loc == null
				|| !loc.HasLatLong
				|| source == null)
				return Point.Empty;
			return source.LocationToScreen(mapArea, loc);
		}

		public ILocation ScreenToLocation(Point point)
		{
			if (source == null)
				return WiFindUs.Eye.Location.EMPTY;

			return new WiFindUs.Eye.Location(
				source.NorthWest.Latitude - ((point.Y - mapArea.Top) / (float)mapArea.Height) * source.LatitudinalSpan,
				source.NorthWest.Longitude + ((point.X - mapArea.Left) / (float)mapArea.Width) * source.LongitudinalSpan
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

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			//design mode
			if (IsDesignMode)
			{
				pevent.Graphics.Clear(SystemColors.InactiveCaption);

				string text = "Wave Engine 2D Map Control";
				var sizeText = pevent.Graphics.MeasureString(text, Font);
				pevent.Graphics.DrawString(text,
					SystemFonts.DefaultFont,
					SystemBrushes.InactiveCaptionText,
					(Width - sizeText.Width) / 2,
					(Height - sizeText.Height) / 2,
					StringFormat.GenericTypographic);

				return;
			}

			//clear canvas
			pevent.Graphics.SetQuality(GraphicsExtensions.GraphicsQuality.Low);
			pevent.Graphics.Clear(Theme.Current.Background.Dark.Colour);

			//draw rectangle if no source present
			if (source == null || source.ImageState != Tile.LoadingState.Finished)
			{
				pevent.Graphics.FillRectangle(Theme.Current.Background.Light.Brush, mapArea);
				return;
			}

			//draw image
			pevent.Graphics.DrawImageSafe(source.Image, mapArea,
				Theme.Current.Background.Light.Brush, CompositingMode.SourceCopy);

		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			//e.Graphics.Clear(Theme.Current.Background.Dark.Colour);
			//if (source == null || source.ImageState != Tile.LoadingState.Finished)
			///{
			//	e.Graphics.FillRectangle(Theme.Current.Background.Light.Brush, mapArea);
			//	return;
			//}

			//draw base image
			//e.Graphics.DrawImageSafe(source.Image, mapArea, Brushes.White, CompositingMode.SourceCopy);

			/*
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
			 * */
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
				Focus();
		}


		protected virtual void OnDisposing()
		{

		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				OnDisposing();
			base.Dispose(disposing);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void source_ImageChanged(Tile obj)
		{
			this.RefreshThreadSafe();
		}

		private void RecalculateMapArea()
		{
			int size = Math.Min(ClientRectangle.Width, ClientRectangle.Height);
			mapArea = new Rectangle(ClientRectangle.Width / 2 - size / 2,
				ClientRectangle.Height / 2 - size / 2,
				size, size);
		}
	}
}

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
	public partial class Map2D : ThemedControl
	{
		private Rectangle mapArea = Rectangle.Empty;
		private Map source;

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public Map2D()
		{
			TabStop = false;
			if (IsDesignMode)
				return;
			source = (WFUApplication.MainForm as EyeMainForm).Map;
			source.CompositeImageChanged += source_CompositeImageChanged;
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
				return WiFindUs.Location.EMPTY;

			return new WiFindUs.Location(
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

				string text = "2D Map Control";
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

			//draw image
			pevent.Graphics.DrawImageSafe(source.Composite, mapArea,
				Theme.Current.Background.Light.Brush, CompositingMode.SourceCopy);

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
			
		}

		private void source_CompositeImageChanged(Map source)
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

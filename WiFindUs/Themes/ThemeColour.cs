using System;
using System.Drawing;

namespace WiFindUs.Themes
{
	public class ThemeColour : IDisposable
	{
		private Color color;
		private Brush brush;
		private Pen pen;
		private bool disposed = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The actual colour value used by this ThemeColour. All created pens, brushes, etc. will use this value.
		/// </summary>
		public Color Colour
		{
			get { return color; }
			private set { color = value; }
		}

		/// <summary>
		/// A SolidBrush with the colour of this ThemeColour.
		/// </summary>
		public Brush Brush
		{
			get
			{
				if (brush == null)
					brush = new SolidBrush(color);
				return brush;
			}
		}

		/// <summary>
		/// A Pen with the colour of this ThemeColour.
		/// </summary>
		public Pen Pen
		{
			get
			{
				if (pen == null)
					pen = new Pen(color);
				return pen;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public ThemeColour(Color color)
		{
			Colour = color;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
				Destroy();

			disposed = true;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void Destroy()
		{
			if (brush != null)
			{
				brush.Dispose();
				brush = null;
			}
			if (pen != null)
			{
				pen.Dispose();
				pen = null;
			}
		}
	}
}

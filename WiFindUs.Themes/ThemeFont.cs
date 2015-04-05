using System;
using System.Drawing;
using WiFindUs.Extensions;

namespace WiFindUs.Themes
{
	public class ThemeFont : IDisposable
	{
		public enum Style
		{
			Regular = 0,
			Bold = 1,
			Italic = 2,
			BoldItalic = 3
		}
		private string family;
		private float size;
		private Font[] fonts = new Font[4] {null,null,null,null};
		private bool disposed = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The font family of this ThemeFont.
		/// </summary>
		public String Family
		{
			get { return family; }
			private set
			{
				string val = value.Trim();
				if (val == null || val.Length == 0)
					throw new ArgumentException("Family", "Font family cannot be null or blank");
				family = value;
			}
		}

		/// <summary>
		/// The size of this ThemeFont.
		/// </summary>
		public float Size
		{
			get { return size; }
			private set { size = value.Clamp(2.0f, 1000f); }
		}

		/// <summary>
		/// 
		/// </summary>
		public Font Regular
		{
			get { return GetFont(Style.Regular); }
		}
		public Font Bold
		{
			get { return GetFont(Style.Bold); }
		}

		public Font Italic
		{
			get { return GetFont(Style.Italic); }
		}

		public Font BoldItalic
		{
			get { return GetFont(Style.BoldItalic); }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public ThemeFont(String family, float size)
		{
			Family = family;
			Size = size;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public Font GetFont(Style style)
		{
			uint index = (uint)style;
			if (fonts[index] == null)
			{
				FontStyle fs = FontStyle.Regular;
				if (style == Style.Bold || style == Style.BoldItalic)
					fs |= FontStyle.Bold;
				if (style == Style.Italic || style == Style.BoldItalic)
					fs |= FontStyle.Italic;
				fonts[index] = new Font(family, size, fs);
			}
			return fonts[index];
		}

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
			for (int i = 0; i < fonts.Length; i++)
			{
				if (fonts[i] != null)
				{
					fonts[i].Dispose();
					fonts[i] = null;
				}
			}
		}
	}
}

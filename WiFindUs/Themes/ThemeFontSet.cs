using System;
using WiFindUs.Extensions;

namespace WiFindUs.Themes
{
    public class ThemeFontSet : IDisposable
	{
		public enum Size
		{
			Tiny = 0,
			Small = 1,
			Normal = 2,
			Large = 3,
			Huge = 4,
		}
		private string baseFamily;
		private float baseSize;
		private ThemeFont[] fonts = new ThemeFont[5];
		private bool disposed = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The font family of this font set. All created ThemeFonts will be of this font family.
		/// </summary>
		public String BaseFamily
		{
			get { return baseFamily; }
			private set
			{
				string val = value.Trim();
				if (val == null || val.Length == 0)
					throw new ArgumentException("BaseFamily", "BaseFamily cannot be null or blank");
				baseFamily = value;
			}
		}

		/// <summary>
		/// The base size of this font set. All created ThemeFonts will be sized as a multiplier of this value,
		/// according to their type (e.g. the "Tiny" fonts will be this value * 0.4).
		/// </summary>
		public float BaseSize
		{
			get { return baseSize; }
			private set { baseSize = value.Clamp(2.0f, 1000f); }
		}

		/// <summary>
		/// ThemeFont created using this font set's BaseFamily, sized at 0.4 * BaseSize. 
		/// </summary>
		public ThemeFont Tiny
		{
			get { return GetFont(Size.Tiny); }
		}

		/// <summary>
		/// ThemeFont created using this font set's BaseFamily, sized at 0.7 * BaseSize. 
		/// </summary>
		public ThemeFont Small
		{
			get { return GetFont(Size.Small); }
		}

		/// <summary>
		/// ThemeFont created using this font set's BaseFamily, sized at BaseSize. 
		/// </summary>
		public ThemeFont Normal
		{
			get { return GetFont(Size.Normal); }
		}

		/// <summary>
		/// ThemeFont created using this font set's BaseFamily, sized 1.3 * BaseSize.
		/// </summary>
		public ThemeFont Large
		{
			get { return GetFont(Size.Large); }
		}

		/// <summary>
		/// ThemeFont created using this font set's BaseFamily, sized 2.0 * BaseSize.
		/// </summary>
		public ThemeFont Huge
		{
			get { return GetFont(Size.Huge); }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public ThemeFontSet(String baseFamily, float baseSize)
		{
			BaseFamily = baseFamily;
			BaseSize = baseSize;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public ThemeFont GetFont(Size size)
		{
			uint index = (uint)size;
			if (fonts[index] == null)
			{
				float sz = baseSize;
				switch (size)
				{
					case Size.Tiny: sz *= 0.4f; break;
					case Size.Small: sz *= 0.7f; break;
					case Size.Large: sz *= 1.3f; break;
					case Size.Huge: sz *= 2.0f; break;
				}
				fonts[index] = new ThemeFont(baseFamily, sz);
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

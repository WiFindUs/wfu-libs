using System;
using System.Drawing;

namespace WiFindUs.Themes
{
	public class Theme : IDisposable, ITheme
	{
		public static event Action<ITheme> ThemeChanged;
		private static Theme currentTheme = null;
		
		//coloursets
		public enum Colour
		{
			Foreground = 0,
			Background = 1,
			Highlight = 2,
			Error = 3,
			OK = 4,
			Warning = 5
		}
		private const int COLOURS_LENGTH = 6;
		private Color[] colours = new Color[COLOURS_LENGTH];
		private ThemeColourSet[] colourSets = new ThemeColourSet[COLOURS_LENGTH];
		
		//fontsets
		public enum Font
		{
			Controls = 0,
			Titles = 1,
			Monospaced = 2
		}
		private const int FONTS_LENGTH = 3;
		private string[] fontFamilies = new string[FONTS_LENGTH];
		private float[] fontSizes = new float[FONTS_LENGTH];
		private ThemeFontSet[] fontSets = new ThemeFontSet[FONTS_LENGTH];

		private bool disposed = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets or sets the current theme in use by this application.
		/// Setting the theme using this property is akin to calling ChangeCurrentTheme() with disposePrevious == true.
		/// If you do not wish the previous theme to be automatically disposed, use ChangeCurrentTheme() directly.
		/// </summary>
		public static Theme Current
		{
			get { return currentTheme; }
			set { ChangeCurrentTheme(value, true); }
		}

		public ThemeColourSet Foreground
		{
			get { return GetColour(Colour.Foreground); }
		}

		public ThemeColourSet Background
		{
			get { return GetColour(Colour.Background); }
		}

		public ThemeColourSet Highlight
		{
			get { return GetColour(Colour.Highlight); }
		}

		public ThemeColourSet Error
		{
			get { return GetColour(Colour.Error); }
		}

		public ThemeColourSet OK
		{
			get { return GetColour(Colour.OK); }
		}

		public ThemeColourSet Warning
		{
			get { return GetColour(Colour.Warning); }
		}

		public ThemeFontSet Controls
		{
			get { return GetFont(Font.Controls); }
		}

		public ThemeFontSet Titles
		{
			get { return GetFont(Font.Titles); }
		}

		public ThemeFontSet Monospaced
		{
			get { return GetFont(Font.Monospaced); }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public Theme(bool withDefaults = true)
		{
			for (int i = 0; i < COLOURS_LENGTH; i++)
				colours[i] = Color.Empty;

			for (int i = 0; i < FONTS_LENGTH; i++)
			{
				fontFamilies[i] = null;
				fontSizes[i] = -1.0f;
			}

			if (!withDefaults)
				return;

			SetFont(Font.Controls, "Segoe UI", 9.0f);
			SetFont(Font.Titles, "Segoe UI", 22.0f);
			SetFont(Font.Monospaced, "Consolas", 10.0f);

			SetColour(Colour.Foreground,	ColorTranslator.FromHtml("#BBBBBB"));
			SetColour(Colour.Background,	ColorTranslator.FromHtml("#252526"));
			SetColour(Colour.Highlight,	ColorTranslator.FromHtml("#007acc"));
			SetColour(Colour.Error,		ColorTranslator.FromHtml("#df3f26"));
			SetColour(Colour.OK,			ColorTranslator.FromHtml("#39903c"));
			SetColour(Colour.Warning,		ColorTranslator.FromHtml("#FF6600"));
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Sets the current colour and font theme in use by this application.
		/// </summary>
		/// <param name="newTheme">The new theme to apply.</param>
		/// <param name="disposePrevious">Whether or not to call Dispose() on the previous theme.</param>
		public static void ChangeCurrentTheme(Theme newTheme, bool disposePrevious)
		{
			if (newTheme == currentTheme)
				return;
			Theme old = currentTheme;
			currentTheme = newTheme;
			if (ThemeChanged != null)
				ThemeChanged(currentTheme);
			if (old != null && disposePrevious)
				old.Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public ITheme SetFont(Font font, string family, float size)
		{
			uint index = (uint)font;
			if (fontSets[index] != null)
				return this;
			fontFamilies[index] = family == null ? null : family.Trim();
			fontSizes[index] = size;
			return this;
		}

		public ITheme SetColour(Colour colour, Color value)
		{
			uint index = (uint)colour;
			if (colourSets[index] != null)
				return this;
			colours[index] = value;
			return this;
		}

		public ThemeFontSet GetFont(Font font)
		{
			uint index = (uint)font;
			if (fontSets[index] == null)
			{
				if (fontFamilies[index] == null || fontFamilies[index].Length == 0)
					throw new ArgumentException("font", "Font Family has not been assigned. You need to call SetFont to initialize the fontset you're trying to use.");
				if (fontSizes[index] <= 0.0f)
					throw new ArgumentException("font", "Font Size has not been assigned. You need to call SetFont to initialize the fontset you're trying to use.");
				fontSets[index] = new ThemeFontSet(fontFamilies[index], fontSizes[index]);
			}
			return fontSets[index];
		}

		public ThemeColourSet GetColour(Colour colour)
		{
			uint index = (uint)colour;
			if (colourSets[index] == null)
			{
				if (colours[index] == Color.Empty)
					throw new ArgumentException("colour", "Colour has not been assigned. You need to call SetColour to initialize the colourset you're trying to use.");
				colourSets[index] = new ThemeColourSet(colours[index]);
			}
			return colourSets[index];
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
			for (int i = 0; i < fontSets.Length; i++)
			{
				if (fontSets[i] != null)
				{
					fontSets[i].Dispose();
					fontSets[i] = null;
				}
			}

			for (int i = 0; i < colourSets.Length; i++)
			{
				if (colourSets[i] != null)
				{
					colourSets[i].Dispose();
					colourSets[i] = null;
				}
			}
		}

	}
}


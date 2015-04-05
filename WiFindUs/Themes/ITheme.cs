using System;
using System.Drawing;

namespace WiFindUs.Themes
{
	public interface ITheme
	{
		ThemeColourSet Foreground { get; }
		ThemeColourSet Background { get; }
		ThemeColourSet Highlight { get; }
		ThemeColourSet Error { get; }
		ThemeColourSet OK { get; }
		ThemeColourSet Warning { get; }

		ThemeFontSet Controls { get; }
		ThemeFontSet Titles { get; }
		ThemeFontSet Monospaced { get; }

		ThemeFontSet GetFont(Theme.Font font);
		ThemeColourSet GetColour(Theme.Colour colour);

		ITheme SetFont(Theme.Font font, string family, float size);
		ITheme SetColour(Theme.Colour colour, Color value);
	}
}

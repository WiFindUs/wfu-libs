using System.Drawing;

namespace WiFindUs.Extensions
{
    public static class ColorExtensions
	{
		/// <summary>
		/// Adjusts the brightness of a colour.
		/// </summary>
		/// <param name="colour">The base colour to adjust.</param>
		/// <param name="factor">The brightness of the new colour, from -1.0f (completely dark), to 1.0f (completely bright).</param>
		/// <returns>A brightness-adjusted version of the input colour.</returns>
		public static Color Adjust(this Color colour, float factor)
		{
			factor = factor.Clamp(-1.0f, 1.0f);
			if (factor.Tolerance(0.0f, 0.001f))
				return colour;

			float r = (float)colour.R;
			float g = (float)colour.G;
			float b = (float)colour.B;

			if (factor < 0.0f)
			{
				factor += 1.0f;
				r *= factor;
				g *= factor;
				b *= factor;
			}
			else
			{
				r = (255 - colour.R) * factor + colour.R;
				g = (255 - colour.G) * factor + colour.G;
				b = (255 - colour.B) * factor + colour.B;
			}

			return Color.FromArgb(colour.A, ((int)r).Clamp(0, 255), ((int)g).Clamp(0, 255), ((int)b).Clamp(0, 255));
		}

		/// <summary>
		/// Increases the brightness of a colour.
		/// </summary>
		/// <param name="colour">The base colour to lighten.</param>
		/// <param name="factor">The brightness of the new colour, from 0.0f (no change), to 1.0f (completely bright).</param>
		/// <returns>A lightened version of the input colour.</returns>
		public static Color Lighten(this Color colour, float factor)
		{
			return colour.Adjust(factor.Clamp(0.0f, 1.0f));
		}

		/// <summary>
		/// Increases the darkness of a colour.
		/// </summary>
		/// <param name="colour">The base colour to darken.</param>
		/// <param name="factor">The brightness of the new colour, from 0.0f (no change), to 1.0f (completely dark).</param>
		/// <returns>A darkened version of the input colour.</returns>
		public static Color Darken(this Color colour, float factor)
		{
			return colour.Adjust(factor.Clamp(0.0f, 1.0f) * -1.0f);
		}
	}
}

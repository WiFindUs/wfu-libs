using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace WiFindUs.Extensions
{
	public static class GraphicsExtensions
	{
		public enum GraphicsQuality
		{
			Low,
			Medium,
			High
		}

		public static void SetQuality(this Graphics graphics, GraphicsQuality quality)
		{
			if (quality == GraphicsQuality.High)
			{
				graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			}
			if (quality == GraphicsQuality.Medium)
			{
				graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
				graphics.CompositingQuality = CompositingQuality.HighSpeed;
				graphics.SmoothingMode = SmoothingMode.HighSpeed;
				graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
				graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			}
			else
			{
				graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
				graphics.CompositingQuality = CompositingQuality.HighSpeed;
				graphics.SmoothingMode = SmoothingMode.HighSpeed;
				graphics.InterpolationMode = InterpolationMode.Default;
				graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			}
		}

		public static void DrawImageSafe(this Graphics graphics, Image image, Rectangle area, Brush fallback)
		{
			//draw base image
			bool imageError = image == null;
			if (!imageError)
			{
				try
				{
					graphics.DrawImage(image, area);
				}
				catch
				{
					imageError = true;
				}
			}
			if (imageError && fallback != null)
				graphics.FillRectangle(fallback, area);
		}


		/// <summary>
		/// Resizes an image.
		/// </summary>
		/// <param name="input">The source image</param>
		/// <param name="w">The new width</param>
		/// <param name="h">The new height</param>
		/// <returns>A resized copy of the original image, or null if an error occurred.</returns>
		public static Image Resize(this Image input, int w, int h)
		{
			//check source
			if (input == null)
				return null;

			//check dimensions
			w = w < 0 ? 0 : w;
			h = h < 0 ? 0 : h;

			//create resized image
			Image output = new Bitmap(w, h);
			using (Graphics g = Graphics.FromImage(output))
			{
				g.SetQuality(GraphicsExtensions.GraphicsQuality.High);
				g.DrawImage(input, new System.Drawing.Rectangle(0, 0, output.Width, output.Height));
			}
			return output;
		}

		public static Stream GetStream(this Image input)
		{
			if (input == null)
				return null;

			Stream stream = new MemoryStream();
			input.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
			return stream;
		}

		public static void DrawCircle(this Graphics g, Pen pen,
								  float centerX, float centerY, float radius)
		{
			g.DrawEllipse(pen, centerX - radius, centerY - radius,
						  radius + radius, radius + radius);
		}

		public static void FillCircle(this Graphics g, Brush brush,
									  float centerX, float centerY, float radius)
		{
			g.FillEllipse(brush, centerX - radius, centerY - radius,
						  radius + radius, radius + radius);
		}
	}
}

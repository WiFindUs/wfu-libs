﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

		public class GraphicsQualitySettings
		{
			public TextRenderingHint TextRenderingHint;
			public CompositingQuality CompositingQuality;
			public SmoothingMode SmoothingMode;
			public InterpolationMode InterpolationMode;
			public PixelOffsetMode PixelOffsetMode;
		}

		public static GraphicsQualitySettings GetQuality(this Graphics graphics)
		{
			return new GraphicsQualitySettings()
			{
				TextRenderingHint = graphics.TextRenderingHint,
				CompositingQuality = graphics.CompositingQuality,
				SmoothingMode = graphics.SmoothingMode,
				InterpolationMode = graphics.InterpolationMode,
				PixelOffsetMode = graphics.PixelOffsetMode
			};
		}

		public static void SetQuality(this Graphics graphics, GraphicsQualitySettings qualitySettings)
		{
			if (graphics.TextRenderingHint != qualitySettings.TextRenderingHint)
				graphics.TextRenderingHint = qualitySettings.TextRenderingHint;
			if (graphics.CompositingQuality != qualitySettings.CompositingQuality)
				graphics.CompositingQuality = qualitySettings.CompositingQuality;
			if (graphics.SmoothingMode != qualitySettings.SmoothingMode)
				graphics.SmoothingMode = qualitySettings.SmoothingMode;
			if (graphics.InterpolationMode != qualitySettings.InterpolationMode)
				graphics.InterpolationMode = qualitySettings.InterpolationMode;
			if (graphics.PixelOffsetMode != qualitySettings.PixelOffsetMode)
				graphics.PixelOffsetMode = qualitySettings.PixelOffsetMode;
		}

		public static void SetQuality(this Graphics graphics, GraphicsQuality quality)
		{
			GraphicsQualitySettings settings = new GraphicsQualitySettings();
			if (quality == GraphicsQuality.High)
			{
				settings.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
				settings.CompositingQuality = CompositingQuality.HighQuality;
				settings.SmoothingMode = SmoothingMode.HighQuality;
				settings.InterpolationMode = InterpolationMode.HighQualityBicubic;
				settings.PixelOffsetMode = PixelOffsetMode.HighQuality;
			}
			if (quality == GraphicsQuality.Medium)
			{
				settings.TextRenderingHint = TextRenderingHint.SystemDefault;
				settings.CompositingQuality = CompositingQuality.HighSpeed;
				settings.SmoothingMode = SmoothingMode.HighSpeed;
				settings.InterpolationMode = InterpolationMode.HighQualityBilinear;
				settings.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			}
			else
			{
				settings.TextRenderingHint = TextRenderingHint.SystemDefault;
				settings.CompositingQuality = CompositingQuality.HighSpeed;
				settings.SmoothingMode = SmoothingMode.None;
				settings.InterpolationMode = InterpolationMode.NearestNeighbor;
				settings.PixelOffsetMode = PixelOffsetMode.Half;
			}
			graphics.SetQuality(settings);
		}

		public static void DrawImageSafe(this Graphics graphics, Image image, Rectangle area, Brush fallback,
			CompositingMode mode = CompositingMode.SourceOver)
		{
			//draw base image
			bool imageError = image == null;
			if (!imageError)
			{
				CompositingMode originalMode = graphics.CompositingMode;
				if (originalMode != mode)
					graphics.CompositingMode = mode;
				try
				{
					graphics.DrawImage(image, area);
				}
				catch
				{
					imageError = true;
				}
				if (originalMode != mode)
					graphics.CompositingMode = originalMode;
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
		public static Image Resize(this Image input, int w, int h, PixelFormat format = PixelFormat.Format32bppArgb)
		{
			//check source
			if (input == null)
				return null;

			//check dimensions
			w = w < 0 ? 0 : w;
			h = h < 0 ? 0 : h;

			//create resized image
			Image output = new Bitmap(w, h, format);
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

﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WiFindUs.Extensions
{
    public static class ImageExtensions
	{
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
			stream.Flush();
			return stream;
		}

		public static void G(this Image input, Action<Graphics> paintMethod,
			GraphicsExtensions.GraphicsQuality quality = GraphicsExtensions.GraphicsQuality.Low)
		{
			if (input == null || paintMethod == null)
				return;

			using (Graphics g = Graphics.FromImage(input))
			{
				g.SetQuality(quality);
				paintMethod(g);
			}
		}
	}
}

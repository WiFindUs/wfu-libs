using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        /// <summary>
        /// Resizes an image.
        /// </summary>
        /// <param name="input">The source image</param>
        /// <param name="w">The new width</param>
        /// <param name="h">The new height</param>
        /// <param name="resizedImage">The new resized image. Will be set to null if no resizing was necessary (if the source and destination dimensions were the same), or if an error occurred.</param>
        /// <returns>True if the operation was completed successfully.</returns>
        public static bool Resize(this Image input, int w, int h, out Image output)
        {
            output = null;

            //check source
            if (input == null)
                return false;

            //check dimensions
            w = w < 0 ? 0 : w;
            h = h < 0 ? 0 : h;
            if (w == input.Width && h == input.Height)
                return false;

            //create resized image
            output = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(output))
            {
                g.SetQuality(GraphicsExtensions.GraphicsQuality.High);
                g.DrawImage(input, new System.Drawing.Rectangle(0, 0, output.Width, output.Height));
            }
            return true;
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

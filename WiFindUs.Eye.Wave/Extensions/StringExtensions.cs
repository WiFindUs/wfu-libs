using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave.Extensions
{
	public static class StringExtensions
	{
		public static Texture2D Load(this string texturePath, GraphicsDevice device)
		{
			Texture2D tex;
			using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap("textures/white.png"))
				using (Stream stream = bmp.GetStream())
					tex = Texture2D.FromFile(device, stream);
			return tex;
		}
	}
}

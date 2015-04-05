using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye.Wave.Extensions
{
	public static class ColorExtensions
	{
		public static WaveEngine.Common.Graphics.Color Wave(this System.Drawing.Color color)
		{
			return new WaveEngine.Common.Graphics.Color(color.R, color.G, color.B, color.A);
		}

		public static System.Drawing.Color Sys(this WaveEngine.Common.Graphics.Color color)
		{
			return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
		}
	}
}

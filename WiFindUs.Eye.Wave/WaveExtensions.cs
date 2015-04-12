using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
	public static class WaveExtensions
	{
		public static Texture2D Load(this GraphicsDevice device, string texturePath)
		{
			Texture2D tex;
			using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(texturePath))
				using (Stream stream = bmp.GetStream())
					tex = Texture2D.FromFile(device, stream);
			return tex;
		}

		public static Vector3 LocationToVector(this Tile tile, Vector3 tl, Vector3 br, double latitude, double longitude)
		{
			float width = br.X - tl.X;
			float depth = br.Z - tl.Z;

			double w = (longitude - tile.NorthWest.Longitude.Value) / tile.LongitudinalSpan;
			double d = (tile.NorthWest.Latitude.Value - latitude) / tile.LatitudinalSpan;

			return new Vector3(
					tl.X + (float)(w * width),
					0,
					tl.Z + (float)(d * depth)
				);
		}

		public static Vector3 LocationToVector(this Tile tile, Vector3 tl, Vector3 br, ILocation location)
		{
			if (!location.HasLatLong)
				throw new ArgumentOutOfRangeException("location", "Location must contain latitude and longitude.");
			return LocationToVector(tile, tl, br, location.Latitude.Value, location.Longitude.Value);
		}

		public static WaveEngine.Common.Graphics.Color Wave(this System.Drawing.Color color)
		{
			return new WaveEngine.Common.Graphics.Color(color.R, color.G, color.B, color.A);
		}

		public static System.Drawing.Color Sys(this WaveEngine.Common.Graphics.Color color)
		{
			return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		public static bool SetVisible(this WaveEngine.Framework.Entity entity, bool visible)
		{
			if (visible != entity.IsVisible)
				entity.IsVisible = visible;
			return visible;
		}

		public static bool SetActive(this WaveEngine.Framework.Entity entity, bool active)
		{
			if (active != entity.IsActive)
				entity.IsActive = active;
			return active;
		}

		public static WaveEngine.Common.Graphics.Color Coserp(this WaveEngine.Common.Graphics.Color start,
			WaveEngine.Common.Graphics.Color finish, float amount)
		{
			return new WaveEngine.Common.Graphics.Color((byte)(((float)start.R).Coserp((float)finish.R, amount)),
				(byte)(((float)start.G).Coserp((float)finish.G, amount)),
				(byte)(((float)start.B).Coserp((float)finish.B, amount)),
				(byte)(((float)start.A).Coserp((float)finish.A, amount)));
		}
	}
}

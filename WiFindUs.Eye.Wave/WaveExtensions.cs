using System;
using System.IO;
using System.Linq;
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

		public static WaveEngine.Common.Graphics.Color Wave(this System.Drawing.Color color, byte alpha)
		{
			return new WaveEngine.Common.Graphics.Color(color.R, color.G, color.B, alpha);
		}

		public static WaveEngine.Common.Graphics.Color Wave(this System.Drawing.Color color)
		{
			return Wave(color, color.A);
		}

		public static System.Drawing.Color Sys(this WaveEngine.Common.Graphics.Color color, byte alpha)
		{
			return System.Drawing.Color.FromArgb(alpha, color.R, color.G, color.B);
		}

		public static System.Drawing.Color Sys(this WaveEngine.Common.Graphics.Color color)
		{
			return Sys(color, color.A);
		}

		public static WaveEngine.Common.Graphics.Color Coserp(this WaveEngine.Common.Graphics.Color start,
			WaveEngine.Common.Graphics.Color finish, float amount)
		{
			return new WaveEngine.Common.Graphics.Color((byte)(((float)start.R).Coserp((float)finish.R, amount)),
				(byte)(((float)start.G).Coserp((float)finish.G, amount)),
				(byte)(((float)start.B).Coserp((float)finish.B, amount)),
				(byte)(((float)start.A).Coserp((float)finish.A, amount)));
		}

		public static BoundingBox Transform(this BoundingBox box, ref Matrix transform)
		{
			BoundingBox bb = new BoundingBox(box.Min, box.Max);
			Vector3.Transform(ref bb.Min, ref transform, out bb.Min);
			Vector3.Transform(ref bb.Max, ref transform, out bb.Max);
			return bb;
		}

		public static Vector2 ScreenCoords(this Transform3D t3d, Camera3D camera)
		{
			Vector3 v = (t3d.Position - camera.Position), u;
			v.Normalize();
			v += camera.Position;
			camera.Project(ref v, out u);
			return new Vector2(u.X, u.Y);
		}

		public static Vector2 Add(this Vector2 v2, float x, float y)
		{
			return new Vector2(v2.X + x, v2.Y + y);
		}

		public static Vector2 Subtract(this Vector2 v2, float x, float y)
		{
			return Add(v2, -x, -y);
		}

		public static void WithComponent<T>(this Entity entity, Action<T> action) where T : Component
		{
			if (entity == null || action == null)
				return;
			T t = entity.FindComponent<T>();
			if (t != null)
				action(t);
		}
	}
}

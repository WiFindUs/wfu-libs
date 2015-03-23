using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Math;

namespace WiFindUs.Eye.Wave.Extensions
{
	public static class RegionExtensions
	{
		public static Vector3 LocationToVector(this Region region, Vector3 tl, Vector3 br, double latitude, double longitude)
		{
			float width = br.X - tl.X;
			float depth = br.Z - tl.Z;

			double w = (longitude - region.NorthWest.Longitude.Value) / region.LongitudinalSpan;
			double d = (region.NorthWest.Latitude.Value - latitude) / region.LatitudinalSpan;

			return new Vector3(
					tl.X + (float)(w * width),
					0,
					tl.Z + (float)(d * depth)
				);
		}

		public static Vector3 LocationToVector(this Region region, Vector3 tl, Vector3 br, ILocation location)
		{
			if (!location.HasLatLong)
				throw new ArgumentOutOfRangeException("location", "Location must contain latitude and longitude.");
			return LocationToVector(region, tl, br, location.Latitude.Value, location.Longitude.Value);
		}
	}
}

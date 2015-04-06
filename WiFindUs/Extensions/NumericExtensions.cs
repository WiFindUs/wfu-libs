using System;

namespace WiFindUs.Extensions
{
	public static class NumericExtensions
	{
		public const double DEG_TO_RAD = Math.PI / 180.0;
		public const double RAD_TO_DEG = 180.0 / Math.PI;

		/////////////////////////////////////////////////////////////////////
		// INTS
		/////////////////////////////////////////////////////////////////////

		public static int WholePercentage(this int current, int max)
		{
			return (int)(((double)current / (double)max) * 100.0);
		}

		public static int Clamp(this int value, int min, int max)
		{
			return value < min ? min : (value > max ? max : value);
		}

		/////////////////////////////////////////////////////////////////////
		// FLOATS
		/////////////////////////////////////////////////////////////////////

		public static float Lerp(this float start, float finish, float amount)
		{
			return (1.0f - amount) * start + amount * finish;
		}

		public static float Coserp(this float start, float finish, float amount)
		{
			return start + (finish - start) * ((1.0f + (float)Math.Cos(amount * Math.PI)) * 0.5f);
		}

		public static bool Tolerance(this float val, float other, float tolerance)
		{
			return Math.Abs(val - other) <= tolerance;
		}

		public static bool Tolerance(this float? val, float? other, float tolerance)
		{
			if (other.HasValue != val.HasValue
				|| (val.HasValue && Math.Abs(val.Value - other.Value) > tolerance))
				return false;
			return true;
		}

		public static float ToRadians(this float degrees)
		{
			return (float)(degrees * DEG_TO_RAD);
		}

		public static float ToDegrees(this float radians)
		{
			return (float)(radians * RAD_TO_DEG);
		}

		public static float Clamp(this float value, float min, float max)
		{
			return value < min ? min : (value > max ? max : value);
		}

		/////////////////////////////////////////////////////////////////////
		// DOUBLES
		/////////////////////////////////////////////////////////////////////

		public static double Lerp(this double start, double finish, double amount)
		{
			return (1.0 - amount) * start + amount * finish;
		}

		public static double Coserp(this double start, double finish, double amount)
		{
			return start + (finish - start) * ((1.0 + Math.Cos(amount * Math.PI)) * 0.5);
		}


		public static bool Tolerance(this double val, double other, double tolerance)
		{
			return Math.Abs(val - other) <= tolerance;
		}

		public static bool Tolerance(this double? val, double? other, double tolerance)
		{
			if (other.HasValue != val.HasValue
				|| (val.HasValue && Math.Abs(val.Value - other.Value) > tolerance))
				return false;
			return true;
		}

		public static double ToRadians(this double degrees)
		{
			return degrees * DEG_TO_RAD;
		}

		public static double ToDegrees(this double radians)
		{
			return radians * RAD_TO_DEG;
		}

		public static double Clamp(this double value, double min, double max)
		{
			return value < min ? min : (value > max ? max : value);
		}

		/////////////////////////////////////////////////////////////////////
		// DECIMALS
		/////////////////////////////////////////////////////////////////////

		public static bool Tolerance(this decimal val, decimal other, decimal tolerance)
		{
			return Math.Abs(val - other) <= tolerance;
		}

		public static bool Tolerance(this decimal? val, decimal? other, decimal tolerance)
		{
			if (other.HasValue != val.HasValue
				|| (val.HasValue && Math.Abs(val.Value - other.Value) > tolerance))
				return false;
			return true;
		}

		/////////////////////////////////////////////////////////////////////
		// DATETIMES
		/////////////////////////////////////////////////////////////////////

		public static ulong ToUnixTimestamp(this DateTime dt)
		{
			return (ulong)(dt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;
		}

		public static DateTime ToDateTime(this long unixTimestamp)
		{
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimestamp).ToLocalTime();
			return dtDateTime;
		}
	}
}

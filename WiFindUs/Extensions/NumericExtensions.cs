using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Extensions
{
    public static class NumericExtensions
    {
        public const double DEG_TO_RAD = Math.PI / 180.0;
        public const double RAD_TO_DEG = 180.0 / Math.PI;
        
        /////////////////////////////////////////////////////////////////////
        // FLOATS
        /////////////////////////////////////////////////////////////////////

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
        
        /////////////////////////////////////////////////////////////////////
        // DOUBLES
        /////////////////////////////////////////////////////////////////////

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

        public static long ToUnixTimestamp(this DateTime dt)
        {
            return (long)(dt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;
        }

        public static DateTime ToDateTime(this long unixTimestamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimestamp).ToLocalTime();
            return dtDateTime;
        }
    }
}

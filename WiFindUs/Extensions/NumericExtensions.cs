﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Extensions
{
    public static class NumericExtensions
    {
        /////////////////////////////////////////////////////////////////////
        // FLOATS
        /////////////////////////////////////////////////////////////////////

        public static float ToRadians(this float val)
        {
            return (float)(Math.PI / 180.0) * val;
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
        
        /////////////////////////////////////////////////////////////////////
        // DOUBLES
        /////////////////////////////////////////////////////////////////////

        public static double ToRadians(this double val)
        {
            return (Math.PI / 180.0) * val;
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

        /////////////////////////////////////////////////////////////////////
        // DECIMALS
        /////////////////////////////////////////////////////////////////////

        public static decimal ToRadians(this decimal val)
        {
            return ((decimal)Math.PI / 180.0m) * val;
        }

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

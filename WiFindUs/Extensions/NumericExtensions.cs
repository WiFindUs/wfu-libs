using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Extensions
{
    public static class NumericExtensions
    {
        /////////////////////////////////////////////////////////////////////
        // DOUBLES
        /////////////////////////////////////////////////////////////////////

        public static double ToRadians(this double val)
        {
            return (Math.PI / 180) * val;
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
            return ((decimal)Math.PI / 180m) * val;
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs
{
    public static class MathHelper
    {
        public static double Lerp(double start, double finish, double amount)
        {
            return (1.0 - amount) * start + amount * finish;
        }

        public static double Coserp(double start, double finish, double amount)
        {
            double prc = (1.0 - Math.Cos(amount * Math.PI)) * 0.5;
            return start * (1.0 - prc) + finish * prc;
        }
    }
}

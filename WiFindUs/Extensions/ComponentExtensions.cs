using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WiFindUs.Extensions
{
    public static class ComponentExtensions
    {
        public static bool IsDesignMode(this Component comp)
        {
            return (comp.Site != null && comp.Site.DesignMode)
                || LicenseManager.UsageMode == LicenseUsageMode.Designtime
                || AppDomain.CurrentDomain.FriendlyName.Equals("DefaultDomain");
        }
    }
}

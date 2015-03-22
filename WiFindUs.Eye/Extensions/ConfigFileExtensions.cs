using System;
using WiFindUs.IO;

namespace WiFindUs.Eye.Extensions
{
	public static class ConfigFileExtensions
	{
		public static ILocation Get(this ConfigFile config, string key, int index, ILocation defaultValue)
		{
			//try named values first
			string locationName = config.Get(key, index, "");
			if (locationName.Length > 0)
			{
				ILocation loc = Location.FromName(locationName);
				if (loc != null)
					return loc;
			}

			//now parse for lat/long
			double[] locArray = WFUApplication.Config.Get(key, index, (double[])null);
			if (locArray == null)
				return null;
			else
			{
				ILocation loc = null;
				try
				{
					loc = new Location(locArray);
				}
				catch (Exception) { }

				if (loc != null)
					return loc;
			}

			return null;
		}

		public static ILocation Get(this ConfigFile config, string key, ILocation defaultValue)
		{
			return Get(config, key, 0, defaultValue);
		}

		public static ILocation Get(this ConfigFile config, string key)
		{
			return Get(config, key, 0, null);
		}
	}
}

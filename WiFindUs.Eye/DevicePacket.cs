using System;
using System.Net;

namespace WiFindUs.Eye
{
	public class DevicePacket : EyePacket, ILocation, IBatteryStats
	{
		private double? latitude, longitude, altitude, accuracy, batteryLevel;
		private bool? charging;
		private bool? gpsEnabled, gpsHasFix;
		private string deviceType = null;
		private uint? userID;
		private uint? nodeNumber;

		public double? Latitude
		{
			get { return latitude; }
		}

		public double? Longitude
		{
			get { return longitude; }
		}

		public double? Accuracy
		{
			get { return accuracy; }
		}

		public double? Altitude
		{
			get { return altitude; }
		}

		public string DeviceType
		{
			get { return deviceType; }
		}

		public bool? IsGPSEnabled
		{
			get { return gpsEnabled; }
		}

		public bool? IsGPSFixed
		{
			get { return gpsHasFix; }
		}

		public uint? UserID
		{
			get { return userID; }
		}

		public uint? NodeNumber
		{
			get { return nodeNumber; }
		}

		public bool HasLatLong
		{
			get
			{
				return Latitude.HasValue
					&& Longitude.HasValue;
			}
		}

		public bool EmptyLocation
		{
			get
			{
				return !Latitude.HasValue
					&& !Longitude.HasValue
					&& !Accuracy.HasValue
					&& !Altitude.HasValue;
			}
		}

		public double DistanceTo(ILocation other)
		{
			return WiFindUs.Eye.Location.Distance(this, other);
		}

		public bool? Charging
		{
			get { return charging; }
		}

		public double? BatteryLevel
		{
			get { return batteryLevel; }
		}

		public bool EmptyBatteryStats
		{
			get
			{
				return !Charging.HasValue && !BatteryLevel.HasValue;
			}
		}

		public DevicePacket(IPEndPoint sender, string type, uint id, ulong timestamp, string payload)
			: base(sender, type, id, timestamp, payload)
		{
			//check packet
			if (type.CompareTo("DEV") != 0)
				throw new ArgumentOutOfRangeException("packet", "Attempt to create a DevicePacket from an eye packet other than type DEV!");
		}

		protected override bool ProcessPayloadKVP(string key, string value)
		{
			try
			{
				switch (key)
				{
					case "dt": deviceType = value; return true;
					case "gps": gpsEnabled = UInt32.Parse(value) == 1; return true;
					case "fix": gpsHasFix = UInt32.Parse(value) == 1; return true;
					case "lat": latitude = LocationComponent(value); return true;
					case "long": longitude = LocationComponent(value); return true;
					case "acc": accuracy = LocationComponent(value); return true;
					case "alt": altitude = LocationComponent(value); return true;
					case "chg": charging = UInt32.Parse(value) == 1; return true;
					case "batt": batteryLevel = Double.Parse(value); return true;
					case "user": userID = UInt32.Parse(value, System.Globalization.NumberStyles.HexNumber); return true;
					case "node": nodeNumber = UInt32.Parse(value); return true;
				}
			}
			catch (FormatException) { }
			return false;
		}
	}
}

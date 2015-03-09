using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public class DevicePacket : EyePacket, ILocation, IBatteryStats
    {
        private double? latitude, longitude, altitude, accuracy, batteryLevel;
        private bool? charging;
        private string deviceType = null;
        private long? userID;

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

        public long? UserID
        {
            get { return userID; }
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

        public DevicePacket(IPEndPoint sender, string type, long id, long timestamp, string payload)
            : base(sender, type, id, timestamp, payload)
        {
            //check packet
            if (type.CompareTo("DEV") != 0)
                throw new ArgumentOutOfRangeException("packet", "Attempt to create a DevicePacket from an eye packet other than type DEV!");
            
            //check for payload
            if (Payload.Length == 0)
                return;
            string[] payloads = Payload.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (payloads == null || payloads.Length == 0)
                return;

            //parse arguments
            foreach (string token in payloads)
            {
                Match match = PACKET_KVP.Match(token);
                if (!match.Success)
                    continue;
                String val = match.Groups[2].Value.Trim();
                switch (match.Groups[1].Value.ToLower())
                {
                    case "dt": deviceType = val; break;
                    case "lat": latitude = LocationComponent(val); break;
                    case "long": longitude = LocationComponent(val); break;
                    case "acc": accuracy = LocationComponent(val); break;
                    case "alt": altitude = LocationComponent(val); break;
                    case "chg": charging = Int32.Parse(val) == 1; break;
                    case "batt": batteryLevel = Double.Parse(val); break;
                    case "user":
                        try
                        {
                            userID = Int64.Parse(val, System.Globalization.NumberStyles.HexNumber);
                        }
                        catch (FormatException) { }
                        break;
                }
            }
        }
    }
}

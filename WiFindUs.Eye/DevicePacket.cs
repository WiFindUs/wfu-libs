using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public class DevicePacket : ILocation, ILocatable, IBatteryStats
    {
        private static readonly Regex PACKET_KVP
            = new Regex("^([a-zA-Z0-9_\\-.]+)\\s*[:=]\\s*(.+)\\s*$");
        
        private EyePacket packet;
        private double? latitude, longitude, altitude, accuracy, batteryLevel;
        private bool? charging;
        private string deviceType = "PHO";
        private long userID = -1;

        public EyePacket Packet
        {
            get { return packet; }
        }

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

        public long ID
        {
            get { return packet.ID; }
        }

        public long UserID
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

        public ILocation Location
        {
            get { return this; }
        }

        public bool? Charging
        {
            get { return charging; }
        }

        public double? BatteryLevel
        {
            get { return batteryLevel; }
        }

        public DevicePacket(EyePacket packet)
        {
            //check packet
            if (packet == null)
                throw new ArgumentNullException("packet");
            if (packet.Type.CompareTo("DEV") != 0)
                throw new ArgumentOutOfRangeException("packet", "Attempt to create a DevicePacket from an EyePacket type other than DEV!");
            
            //check for payload
            if (packet.Payload == null || packet.Payload.Length == 0)
                throw new ArgumentOutOfRangeException("packet", "EyePacket did not contain a payload!");
            this.packet = packet;
            string[] payloads = packet.Payload.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (payloads == null || payloads.Length == 0)
                throw new ArgumentOutOfRangeException("packet", "EyePacket did not contain a useful payload!");

            //parse arguments
            foreach (string token in payloads)
            {
                Match match = PACKET_KVP.Match(token);
                if (!match.Success)
                    continue;
                switch (match.Groups[1].Value.ToLower())
                {
                    case "dt": deviceType = match.Groups[2].Value; break;
                    case "lat": latitude = Double.Parse(match.Groups[2].Value); break;
                    case "long": longitude = Double.Parse(match.Groups[2].Value); break;
                    case "acc": accuracy = Double.Parse(match.Groups[2].Value); break;
                    case "alt": altitude = Double.Parse(match.Groups[2].Value); break;
                    case "chg": charging = Int32.Parse(match.Groups[2].Value) == 1; break;
                    case "batt": batteryLevel = Double.Parse(match.Groups[2].Value); break;
                    case "user":
                        try
                        {
                            userID = Int64.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                        }
                        catch (FormatException) { }
                        break;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}, {4}, {5}, {6}]",
                packet.ToString(),
                latitude.GetValueOrDefault(),
                longitude.GetValueOrDefault(),
                accuracy.GetValueOrDefault(),
                altitude.GetValueOrDefault(),
                batteryLevel.GetValueOrDefault(),
                deviceType);
        }
    }
}

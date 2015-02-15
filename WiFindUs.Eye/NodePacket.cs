using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public class NodePacket : EyePacket, ILocation
    {
        private double? latitude, longitude, altitude, accuracy;
        private int? satellites;
        private long? number;
        private bool? meshPoint, apDaemon, dhcpDaemon, gpsDaemon;
        private int[] meshPeers;

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

        public int? VisibleSatellites
        {
            get { return satellites; }
        }

        public bool? IsGPSDaemonRunning
        {
            get { return gpsDaemon; }
        }

        public bool? IsDHCPDaemonRunning
        {
            get { return dhcpDaemon; }
        }

        public bool? IsMeshPoint
        {
            get { return meshPoint; }
        }

        public bool? IsAPDaemonRunning
        {
            get { return apDaemon; }
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

        public long? Number
        {
            get { return number; }
        }

        public int[] MeshPeers
        {
            get { return meshPeers; }
        }

        public double DistanceTo(ILocation other)
        {
            return WiFindUs.Eye.Location.Distance(this, other);
        }

        public NodePacket(IPEndPoint sender, string type, long id, long timestamp, string payload)
            : base(sender, type, id, timestamp, payload)
        {
            Debugger.C("NodePacket()");
            
            //check packet
            if (type.CompareTo("NODE") != 0)
                throw new ArgumentOutOfRangeException("packet", "Attempt to create a NodePacket from an eye packet other than type NODE!");
            
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
                    //[11:49:13 PM,013] EYE{1|mp:1|ap:1|dhcp:1|gps:1|sats:10}}
                    case "lat": latitude = Double.Parse(val); break;
                    case "long": longitude = Double.Parse(val); break;
                    case "acc": accuracy = Double.Parse(val); break;
                    case "alt": altitude = Double.Parse(val); break;
                    case "num": number = Int64.Parse(val); break;
                    case "sats": satellites = Int32.Parse(val); break;
                    case "mp": meshPoint = Int32.Parse(val) == 1; break;
                    case "ap": apDaemon = Int32.Parse(val) == 1; break;
                    case "dhcp": dhcpDaemon = Int32.Parse(val) == 1; break;
                    case "gps": gpsDaemon = Int32.Parse(val) == 1; break;
                    case "mpl":
                        if (val.CompareTo("0") == 0)
                        {
                            meshPeers = new int[0];
                            break;
                        }
                        string[] peers = match.Groups[2].Value.Trim().Split( new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                        if (peers.Length == 0)
                        {
                            meshPeers = new int[0];
                            break;
                        }
                        List<int> peerList = new List<int>();
                        foreach (String peer in peers)
                        {
                            String p = peer.Trim();
                            if (p.Length == 0)
                                continue;
                            try
                            {
                                int n = Int32.Parse(p);
                                if (n <= 0 || n >= 255 || peerList.Contains(n))
                                    continue;
                                peerList.Add(n);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        peerList.Sort();
                        meshPeers = peerList.ToArray();
                        break;
                }
            }
        }
    }
}

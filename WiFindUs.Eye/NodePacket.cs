using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace WiFindUs.Eye
{
	public class NodePacket : EyePacket, ILocation
	{
		private static readonly Regex REGEX_MESH_POINT //num, signal strength, link speed
			= new Regex(@"([0-9]+)\(([+-]?[0-9]+(?:[.][0-9]+)?)\)\(([+-]?[0-9]+(?:[.][0-9]+)?)\)",RegexOptions.Compiled);
		private double? latitude, longitude, altitude, accuracy;
		private uint? satellites;
		private uint? number;
		private bool? meshPoint, apDaemon, dhcpDaemon, gpsDaemon, gpsFake;
		private LinkData[] links;

		public class LinkData
		{
			public uint NodeNumber;
			public double? SignalStrength;
			public double? LinkSpeed;
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

		public uint? VisibleSatellites
		{
			get { return satellites; }
		}

		public bool? IsGPSDaemonRunning
		{
			get { return gpsDaemon; }
		}

		public bool? IsGPSFake
		{
			get { return gpsFake; }
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

		public uint? Number
		{
			get { return number; }
		}

		public LinkData[] NodeLinks
		{
			get { return links; }
		}

		public double DistanceTo(ILocation other)
		{
			return WiFindUs.Eye.Location.Distance(this, other);
		}

		public NodePacket(IPEndPoint sender, string type, uint id, ulong timestamp, string payload)
			: base(sender, type, id, timestamp, payload)
		{
			//check packet
			if (type.CompareTo("NODE") != 0)
				throw new ArgumentOutOfRangeException("packet", "Attempt to create a NodePacket from an eye packet other than type NODE!");
		}

		protected override bool ProcessPayloadKVP(string key, string value)
		{
			try
			{
				switch (key)
				{
					case "lat": latitude = LocationComponent(value); return true;
					case "long": longitude = LocationComponent(value); return true;
					case "acc": accuracy = LocationComponent(value); return true;
					case "alt": altitude = LocationComponent(value); return true;
					case "num": number = UInt32.Parse(value); return true;
					case "sats": satellites = UInt32.Parse(value); return true;
					case "mp": meshPoint = UInt32.Parse(value) == 1; return true;
					case "ap": apDaemon = UInt32.Parse(value) == 1; return true;
					case "dhcp": dhcpDaemon = UInt32.Parse(value) == 1; return true;
					case "gps":
						uint gpsVal = UInt32.Parse(value);
						gpsDaemon = gpsVal == 1 || gpsVal == 2;
						gpsFake = gpsVal == 2;
						return true;
					case "mpl":
						if (value.CompareTo("0") == 0)
						{
							links = new LinkData[0];
							return true;
						}
						string[] peers = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
						if (peers.Length == 0)
						{
							links = new LinkData[0];
							return true;
						}
						List<LinkData> peerList = new List<LinkData>();
						List<uint> knownPeers = new List<uint>();
						foreach (String peer in peers)
						{
							String p = peer.Trim();
							if (p.Length == 0)
								continue;
							Match match = REGEX_MESH_POINT.Match(p);
							if (!match.Success)
								continue;
							try
							{
								uint n = UInt32.Parse(match.Groups[1].Value);
								if (n == 0 || n >= 255 || knownPeers.Contains(n))
									continue;
								LinkData link = new LinkData() { NodeNumber = n };
								double val = 0.0;
								if (Double.TryParse(match.Groups[2].Value, out val))
									link.SignalStrength = val;
								if (Double.TryParse(match.Groups[3].Value, out val))
									link.LinkSpeed = val;
								knownPeers.Add(n);
								peerList.Add(link);
							}
							catch
							{
								continue;
							}
						}
						links = peerList.ToArray();
						return true;
				}
			}
			catch (FormatException) { }
			return false;
		}
	}
}

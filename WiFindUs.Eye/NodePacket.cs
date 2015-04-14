using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace WiFindUs.Eye
{
	public class NodePacket : EyePacket, ILocation
	{
		private const ulong FLAG_MESH_POINT		= 1;
		private const ulong FLAG_ACCESS_POINT	= 2;
		private const ulong FLAG_DHCPD			= 4;
		private const ulong FLAG_GPSD			= 8;
		private const ulong FLAG_GPSD_FAKE		= 16;
		private static readonly Regex REGEX_MESH_POINT //num, signal strength, link speed
			= new Regex(@"([0-9]+);([+-]?[0-9]+(?:[.][0-9]+)?);([+-]?[0-9]+(?:[.][0-9]+)?)",RegexOptions.Compiled);

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

		public uint? SatelliteCount
		{
			get { return satellites; }
		}

		public bool? GPSD
		{
			get { return gpsDaemon; }
		}

		public bool? MockLocation
		{
			get { return gpsFake; }
		}

		public bool? DHCPD
		{
			get { return dhcpDaemon; }
		}

		public bool? MeshPoint
		{
			get { return meshPoint; }
		}

		public bool? AccessPoint
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

		public NodePacket(string type, uint id, ulong timestamp, string payload)
			: base(type, id, timestamp, payload)
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
					case "flg":
						ulong flags = UInt64.Parse(value);
						meshPoint = ((flags & FLAG_MESH_POINT) == FLAG_MESH_POINT);
						apDaemon = ((flags & FLAG_ACCESS_POINT) == FLAG_ACCESS_POINT);
						dhcpDaemon = ((flags & FLAG_DHCPD) == FLAG_DHCPD);
						gpsFake = ((flags & FLAG_GPSD_FAKE) == FLAG_GPSD_FAKE);
						gpsDaemon = gpsFake.GetValueOrDefault() || ((flags & FLAG_GPSD) == FLAG_GPSD);
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

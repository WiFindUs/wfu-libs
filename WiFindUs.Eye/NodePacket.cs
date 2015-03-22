﻿using System;
using System.Collections.Generic;
using System.Net;

namespace WiFindUs.Eye
{
	public class NodePacket : EyePacket, ILocation
	{
		private double? latitude, longitude, altitude, accuracy;
		private uint? satellites;
		private uint? number;
		private bool? meshPoint, apDaemon, dhcpDaemon, gpsDaemon;
		private uint[] meshPeers;

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

		public uint[] MeshPeers
		{
			get { return meshPeers; }
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
					case "gps": gpsDaemon = UInt32.Parse(value) == 1; return true;
					case "mpl":
						if (value.CompareTo("0") == 0)
						{
							meshPeers = new uint[0];
							return true;
						}
						string[] peers = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
						if (peers.Length == 0)
						{
							meshPeers = new uint[0];
							return true;
						}
						List<uint> peerList = new List<uint>();
						foreach (String peer in peers)
						{
							String p = peer.Trim();
							if (p.Length == 0)
								continue;
							try
							{
								uint n = UInt32.Parse(p);
								if (n == 0 || n >= 255 || peerList.Contains(n))
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
						return true;
				}
			}
			catch (FormatException) { }
			return false;
		}
	}
}

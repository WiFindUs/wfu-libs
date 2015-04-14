using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WiFindUs.Eye
{
	public class EyePacketListener : IDisposable
	{
		public const int PORT_FIRST = 33339;
		public const int PORT_LAST = 33345;
		public const int PORT_COUNT = PORT_LAST - PORT_FIRST;

		public event Action<EyePacketListener, DevicePacket> DevicePacketReceived;
		public event Action<EyePacketListener, NodePacket> NodePacketReceived;

		private static readonly Regex REGEX_EYE_MESSAGE = new Regex(
			@"^EYE\{([A-Za-z]+)\|([0-9A-Fa-f]+)\|([0-9A-Fa-f]+)\s*(?:\{\s*(.*)\s*\}\s*)?\}$", RegexOptions.Compiled);
		private Thread[] threads = new Thread[PORT_COUNT];
		private ConcurrentDictionary<string, ConcurrentDictionary<uint, ulong>> timestamps
			= new ConcurrentDictionary<string, ConcurrentDictionary<uint, ulong>>();
		private volatile bool listening = false;
		private volatile bool cancel = false;
		private volatile bool disposed = false;
		private volatile bool logPackets = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public bool LogPackets
		{
			get { return logPackets; }
			set { logPackets = value; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public EyePacketListener(bool startImmediately = true)
		{
			if (startImmediately)
				Start();
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public void Start()
		{
			if (listening || WiFindUs.Forms.MainForm.HasClosed)
				return;
			listening = true;
			cancel = false;
			for (int i = 0; i < PORT_COUNT; i++)
			{
				threads[i] = new Thread(new ParameterizedThreadStart(ListenThread));
				threads[i].IsBackground = true;
				threads[i].Start(PORT_FIRST + i);
			}
		}

		public void Stop()
		{
			if (!listening)
				return;
			cancel = true;
			for (int i = 0; i < PORT_COUNT; i++)
				threads[i] = null;
			listening = false;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				Stop();
				DevicePacketReceived = null;
				NodePacketReceived = null;
			}

			disposed = true;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void ListenThread(object portObj)
		{
			int port = (int)portObj;
			int index = port - PORT_FIRST;
			Debugger.V("Eye listener thread started on port {0}.", port);
			UdpClient listener;
			try
			{
				listener = new UdpClient(port);
			}
			catch (SocketException)
			{
				Debugger.E("Error creating UDP listener on port {0}", port);
				listener = null;
			}

			if (listener != null)
			{
				while (!cancel && !WiFindUs.Forms.MainForm.HasClosed)
				{
					IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
					byte[] bytes = null;
					try
					{
						bytes = listener.Receive(ref endPoint);
					}
					catch (Exception) { }

					if (cancel || WiFindUs.Forms.MainForm.HasClosed)
						break;
					if (bytes == null || bytes.Length == 0)
						continue;
					string message = Encoding.UTF8.GetString(bytes).Trim();

					Match match = REGEX_EYE_MESSAGE.Match(message);
					if (!match.Success)
						continue;

					//get identifiers
					string type = match.Groups[1].Value.Trim().ToUpper();
					uint id = UInt32.Parse(match.Groups[2].Value.ToUpper(), System.Globalization.NumberStyles.HexNumber);
					ulong timestamp = UInt64.Parse(match.Groups[3].Value.ToUpper(), System.Globalization.NumberStyles.HexNumber);

					//check for existing timestamp
					ConcurrentDictionary<uint, ulong> idTimestamps = null;
					if (!timestamps.TryGetValue(type, out idTimestamps))
						idTimestamps = timestamps[type] = new ConcurrentDictionary<uint, ulong>();
					ulong lastTimeStamp;
					if (!idTimestamps.TryGetValue(id, out lastTimeStamp))
						lastTimeStamp = 0;

					//check timestamp age, discard if same or older
					if (timestamp <= lastTimeStamp)
						continue;
					idTimestamps[id] = timestamp;

					if (logPackets)
						Debugger.V(message);

					Type eyePacketType = null;
					switch (type)
					{
						case "DEV":
							if (DevicePacketReceived != null)
								eyePacketType = typeof(DevicePacket);
							break;
						case "NODE":
							if (NodePacketReceived != null)
								eyePacketType = typeof(NodePacket);
							break;
					}
					if (eyePacketType != null)
					{
						EyePacket packet = (EyePacket)eyePacketType.GetConstructor(
							new Type[]
							{
								typeof(IPEndPoint),
								typeof(String),
								typeof(uint),
								typeof(ulong),
								typeof(String)
							})
							.Invoke(new object[]
							{
								endPoint, //sender
								type, //type
								id, //id
								timestamp, //timestamp
								match.Groups[4].Value //payload
							});
						switch (type)
						{
							case "DEV": DevicePacketReceived(this, packet as DevicePacket); break;
							case "NODE": NodePacketReceived(this, packet as NodePacket); break;
						}
					}
					else
						Debugger.W("EyePacket received with unknown type: \"{0}\"", type);
				}
				if (listener != null)
				{
					try { listener.Close(); }
					catch { }
					listener = null;
				}
			}
			Debugger.I("Eye listener thread terminated.");
		}
	}
}

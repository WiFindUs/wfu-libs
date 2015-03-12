using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public class EyePacketListener : IDisposable
    {
        public event Action<EyePacketListener, DevicePacket> DevicePacketReceived;
        public event Action<EyePacketListener, NodePacket> NodePacketReceived;
        
        private static readonly Regex REGEX_EYE_MESSAGE = new Regex(
            @"^EYE\{([A-Za-z]+)\|([0-9A-Fa-f]+)\|([0-9A-Fa-f]+)\s*(?:\{\s*(.*)\s*\}\s*)?\}$", RegexOptions.Compiled);
        private int port = 33339;
        private Thread thread;
        private bool cancel = false;
        private UdpClient listener = null;
        private bool disposed = false;
        private Dictionary<string, Dictionary<ulong, ulong>> timestamps = new Dictionary<string, Dictionary<ulong, ulong>>();
        private bool logPackets = false;

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

        public EyePacketListener(int port = 33339, bool startImmediately = true)
        {
            if (port < 1024 || port > 65535)
                throw new ArgumentOutOfRangeException("port", "Port must be between 1024 and 65535 (inclusive)");
            this.port = port;

            if (startImmediately)
                Start();
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public void Start()
        {
            if (thread != null)
                return;
            cancel = false;
            thread = new Thread(new ThreadStart(ListenThread));
            thread.Start();
        }

        public void Stop()
        {
            cancel = true;
            if (listener != null)
            {
                listener.Close();
                listener = null;
            }
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
                thread = null;
            }

            disposed = true;
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void ListenThread()
        {
            Debugger.I("Eye listener thread started.");
            try
            {
                listener = new UdpClient(port);
            }
            catch (SocketException)
            {
                Debugger.E("Error creating UDP listener on port {0}", port);
            }

            if (listener != null)
            {
                while (!cancel)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                    byte[] bytes = null;
                    try
                    {
                        bytes = listener.Receive(ref endPoint);
                    }
                    catch (Exception) { }

                    if (cancel)
                        break;
                    if (bytes == null)
                        continue;
                    string message = Encoding.UTF8.GetString(bytes).Trim();

                    Match match = REGEX_EYE_MESSAGE.Match(message);
                    if (!match.Success)
                        continue;

                    //get identifiers
                    string type = match.Groups[1].Value.Trim().ToUpper();
                    ulong id = UInt64.Parse(match.Groups[2].Value.ToUpper(), System.Globalization.NumberStyles.HexNumber);
                    ulong timestamp = UInt64.Parse(match.Groups[3].Value.ToUpper(), System.Globalization.NumberStyles.HexNumber);

                    //check for existing timestamp
                    Dictionary<ulong, ulong> idTimestamps = null;
                    if (!timestamps.TryGetValue(type, out idTimestamps))
                        idTimestamps = timestamps[type] = new Dictionary<ulong, ulong>();
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
                }
                if (listener != null)
                {
                    listener.Close();
                    listener = null;
                }
            }
            thread = null;
            Debugger.I("Eye listener thread terminated.");
        }
    }
}

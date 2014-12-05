﻿using System;
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
        public event Action<EyePacket> PacketReceived;
        
        private static readonly Regex REGEX_EYE_MESSAGE = new Regex(
            @"^EYE\|(DEV|NODE)\|([0-9A-F]+)\|([0-9A-F]+)\|\s*(.+)\s*$", RegexOptions.Compiled);
        private int port = 33339;
        private Thread thread;
        private bool cancel = false;
        private UdpClient listener = null;
        private bool disposed = false;
        private Dictionary<string, Dictionary<long, long>> timestamps = new Dictionary<string, Dictionary<long, long>>();

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
                PacketReceived = null;
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
                Debugger.E("Error creating UDP listener on port " + port);
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
                    catch (Exception)
                    {
                    }

                    if (cancel)
                        break;
                    if (bytes == null)
                        continue;
                    string message = Encoding.UTF8.GetString(bytes);

                    Match match = REGEX_EYE_MESSAGE.Match(message);
                    if (!match.Success)
                        continue;

                    //get identifiers
                    string type = match.Groups[1].Value;
                    long timestamp = Int64.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
                    long id = Int64.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);

                    //check for existing timestamp
                    Dictionary<long, long> idTimestamps = null;
                    if (!timestamps.TryGetValue(type, out idTimestamps))
                        idTimestamps = timestamps[type] = new Dictionary<long, long>();
                    long lastTimeStamp;
                    if (!idTimestamps.TryGetValue(id, out lastTimeStamp))
                        lastTimeStamp = -1;

                    //check timestamp age, discard if same or older
                    if (timestamp <= lastTimeStamp)
                        continue;
                    idTimestamps[id] = timestamp;

                    if (PacketReceived != null)
                    {
                        EyePacket packet = new EyePacket(
                            endPoint, //sender
                            type, //type
                            id, //id
                            timestamp, //timestamp
                            match.Groups[4].Value //payload
                            );

                        PacketReceived(packet);
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
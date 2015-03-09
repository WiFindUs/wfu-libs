using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public class EyePacket
    {
        protected static readonly Regex PACKET_KVP
            = new Regex("^([a-zA-Z0-9_\\-.]+)\\s*[:=]\\s*(.+)\\s*$");
        
        private IPAddress address;
        private int port;
        private string type;
        private ulong id;
        private ulong timestamp;
        private string payload;

        public ulong ID
        {
            get { return id; }
        }

        public ulong Timestamp
        {
            get { return timestamp; }
        }

        public IPAddress Address
        {
            get { return address; }
        }

        public int Port
        {
            get { return port; }
        }

        public string Type
        {
            get { return type; }
        }

        public string Payload
        {
            get { return payload; }
        }

        public EyePacket(IPEndPoint sender, string type, ulong id, ulong timestamp, string payload)
        {
            if (sender == null)
                throw new ArgumentNullException("sender", "Sender cannot be null");
            if (sender.Address == null || sender.Address == IPAddress.None)
                throw new ArgumentOutOfRangeException("sender", "Sender did not contain valid IPv4 addressing information");
            this.address = sender.Address;
            this.port = sender.Port;
            this.type = (type ?? "");
            this.id = id;
            this.timestamp = timestamp;
            this.payload = (payload ?? "").Trim();
        }

        public override string ToString()
        {
            return String.Format("[{0}:{1}, {2}, {3}, {4}, \"{5}\"]", address.ToString(),
                port,
                type,
                id.ToString("X"),
                timestamp, payload);
        }

        protected static Double? LocationComponent(string input)
        {
            if (input == null || input.Length == 0 || input.CompareTo("?") == 0)
                return null;
            return Double.Parse(input);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public abstract class EyePacket
    {
        private static readonly Regex PACKET_KVP
            = new Regex("^([a-zA-Z0-9_\\-.]+)\\s*[:=]\\s*(.+)\\s*$", RegexOptions.Compiled);
        
        private IPAddress address;
        private int port;
        private string type;
        private uint id;
        private ulong timestamp;
        private string payload;

        public uint ID
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

        public EyePacket(IPEndPoint sender, string type, uint id, ulong timestamp, string payload)
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
            
            //check for payload
            this.payload = (payload ?? "").Trim();
            if (this.payload.Length == 0)
                return;
            string[] payloads = Payload.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (payloads == null || payloads.Length == 0)
                return;
            
            //parse payload arguments
            foreach (string token in payloads)
            {
                Match match = PACKET_KVP.Match(token);
                if (!match.Success)
                    continue;
                ProcessPayloadKVP(match.Groups[1].Value.Trim().ToLower(), match.Groups[2].Value.Trim());
            }
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

        protected abstract bool ProcessPayloadKVP(string key, string value);
    }
}

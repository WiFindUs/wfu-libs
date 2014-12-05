using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public class EyePacket
    {
        private IPAddress address;
        private int port;
        private string type;
        private long id;
        private long timestamp;
        private string payload;

        public long ID
        {
            get { return id; }
        }

        public long Timestamp
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

        public EyePacket(IPEndPoint sender, string type, long id, long timestamp, string payload)
        {
            this.address = sender.Address;
            this.port = sender.Port;
            this.type = type;
            this.id = id;
            this.timestamp = timestamp;
            this.payload = payload;
        }

        public override string ToString()
        {
            return String.Format("[{0}:{1}, {2}, {3}, {4}]", address.ToString(),
                port,
                type,
                id.ToString("X"),
                timestamp);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiFindUs.Controls;
using WiFindUs.Eye.Wave;
using WiFindUs.Forms;
using WiFindUs.Extensions;
using WiFindUs.Eye.Extensions;

namespace WiFindUs.Eye
{
    public class EyeMainForm : MainForm, IMapForm
    {
        private EyeContext eyeContext = null;
        private EyePacketListener eyeListener = null;
        private IEnumerable<Device> devices;
        private IEnumerable<Node> nodes;
        private IEnumerable<DeviceHistory> deviceHistories;
        private IEnumerable<NodeHistory> nodeHistories;
        private IEnumerable<User> users;
        private IEnumerable<Waypoint> waypoints;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        protected virtual MapControl Map
        {
            get { return null; }
        }

        protected virtual IEnumerable<Device> Devices
        {
            get { return devices;  }
        }

        protected virtual IEnumerable<Node> Nodes
        {
            get { return nodes; }
        }

        protected virtual IEnumerable<DeviceHistory> DeviceHistories
        {
            get { return deviceHistories; }
        }

        protected virtual IEnumerable<NodeHistory> NodeHistories
        {
            get { return nodeHistories; }
        }

        protected virtual IEnumerable<User> Users
        {
            get { return users; }
        }

        protected virtual IEnumerable<Waypoint> Waypoints
        {
            get { return waypoints; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public EyeMainForm()
        {
            if (DesignMode)
                return;
            if (WFUApplication.UsesMySQL)
                eyeContext = WFUApplication.MySQLDataContext as WiFindUs.Eye.EyeContext;
            else
            {
                devices = new List<Device>();
                nodes = new List<Node>();
                deviceHistories = new List<DeviceHistory>();
                nodeHistories = new List<NodeHistory>();
                users = new List<User>();
                waypoints = new List<Waypoint>();
            }
            if (WFUApplication.Config.Get("server.enabled", true))
                eyeListener = new EyePacketListener(WFUApplication.Config.Get("server.udp_port", 33339));
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public void RenderMap()
        {
            if (Map != null)
                Map.Render();
        }

        public Device Device(long id, out bool isNew)
        {
            if (id < 0)
            {
                isNew = false;
                return null;
            }

            //fetch
            Device device = null;
            try
            {
                if (eyeContext != null)
                    device = eyeContext.Devices.Where(d => d.ID == id).Single();
                else
                {
                    foreach (Device d in devices)
                        if (d.ID == id)
                        {
                            device = d;
                            break;
                        }
                }

            }
            catch { }

            //create
            if (device == null)
            {
                long ts = DateTime.UtcNow.UnixTimestamp();
                Devices.InsertOnSubmit(device = new Device()
                {
                    ID = id,
                    Created = ts,
                    Updated = ts
                });
                isNew = true;
            }
            else
                isNew = false;

            return device;
        }

        public User User(long id, out bool isNew)
        {
            if (id < 0)
            {
                isNew = false;
                return null;
            }

            //fetch
            User user = null;
            try
            {
                user = Users.Where(d => d.ID == id).Single();
            }
            catch { }

            //create
            if (user == null)
            {
                Users.InsertOnSubmit(user = new User()
                {
                    ID = id,
                    NameFirst = "",
                    NameLast = "",
                    NameMiddle = "",
                    Type = ""
                });
                isNew = true;
            }
            else
                isNew = false;

            return user;
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnFirstShown(EventArgs e)
        {
            base.OnFirstShown(e);
            if (Map != null)
            {
                MapScene.SceneStarted += MapSceneStarted;
                Map.StartMapApplication();
            }
        }

        protected override void OnThemeChanged(Theme theme)
        {
            base.OnThemeChanged(theme);
            if (Map != null)
                Map.BackColor = theme.ControlDarkColour;
        }

        protected override void OnDisposing()
        {            
            if (Map != null)
            {
                Controls.Remove(Map);
                Map.Dispose();
            }
            
            if (eyeListener != null)
            {
                eyeListener.PacketReceived -= EyePacketReceived;
                eyeListener.Dispose();
                eyeListener = null;
            }
            base.OnDisposing();
        }

        protected virtual void EyePacketReceived (EyePacket obj)
        {
            if (obj.Type.CompareTo("DEV") == 0)
            {
                //parse packet for validity
                DevicePacket devicePacket;
                try
                {
                    devicePacket = new DevicePacket(obj);
                    if (devicePacket.ID <= -1)
                        return;
                }
                catch (Exception)
                {
                    Debugger.E("Malformed device update packet recieved from " + obj.Address.ToString());
                    return;
                }
                Debugger.V(devicePacket.ToString());

                //first get user
                bool newUser = false;
                User user = devicePacket.UserID >= 0 ? EyeContext.User(devicePacket.UserID, out newUser) : null;

                //now get device
                long ts = DateTime.UtcNow.ToUnixTimestamp();
                bool newDevice = false;
                Device device = null;
                try
                {
                    device = EyeContext.Devices.Where(d => d.ID == devicePacket.ID).Single();
                }
                catch { }
                if (device == null)
                {
                    newDevice = true;
                    EyeContext.Devices.InsertOnSubmit(device = new Device()
                    {
                        ID = devicePacket.ID,
                        Created = ts,
                        Updated = ts,
                        User = user,
                        Type = devicePacket.DeviceType,
                        Location = devicePacket
                    });
                }
                else
                {
                    device.Updated = ts;
                    device.User = user;
                    if (!WiFindUs.Eye.Location.Equals(device.Location, devicePacket))
                        device.Location = devicePacket;
                }

                if (newDevice || newUser)
                    EyeContext.SubmitChangesThreaded();
            }
        }

        protected virtual void MapSceneStarted(MapScene obj)
        {
            ILocation location = WFUApplication.Config.Get("map.center", (ILocation)null);
            if (location == null)
                Debugger.E("Could not parse map.center from config files!");
            else
                obj.CenterLocation = location;
            Map.Theme = Theme;

            if (eyeListener != null)
                eyeListener.PacketReceived += EyePacketReceived;
        }

       // ,
    }
}

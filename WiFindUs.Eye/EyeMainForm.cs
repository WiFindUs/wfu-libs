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
using System.Collections;
using System.Windows.Forms;

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
        private Timer sqlSubmitTimer;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        protected override List<Func<bool>> LoadingTasks
        {
            get
            {
                List<Func<bool>> tasks = base.LoadingTasks;
                tasks.Add(InitializeEntites);
                tasks.Add(PreCacheUsers);
                tasks.Add(PreCacheDevices);
                tasks.Add(PreCacheDeviceHistories);
                tasks.Add(PreCacheNodes);
                tasks.Add(PreCacheNodeHistories);
                tasks.Add(PreCacheWaypoints);
                tasks.Add(StartServerThread);
                return tasks;
            }
        }

        protected virtual MapControl Map
        {
            get { return null; }
        }

        public virtual IEnumerable<Device> Devices
        {
            get { return devices;  }
        }

        public virtual IEnumerable<Node> Nodes
        {
            get { return nodes; }
        }

        public virtual IEnumerable<DeviceHistory> DeviceHistories
        {
            get { return deviceHistories; }
        }

        public virtual IEnumerable<NodeHistory> NodeHistories
        {
            get { return nodeHistories; }
        }

        public virtual IEnumerable<User> Users
        {
            get { return users; }
        }

        public virtual IEnumerable<Waypoint> Waypoints
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
            isNew = false;
            if (id < 0)
                return null;

            //fetch
            Device device = null;
            if (eyeContext != null)
            {
                try
                {
                    device = eyeContext.Devices.Where(d => d.ID == id).Single();
                }
                catch (Exception e)
                {
                    Debugger.Ex(e);
                    return null;
                }
            }
            else
            {
                foreach (Device d in devices)
                    if (d.ID == id)
                    {
                        device = d;
                        break;
                    }
            }

            //create
            if (device == null)
            {
                long ts = DateTime.UtcNow.ToUnixTimestamp();
                device = new Device()
                {
                    ID = id,
                    Created = ts,
                    Updated = ts
                };
                if (eyeContext == null)
                    ((List<Device>)devices).Add(device);
                isNew = true;
            }

            return device;
        }

        public User User(long id, out bool isNew)
        {
            isNew = false;
            if (id < 0)
                return null;

            //fetch
            User user = null;
            if (eyeContext != null)
            {
                try
                {
                    user = Users.Where(d => d.ID == id).Single();
                }
                catch (Exception e)
                {
                    Debugger.Ex(e);
                    return null;
                }
            }
            else
            {
                foreach (User u in users)
                    if (u.ID == id)
                    {
                        user = u;
                        break;
                    }
            }


            //create
            if (user == null)
            {
                user = new User()
                {
                    ID = id,
                    NameFirst = "",
                    NameLast = "",
                    NameMiddle = "",
                    Type = ""
                };
                if (eyeContext == null)
                    ((List<User>)users).Add(user);
                isNew = true;
            }

            return user;
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnFirstShown(EventArgs e)
        {
            base.OnFirstShown(e);
            MapScene.SceneStarted += MapSceneStarted;
            if (Map != null)
                Map.StartMapApplication();
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
                Map.CancelThreads();
                Controls.Remove(Map);
                Map.Dispose();
            }

            if (sqlSubmitTimer != null)
            {
                sqlSubmitTimer.Tick -= SqlSubmitTimerTick;
                sqlSubmitTimer.Enabled = false;
                sqlSubmitTimer.Dispose();
            }
            
            if (eyeListener != null)
            {
                eyeListener.PacketReceived -= EyePacketReceived;
                eyeListener.Dispose();
                eyeListener = null;
            }
            base.OnDisposing();
        }

        protected virtual void EyePacketReceived(EyePacket obj)
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
                User user = User(devicePacket.UserID, out newUser);

                //now get device
                bool newDevice = false;
                Device device = Device(devicePacket.ID, out newDevice);
                device.User = user;
                if (!WiFindUs.Eye.Location.Equals(device.Location, devicePacket))
                    device.Location = devicePacket;
                if (newDevice)
                    device.Type = devicePacket.DeviceType;
                else 
                    device.Updated = DateTime.UtcNow.ToUnixTimestamp();

                if (eyeContext != null && (newDevice || newUser))
                    eyeContext.SubmitChangesThreaded();
            }
        }

        protected virtual void MapSceneStarted(MapScene obj)
        {
            MapScene.SceneStarted -= MapSceneStarted;
            ILocation location = WFUApplication.Config.Get("map.center", (ILocation)null);
            if (location == null)
                Debugger.E("Could not parse map.center from config files!");
            else
                obj.CenterLocation = location;
            Map.Theme = Theme;
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private bool InitializeEntites()
        {
            WFUApplication.SplashStatus = "Initializing entity collections";
            if (WFUApplication.UsesMySQL)
            {
                eyeContext = WFUApplication.MySQLDataContext as WiFindUs.Eye.EyeContext;

                devices = eyeContext.Devices;
                deviceHistories = eyeContext.DeviceHistories;
                nodes = eyeContext.Nodes;
                nodeHistories = eyeContext.NodeHistories;
                users = eyeContext.Users;
                waypoints = eyeContext.Waypoints;

                sqlSubmitTimer = new Timer();
                sqlSubmitTimer.Interval = WFUApplication.Config.Get("mysql.submit_rate", 0, 10000);
                sqlSubmitTimer.Tick += SqlSubmitTimerTick;
                sqlSubmitTimer.Enabled = true;
            }
            else
            {
                devices = new List<Device>();
                deviceHistories = new List<DeviceHistory>();
                nodes = new List<Node>();
                nodeHistories = new List<NodeHistory>();
                users = new List<User>();
                waypoints = new List<Waypoint>();
            }
            return true;
        }

        private bool PreCacheUsers()
        {
            if (!WFUApplication.UsesMySQL)
                return true;
            WFUApplication.SplashStatus = "Pre-caching users";
            foreach (User user in Users)
                ;
            return true;
        }

        private bool PreCacheDevices()
        {
            if (!WFUApplication.UsesMySQL)
                return true;
            WFUApplication.SplashStatus = "Pre-caching devices";
            foreach (Device device in Devices)
                ;
            return true;
        }

        private bool PreCacheDeviceHistories()
        {
            if (!WFUApplication.UsesMySQL)
                return true;
            WFUApplication.SplashStatus = "Pre-caching device history";
            foreach (DeviceHistory history in DeviceHistories)
                ;
            return true;
        }

        private bool PreCacheNodes()
        {
            if (!WFUApplication.UsesMySQL)
                return true;
            WFUApplication.SplashStatus = "Pre-caching nodes";
            foreach (Node node in Nodes)
                ;
            return true;
        }

        private bool PreCacheNodeHistories()
        {
            if (!WFUApplication.UsesMySQL)
                return true;
            WFUApplication.SplashStatus = "Pre-caching node history";
            foreach (NodeHistory history in NodeHistories)
                ;
            return true;
        }

        private bool PreCacheWaypoints()
        {
            if (!WFUApplication.UsesMySQL)
                return true;
            WFUApplication.SplashStatus = "Pre-caching waypoints";
            foreach (Waypoint waypoint in Waypoints)
                ;
            return true;
        }

        private bool StartServerThread()
        {
            if (!WFUApplication.Config.Get("server.enabled", true))
                return true;
            WFUApplication.SplashStatus = "Starting server thread";
            try
            {
                eyeListener = new EyePacketListener(WFUApplication.Config.Get("server.udp_port", 33339));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                String message = "There was an error creating the server thread: " + ex.Message;
                Debugger.E(message);
                MessageBox.Show(message + "\n\nThe application will now exit.", "Server thread creation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            eyeListener.PacketReceived += EyePacketReceived;
            return true;
        }

        private void SqlSubmitTimerTick(object sender, EventArgs e)
        {
            eyeContext.SubmitChangesThreaded();
        }
    }
}

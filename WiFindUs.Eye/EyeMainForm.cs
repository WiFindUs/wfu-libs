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
        private Timer timer;
        private long sqlSubmitInterval = 10000, sqlSubmitTimer = 0;
        private long timeoutCheckInterval = 1000, timeoutCheckTimer = 0;
        private List<IUpdateable> updateables = new List<IUpdateable>();
        private bool serverMode = false;
        private MapControl map;

        //non-mysql collections (client mode):
        private Dictionary<long, Device> devices;
        private Dictionary<long, Node> nodes;
        private Dictionary<long, List<DeviceHistory>> deviceHistories; //device id, history list
        private Dictionary<long, List<NodeHistory>> nodeHistories;
        private Dictionary<long, User> users;
        private Dictionary<long, Waypoint> waypoints;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public bool ServerMode
        {
            get { return serverMode; }
        }

        public virtual IEnumerable<Device> Devices
        {
            get { return serverMode ? (IEnumerable<Device>)eyeContext.Devices : devices.Values; }
        }

        public virtual IEnumerable<Node> Nodes
        {
            get { return serverMode ? (IEnumerable<Node>)eyeContext.Nodes : nodes.Values; }
        }

        public virtual IEnumerable<User> Users
        {
            get { return serverMode ? (IEnumerable<User>)eyeContext.Users : users.Values; }
        }

        public virtual IEnumerable<Waypoint> Waypoints
        {
            get { return serverMode ? (IEnumerable<Waypoint>)eyeContext.Waypoints : waypoints.Values; }
        }

        public override List<Func<bool>> LoadingTasks
        {
            get
            {
                List<Func<bool>> tasks = base.LoadingTasks;
                tasks.Add(InitializeApplicationMode);
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

        protected string MySQLConnectionString
        {
            get
            {
                return "Host=" + WFUApplication.Config.Get("mysql.address", "localhost")
                        + ";Port=" + WFUApplication.Config.Get("mysql.port", 3306)
                        + ";Persist Security Info=True"
                        + ";User Id=" + WFUApplication.Config.Get("mysql.username", "root")
                        + ";Database=" + WFUApplication.Config.Get("mysql.database", "")
                        + ";Password=" + WFUApplication.Config.Get("mysql.password", "") + ";";
            }
        }

        protected MapControl Map
        {
            get { return map; }
            set
            {
                if (map != null || value == null)
                    return;
                map = value;
                map.ApplicationStarting += MapApplicationStarting;
                map.SceneStarted += MapSceneStarted;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public EyeMainForm()
        {
            WiFindUs.Eye.Device.OnDeviceCreated += OnDeviceCreated;
            WiFindUs.Eye.Node.OnNodeCreated += OnNodeCreated;
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
            if (ServerMode)
            {
                try
                {
                    var devices = eyeContext.Devices.Where(d => d.ID == id);
                    if (devices == null || devices.Count() > 1)
                        return null;
                    if (devices.Count() == 1)
                        device = devices.Single();
                }
                catch (Exception e)
                {
                    Debugger.Ex(e);
                    return null;
                }
            }
            else
            {
                devices.TryGetValue(id, out device);
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
                if (ServerMode)
                    eyeContext.Devices.InsertOnSubmit(device);
                else
                    devices[id] = device;
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
            if (ServerMode)
            {
                try
                {
                    var users = eyeContext.Users.Where(u => u.ID == id);
                    if (users == null || users.Count() > 1)
                        return null;
                    if (users.Count() == 1)
                        user = users.Single();
                }
                catch (Exception e)
                {
                    Debugger.Ex(e);
                    return null;
                }
            }
            else
            {
                users.TryGetValue(id, out user);
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
                if (ServerMode)
                    eyeContext.Users.InsertOnSubmit(user);
                else
                    users[id] = user;
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
            if (DesignMode)
                return;

            //start map scene
            if (Map != null)
                Map.StartMapApplication();

            //start timer
            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += TimerTick;
            timer.Enabled = true;
        }

        protected override void OnThemeChanged(Theme theme)
        {
            base.OnThemeChanged(theme);
            if (DesignMode)
                return;
            if (Map != null)
            {
                Map.BackColor = theme.ControlDarkColour;
                if (Map.Scene != null)
                    Map.Scene.Theme = Theme;
            }
        }

        protected override void OnDisposing()
        {            
            if (Map != null)
            {
                Map.CancelThreads();
                Map.Dispose();
            }

            if (timer != null)
            {
                timer.Tick -= TimerTick;
                timer.Enabled = false;
                timer.Dispose();
            }
            
            if (eyeListener != null)
            {
                eyeListener.PacketReceived -= EyePacketReceived;
                eyeListener.Dispose();
                eyeListener = null;
            }

            if (eyeContext != null)
            {
                //apply any pending database changes
                try { eyeContext.SubmitChanges(); }
                catch (Exception e) { Debugger.Ex(e, false); }

                eyeContext.Dispose();
                eyeContext = null;
            }
            base.OnDisposing();
        }

        private void EyePacketReceived(EyePacket obj)
        {
            if (!ServerMode)
                return;
            
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
                if (device == null) //error
                    return;
                device.User = user;
                if (!WiFindUs.Eye.Location.Equals(device.Location, devicePacket))
                    device.Location = devicePacket;
                if (newDevice)
                    device.Type = devicePacket.DeviceType;
                else 
                    device.Updated = DateTime.UtcNow.ToUnixTimestamp();

                if (newDevice || newUser)
                    eyeContext.SubmitChanges();
            }
        }

        protected virtual void MapSceneStarted(MapScene obj)
        {
            ILocation location = WFUApplication.Config.Get("map.center", (ILocation)null);
            if (location == null)
                Debugger.E("Could not parse map.center from config files!");
            else
                obj.CenterLocation = location;
            Map.Scene.Theme = Theme;
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private bool InitializeApplicationMode()
        {
            serverMode = WFUApplication.Config.Get("server", false);
            if (serverMode)
            {
                WFUApplication.SplashStatus = "Creating MySQL Context";

                try
                {
                    eyeContext = new EyeContext(MySQLConnectionString);
                    Debugger.I("MySQL connection created OK.");
                }
                catch (Exception ex)
                {
                    String message = "There was an error establishing the MySQL connection: " + ex.Message;
                    Debugger.E(message);
                    MessageBox.Show(message + "\n\nThe application will now exit.", "MySQL Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                sqlSubmitInterval = WFUApplication.Config.Get("mysql.auto_submit_rate", 0, 10000);
                timeoutCheckInterval = WFUApplication.Config.Get("server.timeout_check_rate", 0, 1000);

                Debugger.I("Application running in SERVER mode.");
            }
            else
            {
                WFUApplication.SplashStatus = "Initializing entity collections";
                devices = new Dictionary<long, Device>();
                nodes = new Dictionary<long, Node>();
                deviceHistories = new Dictionary<long, List<DeviceHistory>>();
                nodeHistories = new Dictionary<long, List<NodeHistory>>();
                users = new Dictionary<long, User>();
                waypoints = new Dictionary<long, Waypoint>();

                Debugger.I("Application running in CLIENT mode.");
            }
            return true;
        }

        /// <summary>
        /// Pre-caches the user collection.
        /// </summary>
        /// <remarks>This probably seems pointless, but it isn't; LinqConnect doesn't fully
        /// initialize entities until you iterate over them at least once, so this forces
        /// that to take place.</remarks>
        /// <returns></returns>
        private bool PreCacheUsers()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching users";
            foreach (User user in eyeContext.Users)
                ;
            return true;
        }

        private bool PreCacheDevices()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching devices";
            foreach (Device device in eyeContext.Devices)
                device.CheckTimeout();
            return true;
        }

        private bool PreCacheDeviceHistories()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching device history";
            foreach (DeviceHistory history in eyeContext.DeviceHistories)
                ;
            return true;
        }

        private bool PreCacheNodes()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching nodes";
            foreach (Node node in eyeContext.Nodes)
                ;
            return true;
        }

        private bool PreCacheNodeHistories()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching node history";
            foreach (NodeHistory history in eyeContext.NodeHistories)
                ;
            return true;
        }

        private bool PreCacheWaypoints()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching waypoints";
            foreach (Waypoint waypoint in eyeContext.Waypoints)
                ;
            return true;
        }

        private bool StartServerThread()
        {
            if (!ServerMode)
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

        private void OnDeviceCreated(Device obj)
        {
            updateables.Add(obj);
        }

        private void OnNodeCreated(Node obj)
        {
            updateables.Add(obj);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (!ServerMode)
                return;

            if (timeoutCheckInterval > 0)
            {
                timeoutCheckTimer += timer.Interval;
                if (timeoutCheckTimer >= timeoutCheckInterval)
                {
                    timeoutCheckTimer = 0;
                    foreach (IUpdateable updateable in updateables)
                        updateable.CheckTimeout();
                }
            }

            if (sqlSubmitInterval > 0)
            {
                sqlSubmitTimer += timer.Interval;
                if (sqlSubmitTimer >= sqlSubmitInterval)
                {
                    sqlSubmitTimer = 0;
                    eyeContext.SubmitChangesThreaded();
                }
            }
        }

        private void MapApplicationStarting(MapControl map)
        {
            if (WFUApplication.Config != null)
                map.BackBufferScale = WFUApplication.Config.Get("map.resolution_scale", 1.0f);
        }
    }
}

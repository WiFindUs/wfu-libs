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
using Devart.Data.Linq;
using System.ComponentModel;

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
        private SelectableEntityGroup defaultEntityGroup = new SelectableEntityGroup();

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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ServerMode
        {
            get { return serverMode; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable<Device> Devices
        {
            get { return serverMode ? (IEnumerable<Device>)eyeContext.Devices : devices.Values; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable<Node> Nodes
        {
            get { return serverMode ? (IEnumerable<Node>)eyeContext.Nodes : nodes.Values; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable<User> Users
        {
            get { return serverMode ? (IEnumerable<User>)eyeContext.Users : users.Values; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable<Waypoint> Waypoints
        {
            get { return serverMode ? (IEnumerable<Waypoint>)eyeContext.Waypoints : waypoints.Values; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected MapControl Map
        {
            get { return map; }
            set
            {
                if (map != null || value == null)
                    return;
                map = value;
                map.Theme = Theme;
                map.ApplicationStarting += MapApplicationStarting;
                map.SceneStarted += MapSceneStarted;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISelectableEntityGroup DefaultEntitySelectionGroup
        {
            get { return defaultEntityGroup; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public EyeMainForm()
        {
            WiFindUs.Eye.Device.OnDeviceLoaded += OnDeviceLoaded;
            WiFindUs.Eye.Node.OnNodeLoaded += OnNodeLoaded;
            WiFindUs.Eye.Waypoint.OnWaypointLoaded += OnWaypointLoaded;
            WiFindUs.Eye.User.OnUserLoaded += OnUserLoaded;
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
                    var dev = from d in eyeContext.Devices where d.ID == id select d;
                    foreach (Device d in dev) //force load
                    {
                        if (device == null)
                            device = d;
                    }
                }
                catch (Exception e)
                {
                    Debugger.Ex(e);
                    return null;
                }
            }
            else
                devices.TryGetValue(id, out device);

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
                    var usr = from u in eyeContext.Users where u.ID == id select u;
                    foreach (User u in usr) //force load
                    {
                        if (user == null)
                            user = u;
                    }
                }
                catch (Exception e)
                {
                    Debugger.Ex(e);
                    return null;
                }
            }
            else
                users.TryGetValue(id, out user);

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

        private void EyePacketReceived(EyePacketListener sender, EyePacket obj)
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
                
                if (sender.LogPackets)
                    Debugger.V(devicePacket.ToString());

                //first get user
                bool newUser = false;
                User user = User(devicePacket.UserID, out newUser);

                //now get device
                bool newDevice = false;
                Device device = Device(devicePacket.ID, out newDevice);
                if (device == null) //error
                {
                    Debugger.E("There was an error retrieving or creating a Device entry for ID {0}", devicePacket.ID);
                    return;
                }
                device.User = user;
                if (!WiFindUs.Eye.Location.Equals(device.Location, devicePacket))
                    device.Location = devicePacket;
                if (newDevice)
                    device.Type = devicePacket.DeviceType;
                else 
                    device.Updated = DateTime.UtcNow.ToUnixTimestamp();

                //update database
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

                    DataLoadOptions options = new DataLoadOptions();
                    options.LoadWith<Device>(d => d.User);
                    options.LoadWith<Device>(d => d.History);
                    options.LoadWith<Device>(d => d.AssignedWaypoint);
                    eyeContext.LoadOptions = options;

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
                ;
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
                eyeListener.LogPackets = WFUApplication.Config.Get("server.log_packets", false);
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

        private void OnDeviceLoaded(Device device)
        {
            device.SelectionGroup = DefaultEntitySelectionGroup;
            updateables.Add(device);
            device.CheckTimeout();
        }

        private void OnNodeLoaded(Node node)
        {
            node.SelectionGroup = DefaultEntitySelectionGroup;
            updateables.Add(node);
            node.CheckTimeout();
        }

        private void OnUserLoaded(User user)
        {
            user.SelectionGroup = DefaultEntitySelectionGroup;
        }

        private void OnWaypointLoaded(Waypoint waypoint)
        {
            waypoint.SelectionGroup = DefaultEntitySelectionGroup;
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

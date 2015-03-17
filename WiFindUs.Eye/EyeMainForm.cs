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
        private const long TIMEOUT_CHECK_INTERVAL = 1000;
        
        private volatile EyeContext eyeContext = null;
        private EyePacketListener eyeListener = null;
        private Timer timer;
        private long timeoutCheckTimer = 0;
        private List<IUpdateable> updateables = new List<IUpdateable>();
        private bool serverMode = false;
        private MapControl map;
        private double deviceMaxAccuracy = 20.0;
        private double nodeMaxAccuracy = 20.0;

        //non-mysql collections (client mode):
        private Dictionary<ulong, Device> devices;
        private Dictionary<ulong, Node> nodes;
        private Dictionary<ulong, List<DeviceHistory>> deviceHistories; //device id, history list
        private Dictionary<ulong, User> users;
        private Dictionary<ulong, Waypoint> waypoints;

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

        public Node Node(uint id, out bool isNew)
        {
            isNew = false;
            if (id < 0)
                return null;

            //fetch
            Node node = null;
            if (ServerMode)
            {
                try
                {
                    var nod = from n in eyeContext.Nodes where n.ID == id select n;
                    foreach (Node n in nod) //force load
                    {
                        if (node == null)
                        {
                            node = n;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debugger.Ex(e);
                    return null;
                }
            }
            else
                nodes.TryGetValue(id, out node);

            //create
            if (node == null)
            {
                ulong ts = DateTime.UtcNow.ToUnixTimestamp();
                node = new Node()
                {
                    ID = id,
                    Created = ts,
                    Updated = ts
                };
                if (ServerMode)
                    eyeContext.Nodes.InsertOnSubmit(node);
                else
                    nodes[id] = node;
                isNew = true;
            }

            return node;
        }

        public Device Device(uint id, out bool isNew)
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
                        {
                            device = d;
                            break;
                        }
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
                ulong ts = DateTime.UtcNow.ToUnixTimestamp();
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

        public User User(uint id, out bool isNew)
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
                        {
                            user = u;
                            break;
                        }
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

            //attach server thread listener
            if (ServerMode)
            {
                eyeListener.DevicePacketReceived += DevicePacketReceived;
                eyeListener.NodePacketReceived += NodePacketReceived;
            }

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
                eyeListener.DevicePacketReceived -= DevicePacketReceived;
                eyeListener.NodePacketReceived -= NodePacketReceived;
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


        private void DevicePacketReceived(EyePacketListener sender, DevicePacket devicePacket)
        {
            if (!ServerMode || devicePacket == null)
                return;

            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action<EyePacketListener, DevicePacket>(DevicePacketReceived), new object[] { sender, devicePacket });
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (InvalidAsynchronousStateException)
                {
                    return;
                }
                return;
            }

            //get device
            bool newDevice = false;
            Device device = Device(devicePacket.ID, out newDevice);
            if (device == null) //error
            {
                Debugger.E("There was an error retrieving or creating a Device entry for ID {0}", devicePacket.ID);
                return;
            }
            //if (!device.Loaded)
              //  return;

            //get user
            User user = null;
            if (devicePacket.UserID.HasValue)
            {
                bool newUser = false;
                user = devicePacket.UserID.Value <= 0 ? null : User(devicePacket.UserID.Value, out newUser);
            }

            //device stats
            device.Updated = DateTime.UtcNow.ToUnixTimestamp();
            device.IPAddress = devicePacket.Address;
            if (devicePacket.UserID.HasValue)
                device.User = user;
            if (devicePacket.DeviceType != null)
                device.Type = devicePacket.DeviceType;
            if (devicePacket.Charging.HasValue)
                device.Charging = devicePacket.Charging;
            if (devicePacket.BatteryLevel.HasValue)
                device.BatteryLevel = devicePacket.BatteryLevel;

            //location
            if (devicePacket.IsGPSEnabled.HasValue)
                device.IsGPSEnabled = devicePacket.IsGPSEnabled;
            if (device.IsGPSEnabled.GetValueOrDefault())
            {
                if (devicePacket.IsGPSFixed.HasValue)
                    device.IsGPSFixed = devicePacket.IsGPSFixed;
                if (device.IsGPSFixed.GetValueOrDefault())
                {
                    device.LockLocationEvents();
                    if (devicePacket.Accuracy.HasValue)
                        device.Accuracy = devicePacket.Accuracy;
                    if (device.Accuracy.HasValue && device.Accuracy.Value <= deviceMaxAccuracy)
                    {
                        if (devicePacket.Latitude.HasValue)
                            device.Latitude = devicePacket.Latitude;
                        if (devicePacket.Longitude.HasValue)
                            device.Longitude = devicePacket.Longitude;
                        if (devicePacket.Altitude.HasValue)
                            device.Altitude = devicePacket.Altitude;
                    }
                    else
                    {
                        device.Latitude = null;
                        device.Longitude = null;
                        device.Altitude = null;
                    }
                    device.UnlockLocationEvents();
                }
            }

            eyeContext.SubmitChanges();
        }

        private void NodePacketReceived(EyePacketListener sender, NodePacket nodePacket)
        {
            if (!ServerMode || nodePacket == null)
                return;

            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action<EyePacketListener, NodePacket>(NodePacketReceived), new object[] { sender, nodePacket });
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (InvalidAsynchronousStateException)
                {
                    return;
                }
                return;
            }

            //get node
            bool newNode = false;
            Node node = Node(nodePacket.ID, out newNode);
            if (node == null) //error
            {
                Debugger.E("There was an error retrieving or creating a Node entry for ID {0}", nodePacket.ID);
                return;
            }
            //if (!node.Loaded)
              //  return;
            node.Updated = DateTime.UtcNow.ToUnixTimestamp();
            node.IPAddress = nodePacket.Address;
            if (nodePacket.Number.HasValue)
                node.Number = nodePacket.Number;
            if (nodePacket.IsGPSDaemonRunning.HasValue)
                node.IsGPSDaemonRunning = nodePacket.IsGPSDaemonRunning;
            if (nodePacket.IsGPSDaemonRunning.GetValueOrDefault())
            {
                node.LockLocationEvents();
                if (nodePacket.Accuracy.HasValue)
                    node.Accuracy = nodePacket.Accuracy;
                if (node.Accuracy.HasValue && node.Accuracy.Value <= nodeMaxAccuracy)
                {
                    if (nodePacket.Latitude.HasValue)
                        node.Latitude = nodePacket.Latitude;
                    if (nodePacket.Longitude.HasValue)
                        node.Longitude = nodePacket.Longitude;
                    if (nodePacket.Altitude.HasValue)
                        node.Altitude = nodePacket.Altitude;
                }
                else
                {
                    node.Latitude = null;
                    node.Longitude = null;
                    node.Altitude = null;
                }
                node.UnlockLocationEvents();
            }
            if (nodePacket.IsAPDaemonRunning.HasValue)
                node.IsAPDaemonRunning = nodePacket.IsAPDaemonRunning;
            if (nodePacket.IsDHCPDaemonRunning.HasValue)
                node.IsDHCPDaemonRunning = nodePacket.IsDHCPDaemonRunning;
            if (nodePacket.IsMeshPoint.HasValue)
                node.IsMeshPoint = nodePacket.IsMeshPoint;
            if (nodePacket.VisibleSatellites.HasValue)
                node.VisibleSatellites = nodePacket.VisibleSatellites;
            if (nodePacket.MeshPeers != null)
            {
                List<Node> peers = new List<Node>();
                foreach (int nodeNumber in nodePacket.MeshPeers)
                {
                    if (nodeNumber <= 0 || nodeNumber >= 255)
                        continue;
                    var peer = from p in eyeContext.Nodes where p.Number == nodeNumber select p;
                    foreach (Node p in peer)
                    {
                        if (!p.TimedOut && p != node && !peers.Contains(p))
                            peers.Add(p);
                    }
                }
                node.MeshPeers = peers;
            }

            eyeContext.SubmitChanges();
        }

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

                deviceMaxAccuracy = WFUApplication.Config.Get("server.max_node_accuracy", 20.0).Clamp(5.0,100.0);
                nodeMaxAccuracy = WFUApplication.Config.Get("server.max_node_accuracy", 20.0).Clamp(5.0, 100.0);

                Debugger.I("Application running in SERVER mode.");
            }
            else
            {
                WFUApplication.SplashStatus = "Initializing entity collections";
                devices = new Dictionary<ulong, Device>();
                nodes = new Dictionary<ulong, Node>();
                deviceHistories = new Dictionary<ulong, List<DeviceHistory>>();
                users = new Dictionary<ulong, User>();
                waypoints = new Dictionary<ulong, Waypoint>();

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
            try
            {
                foreach (User user in eyeContext.Users)
                    ;
            }
            catch (Exception e)
            {
                Debugger.Ex(e);
            }
            return true;
        }

        private bool PreCacheDevices()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching devices";
            try
            {
                foreach (Device device in eyeContext.Devices)
                    ;
            }
            catch (Exception e)
            {
                Debugger.Ex(e);
            }
            return true;
        }

        private bool PreCacheDeviceHistories()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching device history";
            try
            {
                foreach (DeviceHistory history in eyeContext.DeviceHistories)
                    ;
            }
            catch (Exception e)
            {
                Debugger.Ex(e);
            }
            return true;
        }

        private bool PreCacheNodes()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching nodes";
            try
            {
                foreach (Node node in eyeContext.Nodes)
                    ;
            }
            catch (Exception e)
            {
                Debugger.Ex(e);
            }
            return true;
        }

        private bool PreCacheWaypoints()
        {
            if (!ServerMode)
                return true;
            WFUApplication.SplashStatus = "Pre-caching waypoints";
            try
            {
                foreach (Waypoint waypoint in eyeContext.Waypoints)
                    ;
            }
            catch (Exception e)
            {
                Debugger.Ex(e);
            }
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
            return true;
        }

        private void OnDeviceLoaded(Device device)
        {
            device.CheckTimeout();
            updateables.Add(device);
        }

        private void OnNodeLoaded(Node node)
        {
            node.CheckTimeout();
            updateables.Add(node);
        }

        private void OnUserLoaded(User user)
        {

        }

        private void OnWaypointLoaded(Waypoint waypoint)
        {

        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (!ServerMode)
                return;

            timeoutCheckTimer += timer.Interval;
            if (timeoutCheckTimer >= TIMEOUT_CHECK_INTERVAL)
            {
                timeoutCheckTimer = 0;
                foreach (IUpdateable updateable in updateables)
                    updateable.CheckTimeout();
            }
        }

        private void MapApplicationStarting(MapControl map)
        {
            if (WFUApplication.Config != null)
                map.BackBufferScale = WFUApplication.Config.Get("map.resolution_scale", 1.0f);
        }
    }
}

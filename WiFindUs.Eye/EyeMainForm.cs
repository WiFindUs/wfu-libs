using Devart.Data.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using WiFindUs.Extensions;
using WiFindUs.Forms;

namespace WiFindUs.Eye
{
	public class EyeMainForm : MainForm
	{
		private const long TIMEOUT_CHECK_INTERVAL = 1000;

		private volatile EyeContext eyeContext = null;
		private EyePacketListener eyeListener = null;
		private Timer timer;
		private long timeoutCheckTimer = 0;
		private List<IUpdateable> updateables = new List<IUpdateable>();
		private bool serverMode = false;
		private double deviceMaxAccuracy = 20.0;
		private double nodeMaxAccuracy = 20.0;

		//non-mysql collections (client mode):
		private Dictionary<ulong, Device> devices;
		private Dictionary<ulong, Node> nodes;
		private Dictionary<ulong, List<DeviceHistory>> deviceHistories; //device id, history list
		private Dictionary<ulong, User> users;
		private Dictionary<ulong, Waypoint> waypoints;
		private List<NodeLink> nodeLinks;

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
		public virtual IEnumerable<NodeLink> NodeLinks
		{
			get { return serverMode ? (IEnumerable<NodeLink>)eyeContext.NodeLinks : nodeLinks; }
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
				tasks.Add(PreCacheNodeLinks);
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

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public EyeMainForm()
		{
			WiFindUs.Eye.Device.OnDeviceLoaded += OnDeviceLoaded;
			WiFindUs.Eye.Node.OnNodeLoaded += OnNodeLoaded;
			WiFindUs.Eye.NodeLink.OnNodeLinkLoaded += OnNodeLinkLoaded;
			WiFindUs.Eye.Waypoint.OnWaypointLoaded += OnWaypointLoaded;
			WiFindUs.Eye.User.OnUserLoaded += OnUserLoaded;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the most recently active node assigned to a particular station number.
		/// </summary>
		public Node NodeByNumber(uint number)
		{
			if (number == 0 || number >= 255)
				throw new ArgumentOutOfRangeException("number", "number must be between 1 and 254 (inclusive)");

			try
			{
				return (from n in Nodes
						where n.Number.HasValue && n.Number.Value == number
						select n).OrderByDescending(n => n.LastUpdated).First();
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the node link connecting the two nodes (regardless of order)
		/// </summary>
		public NodeLink NodeLink(Node A, Node B)
		{
			if (A == null || B == null)
				throw new ArgumentOutOfRangeException("nodes", "node parameters cannot be null");
			if (!A.Loaded || !B.Loaded)
				throw new ArgumentOutOfRangeException("nodes", "node parameters must be loaded");
			if (A == B || A.ID == B.ID)
				throw new ArgumentOutOfRangeException("nodes", "node parameters cannot be the same node");

			try
			{
				return (from link in NodeLinks
						where (link.StartNodeID == A.ID && link.EndNodeID == B.ID)
						|| (link.StartNodeID == B.ID && link.EndNodeID == A.ID)
						select link).First();
			}
			catch
			{
				return null;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnFirstShown(EventArgs e)
		{
			base.OnFirstShown(e);
			if (IsDesignMode)
				return;

			//attach server thread listener
			if (ServerMode)
			{
				eyeListener.DevicePacketReceived += DevicePacketReceived;
				eyeListener.NodePacketReceived += NodePacketReceived;
			}

			//start timer
			timer = new Timer();
			timer.Interval = 100;
			timer.Tick += TimerTick;
			timer.Enabled = true;
		}

		protected override void OnDisposing()
		{
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

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void DevicePacketReceived(EyePacketListener sender, DevicePacket devicePacket)
		{
			if (!ServerMode || devicePacket == null || devicePacket.ID <= 0)
				return;

			if (InvokeRequired)
			{
				try
				{
					Invoke(new Action<EyePacketListener, DevicePacket>(DevicePacketReceived), new object[] { sender, devicePacket });
				}
				catch (ObjectDisposedException) { return; }
				catch (InvalidAsynchronousStateException) { return; }
				return;
			}


			//get device
			Device device = null;
			try
			{
				device = (from d in Devices where d.ID == devicePacket.ID select d).First();
				if (device != null && !device.Loaded)
					return;
			}
			catch //new device
			{
				ulong ts = DateTime.UtcNow.ToUnixTimestamp();
				device = new Device()
				{
					ID = devicePacket.ID,
					Created = ts,
					LastUpdated = ts
				};
				if (ServerMode)
					eyeContext.Devices.InsertOnSubmit(device);
				else
					devices[devicePacket.ID] = device;
			}

			//get user
			User user = null;
			if (devicePacket.UserID.HasValue && devicePacket.UserID.Value > 0)
			{
				try
				{
					user = (from u in Users where u.ID == devicePacket.UserID.Value select u).First();
					if (user != null && !user.Loaded)
						user = null;
				}
				catch //new user
				{
					user = new User()
					{
						ID = devicePacket.UserID.Value,
						NameFirst = "",
						NameLast = "",
						NameMiddle = "",
						Type = ""
					};
					if (ServerMode)
						eyeContext.Users.InsertOnSubmit(user);
					else
						users[devicePacket.UserID.Value] = user;
				}
			}

			//process packet
			device.ProcessPacket(devicePacket);

			//current user
			if (devicePacket.UserID.HasValue)
				device.User = user;

			//connected node
			if (devicePacket.NodeNumber.HasValue)
			{
				Node node = devicePacket.NodeNumber.Value == 0 ? null : NodeByNumber(devicePacket.NodeNumber.Value);
				device.Node = node == null || !node.Loaded ? null : node;
			}

			//save changes
			SubmitPacketChanges();
		}

		private void NodePacketReceived(EyePacketListener sender, NodePacket nodePacket)
		{
			if (!ServerMode || nodePacket == null || nodePacket.ID <= 0)
				return;

			if (InvokeRequired)
			{
				try
				{
					Invoke(new Action<EyePacketListener, NodePacket>(NodePacketReceived), new object[] { sender, nodePacket });
				}
				catch (ObjectDisposedException) { return; }
				catch (InvalidAsynchronousStateException) { return; }
				return;
			}

			//get node
			Node node = null;
			try
			{
				node = (from n in Nodes where n.ID == nodePacket.ID select n).First();
				if (node != null && !node.Loaded)
					return;
			}
			catch //new node
			{
				ulong ts = DateTime.UtcNow.ToUnixTimestamp();
				node = new Node()
				{
					ID = nodePacket.ID,
					Created = ts,
					LastUpdated = ts
				};
				if (ServerMode)
					eyeContext.Nodes.InsertOnSubmit(node);
				else
					nodes[nodePacket.ID] = node;
			}

			//process packet
			node.ProcessPacket(nodePacket);

			//process node-node links
			if (nodePacket.NodeLinks != null)
			{
				//get all existing links for node
				List<NodeLink> inactiveLinks = new List<NodeLink>();
				inactiveLinks.AddRange(from link in NodeLinks
							   where (link.StartNodeID == node.ID || link.EndNodeID == node.ID)
							   select link);

				//update identified links
				foreach (NodePacket.LinkData linkData in nodePacket.NodeLinks)
				{
					try
					{ 
						Node end = NodeByNumber(linkData.NodeNumber);
						if (end == null || !end.Loaded)
							continue;

						NodeLink activeLink = NodeLink(node, end);
						if (activeLink != null)
						{
							if (!activeLink.Loaded)
								continue;
							inactiveLinks.Remove(activeLink);
						}
						else
						{
							activeLink = new NodeLink()
							{
								Start = node,
								End = end,
								Active = false
							};
							if (ServerMode)
								eyeContext.NodeLinks.InsertOnSubmit(activeLink);
							else
								nodeLinks.Add(activeLink);
						}
						bool alreadyActive = activeLink.Active;
						activeLink.Active = true;
						if (alreadyActive) //don't clobber (future-proofing)
						{
							if (linkData.SignalStrength.HasValue)
								activeLink.SignalStrength = linkData.SignalStrength;
							if (linkData.LinkSpeed.HasValue)
								activeLink.LinkSpeed = linkData.LinkSpeed;
						}
						else
						{
							activeLink.SignalStrength = linkData.SignalStrength;
							activeLink.LinkSpeed = linkData.LinkSpeed;
						}
					}
					catch
					{
						continue;
					}
				}

				//set old ones to inactive
				foreach (NodeLink link in inactiveLinks)
					link.Active = false;
			}

			//save changes
			SubmitPacketChanges();
		}

		private void SubmitPacketChanges()
		{
			try
			{
				eyeContext.SubmitChanges(ConflictMode.ContinueOnConflict);
			}
			catch (ChangeConflictException)
			{
				foreach (ObjectChangeConflict objConflict in eyeContext.ChangeConflicts)
					foreach (MemberChangeConflict memberConflict in objConflict.MemberConflicts)
						memberConflict.Resolve(RefreshMode.OverwriteCurrentValues);
				eyeContext.SubmitChanges(ConflictMode.ContinueOnConflict);
			}
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
					options.LoadWith<Device>(d => d.Node);
					options.LoadWith<Node>(n => n.Devices);
					options.LoadWith<Node>(n => n.StartLinks);
					options.LoadWith<Node>(n => n.EndLinks);
					options.LoadWith<NodeLink>(nl => nl.Start);
					options.LoadWith<NodeLink>(nl => nl.End);
					options.LoadWith<User>(u => u.Device);
					eyeContext.LoadOptions = options;

					Debugger.V("MySQL connection created OK.");
				}
				catch (Exception ex)
				{
					String message = "There was an error establishing the MySQL connection: " + ex.Message;
					Debugger.E(message);
					MessageBox.Show(message + "\n\nThe application will now exit.", "MySQL Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				deviceMaxAccuracy = WFUApplication.Config.Get("server.max_node_accuracy", 20.0).Clamp(5.0, 100.0);
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
				nodeLinks = new List<NodeLink>();

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

		private bool PreCacheNodeLinks()
		{
			if (!ServerMode)
				return true;
			WFUApplication.SplashStatus = "Pre-caching node links";
			try
			{
				foreach (NodeLink nodeLink in eyeContext.NodeLinks)
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
#if DEBUG
				eyeListener.LogPackets = WFUApplication.Config.Get("server.log_packets", true);
#else
				eyeListener.LogPackets = WFUApplication.Config.Get("server.log_packets", false);
#endif
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
			updateables.Add(device);
		}

		private void OnNodeLoaded(Node node)
		{
			updateables.Add(node);
		}

		private void OnNodeLinkLoaded(NodeLink nodeLink)
		{

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
					updateable.CheckActive();
			}
		}


	}
}

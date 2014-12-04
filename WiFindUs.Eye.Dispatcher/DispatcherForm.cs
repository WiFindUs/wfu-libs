using Devart.Data.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Eye;
using WiFindUs.Extensions;
using WiFindUs.Forms;
using WiFindUs.Controls;
using WiFindUs.Eye.Wave;
using System.Text.RegularExpressions;
using System.Threading;
using WiFindUs.IO;
using WiFindUs.Eye.Extensions;

namespace WiFindUs.Eye.Dispatcher
{
    public partial class DispatcherForm : MainForm, IMapForm
    {
        private EyeContext eyeContext = null;
        private EyePacketListener eyeListener = null;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        protected EyeContext EyeContext
        {
            get { return eyeContext; }
        }

        protected MapControl Map
        {
            get { return mapControl; }
        }

        protected override List<Func<bool>> LoadingTasks
        {
            get
            {
                List<Func<bool>> tasks = base.LoadingTasks;
                tasks.Add(PreCacheUsers);
                tasks.Add(PreCacheDevices);
                tasks.Add(PreCacheDeviceHistories);
                tasks.Add(PreCacheNodes);
                tasks.Add(PreCacheNodeHistories);
                tasks.Add(PreCacheWaypoints);
                tasks.Add(StartListener);
                return tasks;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public DispatcherForm()
        {
            InitializeComponent();
            if (DesignMode)
                return;
            eyeContext = WFUApplication.MySQLDataContext as WiFindUs.Eye.EyeContext;
            WFUApplication.StartSplashLoading(LoadingTasks);
            MapScene.SceneStarted += MapScene_SceneStarted;
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public void RenderMap()
        {
            if (mapControl != null)
                mapControl.Render();
        }

        public void SetApplicationStatus(string text, Color colour)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, Color>(SetApplicationStatus), new object[] { text, colour });
                return;
            }

            toolStripStatusLabel.Text = text ?? "";
            windowStatusStrip.BackColor = colour;
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnFirstShown(EventArgs e)
        {
            base.OnFirstShown(e);
            SetApplicationStatus("Initializing 3D scene...", Theme.WarningColour);
#if DEBUG
            infoTabs.SelectedIndex = 1;
#endif
        }

        protected override void OnThemeChanged(Theme theme)
        {
            base.OnThemeChanged(theme);
            Map.BackColor = theme.ControlDarkColour;
            windowStatusStrip.BackColor = theme.ControlLightColour;
            windowStatusStrip.ForeColor = theme.TextLightColour;
        }

        protected override void OnDisposing()
        {
            updateTimer.Enabled = false;
            
            if (mapControl != null)
            {
                Controls.Remove(mapControl);
                mapControl.Dispose();
                mapControl = null;
            }
            
            if (eyeListener != null)
            {
                eyeListener.PacketReceived -= eyeListener_PacketReceived;
                eyeListener.Dispose();
                eyeListener = null;
            }
            base.OnDisposing();
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private bool PreCacheUsers()
        {
            WFUApplication.SplashStatus = "Pre-caching users";
            eyeContext = WFUApplication.MySQLDataContext as WiFindUs.Eye.EyeContext;
            foreach (User user in EyeContext.Users)
                ;
            return true;
        }

        private bool PreCacheDevices()
        {
            WFUApplication.SplashStatus = "Pre-caching devices";
            foreach (Device device in EyeContext.Devices)
                ;
            return true;
        }

        private bool PreCacheDeviceHistories()
        {
            WFUApplication.SplashStatus = "Pre-caching device history";
            foreach (DeviceHistory history in EyeContext.DeviceHistories)
                ;
            return true;
        }

        private bool PreCacheNodes()
        {
            WFUApplication.SplashStatus = "Pre-caching nodes";
            foreach (Node node in EyeContext.Nodes)
                ;
            return true;
        }

        private bool PreCacheNodeHistories()
        {
            WFUApplication.SplashStatus = "Pre-caching node history";
            foreach (NodeHistory history in EyeContext.NodeHistories)
                ;
            return true;
        }

        private bool PreCacheWaypoints()
        {
            WFUApplication.SplashStatus = "Pre-caching waypoints";
            foreach (Waypoint waypoint in EyeContext.Waypoints)
                ;
            return true;
        }

        private bool StartListener()
        {
            WFUApplication.SplashStatus = "Creating UDP listener";
            eyeListener = new EyePacketListener(WFUApplication.Config.Get("server.udp_port", 33339));
            return true;
        }

        private void MapScene_SceneStarted(MapScene obj)
        {
            MapScene.SceneStarted -= MapScene_SceneStarted;

            ILocation location = WFUApplication.Config.Get("map.center", (ILocation)null);
            if (location == null)
                Debugger.E("Could not parse map.center from config files!");
            else
                obj.CenterLocation = location;

            SetApplicationStatus("Map scene ready.", Theme.HighlightMidColour);
            eyeListener.PacketReceived += eyeListener_PacketReceived;
            updateTimer.Interval = WFUApplication.Config.Get("mysql.submit_rate", 0, 30000);
            updateTimer.Enabled = true;
        }

        private void eyeListener_PacketReceived(EyePacket obj)
        {
            Debugger.V(obj.ToString());

            if (obj.Type.CompareTo("DEV") == 0)
            {
                bool newDevice;
                DevicePacket devicePacket;
                try
                {
                    devicePacket = new DevicePacket(obj);
                }
                catch (Exception)
                {
                    Debugger.E("Malformed device update packet recieved from " + obj.Address.ToString());
                    return;
                }
                Device device = EyeContext.Device(obj.ID, out newDevice);
                if (newDevice)
                    device.Type = devicePacket.DeviceType;
                if (!WiFindUs.Eye.Location.Equals(device.Location, devicePacket))
                    device.Location = devicePacket;
                //if (!WiFindUs.Eye.Atmosphere.Equals(device.Atmosphere, devicePacket))
                //device.Atmosphere = devicePacket;
                device.SetBatteryStats(devicePacket);
                if (devicePacket.UserID > -1)
                {
                    bool newUser;
                    User user = EyeContext.User(devicePacket.UserID, out newUser);
                    device.UserID = devicePacket.UserID;
                }
                else
                    device.UserID = null;
            }
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            EyeContext.SubmitChangesThreaded();
        }
    }
}

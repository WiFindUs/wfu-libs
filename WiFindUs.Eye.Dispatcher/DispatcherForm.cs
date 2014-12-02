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

        protected MapControl MapControl
        {
            get { return mapControl; }
        }

        protected Panel MinimapPanel
        {
            get { return controlsOuterSplitter.Panel1; }
        }

        protected Panel InfoPanel
        {
            get { return controlsInnerSplitter.Panel1; }
        }

        protected Panel CommandsPanel
        {
            get { return controlsInnerSplitter.Panel2; }
        }

        protected override List<Func<bool>> LoadingTasks
        {
            get
            {
                List<Func<bool>> tasks = base.LoadingTasks;
                tasks.Add(PreCacheUsers);
                tasks.Add(PreCacheDevices);
                tasks.Add(PreCacheDeviceStates);
                tasks.Add(PreCacheNodes);
                tasks.Add(PreCacheNodeStates);
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
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public void RenderMap()
        {
            mapControl.Render();
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnFirstShown(EventArgs e)
        {
            base.OnFirstShown(e);
            double[] locArray = WFUApplication.Config.Get("map.center");
            if (locArray == null)
                Debugger.E("Could not find map.center in config files!");
            else
            {

                ILocation location = null;
                try
                {
                    location = new Location(locArray);
                }
                catch (Exception)
                {
                    Debugger.E("Error parsing config map.center as a Location value");
                }

                if (location != null)
                    mapControl.CenterLocation = location;
            }
        }

        protected override void OnThemeChanged(Theme theme)
        {
            base.OnThemeChanged(theme);
            MapControl.BackColor = theme.ControlDarkColour;
            windowStatusStrip.BackColor = theme.ControlLightColour;
            infoTabs.BackColor = theme.ControlLightColour;
        }

        protected override void OnDisposing()
        {
            if (eyeListener != null)
            {
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

        private bool PreCacheDeviceStates()
        {
            WFUApplication.SplashStatus = "Pre-caching device history";
            foreach (DeviceState state in EyeContext.DeviceStates)
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

        private bool PreCacheNodeStates()
        {
            WFUApplication.SplashStatus = "Pre-caching node history";
            foreach (NodeState state in EyeContext.NodeStates)
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
    }
}

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
    public partial class DispatcherForm : EyeMainForm
    {
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        protected override List<Func<bool>> LoadingTasks
        {
            get
            {
                List<Func<bool>> tasks = base.LoadingTasks;
                if (WFUApplication.UsesMySQL)
                {
                    tasks.Add(PreCacheUsers);
                    tasks.Add(PreCacheDevices);
                    tasks.Add(PreCacheDeviceHistories);
                    tasks.Add(PreCacheNodes);
                    tasks.Add(PreCacheNodeHistories);
                    tasks.Add(PreCacheWaypoints);
                }
                return tasks;
            }
        }

        protected override MapControl Map
        {
            get { return mapControl; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public DispatcherForm()
        {
            InitializeComponent();
            if (DesignMode)
                return;
            WFUApplication.StartSplashLoading(LoadingTasks);
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

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
            SetApplicationStatus("Initializing 3D scene...", Theme.WarningColour);
            base.OnFirstShown(e);

#if DEBUG
            infoTabs.SelectedIndex = 1;
#endif
        }

        protected override void OnThemeChanged(Theme theme)
        {
            base.OnThemeChanged(theme);
            windowStatusStrip.BackColor = theme.ControlLightColour;
            windowStatusStrip.ForeColor = theme.TextLightColour;
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


        protected override void MapSceneStarted(MapScene obj)
        {
            base.MapSceneStarted(obj);
            SetApplicationStatus("Map scene ready.", Theme.HighlightMidColour);

            updateTimer.Interval = WFUApplication.Config.Get("mysql.submit_rate", 0, 30000);
            updateTimer.Enabled = true;
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            EyeContext.SubmitChangesThreaded();
        }
    }
}

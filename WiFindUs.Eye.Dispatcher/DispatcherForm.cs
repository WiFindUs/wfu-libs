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
using WiFindUs.Eye.Controls;

namespace WiFindUs.Eye.Dispatcher
{
    public partial class DispatcherForm : EyeMainForm
    {
        private MiniMapControl minimap;
        private FormWindowState oldWindowState;
        private Rectangle oldBounds;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FullScreen
        {
            get { return FormBorderStyle == FormBorderStyle.None; }
            set
            {
                if (value == FullScreen)
                    return;

                //suspend layout stuff
                this.SuspendAllLayout();

                //going fullscreen
                if (value)
                {
                    oldWindowState = WindowState;
                    oldBounds = Bounds;
                    
                    if (WindowState != FormWindowState.Normal)
                        WindowState = FormWindowState.Normal;
                    FormBorderStyle = FormBorderStyle.None;
                    Bounds = Screen.FromControl(this).Bounds;
                    TopMost = true;
                    workingAreaSplitter.HidePanel(1);
                    windowSplitter.HidePanel(2);
                    windowStatusStrip.Visible = false;
                }
                else
                {
                    windowStatusStrip.Visible = true;
                    windowSplitter.ShowPanel(2);
                    workingAreaSplitter.ShowPanel(1);
                    TopMost = false;
                    FormBorderStyle = FormBorderStyle.Sizable;
                    Bounds = oldBounds;
                    if (oldWindowState != FormWindowState.Normal)
                        WindowState = oldWindowState;
                }

                //resume layout stuff
                this.ResumeAllLayout();
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

            //controls
            workingAreaToolStripContainer.ContentPanel.Controls.Add(Map = new MapControl(){ Dock = DockStyle.Fill });
            minimapTab.Controls.Add(minimap = new MiniMapControl() { Dock = DockStyle.Fill });

            //events
            WiFindUs.Eye.Device.OnDeviceLoaded += OnDeviceLoaded;
            WiFindUs.Eye.User.OnUserLoaded += OnUserLoaded;
            WiFindUs.Eye.Waypoint.OnWaypointLoaded += OnWaypointLoaded;

            //load
            this.SuspendAllLayout();
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
            this.ResumeAllLayout();
#if DEBUG
            infoTabs.SelectedIndex = 1;
#endif
            bool startFullScreen = WFUApplication.Config.Get("display.start_fullscreen", false);
            Debugger.V("Start fullscreen: " + startFullScreen);
            
            if (startFullScreen)
                FullScreen = true;
        }

        protected override void OnThemeChanged(Theme theme)
        {
            base.OnThemeChanged(theme);
            windowStatusStrip.ForeColor = theme.TextLightColour;
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void MapSceneStarted(MapScene obj)
        {
            base.MapSceneStarted(obj);
            SetApplicationStatus("Map scene ready.", Theme.HighlightMidColour);
            Map.AltEnterPressed += mapControl_AltEnterPressed;
            Map.Scene.BaseTile.TextureImageLoadingFinished += BaseTile_TextureImageLoadingFinished;
            minimap.Scene = Map.Scene;
        }

        private void BaseTile_TextureImageLoadingFinished(TerrainTile obj)
        {
            minimap.RefreshThreadSafe();
        }

        private void mapControl_AltEnterPressed(MapControl obj)
        {
            FullScreen = !FullScreen;
        }

        private void OnDeviceLoaded(Device device)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Device>(OnDeviceLoaded), device);
                return;
            }
            DeviceListChild dlc = new DeviceListChild(device);
            dlc.Theme = Theme;
            devicesFlowPanel.Controls.Add(dlc);
        }

        private void OnUserLoaded(User user)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<User>(OnUserLoaded), user);
                return;
            }
            UserListChild ulc = new UserListChild(user);
            ulc.Theme = Theme;
            usersFlowPanel.Controls.Add(ulc);
        }

        private void OnWaypointLoaded(Waypoint waypoint)
        {
            if (!waypoint.IsIncident)
                return;
            
            if (InvokeRequired)
            {
                Invoke(new Action<Waypoint>(OnWaypointLoaded), waypoint);
                return;
            }
            IncidentListChild ilc = new IncidentListChild(waypoint);
            ilc.Theme = Theme;
            incidentsFlowPanel.Controls.Add(ilc);
        }
    }
}

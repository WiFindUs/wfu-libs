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
        private MapControl map;
        private MiniMapControl minimap;
        private FormWindowState oldWindowState;
        private Rectangle oldBounds;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        protected override MapControl Map
        {
            get { return map; }
        }

        public bool FullScreen
        {
            get { return FormBorderStyle == FormBorderStyle.None; }
            set
            {
                if (value == FullScreen)
                    return;

                //suspend layout stuff
                SuspendLayout();
                this.RecurseControls(control => SuspendLayout());

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
                this.RecurseControls(control => ResumeLayout(false));
                ResumeLayout(false);
                PerformLayout();

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
            workingAreaToolStripContainer.ContentPanel.Controls.Add(map = new MapControl(){ Dock = DockStyle.Fill });
            minimapTab.Controls.Add(minimap = new MiniMapControl() { Dock = DockStyle.Fill });

            //events
            WiFindUs.Eye.Device.OnDeviceCreated += OnDeviceCreated;
            WiFindUs.Eye.User.OnUserCreated += OnUserCreated;
            WiFindUs.Eye.Waypoint.OnWaypointCreated += OnWaypointCreated;

            //load
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
            map.AltEnterPressed += mapControl_AltEnterPressed;
            map.Scene.BaseTile.TextureLoadingFinished += BaseTile_TextureLoadingFinished;
            minimap.Scene = map.Scene;
        }

        private void BaseTile_TextureLoadingFinished(TerrainTile obj)
        {
            minimap.RefreshThreadSafe();
        }

        private void mapControl_AltEnterPressed(MapControl obj)
        {
            FullScreen = !FullScreen;
        }

        private void OnDeviceCreated(Device obj)
        {
            devicesListBox.Items.Add(obj);
        }

        private void OnUserCreated(User obj)
        {
            usersListBox.Items.Add(obj);
        }

        private void OnWaypointCreated(Waypoint obj)
        {
            incidentsListBox.Items.Add(obj);
        }
    }
}

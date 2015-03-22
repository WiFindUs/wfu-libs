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
using WiFindUs.Eye.Wave.Markers;
using WiFindUs.Eye.Wave.Adapter;
using WiFindUs.Eye.Wave.Controls;

namespace WiFindUs.Eye.Dispatcher
{
    public partial class DispatcherForm : WaveMainForm
    {
        private MiniMapControl minimap;
        private FormWindowState oldWindowState;
        private Rectangle oldBounds;
        private ISelectableGroup globalSelectionGroup = new SelectableEntityGroup();
        private ActionPanel actionPanel;

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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISelectableGroup GlobalSelectionGroup
        {
            get { return globalSelectionGroup; }
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
            devicesFlowPanel.SelectionGroup = globalSelectionGroup;
            usersFlowPanel.SelectionGroup = globalSelectionGroup;
            incidentsFlowPanel.SelectionGroup = globalSelectionGroup;
            nodesFlowPanel.SelectionGroup = globalSelectionGroup;
            actionsTab.Controls.Add(actionPanel = new ActionPanel(3,3) { Dock = DockStyle.Fill });

            //events
            WiFindUs.Eye.Device.OnDeviceLoaded += OnDeviceLoaded;
            WiFindUs.Eye.User.OnUserLoaded += OnUserLoaded;
            WiFindUs.Eye.Waypoint.OnWaypointLoaded += OnWaypointLoaded;
            WiFindUs.Eye.Node.OnNodeLoaded += OnNodeLoaded;
            globalSelectionGroup.SelectionChanged += OnSelectionGroupSelectionChanged;

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

        public override void OnThemeChanged()
        {
            base.OnThemeChanged();
            windowStatusStrip.ForeColor = Theme.TextLightColour;
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
            Map.Scene.InputBehaviour.MousePressed += InputBehaviour_MousePressed;
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
            devicesFlowPanel.Controls.Add(new DeviceListItem(device));
        }

        private void OnUserLoaded(User user)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<User>(OnUserLoaded), user);
                return;
            }
            usersFlowPanel.Controls.Add(new UserListItem(user));
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
            incidentsFlowPanel.Controls.Add(new WaypointListItem(waypoint));
        }

        private void OnNodeLoaded(Node node)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Node>(OnNodeLoaded), node);
                return;
            }
            nodesFlowPanel.Controls.Add(new NodeListItem(node));
        }

        private void OnSelectionGroupSelectionChanged(ISelectableGroup obj)
        {
            if (obj != globalSelectionGroup)
                return;

            ISelectable[] selectedEntities = globalSelectionGroup.SelectedEntities;
            if (selectedEntities == null || selectedEntities.Length == 0)
                actionPanel.ActionSubscriber = null;
            else if (selectedEntities.Length == 1)
                actionPanel.ActionSubscriber = selectedEntities[0] as IActionSubscriber;
            else
            {
                Type firstType = null;
                bool same = true;
                foreach (ISelectable entity in selectedEntities)
                {
                    Type t = entity.GetType();
                    if (firstType == null)
                        firstType = t;
                    else if (t != firstType && !firstType.IsAssignableFrom(t) && !t.IsAssignableFrom(firstType))
                        same = false;
                }
                   
                if (same)
                {
                    if (firstType == typeof(Device) || typeof(Device).IsAssignableFrom(firstType))
                    {
                        Device[] devices = new Device[selectedEntities.Length];
                        for (int i = 0; i < selectedEntities.Length; i++)
                            devices[i] = selectedEntities[i] as Device;
                        actionPanel.ActionSubscriber = new DeviceGroupActionSubscriber(devices);
                        return;
                    }
                }

                actionPanel.ActionSubscriber = null;
            }
        }

        private void InputBehaviour_MousePressed(MapSceneInput.MapSceneMouseEventArgs args)
        {
            if (args.Button != MapSceneInput.MouseButtons.Left)
                return;

            Marker[] clickedMarkers = args.Scene.MarkersFromScreenRay(args.X, args.Y);

            if (clickedMarkers == null || clickedMarkers.Length == 0)
            {
                globalSelectionGroup.ClearSelection();
                return;
            }
            
            List<ISelectable> selectables = new List<ISelectable>();
            foreach (Marker marker in clickedMarkers)
            {
                ISelectableProxy sp = marker as ISelectableProxy;
                if (sp != null)
                    selectables.Add(sp.Selectable);
            }

            if (selectables.Count == 0)
                globalSelectionGroup.ClearSelection();
            else if (selectables.Count == 1 || globalSelectionGroup.SelectedEntities.Length == 0)
                globalSelectionGroup.SetSelection(selectables[0]);
            else
            {
                ISelectable[] intersection = globalSelectionGroup.SelectedEntities.Intersect(selectables).ToArray();
                if (intersection.Length == 0)
                    globalSelectionGroup.SetSelection(selectables[0]);
                else
                    globalSelectionGroup.SetSelection(selectables[((selectables.IndexOf(intersection[intersection.Length-1]) + 1) % selectables.Count)]);
            }
        }
    }
}

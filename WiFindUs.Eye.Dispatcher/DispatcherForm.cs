using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;
using WiFindUs.Eye.Controls;
using WiFindUs.Eye.Wave;
using WiFindUs.Eye.Wave.Controls;
using WiFindUs.Eye.Wave.Markers;
using WiFindUs.Forms;

namespace WiFindUs.Eye.Dispatcher
{
	public partial class DispatcherForm : WaveMainForm
	{
		private FormWindowState oldWindowState;
		private Rectangle oldBounds;
		private ISelectableGroup globalSelectionGroup = new SelectableEntityGroup();
		//private ActionPanel actionPanel;
		private MapForm mapForm;
		private BaseForm consoleForm;
		private ConsolePanel console;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool FullScreen
		{
			get
			{
				return (ShowMapInWindow ? mapForm.FormBorderStyle : FormBorderStyle)
					== FormBorderStyle.None;
			}
			set
			{
				if (value == FullScreen)
					return;

				//disable/enable mapform user close
				mapForm.PreventUserClose = value;

				//determine target form
				Form targetForm = ShowMapInWindow ? (Form)mapForm : (Form)this;

				//suspend layout stuff
				targetForm.SuspendAllLayout();

				//going fullscreen
				if (value)
				{
					oldWindowState = targetForm.WindowState;
					oldBounds = targetForm.Bounds;
					if (targetForm.WindowState != FormWindowState.Normal)
						targetForm.WindowState = FormWindowState.Normal;
					if (targetForm.FormBorderStyle != FormBorderStyle.None)
						targetForm.FormBorderStyle = FormBorderStyle.None;
					targetForm.Bounds = Screen.FromControl(targetForm).Bounds;
					targetForm.TopMost = true;
					if (targetForm == this)
					{
						splitterLeft.HidePanel(1);
						splitterRight.HidePanel(2);
						statusStrip.Visible = false;
						map.Parent = splitterRight.Panel1;
						tabsMiddle.Visible = false;
					}
				}
				else
				{
					if (targetForm == this)
					{
						tabsMiddle.Visible = true;
						map.Parent = tab3DMap;
						statusStrip.Visible = true;
						splitterLeft.ShowPanel(1);
						splitterRight.ShowPanel(2);
					}
					targetForm.TopMost = false;
					if (targetForm.FormBorderStyle != FormBorderStyle.Sizable)
						targetForm.FormBorderStyle = FormBorderStyle.Sizable;
					targetForm.Bounds = oldBounds;
					if (oldWindowState != FormWindowState.Normal && targetForm.WindowState != oldWindowState)
						targetForm.WindowState = oldWindowState;
				}

				//resume layout stuff
				targetForm.ResumeAllLayout();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ISelectableGroup GlobalSelectionGroup
		{
			get { return globalSelectionGroup; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShowMapInWindow
		{
			get { return mapForm == null ? false : mapForm.Visible; }
			set
			{
				if (FullScreen || value == mapForm.Visible)
					return;
				this.SuspendAllLayout();
				if (mapForm.Visible)
					mapForm.Hide();
				else
					mapForm.Show(this);
				this.ResumeAllLayout();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShowConsole
		{
			get { return consoleForm == null ? false : consoleForm.Visible; }
			set
			{
				if (value == consoleForm.Visible)
					return;
				if (consoleForm.Visible)
					consoleForm.Hide();
				else
				{
					consoleForm.Show(this);
					console.RegenerateFromHistory();
				}
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

			//title
			Text = WFUApplication.Name;

			//child map form for two-window mode
			mapForm = new MapForm()
			{
				HideOnClose = true,
				WindowState = FormWindowState.Normal,
				MinimumSize = new System.Drawing.Size(800, 600),
				Text = "3D Map",
				HelpButton = false,
				KeyPreview = true,
				StartPosition = FormStartPosition.Manual
			};
			mapForm.VisibleChanged += mapForm_VisibleChanged;
			mapForm.KeyDown += TestKeys;

			//console form
			consoleForm = new BaseForm()
			{
				HideOnClose = true,
				WindowState = FormWindowState.Normal,
				MinimumSize = new System.Drawing.Size(400, 300),
				Text = "Console",
				HelpButton = false,
				KeyPreview = true,
				StartPosition = FormStartPosition.Manual
			};
			consoleForm.Controls.Add(console = new ConsolePanel()
			{
				Dock = DockStyle.Fill
			});
			consoleForm.KeyDown += TestKeys;

			//key preview for alt-enter
			KeyPreview = true;

			//map form
			AddMapControl(map);

			//controls
			listDevices.SelectionGroup = globalSelectionGroup;
			listUsers.SelectionGroup = globalSelectionGroup;
			listIncidents.SelectionGroup = globalSelectionGroup;
			listNodes.SelectionGroup = globalSelectionGroup;
			//actionsTab.Controls.Add(actionPanel = new ActionPanel(3, 3) { Dock = DockStyle.Fill });

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

			labelStatus.Text = text ?? "";
			statusStrip.BackColor = colour;
		}

		public override void OnThemeChanged()
		{
			base.OnThemeChanged();
			statusStrip.ForeColor = Theme.TextLightColour;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnKeyDown(KeyEventArgs e)
		{
			TestKeys(this, e);

			if (!e.Handled)
				base.OnKeyDown(e);
		}

		protected override void OnFirstShown(EventArgs e)
		{
			SetApplicationStatus("Initializing 3D scene...", Theme.WarningColour);
			base.OnFirstShown(e);
			this.ResumeAllLayout();

			//set up console form
			consoleForm.ApplyWindowStateFromConfig("console");
#if DEBUG
			ShowConsole = true;
#endif

			//set up map form
			mapForm.ApplyWindowStateFromConfig("map");

			//map windowed
			bool startWindowed = WFUApplication.Config.Get("map.start_windowed", false);
			Debugger.V("Start map windowed: " + startWindowed);
			if (startWindowed)
				ShowMapInWindow = true;

			//fullscreen
			bool startFullScreen = WFUApplication.Config.Get("map.start_fullscreen", false);
			Debugger.V("Start fullscreen: " + startFullScreen);
			if (startFullScreen)
				FullScreen = true;
		}

		protected override void OnDebugModeChanged()
		{
			base.OnDebugModeChanged();
			Debugger.C(DebugMode ? "Debug drawing enabled. Press F2 to disable." : "Debug drawing disabled.");
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void mapForm_VisibleChanged(object sender, EventArgs e)
		{
			if (mapForm.Visible)
			{
				mapForm.Map = map;
				tabsMiddle.Visible = false;
				tabsBottomRight.Parent = splitterRight.Panel1;
				splitterRightMiddle.HidePanel(2);
			}
			else
			{
				splitterRightMiddle.ShowPanel(2);
				tabsBottomRight.Parent = splitterRightMiddle.Panel2;
				tabsMiddle.Visible = true;
				mapForm.Map = null;
				map.Parent = tab3DMap;
			}
		}

		protected override void OnMapSceneStarted(MapScene scene)
		{
			base.OnMapSceneStarted(scene);
			scene.InputBehaviour.MousePressed += InputBehaviour_MousePressed;
			if (minimap != null && scene == map.Scene)
				minimap.Scene = scene;
			SetApplicationStatus("Map scene ready.", Theme.HighlightMidColour);
#if DEBUG
			DebugMode = true;
#endif
		}

		private void OnDeviceLoaded(Device device)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<Device>(OnDeviceLoaded), device);
				return;
			}
			listDevices.Controls.Add(new DeviceListItem(device));
		}

		private void OnUserLoaded(User user)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<User>(OnUserLoaded), user);
				return;
			}
			listUsers.Controls.Add(new UserListItem(user));
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
			listIncidents.Controls.Add(new WaypointListItem(waypoint));
		}

		private void OnNodeLoaded(Node node)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<Node>(OnNodeLoaded), node);
				return;
			}
			listNodes.Controls.Add(new NodeListItem(node));
		}

		private void OnSelectionGroupSelectionChanged(ISelectableGroup obj)
		{
			if (obj != globalSelectionGroup)
				return;
			/*
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
			 * */
		}

		private void InputBehaviour_MousePressed(MapSceneInput.MapSceneMouseEventArgs args)
		{
			if (args.Button != MapSceneInput.MouseButtons.Left)
				return;

			Marker[] clickedMarkers = args.Scene.CameraController.MarkersFromScreenRay(args.X, args.Y);

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
					globalSelectionGroup.SetSelection(selectables[((selectables.IndexOf(intersection[intersection.Length - 1]) + 1) % selectables.Count)]);
			}
		}

		private void TestKeys(object sender, KeyEventArgs e)
		{
			if (!FirstShown)
				return;

			//ALT
			if (e.Modifiers == Keys.Alt)
			{
				switch (e.KeyCode)
				{
					case Keys.Enter: //Keys.Return is the same, apparently
						FullScreen = !FullScreen;
						e.Handled = true;
						break;
				}
			}
			//NO MODIFIER
			else if (e.Modifiers == Keys.None)
			{
				switch (e.KeyCode)
				{
					case Keys.F2:
						DebugMode = !DebugMode;
						e.Handled = true;
						break;

					case Keys.F3:
						ShowMapInWindow = !ShowMapInWindow;
						e.Handled = true;
						break;

					case Keys.F5:
						ShowConsole = !ShowConsole;
						e.Handled = true;
						break;
				}
			}
		}

		private void PaintSplitter(object sender, PaintEventArgs e)
		{
			SplitContainer splitter = sender as SplitContainer;
			if (splitter == null || splitter.IsSplitterFixed)
				return;
			int length = (splitter.Orientation == Orientation.Vertical
				? splitter.Height : splitter.Width) / 10;
			int top = splitter.Orientation == Orientation.Vertical
				? splitter.Height / 2 - length / 2
				: splitter.SplitterDistance + (splitter.SplitterWidth / 2) - 1;
			int left = splitter.Orientation == Orientation.Vertical
				? splitter.SplitterDistance + (splitter.SplitterWidth / 2) - 2
				: splitter.Width / 2 - length / 2;
			int bottom = splitter.Orientation == Orientation.Vertical
				? top + length
				: top + 2;
			int right = splitter.Orientation == Orientation.Vertical
				? left + 2
				: left + length;
			using (Pen p = new Pen(Theme.ControlDarkColour))
			{
				if (splitter.Orientation == Orientation.Vertical)
				{
					e.Graphics.DrawLine(p, left, top, left, bottom);
					e.Graphics.DrawLine(p, right, top, right, bottom);
				}
				else
				{
					e.Graphics.DrawLine(p, left, top, right, top);
					e.Graphics.DrawLine(p, left, bottom, right, bottom);
				}
			}
		}
	}
}

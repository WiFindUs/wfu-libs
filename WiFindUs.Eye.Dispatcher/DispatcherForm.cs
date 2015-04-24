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
using WiFindUs.Eye.Wave.Markers;
using WiFindUs.Forms;
using WiFindUs.Themes;

namespace WiFindUs.Eye.Dispatcher
{
	public partial class DispatcherForm : WaveMainForm
	{
		private FormWindowState oldWindowState;
		private Rectangle oldBounds;
		//private ActionPanel actionPanel;
		private MapForm mapForm;

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

#if DEBUG
				Debugger.T("entry");
#endif

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
						map3D.Parent = splitterRight.Panel1;
						tabsMiddle.Visible = false;
					}
				}
				else
				{
					if (targetForm == this)
					{
						tabsMiddle.Visible = true;
						map3D.Parent = tab3DMap;
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

#if DEBUG
				Debugger.T("exit");
#endif
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShowMapInWindow
		{
			get { return mapForm == null ? false : mapForm.Visible; }
			set
			{
				if (value == mapForm.Visible)
					return;

				bool fs = FullScreen;
				FullScreen = false;
				this.SuspendAllLayout();
				if (mapForm.Visible)
					mapForm.Hide();
				else
					mapForm.Show(this);
				this.ResumeAllLayout();
				FullScreen = fs;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public DispatcherForm()
		{
#if DEBUG
			Debugger.T("entry");
#endif
			InitializeComponent();
			if (IsDesignMode)
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

			//controls
			listDevices.SelectionGroup = map3D.SelectionGroup;
			listUsers.SelectionGroup = map3D.SelectionGroup;
			listIncidents.SelectionGroup = map3D.SelectionGroup;
			listNodes.SelectionGroup = map3D.SelectionGroup;

			//events
			WiFindUs.Eye.Device.OnDeviceLoaded += OnDeviceLoaded;
			WiFindUs.Eye.User.OnUserLoaded += OnUserLoaded;
			WiFindUs.Eye.Waypoint.OnWaypointLoaded += OnWaypointLoaded;
			WiFindUs.Eye.Node.OnNodeLoaded += OnNodeLoaded;
			map3D.SelectionGroup.SelectionChanged += OnSelectionGroupSelectionChanged;
#if DEBUG
			Debugger.T("exit");
#endif
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

		public override void ApplyTheme(ITheme theme)
		{
			base.ApplyTheme(theme);
			if (theme == null)
				return;
			statusStrip.ForeColor = theme.Foreground.Lighter.Colour;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnFirstShown(EventArgs e)
		{
#if DEBUG
			Debugger.T("entry");
#endif
			SetApplicationStatus("Initializing 3D scene...", Theme.Current.Warning.Mid.Colour);
			base.OnFirstShown(e);

			//set up map form
			mapForm.ApplyWindowStateFromConfig("map");

			//map windowed
			bool startWindowed = WFUApplication.Config.Get("map.start_windowed", false);
			if (startWindowed)
				ShowMapInWindow = true;

			//fullscreen
			bool startFullScreen = WFUApplication.Config.Get("map.start_fullscreen", false);
			Debugger.V("Start fullscreen: " + startFullScreen);
			if (startFullScreen)
				FullScreen = true;
#if DEBUG
			Debugger.T("exit");
#endif
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
				mapForm.Map = map3D;
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
				map3D.Parent = tab3DMap;
			}
		}

		protected override void OnMapSceneStarted(MapScene scene)
		{
			base.OnMapSceneStarted(scene);
			SetApplicationStatus("Map scene ready.", Theme.Current.Highlight.Mid.Colour);

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
			if (obj != map3D.SelectionGroup)
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

		protected override void TestKeys(object sender, KeyEventArgs e)
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
						return;
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
						return;

					case Keys.F3:
						ShowMapInWindow = !ShowMapInWindow;
						e.Handled = true;
						return;
				}
			}

			base.TestKeys(sender, e);
		}
	}
}

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

namespace WiFindUs.Eye.Dispatcher
{
    public partial class DispatcherForm : MainForm
    {
        private EyeContext eyeContext = null;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public EyeContext EyeContext
        {
            get { return eyeContext; }
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
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnFirstShown(EventArgs e)
        {
            base.OnFirstShown(e);
            
            Screen primary = Screen.PrimaryScreen;
            Rectangle primaryBounds = primary.WorkingArea;
            if (WFUApplication.UsesConsoleForm)
            {
                Bounds = new Rectangle(primaryBounds.Left, primaryBounds.Top, primaryBounds.Width, 3 * primaryBounds.Height / 4);
                Console.Bounds = new Rectangle(Bounds.Left, Bounds.Bottom + 3, Bounds.Width, primaryBounds.Height - 6 - Bounds.Height);
            }
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
    }
}

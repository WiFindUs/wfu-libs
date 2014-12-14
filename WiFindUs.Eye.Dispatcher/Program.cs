using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Eye.Wave;

namespace WiFindUs.Eye.Dispatcher
{
    static class Program
    {
        [STAThread]
        static void Main(String[] args)
        {
            WFUApplication.Name = "Ubi";
            WFUApplication.Description = "First-Responder Asset Management System";
            WFUApplication.ConfigFilePath += "|eye.conf|dispatcher.conf|ubi.conf";
            WFUApplication.MainFormType = typeof(DispatcherForm);
            WFUApplication.MainLaunchAction = MapControl.StartRenderLoop;
            WFUApplication.Run(args);
        }
    }
}

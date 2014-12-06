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
            WFUApplication.UsesMySQL = true;
            WFUApplication.MySQLContextType = typeof(WiFindUs.Eye.EyeContext);
            WFUApplication.MainFormType = typeof(DispatcherForm);
            WFUApplication.GoogleAPIKey = "AIzaSyDLmgbA9m1Qk23yJHRriXoOyy5XGiPZXM8";
            WFUApplication.MainLaunchAction = MapControl.StartRenderLoop;
            WFUApplication.Run(args);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiFindUs.Eye.Dispatcher
{
    static class Program
    {
        [STAThread]
        static void Main(String[] args)
        {
            WFUApplication.Name = "Dispatcher";
            WFUApplication.Description = "An Eye program designed to help first-responders manage their personnel.";
            WFUApplication.ConfigFilePath += "|eye.conf|dispatcher.conf";
            WFUApplication.UsesMutex = true;
            WFUApplication.UsesMySQL = true;
            WFUApplication.MySQLContextType = typeof(WiFindUs.Eye.EyeContext);
            WFUApplication.MainFormType = typeof(DispatcherForm);
            WFUApplication.Run(args);
        }
    }
}

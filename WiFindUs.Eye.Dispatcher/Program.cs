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
            WFUApplication.Description = "An Eye program designed to help first-responders manage their personell.";
            WFUApplication.ConfigFilePath += "|eye.conf|dispatcher.conf";
            WFUApplication.UsesMutex = true;
            WFUApplication.UsesMySQL = true;
            WFUApplication.MySQLContextType = typeof(WiFindUs.Eye.EyeContext);
            WFUApplication.MainFormType = typeof(DispatcherForm);
#if DEBUG
            WFUApplication.InitialVerbosity = Debugger.Verbosity.Verbose;
            WFUApplication.UsesConsoleForm = true;
#else
            WFUApplication.InitialVerbosity = Debugger.Verbosity.Information;
#endif
            WFUApplication.Run(args);
        }
    }
}

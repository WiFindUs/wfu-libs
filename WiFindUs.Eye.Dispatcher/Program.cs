using System;
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
			WFUApplication.MainFormType = typeof(DispatcherForm);
			WFUApplication.RunApplicationDelegate = WaveMainForm.RunApplication;
			WFUApplication.ConfigFilePath = "eye.conf";
			WFUApplication.Run(args);
		}
	}
}

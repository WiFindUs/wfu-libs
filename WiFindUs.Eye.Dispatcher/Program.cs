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
			WFUApplication.ConfigFilePath += "|eye.conf|dispatcher.conf|ubi.conf";
			WFUApplication.MainFormType = typeof(DispatcherForm);
			WFUApplication.MainLaunchAction = WaveMainForm.StartRenderLoop;
			WFUApplication.Run(args);
		}
	}
}

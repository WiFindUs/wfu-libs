using System;

namespace WiFindUs.Eye.Simulator
{
    static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(String[] args)
		{
			WFUApplication.Name = "EyeSim";
			WFUApplication.Description = "Mesh simulation and testing suite for the Eye platform.";
			WFUApplication.MainFormType = typeof(SimulatorForm);
			WFUApplication.ConfigFilePath = "eye.conf";
			WFUApplication.Run(args);
		}
	}
}

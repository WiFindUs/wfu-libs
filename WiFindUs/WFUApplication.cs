﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace WiFindUs
{
	public static class WFUApplication
	{
		/// <summary>
		/// Returns if a WFUApplication is already running.
		/// </summary>
		public static bool Running
		{
			get { return running; }
		}

		/// <summary>
		/// Determines if any of the properties of this params object can be changed. Will be TRUE once the application has used it for initialization.
		/// </summary>
		public static bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				if (readOnly)
					return;
				readOnly = value;
			}
		}

		/// <summary>
		/// Type of MainForm you'd like to instantiate as the entry point. Must be a WiFindUs.MainForm or subclass.
		/// </summary>
		public static Type MainFormType
		{
			get { return mainFormType; }
			set { if (!readOnly) mainFormType = value; }
		}

		/// <summary>
		/// A reference to the application's main form.
		/// </summary>
		public static MainForm MainForm
		{
			get { return mainForm; }
			set { if (mainForm == null) mainForm = value; }
		}

		/// <summary>
		/// The name of the application.
		/// </summary>
		public static string Name
		{
			get { return applicationName; }
			set { if (!readOnly) applicationName = value; }
		}

		/// <summary>
		/// The company owning the application.
		/// </summary>
		public static string Company
		{
			get { return applicationCompany; }
			set { if (!readOnly) applicationCompany = value; }
		}

		/// <summary>
		/// A description of the software.
		/// </summary>
		public static string Description
		{
			get { return applicationDescription; }
			set { if (!readOnly) applicationDescription = value; }
		}

		/// <summary>
		/// If True, will limit this application to running only one copy at any one time.
		/// </summary>
		public static bool UsesMutex
		{
			get { return usesMutex; }
			set { if (!readOnly) usesMutex = value; }
		}

		/// <summary>
		/// If true, a notify icon will be placed in the system tray. Also causes closing the main form to simply hide it, rather than close the entire application. This is because the notify icon has a right-click menu with an 'Exit' link, which closes the application proper.
		/// </summary>
		public static bool UsesTrayIcon
		{
			get { return usesTrayIcon; }
			set { if (!readOnly) usesTrayIcon = value; }
		}

		/// <summary>
		/// If True, will generate a console form upon form generation. If UsesTrayIcon is also True, a link to the console window will be added to the right-click menu.
		/// </summary>
		public static bool UsesConsoleForm
		{
			get { return usesConsoleForm; }
			set { if (!readOnly) usesConsoleForm = value; }
		}

		/// <summary>
		/// The string to use as the mutex name when UsesMutex is TRUE. If unset, a name based on SoftwareName will be used.
		/// </summary>
		public static string MutexOverride
		{
			get { return mutexOverride; }
			set { if (!readOnly) mutexOverride = value; }
		}

		/// <summary>
		/// The string to use in the error message that is displayed when a mutex collision forces an additional copy of an app to close. If unset, a default message will be generated.
		/// </summary>
		public static string MutexOverrideErrorMessage
		{
			get { return mutexOverrideErrorMessage; }
			set { if (!readOnly) mutexOverrideErrorMessage = value; }
		}

		/// <summary>
		/// If set, the app data (in Users\AppData\Roaming) will be named according to this value. It will use Name if not.
		/// </summary>
		public static string AppDataFolderName
		{
			get { return appDataFolderName; } 
			set { if (!readOnly) appDataFolderName = value; }
		}

		/// <summary>
		/// The built-in debugger will automatically use this as the default verbosity level.
		/// </summary>
		public static Debugger.Verbosity InitialVerbosity
		{
			get { return initialDebugLevel; }
			set { if (!readOnly) initialDebugLevel = value; }
		}

		/// <summary>
		/// If set, this will be the path to which log files are written. Will default to ["./" + Name + "_log.txt"] if unset.
		/// </summary>
		public static string LogPath
		{
			get
			{
				return logPath.Length == 0 ? Name + "_log.txt" : logPath;
			}
			set
			{
				if (!readOnly)
					logPath = value == null ? "" : value.Trim();
			}
		}

		/// <summary>
		/// The default config file to load, if any. Delimit multiple config files with a bar character (|) Will default to "wfu.conf" if not set.
		/// </summary>
		public static string ConfigFilePath
		{
			get
			{
				return configPaths.Length == 0 ? "wfu.conf" : configPaths;
			}
			set
			{
				if (!readOnly)
					configPaths = value == null ? "" : value.Trim();
			}
		}

		/// <summary>
		/// The ConfigFile instance in use by this application.
		/// </summary>
		public static ConfigFile Config
		{
			get { return config; }
		}

		/// <summary>
		/// The entry Assembly of the currently running Application.
		/// </summary>
		public static Assembly Assembly
		{
			get
			{
				if (entryAssembly == null)
					entryAssembly = Assembly.GetEntryAssembly();
				return entryAssembly;
			}
		}

		/// <summary>
		/// The entry AssemblyName object of the currently running Application.
		/// </summary>
		public static AssemblyName AssemblyNameObject
		{
			get
			{
				if (entryAssemblyNameObject == null && Assembly != null)
					entryAssemblyNameObject = Assembly.GetName();
				return entryAssemblyNameObject;
			}
		}

		/// <summary>
		/// The name of the entry Assembly of the currently running Application.
		/// </summary>
		public static String AssemblyName
		{
			get
			{
				if (entryAssemblyName == "" && AssemblyNameObject != null)
					entryAssemblyName = AssemblyNameObject.Name;
				return entryAssemblyName;
			}
		}

		/// <summary>
		/// Gets the Version of the entry Assembly.
		/// </summary>
		public static Version AssemblyVersion
		{
			get
			{
				if (assemblyVersion == null && AssemblyNameObject != null)
					assemblyVersion = AssemblyNameObject.Version;
				return assemblyVersion;
			}
		}

		/// <summary>
		/// Gets the company's data storage path based on the current user's roaming directory.
		/// e.g. "C:\Users\marzer\AppData\Roaming\WiFindUs"
		/// </summary>
		public static string AppDataCompanyPath
		{
			get
			{
				return Path.Combine(
					//app data; e.g. "C:\Users\marzer\AppData\Roaming"
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),

					//"WiFindUs"
					Company);
			}
		}

		/// <summary>
		/// Gets the data storage path of the current application based on the other properties provided at initialization.
		/// e.g. "C:\Users\marzer\AppData\Roaming\Marzersoft\Flactric"
		/// </summary>
		public static string DataPath
		{
			get
			{
				return Path.Combine(
					//app data; e.g. "C:\Users\marzer\AppData\Roaming\Marzersoft"
						AppDataCompanyPath,

						//subfolder; e.g. "Flactric"
						AppDataFolderName.Length == 0 ? Name : AppDataFolderName
					);
			}
		}

		/// <summary>
		/// Gets the full path to the application's executable.
		/// e.g. "C:\Program Files (x86)\Marzersoft\Flactric\Flactric.exe"
		/// </summary>
		public static string ExecutablePath
		{
			get
			{
				if (executablePath == "" && Assembly != null)
					executablePath = Assembly.Location;
				return executablePath;
			}
		}

		/// <summary>
		/// Gets the directory containing the application's executable.
		/// e.g. "C:\Program Files (x86)\Marzersoft\Flactric"
		/// </summary>
		public static string ExecutableDirectoryPath
		{
			get
			{
				if (executableDirectoryPath == "" && ExecutablePath != "")
					executableDirectoryPath = Directory.GetParent(ExecutablePath).ToString();
				return executableDirectoryPath;
			}
		}

		/// <summary>
		/// Returns the actual name of any mutexes created by the applicationbased on the other properties provided at initialization.
		/// </summary>
		public static string MutexName
		{
			get
			{
				return mutexOverride.Length > 0 ? mutexOverride
					: Company + "_Mutex_" + Name;
			}
		}

		/// <summary>
		/// Returns the actual error message shown when a mutex collision occurs, based on the other properties provided at initialization.
		/// </summary>
		public static string MutexErrorMessage
		{
			get
			{
				return mutexOverrideErrorMessage.Length > 0 ? mutexOverrideErrorMessage
					: "A copy of " + Name + " is already running.";
			}
		}

		/// <summary>
		/// Returns the Icon packaged into the executable used to launch the assembly.
		/// </summary>
		public static Icon Icon
		{
			get
			{
				if (hostIcon == null)
				{
					try
					{
						hostIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
					}
					catch (Exception e)
					{
						Debugger.Ex(e);
					}
				}
				return hostIcon;
			}
		}

		private static string executablePath = "";
		private static Version assemblyVersion = null;
		private static string entryAssemblyName = "";
		private static AssemblyName entryAssemblyNameObject = null;
		private static Assembly entryAssembly = null;
		private static string logPath = "";
		private static Debugger.Verbosity initialDebugLevel = Debugger.Verbosity.Information;
		private static bool usesConsoleForm = false;
		private static bool usesTrayIcon = false;
		private static bool usesMutex = false;
		private static string applicationDescription = "A WiFindUs application.";
		private static string applicationCompany = "WiFindUs";
		private static string applicationName = "Program";
		private static bool running = false;
		private static bool readOnly = false;
		private static Type mainFormType = null;
		private static MainForm mainForm = null;
		private static string mutexOverride = "";
		private static string mutexOverrideErrorMessage = "";
		private static string appDataFolderName = "";
		private static string executableDirectoryPath = "";
		private static string configPaths = "wfu.conf";
		private static Icon hostIcon = null;
		private static ConfigFile config = null;

		public static void Run(String[] args)
		{
			if (Running)
				throw new InvalidOperationException("A WFUApplication is already running.");

			//check command line for debug flags
			if (args == null)
				args = new String[] { };
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i][0] != '/' && args[i][0] != '-')
					continue;
				args[i] = args[i].Substring(1).ToLower();
				switch (args[i])
				{
					case "0":
					case "1":
					case "2":
					case "3":
					case "4":
						InitialVerbosity = (Debugger.Verbosity)Int32.Parse(args[i]);
						break;

					case "console":
						UsesConsoleForm = true;
						break;

					case "conf":
						if (i == args.Length-1)
							break;
						string file = args[i+1];
						if (!File.Exists(file))
							break;
						configPaths += "|" + file;
						break;
				}
			}

			//create mutex if necessary
			Mutex mutex = null;
			if (UsesMutex)
			{

				try
				{
					mutex = Mutex.OpenExisting(MutexName);
					MessageBox.Show(MutexErrorMessage, Name + " :: Multiple Copies", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				catch (WaitHandleCannotBeOpenedException)
				{
					Debugger.Initialize(LogPath,InitialVerbosity);
					Debugger.V("Creating new thread-locking mutex...");
					mutex = new Mutex(true, MutexName);
					Debugger.V("Mutex created OK.");
				}
				catch (Exception e)
				{
					Debugger.Ex(e);
					MessageBox.Show("An exception was thrown trying to create the Mutex object;\n\n" + e.Message, "Exception creating Mutex", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Debugger.Dispose();
					return;
				}
			}
			else
				Debugger.Initialize(LogPath,InitialVerbosity);

			//check data directory
			Debugger.V("Testing for presence of DataPath (\"" + DataPath + "\")");
			if (!Directory.Exists(DataPath))
			{
				try
				{
					Debugger.W("AppDataPath \"" + DataPath + "\" doesn't exist; creating...");
					Directory.CreateDirectory(DataPath);
					Debugger.W("Created OK.");
				}
				catch (Exception exc)
				{
					Debugger.Ex(exc);
					MessageBox.Show(exc.GetType().ToString() + ": \n\n\"" + exc.Message + "\"\n\nThe application will now exit.", "Exception thrown", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Debugger.Dispose();
					return;
				}
			}
			else
				Debugger.V("AppDataPath found OK.");

			//loading config files
			Debugger.V("Loading config files...");
			String[] files = ConfigFilePath.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			config = new ConfigFile(files);
			Debugger.V(config.ToString());

			//initialize form type
			Type formType = MainFormType;
			Debugger.V("Checking MainFormType...");
			if (formType == null || formType != typeof(MainForm) && !formType.IsSubclassOf(typeof(MainForm)))
			{
				Debugger.W("MainFormType provided to ProgramCreateParams was not a valid subclass of WiFindUs.MainForm! Will override.");
				formType = typeof(MainForm);
			}
			else
				Debugger.V("MainFormType OK.");

			//instantiate form and application
			ReadOnly = true;
			running = true;
			Debugger.V("Setting application visual styles and text rendering properties...");
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Debugger.V("Invoking MainForm constructor...");
			Application.Run(mainForm = (MainForm)formType.GetConstructor(new Type[] { }).Invoke(new object[] { }));
			Debugger.V("Application terminating...");
			running = false;

			//release resources and close debugger
			if (mutex != null)
			{
				Debugger.V("Releasing thread-locking mutex...");
				mutex.ReleaseMutex();
				Debugger.V("Released OK.");
			}
			Debugger.Dispose();
		}
	}
}

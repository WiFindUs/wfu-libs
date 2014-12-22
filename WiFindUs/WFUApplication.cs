﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using WiFindUs.IO;
using WiFindUs.Forms;
using WiFindUs.Controls;

namespace WiFindUs
{
	public static class WFUApplication
	{
        private static Random random = new Random();
        private static string executablePath = "";
        private static Version assemblyVersion = null;
        private static string entryAssemblyName = "";
        private static AssemblyName entryAssemblyNameObject = null;
        private static Assembly entryAssembly = null;
        private static string logPath = "";
        private static Debugger.Verbosity initialDebugLevel
#if DEBUG
            = Debugger.Verbosity.Verbose;
#else
            = Debugger.Verbosity.Information;
#endif
        private static bool usesMutex = false;
        private static string applicationDescription = "A WiFindUs application.";
        private static string applicationCompany = "WiFindUs";
        private static string applicationName = "Program";
        private static string applicationEdition = "Standard";
        private static bool running = false;
        private static bool readOnly = false;
        private static Type mainFormType = null;
        private static MainForm mainForm = null;
        private static string mutexOverride = "";
        private static string mutexOverrideErrorMessage = "";
        private static string appDataFolderName = "";
        private static string executableDirectoryPath = "";
        private static string configPaths = "";
        private static Icon hostIcon = null;
        private static volatile ConfigFile config = null;
        private static ResourceLoader<Image> imageLoader = new ResourceLoader<Image>(Image.FromFile);
        private static Mutex mutex = null;
        private static List<Func<object, bool>> loadingTasks = new List<Func<object, bool>>();
        private static SplashForm splashForm = null;
        private static bool splashLoadingFinished = false;
        private static Theme theme = new Theme();
        private static string googleAPIKey = "AIzaSyDLmgbA9m1Qk23yJHRriXoOyy5XGiPZXM8";
        private static Action<MainForm> mainLaunchAction = null;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////
        
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
                theme.ReadOnly = value;
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
        /// The edition of the application.
        /// </summary>
        public static string Edition
        {
            get { return applicationEdition; }
            set { if (!readOnly) applicationEdition = value; }
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

        /// <summary>
        /// Returns the ImageLoader instance in use by this application.
        /// </summary>
        public static ResourceLoader<Image> Images
        {
            get { return imageLoader; }
        }

        /// <summary>
        /// The base list of tasks passed to the splash screen for application initialization. 
        /// This is merged with the overridable list from your MainForm(), which is passed via StartSplashLoading().
        /// </summary>
        private static List<Func<bool>> LoadingTasks
        {
            get
            {
                List<Func<bool>> tasks = new List<Func<bool>>();
                tasks.Add(InitializeDebugger);
                tasks.Add(CheckAppDataPath);
                tasks.Add(LoadConfigFiles);
                return tasks;
            }
        }

        /// <summary>
        /// Gets or sets the splash screen loading completion status.
        /// This can only be set to true once (the splash form does this itself);
        /// doing so restores the main form so regular application use may begin.
        /// </summary>
        public static bool SplashLoadingFinished
        {
            get { return splashLoadingFinished; }
            set
            {
                if (!value || splashLoadingFinished)
                    return;
                splashLoadingFinished = true;
                mainForm.ShowInTaskbar = true;
                mainForm.WindowState = FormWindowState.Normal;
                mainForm.ShowForm();
                splashForm = null;
            }
        }

        /// <summary>
        /// Gets or sets the current status string displayed by the splash form.
        /// </summary>
        public static string SplashStatus
        {
            get { return splashForm == null ? "" : splashForm.Status; }
            set
            {
                Debugger.I("Splash loading: " + (value ?? ""));
                if (splashForm != null)
                    splashForm.Status = value;
            }
        }

        /// <summary>
        /// Gets or sets the colour and font theme in use by this application.
        /// </summary>
        public static Theme Theme
        {
            get { return theme; }
            set
            {
                if (readOnly || value == null || value == theme)
                    return;
                if (theme != null)
                    theme.Dispose();
                theme = value;
            }
        }

        /// <summary>
        /// Gets or sets the Google API key in use by this application.
        /// </summary>
        public static string GoogleAPIKey
        {
            get { return googleAPIKey; }
            set { if (!readOnly) googleAPIKey = value ?? ""; }
        }

        /// <summary>
        /// The action used to trigger the main application loop.
        /// </summary>
        public static Action<MainForm> MainLaunchAction
        {
            get { return mainLaunchAction; }
            set { if (!readOnly) mainLaunchAction = value; }
        }

        /// <summary>
        /// An application-wide static random number generation instance.
        /// </summary>
        public static Random Random
        {
            get { return random; }
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Runs your WFUApplication. Handles parsing of command line arguments, loading of
        /// config files, and initialization of the main form.
        /// </summary>
        /// <param name="args">The command line arguments from Main().</param>
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

					case "conf":
						if (i == args.Length-1)
							break;
						string file = args[i+1];
						if (File.Exists(file))
						    configPaths += "|" + file;
						break;
				}
			}

            if (UsesMutex && !CreateMutex())
            {
                Free();
                return;
            }

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
            mainForm = (MainForm)formType.GetConstructor(new Type[] { }).Invoke(new object[] { });
			Debugger.V("Invoking Main() launch action...");
            if (mainLaunchAction == null)
                DefaultMainLaunch(mainForm);
            else
                mainLaunchAction(mainForm);
			
			Debugger.V("Application terminating...");

            //release resources and close debugger
            Free();
		}

        public static void StartSplashLoading(List<Func<bool>> tasks)
        {
            if (splashForm != null || splashLoadingFinished)
                return;
            Debugger.V("Invoking splash form...");
            List<Func<bool>> allTasks = LoadingTasks;
            allTasks.AddRange(tasks);
            splashForm = new SplashForm(allTasks);
            splashForm.Show();
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private static void DefaultMainLaunch(MainForm mainForm)
        {
            Application.Run(mainForm);
        }

        private static bool CreateMutex()
        {
            if (!UsesMutex)
                return true;

            try
            {
                mutex = Mutex.OpenExisting(MutexName);
                MessageBox.Show(MutexErrorMessage, Name + " :: Multiple Copies", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                mutex = new Mutex(true, MutexName);
            }
            catch (Exception e)
            {
                MessageBox.Show("An exception was thrown trying to create the Mutex object;\n\n" + e.Message, "Exception creating Mutex", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private static bool InitializeDebugger()
        {
            splashForm.Status = "Initializing debugger";
            Debugger.Initialize(LogPath, InitialVerbosity);
            return true;
        }

        private static bool CheckAppDataPath()
        {
            splashForm.Status = "Verifying file paths";
            
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
                    return false;
                }
            }
            else
                Debugger.V("AppDataPath found OK.");
            
            return true;
        }

        private static bool LoadConfigFiles()
        {
            splashForm.Status = "Loading configuration files";
            
            Debugger.I("Loading config files...");
            String[] files = ConfigFilePath.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            config = new ConfigFile(files);
            config.LogMissingKeys = config.Get("config.log_missing_keys", false);
            Debugger.V(config.ToString());
            Debugger.I("Finished loading config files.");
            return true;
        }

        private static void Free()
        {
            if (theme != null)
            {
                theme.Dispose();
                theme = null;
            }
            
            
            if (imageLoader != null)
            {
                imageLoader.Dispose();
                imageLoader = null;
            }
            
            if (mutex != null)
            {
                Debugger.V("Releasing thread-locking mutex...");
                mutex.ReleaseMutex();
                Debugger.V("Released OK.");
                mutex = null;
            }

            Debugger.Dispose();

            running = false;
        }
	}
}

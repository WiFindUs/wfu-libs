using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using Devart.Data.Linq;

namespace WiFindUs
{
	public static class WFUApplication
	{
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
        private static bool usesMySQL = false;
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
        private static string configPaths = "";
        private static Icon hostIcon = null;
        private static ConfigFile config = null;
        private static ResourceLoader<Image> imageLoader = new ResourceLoader<Image>(Image.FromFile);
        private static string mysqlAddress = "";
        private static int mysqlPort= -1;
        private static string mysqlUsername = "";
        private static string mysqlPassword = "";
        private static string mysqlDatabase = "";
        private static string mysqlCharset = "";
        private static Mutex mutex = null;
        private static Type mysqlContextType = null;
        private static DataContext mysqlContext = null;


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

        /// <summary>
        /// Returns the ImageLoader instance in use by this application.
        /// </summary>
        public static ResourceLoader<Image> Images
        {
            get { return imageLoader; }
        }

        /// <summary>
        /// If true, the application will attempt to connect to a MySQL database upon launching, and will abort if the connection fails.
        /// </summary>
        public static bool UsesMySQL
        {
            get { return usesMySQL; }
            set { if (!readOnly) usesMySQL = value; }
        }

        /// <summary>
        /// Returns the address on which the MySQL connection will be made. First checks the local override,
        /// then the local ConfigFile instance at key "mysql.address", then returns "localhost" as a fallback.
        /// </summary>
        public static string MySQLAddress
        {
            get
            {
                return mysqlAddress.Length > 0 ? mysqlAddress :
                    (config != null ? config.Get("mysql.address", "localhost") : "localhost");
            }
            set { if (!readOnly) mysqlAddress = value == null ? "" : value.Trim(); }
        }

        /// <summary>
        /// Returns the port on which the MySQL connection will be made. First checks the local override,
        /// then the local ConfigFile instance at key "mysql.port", then returns 3306 as a fallback.
        /// </summary>
        public static int MySQLPort
        {
            get
            {
                return mysqlPort >= 1024 && mysqlPort <= 65535 ? mysqlPort :
                    (config != null ? config.Get("mysql.port", 3306) : 3306);
            }
            set { if (!readOnly) mysqlPort = value; }
        }

        /// <summary>
        /// Returns the username with which the MySQL connection will be made. First checks the local override,
        /// then the local ConfigFile instance at key "mysql.username", then returns "root" as a fallback.
        /// </summary>
        public static string MySQLUsername
        {
            get
            {
                return mysqlUsername.Length > 0 ? mysqlUsername :
                    (config != null ? config.Get("mysql.username", "root") : "root");
            }
            set { if (!readOnly) mysqlUsername = value == null ? "" : value.Trim(); }
        }

        /// <summary>
        /// Returns the database name to which the MySQL connection will be made. First checks the local override,
        /// then the local ConfigFile instance at key "mysql.database".
        /// </summary>
        public static string MySQLDatabase
        {
            get
            {
                return mysqlDatabase.Length > 0 ? mysqlDatabase :
                    (config != null ? config.Get("mysql.database", "") : "");
            }
            set { if (!readOnly) mysqlDatabase = value == null ? "" : value.Trim(); }
        }

        /// <summary>
        /// Returns the password with which the MySQL connection will be made. First checks the local override,
        /// then the local ConfigFile instance at key "mysql.password".
        /// </summary>
        public static string MySQLPassword
        {
            get
            {
                return mysqlPassword.Length > 0 ? mysqlPassword :
                    (config != null ? config.Get("mysql.password", "") : "");
            }
            set { if (!readOnly) mysqlPassword = value == null ? "" : value.Trim(); }
        }

        /// <summary>
        /// Returns the charset used for the MySQL connection. First checks the local override,
        /// then the local ConfigFile instance at key "mysql.charset", then returns "utf8" as a fallback.
        /// </summary>
        public static string MySQLCharset
        {
            get
            {
                return mysqlCharset.Length > 0 ? mysqlCharset :
                    (config != null ? config.Get("mysql.charset", "utf8") : "utf8");
            }
            set { if (!readOnly) mysqlCharset = value == null ? "" : value.Trim(); }
        }

        /// <summary>
        /// The Context type that will be created when the application is spawned. Must be Devart.Data.Linq.DataContext or a subclass.
        /// </summary>
        public static Type MySQLContextType
        {
            get { return mysqlContextType; }
            set { if (!readOnly) mysqlContextType = value; }
        }

        private static string MySQLConnectionString
        {
            get
            {
                return "server=" + MySQLAddress
                        + ";user=" + MySQLUsername
                        + ";database=" + MySQLDatabase
                        + ";port=" + MySQLPort
                        + ";charset=" + MySQLCharset
                        + ";password=" + MySQLPassword + ";";
            }
        }

        private static string LinqConnectionString
        {
            get
            {
                return "Host=" + MySQLAddress
                        + ";Port=" + MySQLPort
                        + ";Persist Security Info=True"
                        + ";User Id=" + MySQLUsername
                        + ";Database=" + MySQLDatabase
                        + ";Password=" + MySQLPassword + ";";
            }
        }

        public static DataContext MySQLDataContext
        {
            get { return mysqlContext;  }
        }

        /////////////////////////////////////////////////////////////////////
        // RUN
        /////////////////////////////////////////////////////////////////////

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
						if (File.Exists(file))
						    configPaths += "|" + file;
						break;
				}
			}

			//create mutex if necessary
			if (UsesMutex)
			{

				try
				{
					mutex = Mutex.OpenExisting(MutexName);
					MessageBox.Show(MutexErrorMessage, Name + " :: Multiple Copies", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Free();
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
                    Free();
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
                    Free();
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

            //do mysql
            if (UsesMySQL)
            {
                if (mysqlContextType == null || !mysqlContextType.IsSubclassOf(typeof(DataContext)))
                {
                    String message = "The MySQLContextType was missing or invalid!";
                    Debugger.E(message);
                    MessageBox.Show(message + "\n\nThe application will now exit.", "MySQL Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Free();
                    return;
                }
                
                try
                {
                    mysqlContext = (DataContext)mysqlContextType.GetConstructor(new Type[] { typeof(String) }).Invoke(new object[] { LinqConnectionString });
                }
                catch (Exception ex)
                {
                    String message = "There was an error establishing the MySQL connection: " + ex.Message;
                    Debugger.E(message);
                    MessageBox.Show(message + "\n\nThe application will now exit.", "MySQL Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Free();
                    return;
                }

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
			Application.Run(mainForm = (MainForm)formType.GetConstructor(new Type[] { }).Invoke(new object[] { }));
			Debugger.V("Application terminating...");
			running = false;

			//release resources and close debugger
            Free();
		}

        private static void Free()
        {
            if (mysqlContext != null)
            {
                mysqlContext.Dispose();
                mysqlContext = null;
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
        }
	}
}

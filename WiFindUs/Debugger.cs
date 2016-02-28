using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace WiFindUs
{
    public static class Debugger
	{
		[Flags]
		public enum Verbosity
		{
			None = 0,
			Verbose = 1,
			Information = 2,
			Warning = 4,
			Error = 8,
			Exception = 16,
			Console = 32,
			All = 63
		};

		public static event Action<LogItem> OnDebugOutput;

		internal static readonly Dictionary<Debugger.Verbosity, Color> Colours = new Dictionary<Debugger.Verbosity, Color>()
		{
			{ Debugger.Verbosity.Verbose, ColorTranslator.FromHtml("#999999")},
			{ Debugger.Verbosity.Information, ColorTranslator.FromHtml("#FFFFFF")},
			{ Debugger.Verbosity.Warning, ColorTranslator.FromHtml("#FFA600")},
			{ Debugger.Verbosity.Error, ColorTranslator.FromHtml("#DF3F26")},
			{ Debugger.Verbosity.Exception, ColorTranslator.FromHtml("#df3f26")},
			{ Debugger.Verbosity.Console, ColorTranslator.FromHtml("#1c97ea")}
		};
		
		private const int MAX_HISTORY_LENGTH = 2048;
		private static Verbosity allowedFlags;
		private static StreamWriter outFile = null;
		private static DateTime runningSince;
		private static PerformanceCounter cpuCounter;
		private static PerformanceCounter ramCounter;
		private static LinkedList<LogItem> logHistory = new LinkedList<LogItem>();

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The initialization state of the Debugger.
		/// </summary>
		public static bool Initialized
		{
			get { return outFile != null; }
		}

		/// <summary>
		/// The date and time the application was launched.
		/// </summary>
		public static DateTime RunningSince
		{
			get { return runningSince; }
		}

		/// <summary>
		/// The current CPU usage, as a percentage.
		/// </summary>
		public static float ProcessorUsage
		{
			get { return Initialized ? cpuCounter.NextValue() : 0.0f; }
		}

		/// <summary>
		/// The current system RAM usage, as a percentage.
		/// </summary>
		public static float MemoryUsage
		{
			get { return Initialized ? ramCounter.NextValue() : 0.0f; }
		}

		/// <summary>
		/// The length of time the system has been turned on.
		/// </summary>
		public static TimeSpan SystemUptime
		{
			get { return new TimeSpan(0, 0, 0, 0, Environment.TickCount); }
		}

		/// <summary>
		/// The Debugger's current history.
		/// </summary>
		internal static LogItem[] LogHistory
		{
			get
			{
				LogItem[] array = null;
				lock (logHistory)
					array = logHistory.ToArray();
				return array;
			}
		}

		/// <summary>
		/// The set of allowed message verbosities the Debugger was configured with at initialization.
		/// </summary>
		public static Verbosity AllowedVerbosities
		{
			get { return Debugger.allowedFlags; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		internal static void Initialize(string path, Verbosity allowedFlags)
		{
			if (Initialized || WFUApplication.Running || allowedFlags == Verbosity.None)
				return;

			//initialization
			Debugger.allowedFlags = allowedFlags;
			runningSince = DateTime.Now;
			outFile = new System.IO.StreamWriter(path, false);

			cpuCounter = new PerformanceCounter();
			cpuCounter.CategoryName = "Processor";
			cpuCounter.CounterName = "% Processor Time";
			cpuCounter.InstanceName = "_Total";
			ramCounter = new PerformanceCounter("Memory", "Available MBytes");

			//initial logging
			V(WFUApplication.Company + " " + WFUApplication.Name + " logging session opened.");
			System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			StringBuilder sb = new StringBuilder("Loaded assemblies:");
			List<String> assemblyOutput = new List<string>();
			foreach (System.Reflection.Assembly asm in assemblies)
			{
				System.Reflection.AssemblyName asmn = asm.GetName();
				assemblyOutput.Add(String.Format("{0} ({1})", asmn.Name, asmn.Version));
			}
			assemblyOutput.Sort();
			foreach (string s in assemblyOutput)
				sb.Append("\n    " + s);
			V(sb.ToString());
			V("Windows version: {0}", Environment.OSVersion.ToString());
			V("Dot NET version: {0}", Environment.Version.ToString());
			V("Machine name: {0}", Environment.MachineName);
			V("User Name: {0}", Environment.UserName);
			V("Processor Count: {0}", Environment.ProcessorCount.ToString());
			V("Processor Usage: {0}", cpuCounter.NextValue() + "%");
			V("Available RAM: {0}", ramCounter.NextValue() + "Mb");
			sb = new StringBuilder("System monitors:");
			for (int i = 0; i < Screen.AllScreens.Length; i++)
				sb.Append(String.Format("\n    [{0}]: {1}{2}", i, Screen.AllScreens[i].Bounds, Screen.AllScreens[i].Primary ? " (primary)" : ""));
			V(sb.ToString());
			TimeSpan uptime = SystemUptime;
			V("System uptime: {0} hours, {1} minutes, {2} seconds.", uptime.Hours, uptime.Minutes, uptime.Seconds);
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Logs a formatted string with a verbosity level of Verbose.
		/// </summary>
		/// <returns>True</returns>
		public static bool V(string text, params object[] args)
		{
			if (allowedFlags.HasFlag(Verbosity.Verbose))
				Log(Verbosity.Verbose, text, args);
			return true;
		}

		/// <summary>
		/// Logs a formatted string with a verbosity level of Information.
		/// </summary>
		/// <returns>True</returns>
		public static bool I(string text, params object[] args)
		{
			if (allowedFlags.HasFlag(Verbosity.Information))
				Log(Verbosity.Information, text, args);
			return true;
		}

		/// <summary>
		/// Logs a formatted string with a verbosity level of Warning.
		/// </summary>
		/// <returns>True</returns>
		public static bool W(string text, params object[] args)
		{
			if (allowedFlags.HasFlag(Verbosity.Warning))
				Log(Verbosity.Warning, text, args);
			return true;
		}

		/// <summary>
		/// Logs a formatted string with a verbosity level of Error.
		/// </summary>
		/// <returns>True</returns>
		public static bool E(string text, params object[] args)
		{
			if (allowedFlags.HasFlag(Verbosity.Error))
				Log(Verbosity.Error, text, args);
			return true;
		}

		/// <summary>
		/// Logs an exception.
		/// </summary>
		/// <returns>True</returns>
		public static bool Ex(Exception e, bool printStackTrace = false,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (!allowedFlags.HasFlag(Verbosity.Exception))
				return true;

			StringBuilder sb = new StringBuilder("EXCEPTION THROWN!");
			sb.AppendLine("  Calling Member: " + memberName);
			sb.AppendLine("  Source File: " + sourceFilePath);
			sb.AppendLine("  Source Line: " + sourceLineNumber.ToString());
			sb.AppendLine("  Type: " + e.GetType().FullName);
			sb.AppendLine("  Message: " + e.Message);
			if (e.InnerException != null)
			{
				sb.AppendLine("  Inner Type: " + e.InnerException.GetType().FullName);
				sb.AppendLine("  Message: " + e.InnerException.Message);
			}
#if DEBUG
			printStackTrace = true;
#endif
			if (printStackTrace)
			{
				sb.AppendLine("  Stack trace: ");

				StackTrace stackTrace = new StackTrace(1, true);           // get call stack
				StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

				foreach (StackFrame stackFrame in stackFrames)
					sb.AppendLine("    " + stackFrame.ToString());
			}
			Log(Verbosity.Exception, sb.ToString());
			return true;
		}

		public static bool C(string text, params object[] args)
		{
			if (allowedFlags.HasFlag(Verbosity.Console))
				Log(Verbosity.Console, text, args);
			return true;
		}

#if DEBUG
		/// <summary>
		/// Logs trace information about the caller.
		/// </summary>
		public static bool T(string text = "",
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
		{
			return V("[TRACE] {0}:{1} {2} {3}", Path.GetFileName(sourceFilePath), sourceLineNumber, memberName, text);
		}
#endif

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private static void Log(Verbosity verbosity, string text, params object[] args)
		{
			//parse args
			if (args != null && args.Length > 0)
				text = String.Format(text, args);

			//generate string
			LogItem logItem = new LogItem(verbosity, text);
			string itemText = logItem.ToString();

#if DEBUG
			//print to system console
			System.Diagnostics.Debugger.Log((int)verbosity, verbosity.ToString(), itemText + "\n");
#endif

			//print to log file
			if (outFile != null && verbosity != Verbosity.Console)
			{
				outFile.WriteLine(itemText);
				outFile.Flush();
			}

			//add to history buffer
			lock (logHistory)
			{
				int lengthDelta = (logHistory.Count + 1) - MAX_HISTORY_LENGTH;
				while (lengthDelta > 0)
				{
					logHistory.RemoveFirst();
					lengthDelta--;
				}
				logHistory.AddLast(logItem);
			}

			//fire event handler
			if (OnDebugOutput != null)
			{
				try
				{
					OnDebugOutput(logItem);
				}
				catch (ObjectDisposedException) { }
			}
		}

		internal static void Dispose()
		{
			if (!Initialized || WFUApplication.Running)
				return;

			if (OnDebugOutput != null)
				OnDebugOutput = null;

			V("Freeing Debugger resources...");
			cpuCounter.Dispose();
			ramCounter.Dispose();
			logHistory.Clear();
			V("Freed OK.");

			I(WFUApplication.Name + " logging session closed.");

			outFile.Flush();
			outFile.Close();
			outFile.Dispose();
			outFile = null;
		}

		public class LogItem
		{
			private Verbosity verbosity;
			private string threadAlias;
			private DateTime timestamp;
			private string message = "";

			public Verbosity Verbosity
			{
				get { return verbosity; }
			}

			public string ThreadAlias
			{
				get { return threadAlias; }
			}

			public DateTime Timestamp
			{
				get { return timestamp; }
			}

			public string Message
			{
				get { return message; }
			}

			public LogItem(Verbosity verbosity, string message)
			{
				if (verbosity != Verbosity.Console
					&& verbosity != Verbosity.Error
					&& verbosity != Verbosity.Exception
					&& verbosity != Verbosity.Information
					&& verbosity != Verbosity.Verbose
					&& verbosity != Verbosity.Warning)
					throw new InvalidOperationException("Cannot have a verbosity with a combination of flags!");
				if (verbosity == Verbosity.None)
					throw new InvalidOperationException("Must have a verbosity!");

				this.verbosity = verbosity;
				this.message = message ?? "";
				threadAlias = WFUApplication.GetThreadAlias();
				timestamp = DateTime.Now;
			}

			public override string ToString()
			{
				return String.Format("[{0}, {1}, {2}] {3}",
					timestamp.ToString("H:mm:ss"),
					threadAlias,
					Enum.GetName(typeof(Verbosity), verbosity).Substring(0, 1),
					message);
			}
		}
	}
}

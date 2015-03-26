using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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

		public const int MAX_HISTORY_LENGTH = 2048;

		private static Verbosity allowedFlags;
		private static StreamWriter outFile = null;
		private static DateTime runningSince;
		private static PerformanceCounter cpuCounter;
		private static PerformanceCounter ramCounter;
		private static LinkedList<DebuggerLogItem> logHistory = new LinkedList<DebuggerLogItem>();

		public static readonly Dictionary<Debugger.Verbosity, Color> Colours = new Dictionary<Debugger.Verbosity, Color>()
		{
			{ Debugger.Verbosity.Verbose, ColorTranslator.FromHtml("#999999")},
			{ Debugger.Verbosity.Information, ColorTranslator.FromHtml("#FFFFFF")},
			{ Debugger.Verbosity.Warning, ColorTranslator.FromHtml("#FFA600")},
			{ Debugger.Verbosity.Error, ColorTranslator.FromHtml("#DF3F26")},
			{ Debugger.Verbosity.Exception, ColorTranslator.FromHtml("#df3f26")},
			{ Debugger.Verbosity.Console, ColorTranslator.FromHtml("#1c97ea")}
		};

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public static event Action<DebuggerLogItem> OnDebugOutput;
		public static DateTime RunningSince
		{
			get { return runningSince; }
		}
		public static bool Initialized
		{
			get
			{
				return outFile != null;
			}
		}
		public static float ProcessorUsage
		{
			get { return Initialized ? cpuCounter.NextValue() : 0.0f; }
		}
		public static float MemoryUsage
		{
			get { return Initialized ? ramCounter.NextValue() : 0.0f; }
		}
		public static TimeSpan SystemUptime
		{
			get { return new TimeSpan(0, 0, 0, 0, Environment.TickCount); }
		}
		public static DebuggerLogItem[] LogHistory
		{
			get
			{
				DebuggerLogItem[] array = null;
				lock (logHistory)
					array = logHistory.ToArray();
				return array;
			}
		}
		public static Verbosity AllowedVerbosities
		{
			get { return Debugger.allowedFlags; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public static void Initialize(string path, Verbosity allowedFlags)
		{
			if (Initialized || !WFUApplication.Running || allowedFlags == Verbosity.None)
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
			V("Windows version: " + Environment.OSVersion.ToString());
			V("Dot NET version: " + Environment.Version.ToString());
			V("Machine name: " + Environment.MachineName);
			V("User Name: " + Environment.UserName);
			V("Processor Count: " + Environment.ProcessorCount.ToString());
			V("Processor Usage: " + cpuCounter.NextValue() + "%");
			V("Available RAM: " + ramCounter.NextValue() + "Mb");
			StringBuilder sb = new StringBuilder("System monitors:\n");
			for (int i = 0; i < Screen.AllScreens.Length; i++)
				sb.AppendLine(String.Format("    [{0}]: {1}{2}", i, Screen.AllScreens[i].Bounds, Screen.AllScreens[i].Primary ? " (primary)" : ""));
			V(sb.ToString());
			TimeSpan uptime = SystemUptime;
			V(String.Format("System uptime: {0} hours, {1} minutes, {2} seconds.", uptime.Hours, uptime.Minutes, uptime.Seconds));
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public static void Dispose()
		{
			if (!Initialized || !WFUApplication.Running)
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

		public static void V(string text, params object[] args)
		{
			if (!allowedFlags.HasFlag(Verbosity.Verbose))
				return;
			Log(Verbosity.Verbose, text, args);
		}

		public static void I(string text, params object[] args)
		{
			if (!allowedFlags.HasFlag(Verbosity.Information))
				return;
			Log(Verbosity.Information, text, args);
		}

		public static void W(string text, params object[] args)
		{
			if (!allowedFlags.HasFlag(Verbosity.Warning))
				return;
			Log(Verbosity.Warning, text, args);
		}

		public static void E(string text, params object[] args)
		{
			if (!allowedFlags.HasFlag(Verbosity.Error))
				return;
			Log(Verbosity.Error, text, args);
		}

		public static void Ex(Exception e, bool printStackTrace = false)
		{
			if (!allowedFlags.HasFlag(Verbosity.Exception))
				return;
			string log = "EXCEPTION THROWN!";
			log += "\n  Type: " + e.GetType().FullName;
			log += "\n  Message: " + e.Message;
			if (e.InnerException != null)
			{
				log += "\n  Inner Type: " + e.InnerException.GetType().FullName;
				log += "\n  Message: " + e.InnerException.Message;
			}
#if DEBUG
			printStackTrace = true;
#endif
			if (printStackTrace)
			{
				log += "\n  Stack trace: ";

				StackTrace stackTrace = new StackTrace(1, true);           // get call stack
				StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

				foreach (StackFrame stackFrame in stackFrames)
					log += "\n    " + stackFrame.ToString();
			}
			Log(Verbosity.Exception, log);
		}

		public static void C(string text, params object[] args)
		{
			if (!allowedFlags.HasFlag(Verbosity.Console))
				return;
			Log(Verbosity.Console, text, args);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private static void Log(Verbosity verbosity, string text, params object[] args)
		{
			//parse args
			if (args != null && args.Length > 0)
				text = String.Format(text, args);

			//generate string
			DebuggerLogItem logItem = new DebuggerLogItem(verbosity, text);
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
	}
}

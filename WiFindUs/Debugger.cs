﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace WiFindUs
{
	public static class Debugger
	{
		public enum Verbosity
		{
			/// <summary>
			/// 0: All messages are output by the Debugger.
			/// </summary>
			Verbose = 0,

			/// <summary>
			/// 1: Only messages with a verbosity level of Information and higher are output by the Debugger.
			/// </summary>
			Information = 1,

			/// <summary>
			/// 2: Only messages with a verbosity level of Warning and higher are output by the Debugger.
			/// </summary>
			Warning = 2,

			/// <summary>
			/// 3: Only messages with a verbosity level of Error and higher are output by the Debugger.
			/// </summary>
			Error = 3,

			/// <summary>
			/// 4: Only Exceptions are output by the Debugger.
			/// </summary>
			Exception = 4,

			/// <summary>
			/// 5: Console Messages. You cannot use this as as minimum level.
			/// </summary>
			Console = 5
		};
		public delegate void LogDelegate(Verbosity level, string prefix, string text);

		private static Verbosity minLevel;
		private static StreamWriter outFile = null;
		private static DateTime runningSince;
		private static PerformanceCounter cpuCounter;
		private static PerformanceCounter ramCounter;
        private static List<object[]> logHistory = new List<object[]>();
        private static bool initialflush = false;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public static event LogDelegate OnDebugOutput;
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

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

		public static void Initialize(string path, Verbosity minLevel)
		{
			if (Initialized || WFUApplication.Running)
				return;

			//initialization
			Debugger.minLevel = minLevel >= Verbosity.Console ? Verbosity.Exception : minLevel;
			runningSince = DateTime.Now;
			outFile = new System.IO.StreamWriter(path, false);

			cpuCounter = new PerformanceCounter();
			cpuCounter.CategoryName = "Processor";
			cpuCounter.CounterName = "% Processor Time";
			cpuCounter.InstanceName = "_Total";
			ramCounter = new PerformanceCounter("Memory", "Available MBytes");

			//initial logging
			I(WFUApplication.Company + " " + WFUApplication.Name + " logging session opened.");
			I("Windows version: " + Environment.OSVersion.ToString());
			I("Dot NET version: " + Environment.Version.ToString());
			I("Machine name: " + Environment.MachineName);
			I("User Name: " + Environment.UserName);
			I("Processor Count: " + Environment.ProcessorCount.ToString());
			I("Processor Usage: " + cpuCounter.NextValue() + "%");
			I("Available RAM: " + ramCounter.NextValue() + "Mb");
			TimeSpan uptime = SystemUptime;
			I(String.Format("System uptime: {0} hours, {1} minutes, {2} seconds.", uptime.Hours, uptime.Minutes, uptime.Seconds));
		}

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

		public static void Dispose()
		{
			if (!Initialized || WFUApplication.Running)
				return;

			if (OnDebugOutput != null)
				OnDebugOutput = null;

			V("Freeing Debugger resources...");
			cpuCounter.Dispose();
			ramCounter.Dispose();
			V("Freed OK.");

			I(WFUApplication.Name + " logging session closed.");
			outFile.Flush();
			outFile.Close();
			outFile.Dispose();
			outFile = null;
		}

		public static void V(string text)
		{
			Log(Verbosity.Verbose, text);
		}

		public static void I(string text)
		{
			Log(Verbosity.Information, text);
		}

		public static void W(string text)
		{
			Log(Verbosity.Warning, text);
		}

		public static void E(string text)
		{
			Log(Verbosity.Error, text);
		}

		public static void Ex(Exception e, bool printStackTrace = false)
		{
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

		public static void C(string text)
		{
			Log(Verbosity.Console, text);
		}

        public static void FlushToConsoles()
        {
            if (Flush())
                initialflush = true;
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private static bool Flush()
        {
            if (OnDebugOutput == null)
                return false;

            try
            {
                foreach (object[] arr in logHistory)
                {
                    try
                    {
                        OnDebugOutput((Verbosity)arr[0], arr[1] as string, arr[2] as string);
                    }
                    catch (ObjectDisposedException) { }
                }
                
            }
            catch (InvalidOperationException) {  }
            logHistory.Clear();
            return true;
        }

		private static void Log(Verbosity level, string text)
		{
			if (level < minLevel)
				return;

			string prefix = "[" + DateTime.Now.ToLongTimeString() + "] ";
			if (level < Verbosity.Console)
			{
				System.Diagnostics.Debugger.Log((int)level, level.ToString(), prefix + text + "\n");
				if (outFile != null)
				{
					outFile.WriteLine(prefix + text);
					outFile.Flush();
				}
			}

            logHistory.Add(new object[] { level, prefix, text });
            if (initialflush)
                Flush();
		}
	}
}

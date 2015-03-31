using System;
using System.Threading;

namespace WiFindUs
{
	public class DebuggerLogItem
	{
		private Debugger.Verbosity verbosity;
		private string threadAlias;
		private DateTime timestamp;
		private string message = "";

		public Debugger.Verbosity Verbosity
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

		public DebuggerLogItem(Debugger.Verbosity verbosity, string message)
		{
			if (verbosity != Debugger.Verbosity.Console
				&& verbosity != Debugger.Verbosity.Error
				&& verbosity != Debugger.Verbosity.Exception
				&& verbosity != Debugger.Verbosity.Information
				&& verbosity != Debugger.Verbosity.Verbose
				&& verbosity != Debugger.Verbosity.Warning)
				throw new InvalidOperationException("Cannot have a verbosity with a combination of flags!");
			if (verbosity == Debugger.Verbosity.None)
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
				Enum.GetName(typeof(Debugger.Verbosity), verbosity).Substring(0, 1),
				message);
		}
	}
}

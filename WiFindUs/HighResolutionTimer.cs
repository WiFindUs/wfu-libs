using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs
{
	public class HighResolutionTimer
	{
		private bool isPerfCounterSupported = false;
		private long frequency = 0, stopwatchStart = 0;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES 
		/////////////////////////////////////////////////////////////////////

		public long Frequency
		{
			get { return frequency; }
		}

		public long Value
		{
			get
			{
				long tickCount = 0;

				if (isPerfCounterSupported)
				{
					// Get the value here if the counter is supported.
					QueryPerformanceCounter(ref tickCount);
					return tickCount;
				}
				else
				{
					// Otherwise, use Environment.TickCount.
					return (long)Environment.TickCount;
				}
			}
		}

		public long Stopwatch
		{
			get { return Value - stopwatchStart; }
		}

		public double StopwatchInSeconds
		{
			get { return TicksToSeconds(Stopwatch); }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public HighResolutionTimer()
		{
			// Query the high-resolution timer only if it is supported.
			// A returned frequency of 1000 typically indicates that it is not
			// supported and is emulated by the OS using the same value that is
			// returned by Environment.TickCount.
			// A return value of 0 indicates that the performance counter is
			// not supported.
			int returnVal = QueryPerformanceFrequency(ref frequency);

			if (returnVal != 0 && frequency != 1000)
			{
				// The performance counter is supported.
				isPerfCounterSupported = true;
			}
			else
			{
				// The performance counter is not supported. Use
				// Environment.TickCount instead.
				frequency = 1000;
			}

			//initial stopwatch value is instantiation time
			ResetStopwatch();
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Converts a timer value to seconds, according to the system's high-res timer frequency.
		/// </summary>
		public double TicksToSeconds(long ticks)
		{
			return (double)ticks / (double)frequency;
		}

		public long ResetStopwatch()
		{
			stopwatchStart = Value;
			return stopwatchStart;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		[DllImport("Kernel32.dll")]
		private static extern int QueryPerformanceCounter(ref Int64 count);
		[DllImport("Kernel32.dll")]
		private static extern int QueryPerformanceFrequency(ref Int64 frequency);
	}

}

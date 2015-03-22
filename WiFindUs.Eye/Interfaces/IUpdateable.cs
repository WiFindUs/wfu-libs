using System;

namespace WiFindUs.Eye
{
	public interface IUpdateable
	{
		bool TimedOut { get; }
		void CheckTimeout();
		ulong UpdateAge { get; }
		ulong Updated { get; }
		ulong TimeoutLength { get; }
		event Action<IUpdateable> WhenUpdated;
		event Action<IUpdateable> TimedOutChanged;
	}
}

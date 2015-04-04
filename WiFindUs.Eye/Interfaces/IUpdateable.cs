using System;

namespace WiFindUs.Eye
{
	public interface IUpdateable
	{
		bool Active { get; }
		void CheckActive();
		ulong LastUpdated { get; }
		event Action<IUpdateable> Updated;
		event Action<IUpdateable> ActiveChanged;
	}
}

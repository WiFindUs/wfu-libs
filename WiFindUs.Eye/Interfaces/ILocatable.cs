using System;

namespace WiFindUs.Eye
{
	public interface ILocatable
	{
		ILocation Location { get; }
		event Action<ILocatable> LocationChanged;
	}
}

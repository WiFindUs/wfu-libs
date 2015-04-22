using System;

namespace WiFindUs
{
	public interface ILocatable
	{
		ILocation Location { get; }
		event Action<ILocatable> LocationChanged;
	}
}

using System;

namespace WiFindUs
{
	public class ReferenceCountedLock
	{
		public event Action<ReferenceCountedLock> OnLocked, OnUnlocked;

		private object lockObject = new object();
		private int lockCount = 0;

		public bool IsLocked
		{
			get { return lockCount > 0; }
		}

		public void Lock()
		{
			lock (lockObject)
			{
				lockCount++;
				if (lockCount == 1 && OnLocked != null)
					OnLocked(this);
			}
		}

		public void Unlock()
		{
			if (!IsLocked)
				return;

			lock (lockObject)
			{
				lockCount--;
				if (lockCount == 0 && OnUnlocked != null)
					OnUnlocked(this);
			}
		}
	}
}

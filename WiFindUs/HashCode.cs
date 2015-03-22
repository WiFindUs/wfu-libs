﻿
namespace WiFindUs
{
	public struct HashCode
	{
		private readonly int hashCode;

		public HashCode(int hashCode)
		{
			this.hashCode = hashCode;
		}

		public static HashCode Start
		{
			get { return new HashCode(17); }
		}

		public static implicit operator int(HashCode hashCode)
		{
			return hashCode.GetHashCode();
		}

		public HashCode Hash<T>(T obj)
		{
			var h = obj != null ? obj.GetHashCode() : 0;
			unchecked { h += this.hashCode * 31; }
			return new HashCode(h);
		}

		public override int GetHashCode()
		{
			return this.hashCode;
		}
	}
}

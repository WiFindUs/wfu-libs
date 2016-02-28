using System;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Simulator
{
    public interface IPacketFactory
	{
		string Packet { get; }
	}
	
	public abstract class PacketFactory<T>
		where T : class, IUpdateable, ILocatable
	{
		public readonly T Entity;

		public abstract string Packet { get; }

		public PacketFactory(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException("entity", "Entity cannot be null!");
			Entity = entity;
		}
	}

	public class NodePacketFactory : PacketFactory<Node>, IPacketFactory
	{
		public NodePacketFactory(Node node)
			: base(node) { }
		
		public override string Packet
		{
			get
			{
				string lat = Entity.Latitude.HasValue ? String.Format("|lat:{0:0.######}",Entity.Latitude) : "";
				string lng = Entity.Longitude.HasValue ? String.Format("|lng:{0:0.######}", Entity.Longitude) : "";
				string alt = Entity.Altitude.HasValue ? String.Format("|alt:{0:0.#}", Entity.Altitude) : "";
				string acc = Entity.Accuracy.HasValue ? String.Format("|acc:{0:0.#}", Entity.Accuracy) : "";
				int flags = (Entity.MeshPoint.GetValueOrDefault() ? 1 : 0)
					| (Entity.AccessPoint.GetValueOrDefault() ? 2 : 0)
					| (Entity.DHCPD.GetValueOrDefault() ? 4 : 0)
					| (Entity.GPSD.GetValueOrDefault() && !Entity.MockLocation.GetValueOrDefault() ? 8 : 0)
					| (Entity.GPSD.GetValueOrDefault() && Entity.MockLocation.GetValueOrDefault() ? 16 : 0);

				return String.Format("EYE{{NODE|{0:X}|{1}{{num:{2}|ver:{3}|mpl:0"
					+ "{4}{5}{6}{7}|flg:{8}}}}}",
					Entity.ID, //0
					DateTime.Now.ToUnixTimestamp(), //1
					Entity.Number.GetValueOrDefault(), //2
					String.Format("{0:0000}{1:00}{2:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), //3
					lat, lng, acc, alt, flags
					);
			}
		}
	}
}

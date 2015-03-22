﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using LinqConnect template.
// Code is generated on: 12/03/2015 4:53:59 PM
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------

using Devart.Data.Linq;
using Devart.Data.Linq.Mapping;

namespace WiFindUs.Eye
{

	[DatabaseAttribute(Name = "wfu_eye_db")]
	[ProviderAttribute(typeof(Devart.Data.MySql.Linq.Provider.MySqlDataProvider))]
	public partial class EyeContext : Devart.Data.Linq.DataContext
	{
		public static CompiledQueryCache compiledQueryCache = CompiledQueryCache.RegisterDataContext(typeof(EyeContext));
		private static MappingSource mappingSource = new Devart.Data.Linq.Mapping.AttributeMappingSource();

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnSubmitError(Devart.Data.Linq.SubmitErrorEventArgs args);
		partial void InsertDevice(Device instance);
		partial void UpdateDevice(Device instance);
		partial void DeleteDevice(Device instance);
		partial void InsertNode(Node instance);
		partial void UpdateNode(Node instance);
		partial void DeleteNode(Node instance);
		partial void InsertUser(User instance);
		partial void UpdateUser(User instance);
		partial void DeleteUser(User instance);
		partial void InsertWaypoint(Waypoint instance);
		partial void UpdateWaypoint(Waypoint instance);
		partial void DeleteWaypoint(Waypoint instance);
		partial void InsertDeviceHistory(DeviceHistory instance);
		partial void UpdateDeviceHistory(DeviceHistory instance);
		partial void DeleteDeviceHistory(DeviceHistory instance);

		#endregion

		public EyeContext() :
			base(@"User Id=root;Password=omgwtflol87;Host=192.168.1.1;Database=wfu_eye_db;Persist Security Info=True", mappingSource)
		{
			OnCreated();
		}

		public EyeContext(MappingSource mappingSource) :
			base(@"User Id=root;Password=omgwtflol87;Host=192.168.1.1;Database=wfu_eye_db;Persist Security Info=True", mappingSource)
		{
			OnCreated();
		}

		public EyeContext(string connection) :
			base(connection, mappingSource)
		{
			OnCreated();
		}

		public EyeContext(System.Data.IDbConnection connection) :
			base(connection, mappingSource)
		{
			OnCreated();
		}

		public EyeContext(string connection, MappingSource mappingSource) :
			base(connection, mappingSource)
		{
			OnCreated();
		}

		public EyeContext(System.Data.IDbConnection connection, MappingSource mappingSource) :
			base(connection, mappingSource)
		{
			OnCreated();
		}

		public Devart.Data.Linq.Table<Device> Devices
		{
			get
			{
				return this.GetTable<Device>();
			}
		}

		public Devart.Data.Linq.Table<Node> Nodes
		{
			get
			{
				return this.GetTable<Node>();
			}
		}

		public Devart.Data.Linq.Table<User> Users
		{
			get
			{
				return this.GetTable<User>();
			}
		}

		public Devart.Data.Linq.Table<Waypoint> Waypoints
		{
			get
			{
				return this.GetTable<Waypoint>();
			}
		}

		public Devart.Data.Linq.Table<DeviceHistory> DeviceHistories
		{
			get
			{
				return this.GetTable<DeviceHistory>();
			}
		}
	}
}

﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using LinqConnect template.
// Code is generated on: 1/12/2014 9:13:03 AM
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------

using System;
using Devart.Data.Linq;
using Devart.Data.Linq.Mapping;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

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
        partial void InsertDeviceState(DeviceState instance);
        partial void UpdateDeviceState(DeviceState instance);
        partial void DeleteDeviceState(DeviceState instance);
        partial void InsertDevice(Device instance);
        partial void UpdateDevice(Device instance);
        partial void DeleteDevice(Device instance);
        partial void InsertNodeState(NodeState instance);
        partial void UpdateNodeState(NodeState instance);
        partial void DeleteNodeState(NodeState instance);
        partial void InsertNode(Node instance);
        partial void UpdateNode(Node instance);
        partial void DeleteNode(Node instance);
        partial void InsertUser(User instance);
        partial void UpdateUser(User instance);
        partial void DeleteUser(User instance);
        partial void InsertWaypoint(Waypoint instance);
        partial void UpdateWaypoint(Waypoint instance);
        partial void DeleteWaypoint(Waypoint instance);

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

        public Devart.Data.Linq.Table<DeviceState> DeviceStates
        {
            get
            {
                return this.GetTable<DeviceState>();
            }
        }

        public Devart.Data.Linq.Table<Device> Devices
        {
            get
            {
                return this.GetTable<Device>();
            }
        }

        public Devart.Data.Linq.Table<NodeState> NodeStates
        {
            get
            {
                return this.GetTable<NodeState>();
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
    }
}

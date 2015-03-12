﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using LinqConnect template.
// Code is generated on: 12/03/2015 4:53:59 PM
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

    /// <summary>
    /// There are no comments for WiFindUs.Eye.Waypoint in the schema.
    /// </summary>
    [Table(Name = @"wfu_eye_db.Waypoints")]
    public partial class Waypoint : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(System.String.Empty);
        #pragma warning disable 0649

        private uint _ID;

        private ulong _Created;

        private string _Type;

        private int _Severity = 0;

        private int _Code = 0;

        private string _Description;

        private string _Category;

        private System.Nullable<double> _Latitude;

        private System.Nullable<double> _Longitude;

        private System.Nullable<double> _Altitude;

        private System.Nullable<uint> _NextWaypointID;

        private System.Nullable<uint> _ReportedByID;

        private bool _Archived = false;

        private System.Nullable<ulong> _ArchivedTime;
        #pragma warning restore 0649

        private EntitySet<Device> _AssignedDevices;

        private EntityRef<User> _ReportingUser;

        private EntitySet<User> _ArchivedResponders;

        private EntityRef<Waypoint> _NextWaypoint;
    
        #region Extensibility Method Definitions

        partial void OnLoaded();
        partial void OnValidate(ChangeAction action);
        partial void OnCreated();
        partial void OnIDChanging(uint value);
        partial void OnIDChanged();
        partial void OnCreatedChanging(ulong value);
        partial void OnCreatedChanged();
        partial void OnTypeChanging(string value);
        partial void OnTypeChanged();
        partial void OnSeverityChanging(int value);
        partial void OnSeverityChanged();
        partial void OnCodeChanging(int value);
        partial void OnCodeChanged();
        partial void OnDescriptionChanging(string value);
        partial void OnDescriptionChanged();
        partial void OnCategoryChanging(string value);
        partial void OnCategoryChanged();
        partial void OnLatitudeChanging(System.Nullable<double> value);
        partial void OnLatitudeChanged();
        partial void OnLongitudeChanging(System.Nullable<double> value);
        partial void OnLongitudeChanged();
        partial void OnAltitudeChanging(System.Nullable<double> value);
        partial void OnAltitudeChanged();
        partial void OnNextWaypointIDChanging(System.Nullable<uint> value);
        partial void OnNextWaypointIDChanged();
        partial void OnReportedByIDChanging(System.Nullable<uint> value);
        partial void OnReportedByIDChanged();
        partial void OnArchivedChanging(bool value);
        partial void OnArchivedChanged();
        partial void OnArchivedTimeChanging(System.Nullable<ulong> value);
        partial void OnArchivedTimeChanged();
        #endregion

        public Waypoint()
        {
            this._AssignedDevices = new EntitySet<Device>(new Action<Device>(this.attach_AssignedDevices), new Action<Device>(this.detach_AssignedDevices));
            this._ReportingUser  = default(EntityRef<User>);
            this._ArchivedResponders = new EntitySet<User>(new Action<User>(this.attach_ArchivedResponders), new Action<User>(this.detach_ArchivedResponders));
            this._NextWaypoint  = default(EntityRef<Waypoint>);
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for ID in the schema.
        /// </summary>
        [Column(Storage = "_ID", AutoSync = AutoSync.OnInsert, CanBeNull = false, DbType = "INTEGER UNSIGNED NOT NULL", IsDbGenerated = true, IsPrimaryKey = true)]
        public uint ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                if (this._ID != value)
                {
                    this.OnIDChanging(value);
                    this.SendPropertyChanging();
                    this._ID = value;
                    this.SendPropertyChanged("ID");
                    this.OnIDChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Created in the schema.
        /// </summary>
        [Column(Storage = "_Created", CanBeNull = false, DbType = "bigint UNSIGNED NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public ulong Created
        {
            get
            {
                return this._Created;
            }
            set
            {
                if (this._Created != value)
                {
                    this.OnCreatedChanging(value);
                    this.SendPropertyChanging();
                    this._Created = value;
                    this.SendPropertyChanged("Created");
                    this.OnCreatedChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Type in the schema.
        /// </summary>
        [Column(Storage = "_Type", CanBeNull = false, DbType = "varchar(32) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                if (this._Type != value)
                {
                    this.OnTypeChanging(value);
                    this.SendPropertyChanging();
                    this._Type = value;
                    this.SendPropertyChanged("Type");
                    this.OnTypeChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Severity in the schema.
        /// </summary>
        [Column(Storage = "_Severity", CanBeNull = false, DbType = "int(9) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public int Severity
        {
            get
            {
                return this._Severity;
            }
            set
            {
                if (this._Severity != value)
                {
                    this.OnSeverityChanging(value);
                    this.SendPropertyChanging();
                    this._Severity = value;
                    this.SendPropertyChanged("Severity");
                    this.OnSeverityChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Code in the schema.
        /// </summary>
        [Column(Storage = "_Code", CanBeNull = false, DbType = "int(9) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public int Code
        {
            get
            {
                return this._Code;
            }
            set
            {
                if (this._Code != value)
                {
                    this.OnCodeChanging(value);
                    this.SendPropertyChanging();
                    this._Code = value;
                    this.SendPropertyChanged("Code");
                    this.OnCodeChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Description in the schema.
        /// </summary>
        [Column(Storage = "_Description", CanBeNull = false, DbType = "text NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                if (this._Description != value)
                {
                    this.OnDescriptionChanging(value);
                    this.SendPropertyChanging();
                    this._Description = value;
                    this.SendPropertyChanged("Description");
                    this.OnDescriptionChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Category in the schema.
        /// </summary>
        [Column(Storage = "_Category", CanBeNull = false, DbType = "varchar(32) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Category
        {
            get
            {
                return this._Category;
            }
            set
            {
                if (this._Category != value)
                {
                    this.OnCategoryChanging(value);
                    this.SendPropertyChanging();
                    this._Category = value;
                    this.SendPropertyChanged("Category");
                    this.OnCategoryChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Latitude in the schema.
        /// </summary>
        [Column(Storage = "_Latitude", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> Latitude
        {
            get
            {
                return this._Latitude;
            }
            set
            {
                if (this._Latitude != value)
                {
                    this.OnLatitudeChanging(value);
                    this.SendPropertyChanging();
                    this._Latitude = value;
                    this.SendPropertyChanged("Latitude");
                    this.OnLatitudeChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Longitude in the schema.
        /// </summary>
        [Column(Storage = "_Longitude", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> Longitude
        {
            get
            {
                return this._Longitude;
            }
            set
            {
                if (this._Longitude != value)
                {
                    this.OnLongitudeChanging(value);
                    this.SendPropertyChanging();
                    this._Longitude = value;
                    this.SendPropertyChanged("Longitude");
                    this.OnLongitudeChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Altitude in the schema.
        /// </summary>
        [Column(Storage = "_Altitude", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> Altitude
        {
            get
            {
                return this._Altitude;
            }
            set
            {
                if (this._Altitude != value)
                {
                    this.OnAltitudeChanging(value);
                    this.SendPropertyChanging();
                    this._Altitude = value;
                    this.SendPropertyChanged("Altitude");
                    this.OnAltitudeChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for NextWaypointID in the schema.
        /// </summary>
        [Column(Storage = "_NextWaypointID", DbType = "INTEGER UNSIGNED NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<uint> NextWaypointID
        {
            get
            {
                return this._NextWaypointID;
            }
            set
            {
                if (this._NextWaypointID != value)
                {
                    if (this._NextWaypoint.HasLoadedOrAssignedValue)
                    {
                        throw new ForeignKeyReferenceAlreadyHasValueException();
                    }

                    this.OnNextWaypointIDChanging(value);
                    this.SendPropertyChanging();
                    this._NextWaypointID = value;
                    this.SendPropertyChanged("NextWaypointID");
                    this.OnNextWaypointIDChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for ReportedByID in the schema.
        /// </summary>
        [Column(Storage = "_ReportedByID", DbType = "INTEGER UNSIGNED NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<uint> ReportedByID
        {
            get
            {
                return this._ReportedByID;
            }
            set
            {
                if (this._ReportedByID != value)
                {
                    if (this._ReportingUser.HasLoadedOrAssignedValue)
                    {
                        throw new ForeignKeyReferenceAlreadyHasValueException();
                    }

                    this.OnReportedByIDChanging(value);
                    this.SendPropertyChanging();
                    this._ReportedByID = value;
                    this.SendPropertyChanged("ReportedByID");
                    this.OnReportedByIDChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Archived in the schema.
        /// </summary>
        [Column(Storage = "_Archived", CanBeNull = false, DbType = "tinyint(1) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public bool Archived
        {
            get
            {
                return this._Archived;
            }
            set
            {
                if (this._Archived != value)
                {
                    this.OnArchivedChanging(value);
                    this.SendPropertyChanging();
                    this._Archived = value;
                    this.SendPropertyChanged("Archived");
                    this.OnArchivedChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for ArchivedTime in the schema.
        /// </summary>
        [Column(Storage = "_ArchivedTime", DbType = "bigint UNSIGNED NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<ulong> ArchivedTime
        {
            get
            {
                return this._ArchivedTime;
            }
            set
            {
                if (this._ArchivedTime != value)
                {
                    this.OnArchivedTimeChanging(value);
                    this.SendPropertyChanging();
                    this._ArchivedTime = value;
                    this.SendPropertyChanged("ArchivedTime");
                    this.OnArchivedTimeChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for AssignedDevices in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="Waypoint_Device", Storage="_AssignedDevices", ThisKey="ID", OtherKey="WaypointID", DeleteRule="SET NULL")]
        public EntitySet<Device> AssignedDevices
        {
            get
            {
                return this._AssignedDevices;
            }
            set
            {
                this._AssignedDevices.Assign(value);
            }
        }

    
        /// <summary>
        /// There are no comments for ReportingUser in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="User_Waypoint", Storage="_ReportingUser", ThisKey="ReportedByID", OtherKey="ID", IsForeignKey=true)]
        public User ReportingUser
        {
            get
            {
                return this._ReportingUser.Entity;
            }
            set
            {
                User previousValue = this._ReportingUser.Entity;
                if ((previousValue != value) || (this._ReportingUser.HasLoadedOrAssignedValue == false))
                {
                    this.SendPropertyChanging();
                    if (previousValue != null)
                    {
                        this._ReportingUser.Entity = null;
                    }
                    this._ReportingUser.Entity = value;
                    if (value != null)
                    {
                        this._ReportedByID = value.ID;
                    }
                    else
                    {
                        this._ReportedByID = default(System.Nullable<uint>);
                    }
                    this.SendPropertyChanged("ReportingUser");
                }
            }
        }

    
        /// <summary>
        /// There are no comments for ArchivedResponders in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="Waypoint_User", Storage="_ArchivedResponders", ThisKey="ID", OtherKey="ID", LinkTableName=@"wfu_eye_db.ArchivedWaypointResponders", LinkThisKey=@"WaypointID", LinkOtherKey=@"UserID")]
        public EntitySet<User> ArchivedResponders
        {
            get
            {
                return this._ArchivedResponders;
            }
            set
            {
                this._ArchivedResponders.Assign(value);
            }
        }

    
        /// <summary>
        /// There are no comments for NextWaypoint in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="Waypoint_Waypoint", Storage="_NextWaypoint", ThisKey="NextWaypointID", OtherKey="ID", IsForeignKey=true)]
        public Waypoint NextWaypoint
        {
            get
            {
                return this._NextWaypoint.Entity;
            }
            set
            {
                Waypoint previousValue = this._NextWaypoint.Entity;
                if ((previousValue != value) || (this._NextWaypoint.HasLoadedOrAssignedValue == false))
                {
                    this.SendPropertyChanging();
                    if (previousValue != null)
                    {
                        this._NextWaypoint.Entity = null;
                    }
                    this._NextWaypoint.Entity = value;
                    if (value != null)
                    {
                        this._NextWaypointID = value.ID;
                    }
                    else
                    {
                        this._NextWaypointID = default(System.Nullable<uint>);
                    }
                    this.SendPropertyChanged("NextWaypoint");
                }
            }
        }
   
        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
		        var handler = this.PropertyChanging;
            if (handler != null)
                handler(this, emptyChangingEventArgs);
        }

        protected virtual void SendPropertyChanging(System.String propertyName) 
        {    
		        var handler = this.PropertyChanging;
            if (handler != null)
                handler(this, new PropertyChangingEventArgs(propertyName));
        }

        protected virtual void SendPropertyChanged(System.String propertyName)
        {    
		        var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void attach_AssignedDevices(Device entity)
        {
            this.SendPropertyChanging("AssignedDevices");
            entity.AssignedWaypoint = this;
        }
    
        private void detach_AssignedDevices(Device entity)
        {
            this.SendPropertyChanging("AssignedDevices");
            entity.AssignedWaypoint = null;
        }

        private void attach_ArchivedResponders(User entity)
        {
            this.SendPropertyChanging("ArchivedResponders");
        }
    
        private void detach_ArchivedResponders(User entity)
        {
            this.SendPropertyChanging("ArchivedResponders");
        }
    }

}

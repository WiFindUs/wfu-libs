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

    /// <summary>
    /// There are no comments for WiFindUs.Eye.Waypoint in the schema.
    /// </summary>
    [Table(Name = @"wfu_eye_db.Waypoints")]
    public partial class Waypoint : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(System.String.Empty);
        #pragma warning disable 0649

        private long _ID;

        private System.DateTime _Created = DateTime.Now;

        private double _Latitude;

        private double _Longitude;

        private System.Nullable<double> _Altitude;

        private string _Type;

        private string _Category;

        private string _Description;

        private int _Severity = 0;

        private int _Code = 0;

        private System.Nullable<long> _NextWaypointID;

        private System.Nullable<long> _ReportedByID;

        private bool _ArchivedInternal = false;

        private System.Nullable<System.DateTime> _ArchivedTime;
        #pragma warning restore 0649

        private EntitySet<Device> _AssignedDevices;

        private EntityRef<User> _ReportingUser;

        private EntitySet<User> _ArchivedRespondersInternal;

        private EntityRef<Waypoint> _NextWaypoint;
    
        #region Extensibility Method Definitions

        partial void OnLoaded();
        partial void OnValidate(ChangeAction action);
        partial void OnCreated();
        partial void OnIDChanging(long value);
        partial void OnIDChanged();
        partial void OnCreatedChanging(System.DateTime value);
        partial void OnCreatedChanged();
        partial void OnLatitudeChanging(double value);
        partial void OnLatitudeChanged();
        partial void OnLongitudeChanging(double value);
        partial void OnLongitudeChanged();
        partial void OnAltitudeChanging(System.Nullable<double> value);
        partial void OnAltitudeChanged();
        partial void OnTypeChanging(string value);
        partial void OnTypeChanged();
        partial void OnCategoryChanging(string value);
        partial void OnCategoryChanged();
        partial void OnDescriptionChanging(string value);
        partial void OnDescriptionChanged();
        partial void OnSeverityChanging(int value);
        partial void OnSeverityChanged();
        partial void OnCodeChanging(int value);
        partial void OnCodeChanged();
        partial void OnNextWaypointIDChanging(System.Nullable<long> value);
        partial void OnNextWaypointIDChanged();
        partial void OnReportedByIDChanging(System.Nullable<long> value);
        partial void OnReportedByIDChanged();
        partial void OnArchivedInternalChanging(bool value);
        partial void OnArchivedInternalChanged();
        partial void OnArchivedTimeChanging(System.Nullable<System.DateTime> value);
        partial void OnArchivedTimeChanged();
        #endregion

        public Waypoint()
        {
            this._AssignedDevices = new EntitySet<Device>(new Action<Device>(this.attach_AssignedDevices), new Action<Device>(this.detach_AssignedDevices));
            this._ReportingUser  = default(EntityRef<User>);
            this._ArchivedRespondersInternal = new EntitySet<User>(new Action<User>(this.attach_ArchivedRespondersInternal), new Action<User>(this.detach_ArchivedRespondersInternal));
            this._NextWaypoint  = default(EntityRef<Waypoint>);
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for ID in the schema.
        /// </summary>
        [Column(Storage = "_ID", AutoSync = AutoSync.OnInsert, CanBeNull = false, DbType = "bigint NOT NULL", IsDbGenerated = true, IsPrimaryKey = true)]
        public long ID
        {
            get
            {
                return this._ID;
            }
        }

    
        /// <summary>
        /// There are no comments for Created in the schema.
        /// </summary>
        [Column(Storage = "_Created", CanBeNull = false, DbType = "datetime NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public System.DateTime Created
        {
            get
            {
                return this._Created;
            }
        }

    
        /// <summary>
        /// There are no comments for Latitude in the schema.
        /// </summary>
        [Column(Storage = "_Latitude", CanBeNull = false, DbType = "double NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public double Latitude
        {
            get
            {
                return this._Latitude;
            }
        }

    
        /// <summary>
        /// There are no comments for Longitude in the schema.
        /// </summary>
        [Column(Storage = "_Longitude", CanBeNull = false, DbType = "double NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public double Longitude
        {
            get
            {
                return this._Longitude;
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
        /// There are no comments for NextWaypointID in the schema.
        /// </summary>
        [Column(Storage = "_NextWaypointID", DbType = "bigint NULL", UpdateCheck = UpdateCheck.Never)]
        protected System.Nullable<long> NextWaypointID
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
        [Column(Storage = "_ReportedByID", DbType = "bigint NULL", UpdateCheck = UpdateCheck.Never)]
        protected System.Nullable<long> ReportedByID
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
        /// There are no comments for ArchivedInternal in the schema.
        /// </summary>
        [Column(Name = @"Archived", Storage = "_ArchivedInternal", CanBeNull = false, DbType = "tinyint(1) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        protected bool ArchivedInternal
        {
            get
            {
                return this._ArchivedInternal;
            }
            set
            {
                if (this._ArchivedInternal != value)
                {
                    this.OnArchivedInternalChanging(value);
                    this.SendPropertyChanging();
                    this._ArchivedInternal = value;
                    this.SendPropertyChanged("ArchivedInternal");
                    this.OnArchivedInternalChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for ArchivedTime in the schema.
        /// </summary>
        [Column(Storage = "_ArchivedTime", DbType = "datetime NULL", UpdateCheck = UpdateCheck.Never)]
        protected System.Nullable<System.DateTime> ArchivedTime
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
                        this._ReportedByID = default(System.Nullable<long>);
                    }
                    this.SendPropertyChanged("ReportingUser");
                }
            }
        }

    
        /// <summary>
        /// There are no comments for ArchivedRespondersInternal in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="Waypoint_User", Storage="_ArchivedRespondersInternal", ThisKey="ID", OtherKey="ID", LinkTableName=@"wfu_eye_db.ArchivedWaypointResponders", LinkThisKey=@"WaypointID", LinkOtherKey=@"UserID")]
        internal EntitySet<User> ArchivedRespondersInternal
        {
            get
            {
                return this._ArchivedRespondersInternal;
            }
            set
            {
                this._ArchivedRespondersInternal.Assign(value);
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
                        this._NextWaypointID = default(System.Nullable<long>);
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

        private void attach_ArchivedRespondersInternal(User entity)
        {
            this.SendPropertyChanging("ArchivedRespondersInternal");
        }
    
        private void detach_ArchivedRespondersInternal(User entity)
        {
            this.SendPropertyChanging("ArchivedRespondersInternal");
        }
    }

}

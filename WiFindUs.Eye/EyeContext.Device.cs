﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using LinqConnect template.
// Code is generated on: 5/12/2014 5:20:37 AM
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
    /// There are no comments for WiFindUs.Eye.Device in the schema.
    /// </summary>
    [Table(Name = @"wfu_eye_db.Devices")]
    public partial class Device : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(System.String.Empty);
        #pragma warning disable 0649

        private long _ID;

        private long _Created;

        private long _Updated;

        private string _Type = @"PHO";

        private System.Nullable<double> _Latitude;

        private System.Nullable<double> _Longitude;

        private System.Nullable<double> _Altitude;

        private System.Nullable<double> _Accuracy;

        private System.Nullable<double> _Humidity;

        private System.Nullable<double> _AirPressure;

        private System.Nullable<double> _Temperature;

        private System.Nullable<double> _LightLevel;

        private System.Nullable<bool> _Charging = false;

        private System.Nullable<double> _BatteryLevel;

        private System.Nullable<long> _IPAddressRaw = 0;

        private System.Nullable<long> _WaypointID;

        private System.Nullable<long> _UserID;
        #pragma warning restore 0649

        private EntityRef<Waypoint> _AssignedWaypoint;

        private EntitySet<DeviceHistory> _History;

        private EntityRef<User> _User;
    
        #region Extensibility Method Definitions

        partial void OnLoaded();
        partial void OnValidate(ChangeAction action);
        partial void OnCreated();
        partial void OnIDChanging(long value);
        partial void OnIDChanged();
        partial void OnCreatedChanging(long value);
        partial void OnCreatedChanged();
        partial void OnUpdatedChanging(long value);
        partial void OnUpdatedChanged();
        partial void OnTypeChanging(string value);
        partial void OnTypeChanged();
        partial void OnLatitudeChanging(System.Nullable<double> value);
        partial void OnLatitudeChanged();
        partial void OnLongitudeChanging(System.Nullable<double> value);
        partial void OnLongitudeChanged();
        partial void OnAltitudeChanging(System.Nullable<double> value);
        partial void OnAltitudeChanged();
        partial void OnAccuracyChanging(System.Nullable<double> value);
        partial void OnAccuracyChanged();
        partial void OnHumidityChanging(System.Nullable<double> value);
        partial void OnHumidityChanged();
        partial void OnAirPressureChanging(System.Nullable<double> value);
        partial void OnAirPressureChanged();
        partial void OnTemperatureChanging(System.Nullable<double> value);
        partial void OnTemperatureChanged();
        partial void OnLightLevelChanging(System.Nullable<double> value);
        partial void OnLightLevelChanged();
        partial void OnChargingChanging(System.Nullable<bool> value);
        partial void OnChargingChanged();
        partial void OnBatteryLevelChanging(System.Nullable<double> value);
        partial void OnBatteryLevelChanged();
        partial void OnIPAddressRawChanging(System.Nullable<long> value);
        partial void OnIPAddressRawChanged();
        partial void OnWaypointIDChanging(System.Nullable<long> value);
        partial void OnWaypointIDChanged();
        partial void OnUserIDChanging(System.Nullable<long> value);
        partial void OnUserIDChanged();
        #endregion

        public Device()
        {
            this._AssignedWaypoint  = default(EntityRef<Waypoint>);
            this._History = new EntitySet<DeviceHistory>(new Action<DeviceHistory>(this.attach_History), new Action<DeviceHistory>(this.detach_History));
            this._User  = default(EntityRef<User>);
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for ID in the schema.
        /// </summary>
        [Column(Storage = "_ID", CanBeNull = false, DbType = "bigint NOT NULL", IsPrimaryKey = true)]
        public long ID
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
        [Column(Storage = "_Created", CanBeNull = false, DbType = "bigint NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public long Created
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
        /// There are no comments for Updated in the schema.
        /// </summary>
        [Column(Storage = "_Updated", CanBeNull = false, DbType = "bigint NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public long Updated
        {
            get
            {
                return this._Updated;
            }
            set
            {
                if (this._Updated != value)
                {
                    this.OnUpdatedChanging(value);
                    this.SendPropertyChanging();
                    this._Updated = value;
                    this.SendPropertyChanged("Updated");
                    this.OnUpdatedChanged();
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
        /// There are no comments for Accuracy in the schema.
        /// </summary>
        [Column(Storage = "_Accuracy", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> Accuracy
        {
            get
            {
                return this._Accuracy;
            }
            set
            {
                if (this._Accuracy != value)
                {
                    this.OnAccuracyChanging(value);
                    this.SendPropertyChanging();
                    this._Accuracy = value;
                    this.SendPropertyChanged("Accuracy");
                    this.OnAccuracyChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Humidity in the schema.
        /// </summary>
        [Column(Storage = "_Humidity", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> Humidity
        {
            get
            {
                return this._Humidity;
            }
            set
            {
                if (this._Humidity != value)
                {
                    this.OnHumidityChanging(value);
                    this.SendPropertyChanging();
                    this._Humidity = value;
                    this.SendPropertyChanged("Humidity");
                    this.OnHumidityChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for AirPressure in the schema.
        /// </summary>
        [Column(Storage = "_AirPressure", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> AirPressure
        {
            get
            {
                return this._AirPressure;
            }
            set
            {
                if (this._AirPressure != value)
                {
                    this.OnAirPressureChanging(value);
                    this.SendPropertyChanging();
                    this._AirPressure = value;
                    this.SendPropertyChanged("AirPressure");
                    this.OnAirPressureChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Temperature in the schema.
        /// </summary>
        [Column(Storage = "_Temperature", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> Temperature
        {
            get
            {
                return this._Temperature;
            }
            set
            {
                if (this._Temperature != value)
                {
                    this.OnTemperatureChanging(value);
                    this.SendPropertyChanging();
                    this._Temperature = value;
                    this.SendPropertyChanged("Temperature");
                    this.OnTemperatureChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for LightLevel in the schema.
        /// </summary>
        [Column(Storage = "_LightLevel", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> LightLevel
        {
            get
            {
                return this._LightLevel;
            }
            set
            {
                if (this._LightLevel != value)
                {
                    this.OnLightLevelChanging(value);
                    this.SendPropertyChanging();
                    this._LightLevel = value;
                    this.SendPropertyChanged("LightLevel");
                    this.OnLightLevelChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Charging in the schema.
        /// </summary>
        [Column(Storage = "_Charging", DbType = "tinyint(1) NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<bool> Charging
        {
            get
            {
                return this._Charging;
            }
            set
            {
                if (this._Charging != value)
                {
                    this.OnChargingChanging(value);
                    this.SendPropertyChanging();
                    this._Charging = value;
                    this.SendPropertyChanged("Charging");
                    this.OnChargingChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for BatteryLevel in the schema.
        /// </summary>
        [Column(Storage = "_BatteryLevel", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> BatteryLevel
        {
            get
            {
                return this._BatteryLevel;
            }
            set
            {
                if (this._BatteryLevel != value)
                {
                    this.OnBatteryLevelChanging(value);
                    this.SendPropertyChanging();
                    this._BatteryLevel = value;
                    this.SendPropertyChanged("BatteryLevel");
                    this.OnBatteryLevelChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for IPAddressRaw in the schema.
        /// </summary>
        [Column(Name = @"IPAddress", Storage = "_IPAddressRaw", DbType = "bigint NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<long> IPAddressRaw
        {
            get
            {
                return this._IPAddressRaw;
            }
            set
            {
                if (this._IPAddressRaw != value)
                {
                    this.OnIPAddressRawChanging(value);
                    this.SendPropertyChanging();
                    this._IPAddressRaw = value;
                    this.SendPropertyChanged("IPAddressRaw");
                    this.OnIPAddressRawChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for WaypointID in the schema.
        /// </summary>
        [Column(Storage = "_WaypointID", DbType = "bigint NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<long> WaypointID
        {
            get
            {
                return this._WaypointID;
            }
            set
            {
                if (this._WaypointID != value)
                {
                    if (this._AssignedWaypoint.HasLoadedOrAssignedValue)
                    {
                        throw new ForeignKeyReferenceAlreadyHasValueException();
                    }

                    this.OnWaypointIDChanging(value);
                    this.SendPropertyChanging();
                    this._WaypointID = value;
                    this.SendPropertyChanged("WaypointID");
                    this.OnWaypointIDChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for UserID in the schema.
        /// </summary>
        [Column(Storage = "_UserID", DbType = "bigint NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<long> UserID
        {
            get
            {
                return this._UserID;
            }
            set
            {
                if (this._UserID != value)
                {
                    if (this._User.HasLoadedOrAssignedValue)
                    {
                        throw new ForeignKeyReferenceAlreadyHasValueException();
                    }

                    this.OnUserIDChanging(value);
                    this.SendPropertyChanging();
                    this._UserID = value;
                    this.SendPropertyChanged("UserID");
                    this.OnUserIDChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for AssignedWaypoint in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="Waypoint_Device", Storage="_AssignedWaypoint", ThisKey="WaypointID", OtherKey="ID", IsForeignKey=true)]
        public Waypoint AssignedWaypoint
        {
            get
            {
                return this._AssignedWaypoint.Entity;
            }
            set
            {
                Waypoint previousValue = this._AssignedWaypoint.Entity;
                if ((previousValue != value) || (this._AssignedWaypoint.HasLoadedOrAssignedValue == false))
                {
                    this.SendPropertyChanging();
                    if (previousValue != null)
                    {
                        this._AssignedWaypoint.Entity = null;
                        previousValue.AssignedDevices.Remove(this);
                    }
                    this._AssignedWaypoint.Entity = value;
                    if (value != null)
                    {
                        this._WaypointID = value.ID;
                        value.AssignedDevices.Add(this);
                    }
                    else
                    {
                        this._WaypointID = default(System.Nullable<long>);
                    }
                    this.SendPropertyChanged("AssignedWaypoint");
                }
            }
        }

    
        /// <summary>
        /// There are no comments for History in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="Device_DeviceHistory", Storage="_History", ThisKey="ID", OtherKey="DeviceID", DeleteRule="CASCADE")]
        public EntitySet<DeviceHistory> History
        {
            get
            {
                return this._History;
            }
            set
            {
                this._History.Assign(value);
            }
        }

    
        /// <summary>
        /// There are no comments for User in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="User_Device", Storage="_User", ThisKey="UserID", OtherKey="ID", IsForeignKey=true)]
        public User User
        {
            get
            {
                return this._User.Entity;
            }
            set
            {
                User previousValue = this._User.Entity;
                if ((previousValue != value) || (this._User.HasLoadedOrAssignedValue == false))
                {
                    this.SendPropertyChanging();
                    if (previousValue != null)
                    {
                        this._User.Entity = null;
                        previousValue.Device = null;
                    }
                    this._User.Entity = value;
                    if (value != null)
                    {
                        this._UserID = value.ID;
                        value.Device = this;
                    }
                    else
                    {
                        this._UserID = default(System.Nullable<long>);
                    }
                    this.SendPropertyChanged("User");
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

        private void attach_History(DeviceHistory entity)
        {
            this.SendPropertyChanging("History");
            entity.Device = this;
        }
    
        private void detach_History(DeviceHistory entity)
        {
            this.SendPropertyChanging("History");
            entity.Device = null;
        }
    }

}

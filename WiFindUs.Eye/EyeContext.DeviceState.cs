﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using LinqConnect template.
// Code is generated on: 25/11/2014 2:25:54 AM
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
    /// There are no comments for WiFindUs.Eye.DeviceState in the schema.
    /// </summary>
    [Table(Name = @"wfu_eye_db.DeviceStates")]
    public partial class DeviceState : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(System.String.Empty);
        #pragma warning disable 0649

        private long _DeviceID;

        private System.Nullable<long> _UserID;

        private System.DateTime _Created = DateTime.Now;

        private double _Latitude;

        private double _Longitude;

        private System.Nullable<double> _Altitude;

        private System.Nullable<double> _Accuracy;

        private System.Nullable<double> _Humidity;

        private System.Nullable<double> _AirPressure;

        private System.Nullable<double> _Temperature;

        private System.Nullable<double> _LightLevel;

        private System.Nullable<bool> _Charging = false;

        private System.Nullable<double> _BatteryLevel;

        private long _IPAddressRaw = 0;
        #pragma warning restore 0649

        private EntityRef<Device> _Device;

        private EntityRef<User> _User;
    
        #region Extensibility Method Definitions

        partial void OnLoaded();
        partial void OnValidate(ChangeAction action);
        partial void OnCreated();
        partial void OnDeviceIDChanging(long value);
        partial void OnDeviceIDChanged();
        partial void OnUserIDChanging(System.Nullable<long> value);
        partial void OnUserIDChanged();
        partial void OnCreatedChanging(System.DateTime value);
        partial void OnCreatedChanged();
        partial void OnLatitudeChanging(double value);
        partial void OnLatitudeChanged();
        partial void OnLongitudeChanging(double value);
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
        partial void OnIPAddressRawChanging(long value);
        partial void OnIPAddressRawChanged();
        #endregion

        public DeviceState()
        {
            this._Device  = default(EntityRef<Device>);
            this._User  = default(EntityRef<User>);
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for DeviceID in the schema.
        /// </summary>
        [Column(Storage = "_DeviceID", CanBeNull = false, DbType = "bigint NOT NULL", IsPrimaryKey = true)]
        protected long DeviceID
        {
            get
            {
                return this._DeviceID;
            }
        }

    
        /// <summary>
        /// There are no comments for UserID in the schema.
        /// </summary>
        [Column(Storage = "_UserID", DbType = "bigint NULL", UpdateCheck = UpdateCheck.Never)]
        protected System.Nullable<long> UserID
        {
            get
            {
                return this._UserID;
            }
        }

    
        /// <summary>
        /// There are no comments for Created in the schema.
        /// </summary>
        [Column(Storage = "_Created", CanBeNull = false, DbType = "datetime NOT NULL", IsPrimaryKey = true)]
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
        /// There are no comments for Accuracy in the schema.
        /// </summary>
        [Column(Storage = "_Accuracy", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> Accuracy
        {
            get
            {
                return this._Accuracy;
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
        }

    
        /// <summary>
        /// There are no comments for IPAddressRaw in the schema.
        /// </summary>
        [Column(Name = @"IPAddress", Storage = "_IPAddressRaw", CanBeNull = false, DbType = "bigint NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public long IPAddressRaw
        {
            get
            {
                return this._IPAddressRaw;
            }
        }

    
        /// <summary>
        /// There are no comments for Device in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="Device_DeviceState", Storage="_Device", ThisKey="DeviceID", OtherKey="ID", IsForeignKey=true, DeleteOnNull=true)]
        public Device Device
        {
            get
            {
                return this._Device.Entity;
            }
            set
            {
                Device previousValue = this._Device.Entity;
                if ((previousValue != value) || (this._Device.HasLoadedOrAssignedValue == false))
                {
                    this.SendPropertyChanging();
                    if (previousValue != null)
                    {
                        this._Device.Entity = null;
                        previousValue.DeviceStates.Remove(this);
                    }
                    this._Device.Entity = value;
                    if (value != null)
                    {
                        this._DeviceID = value.ID;
                        value.DeviceStates.Add(this);
                    }
                    else
                    {
                        this._DeviceID = default(long);
                    }
                    this.SendPropertyChanged("Device");
                }
            }
        }

    
        /// <summary>
        /// There are no comments for User in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="User_DeviceState", Storage="_User", ThisKey="UserID", OtherKey="ID", IsForeignKey=true)]
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
                    }
                    this._User.Entity = value;
                    if (value != null)
                    {
                        this._UserID = value.ID;
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
    }

}
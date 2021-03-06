﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using LinqConnect template.
// Code is generated on: 5/04/2015 12:40:05 AM
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------

using System.ComponentModel;

namespace WiFindUs.Eye
{

    /// <summary>
    /// There are no comments for WiFindUs.Eye.DeviceHistory in the schema.
    /// </summary>
    [Table(Name = @"wfu_eye_db.DeviceHistories")]
    public partial class DeviceHistory : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(System.String.Empty);
        #pragma warning disable 0649

        private uint _DeviceID;

        private ulong _Created;

        private System.Nullable<double> _Latitude;

        private System.Nullable<double> _Longitude;

        private System.Nullable<double> _Altitude;

        private System.Nullable<double> _Accuracy;

        private System.Nullable<double> _Humidity;

        private System.Nullable<double> _AirPressure;

        private System.Nullable<double> _Temperature;

        private System.Nullable<double> _LightLevel;

        private System.Nullable<uint> _UserID;
        #pragma warning restore 0649

        private EntityRef<Device> _Device;

        private EntityRef<User> _User;
    
        #region Extensibility Method Definitions

        partial void OnLoaded();
        partial void OnValidate(ChangeAction action);
        partial void OnCreated();
        partial void OnDeviceIDChanging(uint value);
        partial void OnDeviceIDChanged();
        partial void OnCreatedChanging(ulong value);
        partial void OnCreatedChanged();
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
        partial void OnUserIDChanging(System.Nullable<uint> value);
        partial void OnUserIDChanged();
        #endregion

        public DeviceHistory()
        {
            this._Device  = default(EntityRef<Device>);
            this._User  = default(EntityRef<User>);
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for DeviceID in the schema.
        /// </summary>
        [Column(Storage = "_DeviceID", CanBeNull = false, DbType = "INTEGER UNSIGNED NOT NULL", IsPrimaryKey = true)]
        public uint DeviceID
        {
            get
            {
                return this._DeviceID;
            }
            set
            {
                if (this._DeviceID != value)
                {
                    if (this._Device.HasLoadedOrAssignedValue)
                    {
                        throw new ForeignKeyReferenceAlreadyHasValueException();
                    }

                    this.OnDeviceIDChanging(value);
                    this.SendPropertyChanging();
                    this._DeviceID = value;
                    this.SendPropertyChanged("DeviceID");
                    this.OnDeviceIDChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for Created in the schema.
        /// </summary>
        [Column(Storage = "_Created", CanBeNull = false, DbType = "bigint UNSIGNED NOT NULL", IsPrimaryKey = true)]
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
        /// There are no comments for UserID in the schema.
        /// </summary>
        [Column(Storage = "_UserID", DbType = "INTEGER UNSIGNED NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<uint> UserID
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
        /// There are no comments for Device in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="Device_DeviceHistory", Storage="_Device", ThisKey="DeviceID", OtherKey="ID", IsForeignKey=true, DeleteOnNull=true)]
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
                        previousValue.History.Remove(this);
                    }
                    this._Device.Entity = value;
                    if (value != null)
                    {
                        this._DeviceID = value.ID;
                        value.History.Add(this);
                    }
                    else
                    {
                        this._DeviceID = default(uint);
                    }
                    this.SendPropertyChanged("Device");
                }
            }
        }

    
        /// <summary>
        /// There are no comments for User in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="User_DeviceHistory", Storage="_User", ThisKey="UserID", OtherKey="ID", IsForeignKey=true)]
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
                        this._UserID = default(System.Nullable<uint>);
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

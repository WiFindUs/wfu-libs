﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using LinqConnect template.
// Code is generated on: 30/03/2015 8:16:28 PM
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
    /// There are no comments for WiFindUs.Eye.Node in the schema.
    /// </summary>
    [Table(Name = @"wfu_eye_db.Nodes")]
    public partial class Node : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(System.String.Empty);
        #pragma warning disable 0649

        private uint _ID;

        private ulong _Created;

        private ulong _Updated;

        private System.Nullable<uint> _Number = 0;

        private System.Nullable<double> _Latitude;

        private System.Nullable<double> _Longitude;

        private System.Nullable<double> _Altitude;

        private System.Nullable<double> _Accuracy;

        private System.Nullable<double> _Voltage;

        private System.Nullable<long> _IPAddressRaw = 0;
        #pragma warning restore 0649

        private EntitySet<Device> _Devices;
    
        #region Extensibility Method Definitions

        partial void OnLoaded();
        partial void OnValidate(ChangeAction action);
        partial void OnCreated();
        partial void OnIDChanging(uint value);
        partial void OnIDChanged();
        partial void OnCreatedChanging(ulong value);
        partial void OnCreatedChanged();
        partial void OnUpdatedChanging(ulong value);
        partial void OnUpdatedChanged();
        partial void OnNumberChanging(System.Nullable<uint> value);
        partial void OnNumberChanged();
        partial void OnLatitudeChanging(System.Nullable<double> value);
        partial void OnLatitudeChanged();
        partial void OnLongitudeChanging(System.Nullable<double> value);
        partial void OnLongitudeChanged();
        partial void OnAltitudeChanging(System.Nullable<double> value);
        partial void OnAltitudeChanged();
        partial void OnAccuracyChanging(System.Nullable<double> value);
        partial void OnAccuracyChanged();
        partial void OnVoltageChanging(System.Nullable<double> value);
        partial void OnVoltageChanged();
        partial void OnIPAddressRawChanging(System.Nullable<long> value);
        partial void OnIPAddressRawChanged();
        #endregion

        public Node()
        {
            this._Devices = new EntitySet<Device>(new Action<Device>(this.attach_Devices), new Action<Device>(this.detach_Devices));
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for ID in the schema.
        /// </summary>
        [Column(Storage = "_ID", CanBeNull = false, DbType = "INTEGER UNSIGNED NOT NULL", IsPrimaryKey = true)]
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
        /// There are no comments for Updated in the schema.
        /// </summary>
        [Column(Storage = "_Updated", CanBeNull = false, DbType = "bigint UNSIGNED NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public ulong Updated
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
        /// There are no comments for Number in the schema.
        /// </summary>
        [Column(Storage = "_Number", DbType = "INTEGER UNSIGNED NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<uint> Number
        {
            get
            {
                return this._Number;
            }
            set
            {
                if (this._Number != value)
                {
                    this.OnNumberChanging(value);
                    this.SendPropertyChanging();
                    this._Number = value;
                    this.SendPropertyChanged("Number");
                    this.OnNumberChanged();
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
        /// There are no comments for Voltage in the schema.
        /// </summary>
        [Column(Storage = "_Voltage", DbType = "double NULL", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<double> Voltage
        {
            get
            {
                return this._Voltage;
            }
            set
            {
                if (this._Voltage != value)
                {
                    this.OnVoltageChanging(value);
                    this.SendPropertyChanging();
                    this._Voltage = value;
                    this.SendPropertyChanged("Voltage");
                    this.OnVoltageChanged();
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
        /// There are no comments for Devices in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="Node_Device", Storage="_Devices", ThisKey="ID", OtherKey="NodeID", DeleteRule="SET NULL")]
        public EntitySet<Device> Devices
        {
            get
            {
                return this._Devices;
            }
            set
            {
                this._Devices.Assign(value);
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

        private void attach_Devices(Device entity)
        {
            this.SendPropertyChanging("Devices");
            entity.Node = this;
        }
    
        private void detach_Devices(Device entity)
        {
            this.SendPropertyChanging("Devices");
            entity.Node = null;
        }
    }

}

﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using LinqConnect template.
// Code is generated on: 10/03/2015 3:08:51 AM
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
    /// There are no comments for WiFindUs.Eye.User in the schema.
    /// </summary>
    [Table(Name = @"wfu_eye_db.Users")]
    public partial class User : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(System.String.Empty);
        #pragma warning disable 0649

        private ulong _ID;

        private string _NameFirst;

        private string _NameMiddle;

        private string _NameLast;

        private string _Type;
        #pragma warning restore 0649

        private EntityRef<Device> _Device;
    
        #region Extensibility Method Definitions

        partial void OnLoaded();
        partial void OnValidate(ChangeAction action);
        partial void OnCreated();
        partial void OnIDChanging(ulong value);
        partial void OnIDChanged();
        partial void OnNameFirstChanging(string value);
        partial void OnNameFirstChanged();
        partial void OnNameMiddleChanging(string value);
        partial void OnNameMiddleChanged();
        partial void OnNameLastChanging(string value);
        partial void OnNameLastChanged();
        partial void OnTypeChanging(string value);
        partial void OnTypeChanged();
        #endregion

        public User()
        {
            this._Device  = default(EntityRef<Device>);
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for ID in the schema.
        /// </summary>
        [Column(Storage = "_ID", AutoSync = AutoSync.OnInsert, CanBeNull = false, DbType = "bigint UNSIGNED NOT NULL", IsDbGenerated = true, IsPrimaryKey = true)]
        public ulong ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                if (this._ID != value)
                {
                    if (this._Device.HasLoadedOrAssignedValue)
                    {
                        throw new ForeignKeyReferenceAlreadyHasValueException();
                    }

                    this.OnIDChanging(value);
                    this.SendPropertyChanging();
                    this._ID = value;
                    this.SendPropertyChanged("ID");
                    this.OnIDChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for NameFirst in the schema.
        /// </summary>
        [Column(Storage = "_NameFirst", CanBeNull = false, DbType = "varchar(32) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string NameFirst
        {
            get
            {
                return this._NameFirst;
            }
            set
            {
                if (this._NameFirst != value)
                {
                    this.OnNameFirstChanging(value);
                    this.SendPropertyChanging();
                    this._NameFirst = value;
                    this.SendPropertyChanged("NameFirst");
                    this.OnNameFirstChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for NameMiddle in the schema.
        /// </summary>
        [Column(Storage = "_NameMiddle", CanBeNull = false, DbType = "varchar(32) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string NameMiddle
        {
            get
            {
                return this._NameMiddle;
            }
            set
            {
                if (this._NameMiddle != value)
                {
                    this.OnNameMiddleChanging(value);
                    this.SendPropertyChanging();
                    this._NameMiddle = value;
                    this.SendPropertyChanged("NameMiddle");
                    this.OnNameMiddleChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for NameLast in the schema.
        /// </summary>
        [Column(Storage = "_NameLast", CanBeNull = false, DbType = "varchar(32) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string NameLast
        {
            get
            {
                return this._NameLast;
            }
            set
            {
                if (this._NameLast != value)
                {
                    this.OnNameLastChanging(value);
                    this.SendPropertyChanging();
                    this._NameLast = value;
                    this.SendPropertyChanged("NameLast");
                    this.OnNameLastChanged();
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
        /// There are no comments for Device in the schema.
        /// </summary>
        [Devart.Data.Linq.Mapping.Association(Name="User_Device", Storage="_Device", ThisKey="ID", OtherKey="UserID", IsUnique=true, IsForeignKey=false, DeleteRule="SET NULL")]
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
                        previousValue.User = null;
                    }
                    this._Device.Entity = value;
                    if (value != null)
                    {
                        value.User = this;
                    }
                    this.SendPropertyChanged("Device");
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

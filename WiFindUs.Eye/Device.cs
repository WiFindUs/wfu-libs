using Devart.Data.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    public partial class Device
        : ILocatable, ILocation, IAtmospheric, IAtmosphere, IBatteryStats, IUpdateable, ThemedListBoxItem
    {
        public const long TIMEOUT = 60;
        
        public static event Action<Device> OnDeviceCreated;
        public event Action<Device> OnDeviceUpdated;
        public event Action<Device> OnDeviceLoaded;
        public event Action<Device> OnDeviceTypeChanged;
        public event Action<Device> OnDeviceLocationChanged;
        public event Action<Device> OnDeviceAtmosphereChanged;
        public event Action<Device> OnDeviceBatteryChanged;
        public event Action<Device> OnDeviceIPAddressChanged;
        public event Action<Device> OnDeviceUserChanged;
        public event Action<Device> OnDeviceAssignedWaypointChanged;
        public event Action<Device> OnDeviceTimedOutChanged;
        private bool timedOut = false;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public ILocation Location
        {
            get
            {
                return this;
            }
            set
            {
                if (value == null)
                {
                    Altitude = null;
                    Accuracy = null;
                    Longitude = null;
                    Latitude = null;
                }
                else
                {
                    Altitude = value.Altitude;
                    Accuracy = value.Accuracy;
                    Longitude = value.Longitude;
                    Latitude = value.Latitude;
                }

                if (OnDeviceLocationChanged != null)
                    OnDeviceLocationChanged(this);
            }
        }

        public IAtmosphere Atmosphere
        {
            get
            {
                return this;
            }
            set
            {
                if (value == null)
                {
                    Temperature = null;
                    Humidity = null;
                    AirPressure = null;
                    LightLevel = null;
                }
                else
                {
                    Temperature = value.Temperature;
                    Humidity = value.Humidity;
                    AirPressure = value.AirPressure;
                    LightLevel = value.LightLevel;
                }

                if (OnDeviceAtmosphereChanged != null)
                    OnDeviceAtmosphereChanged(this);
            }
        }

        public bool EmptyAtmosphere
        {
            get
            {
                return !Humidity.HasValue
                    && !Temperature.HasValue
                    && !AirPressure.HasValue
                    && !LightLevel.HasValue;
            }
        }

        public bool HasLatLong
        {
            get
            {
                return Latitude.HasValue
                    && Longitude.HasValue;
            }
        }

        public bool EmptyLocation
        {
            get
            {
                return !Latitude.HasValue
                    && !Longitude.HasValue
                    && !Accuracy.HasValue
                    && !Altitude.HasValue;
            }
        }

        public IPAddress IPAddress
        {
            get
            {
                return IPAddressRaw.HasValue ? new IPAddress(IPAddressRaw.Value) : null;
            }
            set
            {
                IPAddressRaw = value == null ? null : new Nullable<long>(value.Address);
            }
        }

        public long UpdateAge
        {
            get
            {
                return (DateTime.UtcNow.ToUnixTimestamp() - Updated);
            }
        }

        public bool TimedOut
        {
            get
            {
                return timedOut;
            }
            private set
            {
                if (value == timedOut)
                    return;

                timedOut = value;
                if (OnDeviceTimedOutChanged != null)
                    OnDeviceTimedOutChanged(this);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return String.Format("Device[{0}]", ID);
        }

        public double DistanceTo(ILocation other)
        {
            return WiFindUs.Eye.Location.Distance(this, other);
        }

        public void SetBatteryStats(IBatteryStats stats)
        {
            if (stats == null)
            {
                Charging = null;
                BatteryLevel = null;
            }
            else
            {
                Charging = stats.Charging;
                BatteryLevel = stats.BatteryLevel;
            }
        }

        public void CheckTimeout()
        {
            TimedOut = UpdateAge > TIMEOUT;
        }

        public int MeasureItemHeight(ThemedListBox host, System.Windows.Forms.MeasureItemEventArgs e)
        {
            return 30;
        }

        public void DrawListboxItem(System.Windows.Forms.DrawItemEventArgs e)
        {
            float offset = 0.0f;
            /*
            if (icon != null)
            {
                offset = (float)icon.Width + 5f;
                e.Graphics.DrawImageUnscaled(icon,
                    new Point(5, e.Bounds.Top + (int)((double)e.Bounds.Height / 2.0) - (int)((double)icon.Size.Height / 2.0)));
            }
            */

            string s = ID.ToString("X");
            SizeF bigSize = e.Graphics.MeasureString(s, e.Font);
            bool grayText = TimedOut || ((e.State & DrawItemState.Disabled) == DrawItemState.Disabled);
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            e.Graphics.DrawString(s, e.Font, grayText ? SystemBrushes.GrayText : (selected ? SystemBrushes.HighlightText : SystemBrushes.MenuText),
                new PointF(5.0f + offset, e.Bounds.Top + 4.0f));

            s = TimedOut ? "Timed out" : "ffffff";
            using (Font f = new Font(e.Font.FontFamily, e.Font.Size - 1f))
            {
                SizeF smallSize = e.Graphics.MeasureString(s, f);

                //e.Graphics.DrawString(s, f, grayText ? SystemBrushes.GrayText : (selected ? SystemBrushes.HighlightText : MenuCaptionColor),
                  //  new PointF(10.0f + offset, e.Bounds.Top + 6 + bigSize.Height));
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnCreated()
        {
            if (OnDeviceCreated != null)
                OnDeviceCreated(this);
        }

        partial void OnLoaded()
        {
            if (OnDeviceLoaded != null)
                OnDeviceLoaded(this);
        }

        partial void OnTypeChanged()
        {
            if (OnDeviceTypeChanged != null)
                OnDeviceTypeChanged(this);
        }

        partial void OnWaypointIDChanged()
        {
            if (OnDeviceAssignedWaypointChanged != null)
                OnDeviceAssignedWaypointChanged(this);
        }

        partial void OnIPAddressRawChanged()
        {
            if (OnDeviceIPAddressChanged != null)
                OnDeviceIPAddressChanged(this);
        }

        partial void OnBatteryLevelChanged()
        {
            if (OnDeviceBatteryChanged != null)
                OnDeviceBatteryChanged(this);
        }

        partial void OnChargingChanged()
        {
            if (OnDeviceBatteryChanged != null)
                OnDeviceBatteryChanged(this);
        }

        partial void OnUserIDChanged()
        {
            if (OnDeviceUserChanged != null)
                OnDeviceUserChanged(this);
        }

        partial void OnUpdatedChanged()
        {
            if (OnDeviceUpdated != null)
                OnDeviceUpdated(this);
            CheckTimeout();
        }
    }
}

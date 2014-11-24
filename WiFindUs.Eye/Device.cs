using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace WiFindUs.Eye
{
    public partial class Device : IIndentifiable, ILocatable, IAtmospheric, IUserClient, ICreationTimestamped, IUpdateTimestamped
    {
        public delegate void DeviceEvent(Device sender);
        public static event DeviceEvent OnDeviceCreated;
        public event DeviceEvent OnDeviceTypeChanged;
        public event DeviceEvent OnAssignedWaypointChanged;
        public event DeviceEvent OnLocationChanged;
        public event DeviceEvent OnAtmosphereChanged;
        public event DeviceEvent OnIPAddressChanged;
        public event DeviceEvent OnBatteryLevelChanged;
        public event DeviceEvent OnChargingChanged;
        public event DeviceEvent OnUserChanged;

        private const double EPSILON_BATTERY_LEVEL = 0.5;
        private DeviceState currentState = null;
        
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        protected DeviceState State
        {
            get
            {
                return currentState;
            }
            set
            {
                if (value == currentState)
                    return;
                DeviceState oldState = currentState;
                currentState = value;

                if (oldState == null || currentState == null)
                {
                    if (OnLocationChanged != null)
                        OnLocationChanged(this);
                    if (OnAtmosphereChanged != null)
                        OnAtmosphereChanged(this);
                    if (OnIPAddressChanged != null)
                        OnIPAddressChanged(this);
                    if (OnBatteryLevelChanged != null)
                        OnBatteryLevelChanged(this);
                    if (OnChargingChanged != null)
                        OnChargingChanged(this);
                    if (OnUserChanged != null)
                        OnUserChanged(this);
                }
                else
                {
                    if (!WiFindUs.Eye.Location.Equals(oldState, currentState) && OnLocationChanged != null)
                        OnLocationChanged(this);
                    if (!WiFindUs.Eye.Atmosphere.Equals(oldState, currentState) && OnAtmosphereChanged != null)
                        OnAtmosphereChanged(this);
                    if (!oldState.BatteryLevel.Tolerance(currentState.BatteryLevel, EPSILON_BATTERY_LEVEL) && OnBatteryLevelChanged != null)
                        OnBatteryLevelChanged(this);
                    if (oldState.Charging.GetValueOrDefault() != currentState.Charging.GetValueOrDefault() && OnChargingChanged != null)
                        OnChargingChanged(this);
                    if (oldState.User != currentState.User && OnUserChanged != null)
                        OnUserChanged(this);
                    if (oldState.IPAddressRaw != currentState.IPAddressRaw && OnIPAddressChanged != null)
                        OnIPAddressChanged(this);
                }
            }
        }

        public ILocation Location
        {
            get
            {
                return State;
            }
        }

        public IAtmosphere Atmosphere
        {
            get
            {
                return State;
            }
        }

        public User User
        {
            get { return State == null ? null : State.User; }
        }

        public DateTime Updated
        {
            get { return State == null ? Created : State.Created; }
        }

        public bool Charging
        {
            get { return State == null ? false : State.Charging.GetValueOrDefault(); }
        }

        public double BatteryLevel
        {
            get { return State == null ? 0.0 : State.BatteryLevel.GetValueOrDefault(); }
        }

        public IPAddress IPAddress
        {
            get
            {
                return State == null ? null : new IPAddress(State.IPAddressRaw);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnCreated()
        {
            DeviceStates.ListChanged += DeviceStates_ListChanged;
            if (OnDeviceCreated != null)
                OnDeviceCreated(this);
        }

        private void DeviceStates_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            DeviceState current = null;
            foreach (DeviceState state in DeviceStates)
                if (current == null || state.Created.Ticks > current.Created.Ticks)
                    current = state;
            State = current;
        }

        partial void OnTypeChanged()
        {
            if (OnDeviceTypeChanged != null)
                OnDeviceTypeChanged(this);
        }

        partial void OnWaypointIDChanged()
        {
            if (OnAssignedWaypointChanged != null)
                OnAssignedWaypointChanged(this);
        }
    }
}

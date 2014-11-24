using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace WiFindUs.Eye
{
    public partial class Node : IIndentifiable, ILocatable, ICreationTimestamped, IUpdateTimestamped
    {
        public delegate void NodeEvent(Node sender);
        public static event NodeEvent OnNodeCreated;
        public event NodeEvent OnIPAddressChanged;
        public event NodeEvent OnVoltageChanged;
        public event NodeEvent OnNumberChanged;
        public event NodeEvent OnLocationChanged;

        private const double EPSILON_VOLTAGE = 0.05;
        private NodeState currentState = null;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        protected NodeState State
        {
            get
            {
                return currentState;
            }
            set
            {
                if (value == currentState)
                    return;
                NodeState oldState = currentState;
                currentState = value;

                if (oldState == null || currentState == null)
                {
                    if (OnLocationChanged != null)
                        OnLocationChanged(this);
                    if (OnVoltageChanged != null)
                        OnVoltageChanged(this);
                    if (OnIPAddressChanged != null)
                        OnIPAddressChanged(this);
                    if (OnNumberChanged != null)
                        OnNumberChanged(this);
                }
                else
                {
                    if (!WiFindUs.Eye.Location.Equals(oldState, currentState) && OnLocationChanged != null)
                        OnLocationChanged(this);
                    if (oldState.Number != currentState.Number && OnNumberChanged != null)
                        OnNumberChanged(this);
                    if (oldState.IPAddressRaw != currentState.IPAddressRaw && OnIPAddressChanged != null)
                        OnIPAddressChanged(this);
                    if (!oldState.Voltage.Tolerance(currentState.Voltage, EPSILON_VOLTAGE) && OnVoltageChanged != null)
                        OnVoltageChanged(this);
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

        public DateTime Updated
        {
            get { return State == null ? Created : State.Created; }
        }

        public IPAddress IPAddress
        {
            get
            {
                return State == null ? null : new IPAddress(State.IPAddressRaw);
            }
        }

        public double Voltage
        {
            get { return State == null ? 0.0 : State.Voltage.GetValueOrDefault(); }
        }

        public long Number
        {
            get { return State == null ? 0 : State.Number; }
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnCreated()
        {
            NodeStates.ListChanged += NodeStates_ListChanged;
            if (OnNodeCreated != null)
                OnNodeCreated(this);
        }

        private void NodeStates_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            NodeState current = null;
            foreach (NodeState state in NodeStates)
                if (current == null || state.Created.Ticks > current.Created.Ticks)
                    current = state;
            State = current;
        }
    }
}

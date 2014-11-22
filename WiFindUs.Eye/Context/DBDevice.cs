using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye.Context
{
    public partial class DBDevice : IDevice, IIndentifiable, ILocatable, ICreationTimestamped
    {
        public IWaypoint AssignedWaypoint
        {
            get { return DBAssignedWaypoint; }
            set
            {
                if (value == null)
                    DBAssignedWaypoint = null;
                else
                {
                    DBWaypoint waypoint = value as DBWaypoint;
                    if (waypoint == null)
                        throw new InvalidOperationException("You must use the database type when making this assignment!");
                    DBAssignedWaypoint = waypoint;
                }
            }
        }

        public DBDeviceState CurrentState
        {
            get
            {
                DBDeviceState current = null;
                foreach (DBDeviceState state in DBDeviceStates)
                    if (current == null || state.Created.Ticks > current.Created.Ticks)
                        current = state;
                return current;
            }
        }

        public List<IAtmosphere> Atmospheres
        {
            get
            {
                return new List<IAtmosphere>(DBDeviceStates);
            }
        }

        public IAtmosphere CurrentAtmosphere
        {
            get
            {
                return CurrentState as IAtmosphere;
            }
        }

        public List<ILocation> Locations
        {
            get
            {
                return new List<ILocation>(DBDeviceStates);
            }
        }

        public ILocation CurrentLocation
        {
            get
            {
                return CurrentState as ILocation;
            }
        }

        public List<IDeviceLogin> Logins
        {
            get
            {
                return new List<IDeviceLogin>(DBDeviceStates);
            }
        }

        public IUser CurrentUser
        {
            get
            {
                DBDeviceState current = CurrentState;
                return current == null ? null : current.DBUser;
            }
        }
    }
}

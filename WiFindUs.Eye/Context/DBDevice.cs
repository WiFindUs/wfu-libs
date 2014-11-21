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
            get { return this.AssignedWaypointDB; }
            set
            {
                if (value == null)
                    AssignedWaypointDB = null;
                else
                {
                    DBWaypoint waypoint = value as DBWaypoint;
                    if (waypoint == null)
                        throw new InvalidOperationException("You must use the database type when making this assignment!");
                    AssignedWaypointDB = waypoint;
                }
            }
        }

        public List<IAtmosphere> Atmospheres
        {
            get
            {
                return new List<IAtmosphere>(AtmospheresDB);
            }
        }

        public IAtmosphere CurrentAtmosphere
        {
            get
            {
                ICreationTimestamped current = null;
                foreach (DBDeviceAtmosphere atmosphere in AtmospheresDB)
                    if (current == null || atmosphere.Created.Ticks > current.Created.Ticks)
                        current = atmosphere;
                return current as IAtmosphere;
            }
        }

        public List<ILocation> Locations
        {
            get
            {
                return new List<ILocation>(LocationsDB);
            }
        }

        public ILocation CurrentLocation
        {
            get
            {
                ICreationTimestamped current = null;
                foreach (DBDeviceLocation location in LocationsDB)
                    if (current == null || location.Created.Ticks > current.Created.Ticks)
                        current = location;
                return current as ILocation;
            }
        }

        public List<IDeviceLogin> Logins
        {
            get
            {
                return new List<IDeviceLogin>(LoginsDB);
            }
        }

        public IUser CurrentUser
        {
            get
            {
                ICreationTimestamped current = null;
                foreach (DBDeviceLogin login in LoginsDB)
                    if (current == null || login.Created.Ticks > current.Created.Ticks)
                        current = login;
                return current == null ? null : (current as DBDeviceLogin).UserDB;
            }
        }
    }
}

﻿using System.ComponentModel;

namespace WiFindUs.Eye.Controls
{
	public class WaypointListItem : EntityListItem
	{
		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Waypoint Waypoint
		{
			get { return Entity as Waypoint; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public WaypointListItem(Waypoint waypoint)
			: base(waypoint)
		{

		}
	}
}

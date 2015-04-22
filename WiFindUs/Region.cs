using System;
using WiFindUs.Extensions;

namespace WiFindUs
{
	/// <summary>
	/// An immutable packet of data describing a rectangular area bound by GPS coordinates, with the primary purpose of mapping those coordinates to a screen space.
	/// </summary>
	public class Region : ILocation, IEquatable<IRegion>, IRegion
	{
		public const uint GOOGLE_MAPS_TILE_MIN_ZOOM = 15;
		public const uint GOOGLE_MAPS_TILE_MAX_ZOOM = 20;

		private static readonly double GOOGLE_MAPS_TILE_RADIUS = 0.01126;
		private static readonly double GOOGLE_MAPS_TILE_LONG_SCALE = 1.22;
		private ILocation northWest, northEast, southWest, southEast, center;
		private double latSpan, longSpan, width, height;
		private uint zoomLevel = 0;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The north-west corner of this region.
		/// </summary>
		public ILocation NorthWest
		{
			get { return northWest; }
		}

		/// <summary>
		/// The north-east corner of this region.
		/// </summary>
		public ILocation NorthEast
		{
			get { return northEast; }
		}

		/// <summary>
		/// The south-east corner of this region.
		/// </summary>
		public ILocation SouthEast
		{
			get { return southEast; }
		}

		/// <summary>
		/// The south-west corner of this region.
		/// </summary>
		public ILocation SouthWest
		{
			get { return southWest; }
		}

		/// <summary>
		/// The latitude of the region's exact center.
		/// </summary>
		public double? Latitude
		{
			get { return center.Latitude; }
		}

		/// <summary>
		/// The longitude of the region's exact center.
		/// </summary>
		public double? Longitude
		{
			get { return center.Longitude; }
		}

		/// <summary>
		/// The mean Accuracy of the region's bounding points, if the data is present.
		/// </summary>
		public double? Accuracy
		{
			get
			{
				return northWest.Accuracy.GetValueOrDefault()
						+ southEast.Accuracy.GetValueOrDefault() / 2.0;
			}
		}

		/// <summary>
		/// The mean Altitude of the region's bounding points, if the data is present.
		/// </summary>
		public double? Altitude
		{
			get
			{
				return northWest.Altitude.GetValueOrDefault()
						+ southEast.Altitude.GetValueOrDefault() / 2.0;
			}
		}

		/// <summary>
		/// The center coordinates of this region.
		/// </summary>
		public ILocation Center
		{
			get { return center; }
		}

		/// <summary>
		/// The longitude range spanned by this region (from west-to-east / left-to-right).
		/// </summary>
		public double LongitudinalSpan
		{
			get { return longSpan; }
		}

		/// <summary>
		/// The latitude range spanned by this region (from north-to-south / top-to-bottom).
		/// </summary>
		public double LatitudinalSpan
		{
			get { return latSpan; }
		}

		/// <summary>
		/// The 'horizontal' distance covered by this region, in meters (as per the haversine formula). 
		/// </summary>
		public double Width
		{
			get { return width; }
		}

		/// <summary>
		/// The 'vertical' distance covered by this region, in meters (as per the haversine formula). 
		/// </summary>
		public double Height
		{
			get { return height; }
		}

		/// <summary>
		/// The google maps zoom level represented by this region, if it was constructed as one. Meaningless otherwise.
		/// </summary>
		public uint ZoomLevel
		{
			get { return zoomLevel; }
		}

		/// <summary>
		/// Implements ILocation's HasLatLong. Always returns true.
		/// </summary>
		public bool HasLatLong
		{
			get { return true; }
		}

		/// <summary>
		/// Implements ILocation's EmptyLocation. Always returns false.
		/// </summary>
		public bool EmptyLocation
		{
			get { return false; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Creates a Region from two ILocation objects.
		/// </summary>
		/// <param name="northWest">The ILocation representing the top-left, or north-west, point of the region.</param>
		/// <param name="southEast">The ILocation representing the bottom-right, or south-east, point of the region.</param>
		public Region(ILocation northWest, ILocation southEast)
		{
			if (northWest == null)
				throw new ArgumentNullException("northWest");
			if (southEast == null)
				throw new ArgumentNullException("southEast");
			if (!northWest.HasLatLong || !southEast.HasLatLong)
				throw new ArgumentException("NW and SE must have both lat and long components.");
			if (southEast.Longitude < northWest.Longitude)
				throw new ArgumentException("SE longitude must be greater than NW longitude (left-to-right).");
			if (northWest.Latitude < southEast.Latitude)
				throw new ArgumentException("SE latitude must be lower than NW latitude (top-to-bottom).");

			this.northWest = northWest;
			this.southEast = southEast;
			northEast = new Location(northWest.Latitude, southEast.Longitude);
			southWest = new Location(southEast.Latitude, northWest.Longitude);
			latSpan = northWest.Latitude.Value - southEast.Latitude.Value;
			longSpan = southEast.Longitude.Value - northWest.Longitude.Value;
			width = northWest.DistanceTo(northEast);
			height = northWest.DistanceTo(southWest);
			center = new Location(northWest.Latitude - latSpan / 2.0, northWest.Longitude + longSpan / 2.0);
		}

		/// <summary>
		/// Creates a Region from a center ILocation and width (longitude) / height (latitude) spans.
		/// </summary>
		/// <param name="center">The point that will be the center of the region.</param>
		/// <param name="latSpan">The region's height, in degrees latitude.</param>
		/// <param name="longSpan">The region's width, in degrees longitude.</param>
		public Region(ILocation center, double latSpan, double longSpan)
		{
			if (center == null)
				throw new ArgumentNullException("center");
			if (!center.HasLatLong)
				throw new ArgumentException("Center must have both lat and long components.");
			if (latSpan < 0.0)
				throw new ArgumentOutOfRangeException("latSpan", "latSpan cannot be negative");
			if (longSpan < 0.0)
				throw new ArgumentOutOfRangeException("longSpan", "longSpan cannot be negative");

			this.center = center;
			this.latSpan = latSpan;
			this.longSpan = longSpan;
			northWest = new Location(center.Latitude + latSpan / 2.0, center.Longitude - longSpan / 2.0);
			southEast = new Location(center.Latitude - latSpan / 2.0, center.Longitude + longSpan / 2.0);
			northEast = new Location(northWest.Latitude, southEast.Longitude);
			southWest = new Location(southEast.Latitude, northWest.Longitude);
			width = northWest.DistanceTo(northEast);
			height = northWest.DistanceTo(southWest);
		}

		/// <summary>
		/// Creates a Region from a center ILocation and a formula for determining lat and long span based on a Google Maps API Zoom level.
		/// </summary>
		/// <param name="center">The point that will be the center of the region.</param>
		/// <param name="zoomLevel">The zoom level at which to create the tile. Must be between Region.GOOGLE_MAPS_TILE_MIN_ZOOM and
		/// Region.GOOGLE_MAPS_TILE_MAX_ZOOM.</param>
		public Region(ILocation center, uint zoomLevel)
		{
			if (center == null)
				throw new ArgumentNullException("center");
			if (!center.HasLatLong)
				throw new ArgumentException("Center must have both lat and long components.");
			if (zoomLevel < GOOGLE_MAPS_TILE_MIN_ZOOM || zoomLevel > GOOGLE_MAPS_TILE_MAX_ZOOM)
				throw new ArgumentOutOfRangeException("zoomLevel", "Zoom level must be between "
					+ GOOGLE_MAPS_TILE_MIN_ZOOM + " and " + GOOGLE_MAPS_TILE_MAX_ZOOM + " (inclusive).");

			this.zoomLevel = zoomLevel;
			double scaledRadius = GOOGLE_MAPS_TILE_RADIUS / Math.Pow(2.0, (zoomLevel - GOOGLE_MAPS_TILE_MIN_ZOOM));
			this.center = center;
			northWest = new Location(center.Latitude + scaledRadius, center.Longitude - (scaledRadius * GOOGLE_MAPS_TILE_LONG_SCALE));
			southEast = new Location(center.Latitude - scaledRadius, center.Longitude + (scaledRadius * GOOGLE_MAPS_TILE_LONG_SCALE));
			northEast = new Location(northWest.Latitude, southEast.Longitude);
			southWest = new Location(southEast.Latitude, northWest.Longitude);
			latSpan = northWest.Latitude.Value - southEast.Latitude.Value;
			longSpan = southEast.Longitude.Value - northWest.Longitude.Value;
			width = northWest.DistanceTo(northEast);
			height = northWest.DistanceTo(southWest);
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public static bool Equals(IRegion A, IRegion B)
		{
			if (A == null || B == null)
				return false;
			if (ReferenceEquals(A, B))
				return true;
			return A.NorthWest.Equals(B.NorthWest) && A.SouthEast.Equals(B.SouthEast);

		}

		public bool Equals(IRegion other)
		{
			return Equals(this, other);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as IRegion);
		}

		public override int GetHashCode()
		{
			return HashCode.Start
					.Hash(northEast)
					.Hash(southWest);
		}

		public static bool Contains(IRegion region, double latitude, double longitude)
		{
			if (region == null)
				return false;
			return latitude <= region.NorthWest.Latitude
				&& latitude >= region.SouthEast.Latitude
				&& longitude >= region.NorthWest.Longitude
				&& longitude <= region.SouthEast.Longitude;
		}

		public bool Contains(double latitude, double longitude)
		{
			return Contains(this, latitude, longitude);
		}

		public static bool Contains(IRegion region, ILocation location)
		{
			if (region == null || location == null)
				return false;
			if (!location.HasLatLong)
				throw new ArgumentOutOfRangeException("location", "Location must contain latitude and longitude.");
			return Contains(region, location.Latitude.Value, location.Longitude.Value);
		}

		public bool Contains(ILocation location)
		{
			return Contains(this, location);
		}

		public void LocationToScreen(System.Drawing.Rectangle screenBounds, double latitude, double longitude, out int x, out int y)
		{
			x = screenBounds.X + (int)(((longitude - northWest.Longitude) / longSpan) * (double)screenBounds.Width);
			y = screenBounds.Y + (int)(((northWest.Latitude - latitude) / latSpan) * (double)screenBounds.Height);
		}

		public System.Drawing.Point LocationToScreen(System.Drawing.Rectangle screenBounds, double latitude, double longitude)
		{
			return new System.Drawing.Point(
					screenBounds.X + (int)(((longitude - northWest.Longitude) / longSpan) * (double)screenBounds.Width),
					screenBounds.Y + (int)(((northWest.Latitude - latitude) / latSpan) * (double)screenBounds.Height)
				);
		}

		public System.Drawing.Point LocationToScreen(System.Drawing.Rectangle screenBounds, ILocation location)
		{
			if (!location.HasLatLong)
				throw new ArgumentOutOfRangeException("location", "Location must contain latitude and longitude.");
			return LocationToScreen(screenBounds, location.Latitude.Value, location.Longitude.Value);
		}

		public double DistanceTo(ILocation other)
		{
			return WiFindUs.Location.Distance(this, other);
		}

		public static string ToString(IRegion region)
		{
			return region.NorthWest + "x" + region.SouthEast;
		}

		public override string ToString()
		{
			return ToString(this);
		}

		public static ILocation Clamp(IRegion region, ILocation location)
		{
			if (region == null || location == null)
				return null;
			if (!location.HasLatLong)
				throw new ArgumentOutOfRangeException("location", "Location must contain latitude and longitude.");
			return new WiFindUs.Location(
				location.Latitude.Value.Clamp(region.SouthEast.Latitude.Value, region.NorthWest.Latitude.Value),
				location.Longitude.Value.Clamp(region.NorthWest.Longitude.Value, region.SouthEast.Longitude.Value),
				location.Accuracy,
				location.Altitude);
		}

		public ILocation Clamp(ILocation location)
		{
			return Clamp(this, location);
		}
	}
}

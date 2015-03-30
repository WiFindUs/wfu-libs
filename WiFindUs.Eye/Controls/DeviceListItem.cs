using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Controls
{
	public class DeviceListItem : EntityListItem
	{
		private Device device;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Device Device
		{
			get { return device; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override Color ImagePlaceholderColour
		{
			get
			{
				return device.User == null
					? base.ImagePlaceholderColour : WFUApplication.Config.Get("type_" + device.User.Type + ".colour", Color.Red);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override String EntityTitleString
		{
			get
			{
				if (device == null)
					return "";
				return String.Format("Device #{0:X}", device.ID);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override String EntityDetailString
		{
			get
			{
				if (device == null)
					return "";
				return String.Format("{0}\n{1}",
					device.User != null ? "in use by " + device.User.FullName : "No assigned user.",
					device.TimedOut ? "Timed out." :
						(!device.IsGPSEnabled.GetValueOrDefault() ? "GPS disabled." :
							(!device.IsGPSFixed.GetValueOrDefault() ? "Waiting for GPS fix..." :
								(!device.HasLatLong ? "Waiting for accurate location..." : WiFindUs.Eye.Location.ToString(device)))));
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public DeviceListItem(Device device)
			: base(device)
		{
			this.device = device;
			if (IsDesignMode)
				return;
			device.OnDeviceTypeChanged += device_OnDeviceTypeChanged;
			device.OnDeviceUserChanged += device_OnDeviceUserChanged;
			device.LocationChanged += device_OnDeviceLocationChanged;
			device.WhenUpdated += device_OnDeviceUpdated;
			device.TimedOutChanged += device_OnDeviceTimedOutChanged;
			device.OnDeviceBatteryChanged += device_OnDeviceBatteryChanged;
			device.OnDeviceAssignedWaypointChanged += device_OnDeviceAssignedWaypointChanged;
			device.OnGPSEnabledChanged += device_OnGPSEnabledChanged;
			device.OnGPSFixedChanged += device_OnGPSFixedChanged;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (device == null || device.TimedOut)
				return;

			int w = 6;
			int p = 2;
			Rectangle rect = new Rectangle(ClientRectangle.Width - w - p, p, w, ClientRectangle.Height - p * 2 - 1);
			e.Graphics.FillRectangle(Theme.ControlMidBrush, rect);
			if (device.BatteryLevel.HasValue)
			{
				double pc = device.BatteryLevel.Value;
				int height = (int)(rect.Height * pc);
				e.Graphics.FillRectangle(
					pc >= 0.75 ? Brushes.LimeGreen : (pc >= 0.5 ? Brushes.Yellow : (pc >= 0.25 ? Brushes.Orange : Brushes.Red)),
					rect.X, rect.Bottom - height, rect.Width, height
					);
			}

			using (Pen pen = new Pen(Theme.ControlDarkColour))
				e.Graphics.DrawRectangle(pen, rect);

			string text = (device.BatteryLevel.HasValue ? String.Format("{0:P0}", device.BatteryLevel.Value) : " ") + "\n"
				+ (device.Charging.HasValue && device.Charging.Value ? "Charging" : " ");
			if (text.Trim().Length > 0)
			{

				using (StringFormat sf = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Far })
				{
					using (Font f = new Font(Font.FontFamily, Font.Size - 2.0f))
					{
						SizeF sz = e.Graphics.MeasureString(
							text,
							f,
							ClientRectangle.Width,
							sf);
						e.Graphics.DrawString(
							text,
							f,
							Theme.TextMidBrush,
							new Point(rect.Left - p, rect.Top),
							sf);
					}
				}
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////


		private void device_OnGPSFixedChanged(Device obj)
		{
			this.RefreshThreadSafe();
		}

		private void device_OnGPSEnabledChanged(Device obj)
		{
			this.RefreshThreadSafe();
		}

		private void device_OnDeviceAssignedWaypointChanged(Device obj)
		{
			this.RefreshThreadSafe();
		}

		private void device_OnDeviceBatteryChanged(Device obj)
		{
			this.RefreshThreadSafe();
		}

		private void device_OnDeviceTimedOutChanged(IUpdateable obj)
		{
			this.RefreshThreadSafe();
		}

		private void device_OnDeviceUpdated(IUpdateable obj)
		{
			this.RefreshThreadSafe();
		}

		private void device_OnDeviceLocationChanged(ILocatable obj)
		{
			this.RefreshThreadSafe();
		}

		private void device_OnDeviceUserChanged(Device obj)
		{
			this.RefreshThreadSafe();
		}

		private void device_OnDeviceTypeChanged(Device obj)
		{
			this.RefreshThreadSafe();
		}
	}
}

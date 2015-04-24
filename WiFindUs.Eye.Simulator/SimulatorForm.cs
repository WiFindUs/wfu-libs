using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Simulator
{
	public partial class SimulatorForm : EyeMainForm
	{
		private readonly List<IPacketFactory> packetFactories = new List<IPacketFactory>();
		private readonly object PacketFactoryLock = new object();
		
		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////
		
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override DBMode DesiredDatabaseMode
		{
			get { return DBMode.Client; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override bool ListenForPackets
		{
			get { return false; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override uint MapLevels
		{
			get { return 1; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override bool AutoApplyConfigState
		{
			get { return false; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool AutoShowConsole
		{
			get { return false; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public SimulatorForm()
		{
			InitializeComponent();
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnFirstShown(EventArgs e)
		{
			base.OnFirstShown(e);
			if (IsDesignMode)
				return;
			if (Map.ThreadlessState)
				Map.Load();

			Node node = Nodes.FirstOrDefault();
			if (node == null)
			{
				node = new Node()
				{
					ID = 10000u + (uint)WFUApplication.Random.Next(20000),
					Latitude = Map.Center.Latitude,
					Longitude = Map.Center.Longitude,
					MeshPoint = true,
					DHCPD = true,
					GPSD = true,
					MockLocation = true,
					Created = DateTime.Now.ToUnixTimestamp(),
					LastUpdated = DateTime.Now.ToUnixTimestamp(),
					AccessPoint = true
				};
			}
			packetFactories.Add(new NodePacketFactory(node));

			//start udp thread
			Thread thread = new Thread(new ThreadStart(PacketSenderThread));
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void PacketSenderThread()
		{
			WFUApplication.SetThreadAlias("PS");
			Debugger.V("Eye simulator packet thread started.");
			UdpClient sender;
			try
			{
				sender = new UdpClient();
			}
			catch (SocketException)
			{
				Debugger.E("Error creating UDP sender!");
				sender = null;
			}

			if (sender != null)
			{
				while (!HasClosed)
				{
					lock (PacketFactoryLock)
					{
						foreach (IPacketFactory pf in packetFactories)
						{
							byte[] bytes = Encoding.UTF8.GetBytes(pf.Packet);
							sender.Send(bytes, bytes.Length,
								new IPEndPoint(IPAddress.Loopback, EyePacketListener.PORT_FIRST
									+ WFUApplication.Random.Next(EyePacketListener.PORT_COUNT)));
							SafeSleep(500 + WFUApplication.Random.Next(500));
						}
					}
				}
			}

			if (sender != null)
			{
				try { sender.Close(); }
				catch { }
				sender = null;
			}
			Debugger.I("Eye simulator packet thread terminated.");
		}
	}


}

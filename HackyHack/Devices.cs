using System.Collections.Generic;

namespace HackyHack
{
	public enum EDeviceConnectionType { DCT_Client, DCT_Network }
	public enum EDeviceConnectionMedium { DCM_Physical, DCM_Wireless, DCM_Satellite, DCM_LOS }

	// the base class for all connection-between-devices types
	public class DeviceConnection
	{
		// type of this connection
		public EDeviceConnectionType Type;
		// medium of this connection
		public EDeviceConnectionMedium Medium;

		// amount of theoretical maximum data bandwidth
		public uint MaxPerConnectionBandwidth;

		// amount of current data usage
		public uint BandwidthAvailable;
		public uint BandwidthDemand;

		// all the devices connected together by this connection
		public readonly List<DeviceConnection> Connections;
		public int MaxConnections;

		// the host device for this connection
		public Device Host;

		// the unique ID to prevent backtracking when crawls touch this connection
		public uint CrawlID;

		// the unique ID to prevent backtracking when bandwidth ticking touches this connection
		public uint BandID;


		public DeviceConnection(Device h, EDeviceConnectionType t, EDeviceConnectionMedium m, uint mcb, int mc)
		{
			Connections = new List<DeviceConnection>();
			Host = h;
			Type = t;
			MaxPerConnectionBandwidth = mcb;
			Medium = m;
			MaxConnections = mc;
		}

		public int GetNumConnections()
		{
			return Connections.Count;
		}

		public int GetNumUncrawledConnections()
		{
			int i = 0;
			foreach (DeviceConnection dc in Connections)
			{
				if (dc.CrawlID == CrawlID) continue;
				if (dc.Host.CrawlID == CrawlID) continue;
				i++;
			}

			return i;
		}

		public int GetNumActiveConnections()
		{
			int aConns = 0;
			foreach (DeviceConnection dc in Connections)
				if (dc.Host.bActive) aConns++;

			return aConns;
		}

		public bool IsConnectedTo(Device d)
		{
			return Connections.Exists(dc => dc.Host == d);
		}

		// Mediums have to match
		// Generic connections can connect to any other type
		// Network connections cannot connect to Client connections
		// this assumes that it isn't trying to connect to the same host or itself
		public bool CanConnectTo(DeviceConnection dc)
		{
			if (Medium != dc.Medium) return false;
			if (Type != dc.Type) return false;

			return true;
		}

		// this assumes that Host has already checked that it can connect to d
		public bool ConnectTo(Device d)
		{
			if (IsConnectedTo(d)) return true;
			if (Connections.Count >= MaxConnections) return false;

			DeviceConnection dc = d.FindConnectionFor(this);
			if (dc == null) return false;

			Connections.Add(dc);

			return true;
		}

		public void CrawlThrough(Queue<Device> queue)
		{
			foreach (DeviceConnection dc in Connections)
			{
				if (dc.Host.CrawlID != CrawlID)
				{
					dc.CrawlID = CrawlID;
					queue.Enqueue(dc.Host);
				}
			}
		}

		public void PropogateBandwidthDemand()
		{
			//foreach (D)
		}
	}

	// the base class for all devices
	// NOTE :
	//   No device should have multiple connections with the same type but different bandwidth ratings
	//   This will prevent issues of players feeling like they need to optimize device connections on one device
	public class Device
	{
		public bool bActive;
		public readonly List<DeviceConnection> Connections;

		public ECrawlOptions CrawlDescriptor;
		public uint CrawlID;

		public uint BandwidthDemand;
		public uint BandwidthAvailable;
		public uint BandID;

		public Device()
		{
			Connections = new List<DeviceConnection>();
		}

		#region Tests for connection possibility
		public bool CanConnectTo(DeviceConnection other)
		{
			foreach (DeviceConnection dc in Connections)
			{
				if (dc.CanConnectTo(other)) return true;
			}

			return false;
		}

		public bool CanConnectTo(Device other)
		{
			foreach (DeviceConnection dc in Connections)
			{
				if (other.CanConnectTo(dc)) return true;
			}

			return false;
		}
		#endregion

		public DeviceConnection FindConnectionFor(DeviceConnection other)
		{
			foreach (DeviceConnection dc in Connections)
			{
				if (dc.CanConnectTo(other) && (dc.MaxConnections < dc.GetNumConnections())) return dc;
			}

			return null;
		}

		public bool ConnectTo(Device other, DeviceConnection by)
		{
			return by.ConnectTo(other);
		}

		public void CrawlThrough(Queue<Device> queue)
		{
			foreach (DeviceConnection dc in Connections)
			{
				if (dc.CrawlID != CrawlID)
				{
					dc.CrawlID = CrawlID;
					dc.CrawlThrough(queue);
				}
			}
		}
	}

	// the base class for all network processing devices
	// such as routers, switches, hubs, firewalls, etc.
	public class NetworkDevice : Device
	{
		public NetworkDevice()
		{
		}
	}

	// the entry point to the network
	// ALL networks have a DTE, and shutting it down closes off the entire network immediately
	public class DTE : NetworkDevice
	{
		public uint IncomingBandwidth;

		public DTE()
		{
			CrawlDescriptor = ECrawlOptions.DTEs;
		}
	}

	public class Firewall : NetworkDevice
	{
		// how long the firewall refreshes connection validations
		// the higher the min, the easier the hack
		// the higher the range, the closer attention the hacker must pay
		public float ConnectionValidationTimeRange;
		public float ConnectionValidationTimeMin;

		// the maximum number of allowed errors per cycle timeframe before the firewall raises an alarm
		// the higher the value, the easier the hack
		public int NumAllowedErrorsPerCycle;
		// the length of time in seconds for the firewall's intrusion detection threshold
		// the higher the cycle time, the easier the hack
		public float ErrorCycleTime;

		// the maximum number of simultaneous connections before the firewall raises an alarm
		public int NumMaxSimultaneousConnections;

		// the general complexity of the firewall's traffic management
		// this is the value that hackers must build up to in order to bypass this firewall
		public float TrafficComplexity;

		// directly used to calculate the number of firewall cells to use
		// indicates in general how busy the firewall is
		// the higher the general activity, the easier the hack
		public int GeneralActivity;
		
		public Firewall()
		{
			CrawlDescriptor = ECrawlOptions.Firewalls;
		}
	}

	public class IDS : NetworkDevice
	{
		public IDS()
		{
			CrawlDescriptor = ECrawlOptions.IDS;
		}
	}

	public class Hub : NetworkDevice
	{
		public Hub()
		{
			CrawlDescriptor = ECrawlOptions.Hubs;
		}
	}

	public class Switch : NetworkDevice
	{
		public Switch()
		{
			CrawlDescriptor = ECrawlOptions.Switches;
		}
	}

	public class Router : NetworkDevice
	{
		public Router()
		{
			CrawlDescriptor = ECrawlOptions.Routers;
		}
	}
}


using System.Collections.Generic;

namespace HackyHack
{
	public enum EDeviceConnectionType { DCT_Generic, DCT_Client, DCT_Network }
	public enum EDeviceConnectionMedium { DCM_Physical, DCM_Wireless, DCM_Satellite, DCM_LOS }

	// the base class for all connection-between-devices types
	public class DeviceConnection
	{
		// type of this connection
		public EDeviceConnectionType Type;
		// medium of this connection
		public EDeviceConnectionMedium Medium;

		// amount of theoretical maximum data bandwidth
		public uint MaxBandwidth;

		// amount of current data usage
		public uint CurBandwidth;

		// all the devices connected together by this connection
		List<DeviceConnection> Connections;
		public bool bMulticast;

		// the host device for this connection
		public Device Host;


		public DeviceConnection(Device h, EDeviceConnectionType t, EDeviceConnectionMedium m, uint mb, bool bM = false)
		{
			Connections = new List<DeviceConnection>();
			Host = h;
			Type = t;
			MaxBandwidth = mb;
			Medium = m;
			bMulticast = bM;
		}


		public int GetNumConnections()
		{
			return Connections.Count;
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
			if ((Type == EDeviceConnectionType.DCT_Network) && (dc.Type == EDeviceConnectionType.DCT_Client)) return false;
			if ((Type == EDeviceConnectionType.DCT_Client) && (dc.Type == EDeviceConnectionType.DCT_Network)) return false;

			return true;
		}

		// this assumes that Host has already checked that it can connect to d
		public bool ConnectTo(Device d)
		{
			if (IsConnectedTo(d)) return true;
			if (!bMulticast && (GetNumConnections() >= 1)) return false;

			return true;
		}
	}

	// the base class for all devices
	public class Device
	{
		List<DeviceConnection> Connections;

		public Device()
		{
			Connections = new List<DeviceConnection>();
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
}


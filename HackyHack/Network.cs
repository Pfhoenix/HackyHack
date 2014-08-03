using System;
using System.Collections.Generic;

namespace HackyHack
{
	[Flags]
	public enum ECrawlOptions : uint
	{
		None = 0,
		// active or inactive options
		Active = None << 1,
		Inactive = Active << 1,
		Active_Inactive = Active | Inactive,
		// network devices
		Routers = Inactive << 1,
		Switches = Routers << 1,
		Hubs = Switches << 1,
		Traffic_Devices = Routers | Switches | Hubs,
		DTEs = Hubs << 1,
		Firewalls = DTEs << 1,
		IDS = Firewalls << 1,
		Network_Devices = Traffic_Devices | DTEs | Firewalls | IDS,
		// customer devices
		Clients = IDS << 1,
		Servers = Clients << 1,
		Printers = Servers << 1,
		Phones = Printers << 1,
		Customer_Devices = Clients | Servers | Printers | Phones,
		// option for crawling over non-matching devices
		TouchNonMatch = Phones << 1,
		// option for starting crawl at DTEs instead of a specified device
		StartDTEs = TouchNonMatch << 1,
		// convenience options
		All_Active = Active | Network_Devices | Customer_Devices,
		All = Active_Inactive | Network_Devices | Customer_Devices,
	}

	public class Network
	{
		// this helps facilitate proper bandwidth allocation crawling
		public readonly List<DTE> DTEs;
		public readonly List<Device> Devices;

		public delegate bool NetworkCrawlCallback(Device d);

		uint CrawlID;

		public Network()
		{
			DTEs = new List<DTE>(5);
			Devices = new List<Device>(100);
			CrawlID = 0;
		}

		public void NetworkCrawl(ECrawlOptions options, NetworkCrawlCallback callback, Device start = null)
		{
			CrawlID++;
		}
	}
}


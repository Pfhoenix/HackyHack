using System;
using System.Collections.Generic;

namespace HackyHack
{
	[Flags]
	public enum ECrawlOptions : uint
	{
		None = 0,
		// active or inactive options
		Active = 1,
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

		// return true means keep crawling, false means to stop immediately
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

			// sanity check that we will start somewhere
			if (!options.HasFlag(ECrawlOptions.StartDTEs) && (start == null)) return;
			Queue<Device> processQueue = new Queue<Device>(20);
			// initialize processQueue
			if (options.HasFlag(ECrawlOptions.StartDTEs))
			{
				foreach (DTE dte in DTEs) processQueue.Enqueue(dte);
			}
			else processQueue.Enqueue(start);

			Device cur;
			while (processQueue.Count > 0)
			{
				cur = processQueue.Dequeue();
				// no need to process cur if it's already been touched by this crawl
				if (cur.CrawlID == CrawlID) continue;

				cur.CrawlID = CrawlID;

				bool bNonMatch = false;
				// check for a device we care about
				if (options.HasFlag(cur.CrawlDescriptor))
				{
					// check Active status
					if (options.HasFlag(ECrawlOptions.Active_Inactive) || (options.HasFlag(ECrawlOptions.Active) && cur.bActive) || (options.HasFlag(ECrawlOptions.Inactive) && !cur.bActive))
					{
						if (!callback(cur)) return;
					}
					else bNonMatch = true;
				}
				else bNonMatch = true;

				// grab connected devices if we matched or if we want to touch nonmatches
				if (!bNonMatch || options.HasFlag(ECrawlOptions.TouchNonMatch))
				{
					cur.CrawlThrough(processQueue);
				}
			}
		}
	}
}


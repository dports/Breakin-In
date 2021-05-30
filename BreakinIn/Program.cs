using System;
using System.Threading;
using System.Threading.Tasks;

namespace BreakinIn
{
	public class Program
	{
		static void Main(string[] args)
		{
			// Initialize DNS Proxy server
			DNSHelper.InitDNSProxyServer();

			// Initialize game server
			var server = new TSBOServer();

			// Start game server
			var result = server.Run(IPHelper.GetExternalIPAddress().ToString());

			if (!result)
			{
				Console.WriteLine("Press any key to continue...");
				Console.ReadKey();
			}
		}
	}
}
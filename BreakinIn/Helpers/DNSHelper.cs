using DNS.Server;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BreakinIn
{
	public class DNSHelper
	{
		static DnsServer _proxyServer;

		public static Task StartDNSProxyServer()
		{
			// Proxy to google's DNS
			MasterFile masterFile = new MasterFile();
			masterFile.AddIPAddressResourceRecord("gate1.us.dnas.playstation.org", "199.195.251.151");
			masterFile.AddIPAddressResourceRecord("tso-e.com", IPHelper.GetExternalIPAddress().ToString());
			masterFile.AddIPAddressResourceRecord("ps2sims04.ea.com", IPHelper.GetExternalIPAddress().ToString());
			_proxyServer = new DnsServer(masterFile, "1.0.0.1");

			// Log every request
			_proxyServer.Responded += (request, response) => Console.WriteLine("{0} => {1}", request, response);
			
			// Start the server (by default it listents on port 53)
			return _proxyServer.Listen();
		}

		public static async void Do(object data)
		{
			CancellationToken token = (CancellationToken)data;
			HttpClient client = new HttpClient();
			client.Timeout = new TimeSpan(0, 0, 5);
			while (!token.IsCancellationRequested)
			{
				try
				{
					HttpResponseMessage response = await client.GetAsync(new Uri($"http://gate1.us.dnas.playstation.org"));
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
				await Task.Delay(1000);
			}
		}

		public static void InitDNSProxyServer()
		{
			// Start and initialze DNS Server
			Task listenTask = DNSHelper.StartDNSProxyServer();

			// Start thread for send requests
			CancellationTokenSource cs = new CancellationTokenSource();
			Thread requestThr = new Thread(new ParameterizedThreadStart(DNSHelper.Do));
			requestThr.Start(cs.Token);

			cs.Cancel();
			requestThr.Join();
			_proxyServer.Dispose();
			listenTask.Wait();
		}
	}
}
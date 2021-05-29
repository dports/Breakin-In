using System;
using System.Net;
using System.Text.RegularExpressions;

namespace BreakinIn
{
	public class IPHelper
	{
		public static WebClient webclient = new WebClient();
		public static string GetExternalIPAddress()
		{
			try
			{
				string result = string.Empty;

				string[] checkIPUrl =
				{
					"https://checkip.amazonaws.com/",
					"https://ipv4.icanhazip.com/",
					"https://ipinfo.io/ip/",
					"https://api.ipify.org/",
					"https://icanhazip.com/",
					"https://ipecho.net/plain/",
					"http://ifconfig.me/",
					"http://bot.whatismyipaddress.com/",
					"http://checkip.dyndns.org/",
					"http://www.ip-adress.com/",
					"https://myip.ru/"
				};

				using (var client = new WebClient())
				{
					client.Headers["User-Agent"] = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command

					foreach (var url in checkIPUrl)
					{
						try
						{
							result = client.DownloadString(url);
						}
						catch
						{
						}

						if (!string.IsNullOrEmpty(result))
						break;
					}
				}

				result = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
				.Matches(result)[0].ToString();
				return result;
			}
			catch (Exception) { return "127.0.0.1"; }
		}
	}
}
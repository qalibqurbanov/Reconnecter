// #define TEST

using System;
using System.Net;
using System.Linq;
using System.Threading;
using System.Net.NetworkInformation;

namespace MainProject.Helpers
{
	/// <summary>
	/// WiFi ile elaqeli funksionalliqlari saxlayir.
	/// </summary>
	public static class WifiHelpers
	{
		/// <summary>
		/// Userin kompyuterinde hazirda aktiv/istifadede olan Network Interface/Adapter Card-i haqqinda melumatlari saxlayir.
		/// </summary>
		private static readonly NetworkInterface activeInterface = NetworkInterface.GetAllNetworkInterfaces()?.FirstOrDefault
		(net =>
			net.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
			net.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
			net.OperationalStatus == OperationalStatus.Up &&
			net.Name.StartsWith("vEthernet") == false

		/*
			* Network Interface/Adapter Card-in axtariwi lazimsiz interfeysleri aradan cixarmaq uzerinedir. Eger interfeys:
					> Loopback(esasen testing ucun iwledilir) ve ya Tunnel(tunelling, datanin network uzerinden, bawqa sozle bir networkden digerine tehlukesiz gonderilmeyi ucun iwledilen bir yoldur/metoddur, daha cox VPN-ler tetbiq edir) network interface deyilse,
					> iwleyirse, yeni hazirda data paketi gondere ve ya qebul ede bilirse,
					> "Hyper-V"-ye aid network interface deyilse
				:tapilan ilk interfeysi secirem.
		*/
		); /* WiFi-i On/Off etmek ucun iwledecem. */



		/// <summary>
		/// Hazirda qowulu oldugumuz WiFi-nin adini(SSID) dondurur.
		/// </summary>
		/// <returns>Hazirda qowulu oldugumuz WiFi-nin adi(SSID), hec bir wifiye qowulu deyilikse NULL dondururuk.</returns>
		public static string GetConnectedWifiSsid()
		{
			try
			{
				string processOutputResult = null;
				string SSID = null;

				ProcessHelpers.StartProcess("netsh.exe", "wlan show interfaces", out processOutputResult);

				if(processOutputResult != null)
				{
					if(processOutputResult.IndexOf("SSID") > -1)
					{
						/* > Awagida etdiyim:
							1. SSID-den etibaren bawlayan hisseni gotururem, goturduyum parca bu cur bawlayacaq: "SSID : ...".
							2. SSID-ni 'SSID :' iki noqteden etibaren yene parcalayiram. Belece IndexOf ilk qarwilawdigi : simvolundan bawlayan hisseni dondurecek, elimde ": WifiAdi ..." olacaq.
							3. 2-ci indeksden bawla, yeni setirle qarwilawanadek olan hisseni gotur.
							@ Netice olaraq elimde yalniz ve yalniz SSID hisse qalmiw olacaq.
						*/

						SSID = processOutputResult.Substring(processOutputResult.IndexOf("SSID"));
						SSID = SSID.Substring(SSID.IndexOf(":"));
						SSID = SSID.Substring(2, SSID.IndexOf("\n")).Trim();
#if TEST
						TestHelpers.CustomizeConsole($"Reconnecter - {SSID}");
#endif
						return SSID;
					}
					else /* WiFi-ye qowulmamiwamsa */
					{
#if TEST
						TestHelpers.CustomizeConsole();
#endif
						return null;
					}
				}

				return null;
			}
			catch
			{
#if TEST
                TestHelpers.CustomizeConsole();
#endif
				return null;
			}
		}



		/// <summary>
		/// Istifadecinin internete cixiwi olub-olmamagi barede boolean dondurur.
		/// </summary>
		/// <param name="URL">(Optional) Internetin olub-olmadigini yoxlamaq meqsedile hansi url-e request gonderilsin?</param>
		/// <param name="TimeoutMs">(Optional) Gondereceyimiz request verdiyimiz zamandan uzun cekse request baw tutmayacaq.</param>
		/// <returns>Internet varsa TRUE, yoxdursa FALSE dondurur.</returns>
		public static bool CheckForInternetConnection(string URL, int TimeoutMs = 10000)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3; /* Sorgu zamani iwledilecek security protokolu */
			ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; /* Serverdeki sertifikatin yoxlaniwini iqnor edirik */

			URL = new UriBuilder(Uri.UriSchemeHttps, URL).Uri.ToString(); /* Elimde olan endpoint adresi ip formasindadir deye, verdiyim IP-ni URL formasina saliram: https://IP/ */

			try
			{
				HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(URL);
				httpRequest.KeepAlive = false;
				httpRequest.Timeout = TimeoutMs;
				httpRequest.Proxy = null;

				using(WebResponse httpResponse = httpRequest.GetResponse()) { return true; }
			}
			catch
			{
				return false;
			}
		}



		/// <summary>
		/// Wifiden cixaraq yeniden baglan.
		/// </summary>
		/// <param name="SSID">Istifadecinin hazirda qowulu oldugu wifinin adi(SSID).</param>
		public static void ReconnectToWifi(string SSID)
		{
			if(activeInterface != null)
			{
				/* Wi-Fi-ye avtomatik baglanmaq quwu qoyulmayibsa, awagidaki baglanma emri iwlemeyecek ve "Hazirda wifiye baglanmamisan..." mesaji spamlanacaq, quw qoydugumuz anda icra olunacaq awagidaki connect emri. */

				ProcessHelpers.StartProcess("netsh.exe", $"netsh interface set interface \"{activeInterface.Name}\" disabled ", out _);
				Thread.Sleep(50);
				ProcessHelpers.StartProcess("netsh.exe", $"netsh interface set interface \"{activeInterface.Name}\" enabled ", out _);

				ProcessHelpers.StartProcess("netsh.exe", $"wlan connect ssid={SSID}", out _); /* ProcessHelpers.StartProcess("netsh.exe", "wlan disconnect", out _); */

				Thread.Sleep(1000); /* WiFi-ye baglandiqdan sonra her ehtimala qarwi 1 deyqe gozle. (Test ucundur: hazirda, wifi-ye sonsuz reconnect etmeye caliwir, wifiye giren saniye internet baglantisi yoxlayir ve baglantinin olmagiyla qarwilawmir hemin saniye deye, tezeden reconnect etmeye caliwir, threadi 1 saniye gozlet wifiye baglandiqdan, yeni reconnect etdikden sonra ve gor problem hell olur?) */
			}
#if TEST
					TestHelpers.CustomizeConsole();
					Console.WriteLine("Kompyuterinizde hec bir network interfeysi aktiv deyil...";
#endif
		}



		/// <summary>
		/// Butun prosesler bu metodun icerisinde baw verir.
		/// </summary>
		/// <param name="cancelToken">Metodun iwin sonlandirmaq ucundur.</param>
		/// <param name="Interval">Hansi araliqlarla yoxlaniw aparilsin.</param>
		/// <param name="URL">Internetin olub-olmadigini yoxlamaq ucun hansi endpointe sorgu gonderilsin.</param>
		public static void CheckOverallStatus(CancellationToken cancelToken, double Interval, string URL)
		{
			while(!cancelToken.IsCancellationRequested)
			{
				string WifiSsid = GetConnectedWifiSsid();

				if(WifiSsid != null)
				{
#if TEST
					Console.WriteLine("Hazirda wifiye baglanmisan...");
#endif
					if(CheckForInternetConnection(URL) == false)
					{
#if TEST
                        Console.WriteLine("Internetin yoxdur...");
#endif

						ReconnectToWifi(WifiSsid);
					}
#if TEST
					else /* Internet varsa, her wey yaxwidir */
                    {

						Console.WriteLine("Internetin var...");
					}
#endif
				}
				else
				{
#if TEST
					Console.WriteLine("Hazirda wifiye baglanmamisan...");

					TestHelpers.CustomizeConsole();
#endif

					ReconnectToWifi(WifiSsid);
				}

				Thread.Sleep(TimeSpan.FromSeconds(Interval));
			}
		}
	}
}
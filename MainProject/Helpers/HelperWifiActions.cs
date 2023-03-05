// #define TEST

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace MainProject.Helpers
{
	public struct HelperWifiActions
	{
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

				HelperMethods.StartProcess("netsh.exe", "wlan show interfaces", out processOutputResult);

				if (processOutputResult != null) /* Umumiyyetle wireless network interfeysi/profili var mi hec? */
				{
					if (processOutputResult.IndexOf("SSID") > -1) /* WiFi-ye qowulmuwamsa SSID-nin indeksi '-1'-den boyuk olacaq */
					{
						SSID = processOutputResult.Substring(processOutputResult.IndexOf("SSID")); /* SSID-den etibaren bawlayan hisseni gotururem, goturduyum parca bu cur bawlayacaq: "SSID : ..." */
						SSID = SSID.Substring(SSID.IndexOf(":")); /* SSID-ni 'SSID :' iki noqteden etibaren yene parcalayiram. Belece IndexOf ilk qarwilawdigi : simvolundan bawlayan hisseni dondurecek, elimde ": WifiAdi ..." olacaq */
						SSID = SSID.Substring(2, SSID.IndexOf("\n")).Trim(); /* 2-ci indeksden bawla, yeni setirle qarwilawanadek olan hisseni gotur */

#if TEST
						HelperMethods.CustomizeConsole($"Reconnecter - {SSID}");
#endif

						return SSID;
					}
					else /* WiFi-ye qowulmamiwamsa */
					{
#if TEST
						HelperMethods.CustomizeConsole();
#endif
						return null;
					}
				}

				return null;
			}
			catch
			{
#if TEST
                HelperMethods.CustomizeConsole();
#endif

				return null;
			}
		}



		/// <summary>
		/// Istifadecinin internete cixiwi var?
		/// </summary>
		/// <param name="URL">(Optional) Internetin olub-olmadigini yoxlamaq meqsedile hansi url-e request gonderilsin?</param>
		/// <param name="TimeoutMs">(Optional) Gondereceyimiz request verdiyimiz zamandan uzun cekse request baw tutmayacaq.</param>
		/// <returns>Internet varsa TRUE dondurecek, eks halda FALSE dondururuk.</returns>
		public static bool CheckForInternetConnection(string URL, int TimeoutMs = 10000)
		{
			URL = new UriBuilder(Uri.UriSchemeHttp, URL).Uri.ToString(); /* Elimde olan endpoint adresi ip formasindadir deye, verdiyim ip esasini url formasina saliram: https://IP/ */

			try
			{
				HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(URL);
				httpRequest.KeepAlive = false;
				httpRequest.Timeout = TimeoutMs;
				httpRequest.Proxy = null;

				using (var httpResponse = httpRequest.GetResponse()) { return true; }
			}
			catch
			{
				return false;
			}
		}



		/// <summary>
		/// Wifiden cixib yeniden baglan.
		/// </summary>
		/// <param name="SSID">Istifadecinin hazirda qowulu oldugu wifinin adi(SSID).</param>
		public static void ReconnectToWifi(string SSID)
		{
			/* Wi-Fi-ye avtomatik baglanmaq quwu qoyulmayibsa, awagidaki baglanma emri iwlemeyecek ve "Hazirda wifiye baglanmamisan..." mesaji spamlanacaq, quw qoydugumuz anda icra olunacaq awagidaki connect emri. */

			// HelperMethods.StartProcess("netsh.exe", "wlan disconnect", out _);

			/*
				Network Interfeysi axtariwi lazimsiz interfeysleri aradan cixartmaq uzerinedir. Eger interfeys:
						> Loopback(esasen testing ucun iwledilir) ve ya Tunnel(tunelling, datanin network uzerinden, bawqa sozle bir networkden digerine tehlukesiz gonderilmeyi ucun iwledilen bir yoldur/metoddur, daha cox VPN-ler tetbiq edir) network deyilse,
						> Interfeys iwleyirse, yeni hazirda data paketi gondere ve ya qebul ede bilirse,
						> Hyper-V-ye aid interfeys deyilse
					:tapilan ilk interfeysi secirem.

				+ Ne vaxtsa iwlemese ya WMI-dan ya da "netsh wlan show interfaces" emrinden kes gotur interfeys adini.
			*/
			NetworkInterface activeInterface = NetworkInterface.GetAllNetworkInterfaces()?.FirstOrDefault(net =>
					net.NetworkInterfaceType != NetworkInterfaceType.Loopback && net.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
					net.OperationalStatus == OperationalStatus.Up &&
					net.Name.StartsWith("vEthernet") == false);

			if (activeInterface != null)
			{
				HelperMethods.StartProcess("netsh.exe", $"netsh interface set interface \"{activeInterface.Name}\" disabled ", out _);
				HelperMethods.StartProcess("netsh.exe", $"netsh interface set interface \"{activeInterface.Name}\" enabled ", out _);
				HelperMethods.StartProcess("netsh.exe", $"wlan connect ssid={SSID} name={SSID}", out _);

				Thread.Sleep(1000); /* WiFi-ye baglandiqdan sonra her ehtimala qarwi 1 deyqe gozle. (Test ucundur: hazirda, wifi-ye sonsuz reconnect etmeye caliwir, wifiye giren saniye internet baglantisi yoxluyur ve baglantinin olmagiyla qarwilawmir hemin saniye deye, tezeden reconnect etmeye caliwir, threadi 1 saniye gozlet wifiye baglandiqdan, yeni reconnect etdikden sonra ve gor problem hell olur?) */
			}
#if TEST
					HelperMethods.CustomizeConsole();
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
			while (!cancelToken.IsCancellationRequested)
			{
				string WifiSsid = GetConnectedWifiSsid();

				if (WifiSsid != null) /* istifadeci hazirda qowulub hansisa WiFi-ye? null deyilse demeli qowulub */
				{
#if TEST
					Console.WriteLine("Hazirda wifiye baglanmisan...");
#endif
					if (CheckForInternetConnection(URL: URL) == false) /* Internet yoxdursa, reconnect edirik */
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
				else /* Istifadeci hazirda hec bir wifi-ye baglanmayibsa */
				{
#if TEST
					Console.WriteLine("Hazirda wifiye baglanmamisan...");

					HelperMethods.CustomizeConsole();
#endif

					ReconnectToWifi(WifiSsid);
				}

				Thread.Sleep(TimeSpan.FromSeconds(Interval));
			}
		}
	}
}
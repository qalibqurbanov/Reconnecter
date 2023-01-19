// #define TEST

using System;
using System.Net;
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

				if (processOutputResult != null)
				{
					if (processOutputResult.IndexOf("SSID") > -1) /* WiFi-ye qowulmuwamsa SSID-nin indeksi '-1'-den boyuk olacaq */
					{
						SSID = processOutputResult.Substring(processOutputResult.IndexOf("SSID")); /* SSID-den etibaren bawlayan hisseni gotururem, goturduyum parca bu cur bawlayacaq: "SSID : ..." */
						SSID = SSID.Substring(SSID.IndexOf(":")); /* SSID-ni 'SSID :' iki noqteden etibaren yene parcalayiram. Belece IndexOf ilk qarwilawdigi : simvolundan bawlayan hisseni dondurecek, elimde ": WifiAdi ..." olacaq */
						SSID = SSID.Substring(2, SSID.IndexOf("\n")).Trim(); /* 2-ci indeksden bawla, yeni setirle qarwilawanadek olan hisseni gotur */

						HelperMethods.CustomizeConsole($"Reconnecter - {SSID}");

						return SSID;
					}
					else /* WiFi-ye qowulmamiwamsa */
					{
#if TEST
						HelperMethods.CustomizeConsole();
#endif
						SSID = null;

						return SSID;
					}
				}

				return SSID;
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
		/// <param name="timeoutMs">(Optional) Gondereceyimiz request verdiyimiz zamandan uzun cekse request baw tutmayacaq.</param>
		/// <param name="URL">(Optional) Internetin olub-olmadigini yoxlamaq meqsedile hansi url-e request gonderilsin?</param>
		/// <returns>Internet varsa TRUE dondurecek, eks halda FALSE dondururuk.</returns>
		public static bool CheckForInternetConnection(string URL, int timeoutMs = 10000)
		{
			URL = new UriBuilder(Uri.UriSchemeHttp, URL).Uri.ToString(); /* Elimde olan endpoint adresi ip formasindadir deye, verdiyim ip esasini url formasina saliram: https://IP/ */

			try
			{
				var httpRequest = (HttpWebRequest)WebRequest.Create(URL);
				httpRequest.KeepAlive = false;
				httpRequest.Timeout = timeoutMs;
				httpRequest.Proxy = null;

				using (var httpResponse = httpRequest.GetResponse()) return true;
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
		public static void ReconnectToWifi(string SSID) /* Wi-Fi-ye avtomatik baglanmaq quwu qoyulmayibsa, awagidaki baglanma emri iwlemeyecek ve "Hazirda wifiye baglanmamisan..." mesaji spamlanacaq, quw qoydugumuz anda icra olunacaq awagidaki emr  */
		{
			HelperMethods.StartProcess("netsh.exe", "wlan disconnect", out _);
			HelperMethods.StartProcess("netsh.exe", $"wlan connect ssid= {SSID} name={SSID}", out _);
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
				if (GetConnectedWifiSsid() != null) /* istifadeci hazirda qowulub hansisa WiFi-ye? null deyilse demeli qowulub */
				{
					Console.WriteLine("Hazirda wifiye baglanmisan...");

					if (CheckForInternetConnection(URL: URL) == true) /* Internet varsa, her wey yaxwidir */
					{
						Console.WriteLine("Internetin var...");
					}
					else /* Internet yoxdursa, reconnect edirik */
					{
						Console.WriteLine("Internetin yoxdur...");

						ReconnectToWifi(GetConnectedWifiSsid());
					}
				}
				else /* Istifadeci hazirda hec bir wifi-ye baglanmayibsa */
				{
					Console.WriteLine("Hazirda wifiye baglanmamisan...");

#if TEST
					HelperMethods.CustomizeConsole();
#endif

					ReconnectToWifi(GetConnectedWifiSsid());
				}

				Thread.Sleep(TimeSpan.FromSeconds(Interval));
			}
		}
	}
}
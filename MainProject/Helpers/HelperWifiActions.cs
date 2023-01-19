using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MainProject.Helpers
{
    public struct HelperWifiActions
	{
		/// <summary>
		/// Her defe CMD-de komanda icra edib daha sonra neticenin icerisinden mueyyen bir hisseni extract etmekdense, bir defe extract edib cacheleyirem daha sonra cacheden istifade edirem.
		/// </summary>
		private static string CachedSsid { get; set; } = null;



		/// <summary>
		/// Hazirda qowulu oldugumuz WiFi-nin adini(SSID) dondurur.
		/// </summary>
		/// <returns>Hazirda qowulu oldugumuz WiFi-nin adi(SSID), hec bir wifiye qowulu deyilikse NULL dondururuk.</returns>
		public static string GetConnectedWifiSsid()
		{
			try
			{
				if (CachedSsid == null)
				{
					string processOutputResult = string.Empty;
					HelperMethods.StartProcess("netsh.exe", "wlan show interfaces", out processOutputResult);

					if (processOutputResult != null)
					{
						string SSID = processOutputResult.Substring(processOutputResult.IndexOf("SSID")); /* SSID-den etibaren bawlayan hisseni gotururem, goturduyum parca bu cur bawlayacaq: "SSID : ..." */
						SSID = SSID.Substring(SSID.IndexOf(":")); /* SSID-ni 'SSID :' iki noqteden etibaren yene parcalayiram. Belece IndexOf ilk qarwilawdigi : simvolundan bawlayan hisseni dondurecek, elimde ": WifiAdi ..." olacaq */
						SSID = SSID.Substring(2, SSID.IndexOf("\n")).Trim(); /* 2-ci indeksden bawla, yeni setirle qarwilawanadek olan hisseni gotur */

						CachedSsid = SSID;
					}

					return CachedSsid;
				}
				else return CachedSsid;
			}
			catch
			{
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
		public static void ReconnectToWifi(string SSID)
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
		/// <param name="SSID">Istifadecinin hazirda qowulu oldugu wifinin adi(SSID).</param>
		public static void CheckOverallStatus(CancellationToken cancelToken, double Interval, string URL)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				if (HelperWifiActions.GetConnectedWifiSsid() != null) /* istifadeci hazirda qowulub hansisa WiFi-ye? null deyilse demeli qowulub */
				{
					Console.WriteLine("Hazirda wifiye baglanmisan...");

					if (HelperWifiActions.CheckForInternetConnection(URL: URL) == true) /* Internet varsa, her wey yaxwidir */
					{
						Console.WriteLine("Internetin var...");
					}
					else /* Internet yoxdursa, reconnect edirik */
					{
						Console.WriteLine("Internetin yoxdur...");

						ReconnectToWifi(CachedSsid);
					}
				}
				else /* Istifadeci hazirda hec bir wifi-ye baglanmayibsa */
				{
					Console.WriteLine("Hazirda wifiye baglanmamisan...");

					ReconnectToWifi(CachedSsid);
				}

				Thread.Sleep(TimeSpan.FromSeconds(Interval));
			}
		}
	}
}
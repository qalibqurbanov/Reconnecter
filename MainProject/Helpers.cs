using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MainProject
{
    public struct Helpers
	{
		#region ProcessStartInfo-nun propertyleri ile konsolu yox ede bilmesen bu win32 api metodunu iwlet.
		/// <summary>
		/// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo?view=net-7.0">ProcessStartInfo</see>-nun propertyleri ile Process.Start neticesinde yaranan pencereler yox olmursa, bu win32 api-ni iwletmeyi yoxla.
		/// </summary>
		/// <param name="hWnd">Yox edilecek pencere.</param>
		/// <param name="nCmdShow">Veziyyet: 0 - Hide, 1 - Normal, 2 - Minimized, 3 - Maximize. (Etrafli: <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow">ShowWindow function (winuser.h)</see>)</param>
		/// <returns></returns>
		// [DllImport("user32.dll")]
		// private static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);
		#endregion ProcessStartInfo-nun propertyleri ile konsolu yox ede bilmesen bu win32 api metodunu iwlet.



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
			using (Process process = new Process())
			{
				try
				{
					if (CachedSsid == null)
					{
						process.StartInfo.ErrorDialog = false;		/* Prosesle bagli error pencerelerini gosterme */
						process.StartInfo.CreateNoWindow = true;	/* Process.Start-in verdiyimiz emri icra etmesi ucun yaranacaq olan konsol penceresi gizledilsin/yaradilmasin */
						process.StartInfo.UseShellExecute = false;	/* CreateNoWindow - teleb edir */
						process.StartInfo.Password = null;          /* CreateNoWindow - teleb edir */
						process.StartInfo.UserName = null;          /* CreateNoWindow - teleb edir */
						process.StartInfo.RedirectStandardOutput = true; /* <- yazmasan oxuya bilmeyessen awagida CMD-den emrin outputunu oxumagi */

						process.StartInfo.FileName = "netsh.exe";
						process.StartInfo.Arguments = "wlan show interfaces";

						process.Start();

						string processOutputResult = process.StandardOutput.ReadToEnd();
						string SSID = processOutputResult.Substring(processOutputResult.IndexOf("SSID")); /* SSID-den etibaren bawlayan hisseni gotururem, goturduyum parca bu cur bawlayacaq: "SSID : ..." */
						SSID = SSID.Substring(SSID.IndexOf(":")); /* SSID-ni 'SSID :' iki noqteden etibaren yene parcalayiram. Belece IndexOf ilk qarwilawdigi : simvolundan bawlayan hisseni dondurecek, elimde ": WifiAdi ..." olacaq */
						SSID = SSID.Substring(2, SSID.IndexOf("\n")).Trim(); /* 2-ci indeksden bawla, yeni setirle qarwilawanadek olan hisseni gotur */

						CachedSsid = SSID;

						process.WaitForExit(); /* Process.Start komeyile yaratdigimiz prosesin(netsh.exe) sonlanmasini gozle */

						return CachedSsid;
					}
					else return CachedSsid;
				}
				catch (Exception error)
				{
					Console.WriteLine(error.Message);
					return null;
				}
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

				using (var httpResponse = httpRequest.GetResponse()) { return true; }
			}
			catch (Exception error)
			{
				Console.WriteLine(error.Message);
				return false;
			}
		}



        /// <summary>
        /// Verilen prosesi gosterdiyimiz parametrler/arqumentler ile icra edecek.
        /// </summary>
        /// <param name="FileName">Icra edilecek olan proqram</param>
        /// <param name="Arguments">Icra edilecek olan proqrama oturmek istediyimiz arqumentler</param>
        /// <returns>Proses bawlayib iwini ugurla bitirse TRUE, prosesin iwleyiwi zamani xeta baw verse false dondururuk.</returns>
        public static bool StartProcess(string FileName, string Arguments)
		{
			try
			{
                Process process = Process.Start
                (
                    new ProcessStartInfo
                    {
                        /* https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.createnowindow?view=net-6.0#remarks */

                        ErrorDialog = false,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        Password = null,
                        UserName = null,

                        /* Acacagim fayl: */
                        FileName = FileName,

                        /* Acacagim fayl hansi arqumentler ile acilsin: */
                        Arguments = Arguments,

                        /* Acacagim fayl acilanda arxa planda, umumen pencere gorsenmeyecek wekilde iwlesin: */
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                );

                process.WaitForExit(); /* Process.Start komeyile yaratdigimiz prosesin sonlanmasini gozle */
				process.Close();
                process.Dispose();

				// ShowWindow(process.MainWindowHandle, 0);

				return true;
			}
            catch (Exception error)
			{
				Console.WriteLine(error.Message);
				return false;
			}
		}



		/// <summary>
		/// Wifiden cixib yeniden baglan.
		/// </summary>
		/// <param name="SSID">Istifadecinin hazirda qowulu oldugu wifinin adi(SSID).</param>
		public static void ReconnectToWifi(string SSID)
        {
			Helpers.StartProcess("netsh.exe", "wlan disconnect");
			Helpers.StartProcess("netsh.exe", "wlan connect ssid=" + SSID + "name=" + SSID);
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
				if (Helpers.GetConnectedWifiSsid() != null) /* istifadeci hazirda qowulub hansisa WiFi-ye? null deyilse demeli qowulub */
				{
					Console.WriteLine("Hazirda wifiye baglanmisan...");

					if (Helpers.CheckForInternetConnection(URL: URL) == true) /* Internet varsa, her wey yaxwidir */
                    {
                        Console.WriteLine("Internetin var...");
                    }
					else /* Internet yoxdursa, reconnect edirik */
					{
						Console.WriteLine("Internetin yoxdur...");

						try
						{
							ReconnectToWifi(CachedSsid);
						}
						catch (Exception error)
						{
							Console.WriteLine(error.Message);
						}
					}
				}
				else /* Istifadeci hazirda hec bir wifi-ye baglanmayibsa */
                {
					Console.WriteLine("Hazirda wifiye baglanmamisan...");
				}

				Thread.Sleep(TimeSpan.FromSeconds(Interval));
			}
		}
	}
}
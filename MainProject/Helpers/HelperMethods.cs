﻿using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

namespace MainProject.Helpers
{
	public struct HelperMethods
	{
		/// <summary>
		/// Verilen prosesi gosterdiyimiz parametrler/arqumentler ile icra edecek.
		/// </summary>
		/// <param name="FileName">Icra edilecek olan proqram</param>
		/// <param name="Arguments">Icra edilecek olan proqrama oturmek istediyimiz arqumentler</param>
		/// <param name="processResult">Metodun daxilinde elde etdiyim konsol outputunu metoddan bayira gondermek ucundur.</param>
		/// <returns>Proses bawlayib iwini ugurla bitirse TRUE, prosesin iwleyiwi zamani xeta baw verse false dondururuk.</returns>
		public static bool StartProcess(string FileName, string Arguments, out string processResult)
		{
			try
			{
				Process process = Process.Start
				(
					new ProcessStartInfo
					{
						/* https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.createnowindow?view=net-6.0#remarks */

						RedirectStandardOutput = true, /* Konsolun outputu StandartOutput-a yazilsin (ki, ReadToEnd() deyib oxuya bilim konsolun outputunu) */

						ErrorDialog = false, /* Prosesle bagli error pencerelerini gosterme */
						CreateNoWindow = true, /* Process.Start-in verdiyimiz emri icra etmesi ucun yaranacaq olan konsol penceresi gizledilsin/yaradilmasin */
						UseShellExecute = false, /* CreateNoWindow - teleb edir */
						Password = null, /* CreateNoWindow - teleb edir */
						UserName = null, /* CreateNoWindow - teleb edir */

						/* Acacagim fayl: */
						FileName = FileName,

						/* Acacagim fayl hansi arqumentler ile acilsin: */
						Arguments = Arguments,

						/* Acacagim fayl acilanda arxa planda, umumen pencere gorsenmeyecek wekilde iwlesin: */
						WindowStyle = ProcessWindowStyle.Hidden
					}
				);

				processResult = process.StandardOutput.ReadToEnd();

				process.WaitForExit(); /* Process.Start komeyile yaratdigimiz prosesin sonlanmasini gozle */
				process.Close();
				process.Dispose();

				return true;
			}
			catch
			{
				processResult = null;

				return false;
			}
		}



		/// <summary>
		/// Hazirki User-in reyestr bolmesindeki Startup qeydlerinin yerlewdiyi Key-i (+ hemin key uzerinde her iwi gore bileceyimiz icaze ile) acir, bawqa sozle kecid edir.
		/// </summary>
		/// <returns>Reyestrda kecid etdiyimiz bolmeni dondurur.</returns>
		public static RegistryKey OpenStartupKey() => Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);



        /// <summary>
        /// Proqrami startup-a elave edir.
        /// </summary>
        /// <param name="KeyName">Yaradilacaq Key-in adi.</param>
		/// <param name="ApplicationPath">Yaradilacaq Key-in Value-su.</param>
        public static void AddToStartup(string KeyName, string ApplicationPath)
		{
			try
			{
				RegistryKey regKey = OpenStartupKey();
				regKey.SetValue
				(
					name: KeyName,
					value: ApplicationPath
				);
			}
			catch { }
		}



		/// <summary>
		/// Proqrami startup-dan silir.
		/// </summary>
		/// <param name="KeyName">Silinecek olan Key-in adi.</param>
		public static void RemoveFromStartup(string KeyName)
        {
			try
			{
                OpenStartupKey().DeleteValue(KeyName, false);

				Console.WriteLine("Silindi...");
			}
			catch { }
		}
	}
}
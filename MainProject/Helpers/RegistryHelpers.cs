using System;
using Microsoft.Win32;

namespace MainProject.Helpers
{
    /// <summary>
    /// Registry ile elaqeli funksionalliqlari saxlayir.
    /// </summary>
    public static class RegistryHelpers
    {
        /// <summary>
		/// Hazirki User-in reyestr bolmesindeki Startup qeydlerinin yerlewdiyi Key-i (+ hemin key uzerinde her iwi gore bileceyimiz icaze ile) acir, bawqa sozle kecid edir.
		/// </summary>
		/// <returns>Reyestrda kecid etdiyimiz bolmeni dondurur.</returns>
		public static RegistryKey OpenStartupKey()
        {
            return Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);
        }



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
                if(OpenStartupKey().GetValue(KeyName) != null)
                {
                    OpenStartupKey().DeleteValue(KeyName, false);

                    Console.WriteLine("Silindi...");
                }
            }
            catch { }
        }
    }
}
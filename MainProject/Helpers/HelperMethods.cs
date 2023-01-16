using System.Diagnostics;

namespace MainProject.Helpers
{
    public struct HelperMethods
    {
		/// <summary>
		/// Verilen prosesi gosterdiyimiz parametrler/arqumentler ile icra edecek.
		/// </summary>
		/// <param name="FileName">Icra edilecek olan proqram</param>
		/// <param name="Arguments">Icra edilecek olan proqrama oturmek istediyimiz arqumentler</param>
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
	}
}
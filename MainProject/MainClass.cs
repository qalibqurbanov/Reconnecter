// #define TEST

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using MainProject.Helpers;

namespace MainProject
{
	class MainClass
	{
		/* OS-in ve ya brauzerin DNS Cache-de, ISP-de ve ya Root Name Serverde URL-me qarwiliq IP her hansi bir sebebe gore tapilmaya biler, ona gore request etmek istediyim endpointin bir bawa IP-sini verirem URL olaraq: */
		private static string URL { get { return "140.82.121.4/qalibqurbanov/Reconnecter"; } }

		/* Reyestrda hansi adla Key yaradacam ve yaratdigim Key-in value-su ne olacaq? sadaladigim bu 2 elementin qarwiliqlari: */
		private static string ExecutablePath { get; } = Assembly.GetExecutingAssembly().Location;                                    // registry ucun - Key value
        private static string ExecutableName { get; } = Path.GetFileNameWithoutExtension(typeof(MainClass).Assembly.GetName().Name); // registry ucun - Key name

        /* Emeliyyati cancel etmek ucun lazimi deyiwenler: */
        private static readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource(); /* 'CancellationTokenSource' obyekti yaradiriq, hasiki CancellationToken-leri, dolayi yolla tasklari dayandiracaq  */
		private static readonly CancellationToken cancelToken = cancelTokenSource.Token; /* 'cancelTokenSource' ile elaqeli yeni bir 'CancellationToken' obyekti elde edirem 'Token' propertysinin komeyile */

		static void Main()
		{
			if (!ProcessHelpers.isAnotherInstanceWorking(ExecutableName))
			{
				Environment.Exit(0);
				return;
			}

#if TEST
			TestHelpers.CustomizeConsole();
#endif

            /* App-e artiq ehtiyacimiz yoxdursa '-del' arqumentiyle acib eyni anda hem reyestrdan celd sile hemde prosesi sonlandira bilerik. */
            string[] args = Environment.GetCommandLineArgs();
			if (args.Contains("-del") == true)
			{
				cancelTokenSource.Cancel();

                RegistryHelpers.RemoveFromStartup(ExecutableName);
                ProcessHelpers.StartProcess("taskkill.exe", $"/im {Process.GetCurrentProcess().ProcessName} /f /t", out _);

				Environment.Exit(0);
			}
			else
			{
                RegistryHelpers.AddToStartup(ExecutableName, ExecutablePath);

				WifiHelpers.CheckOverallStatus
				(
					cancelToken: cancelToken,
					Interval: 1.8d,
					URL: URL
				);
			}
		}
	}
}
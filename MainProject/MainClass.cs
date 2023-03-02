﻿// #define TEST

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Linq;
using MainProject.Helpers;
using System.Diagnostics;

namespace MainProject
{
	class MainClass
	{
		/* ISP-min dns serveri her hansi bir sebebe(cokme ve s.) gore iwlemeye biler, bu sebeble request etmek istediyim endpointin bir bawa ip adresini verirem url olaraq: */
		private static string URL { get { return "142.250.189.164"; } } /* google.com */

		/* Reyestrda hansi adla Key yaradacam ve yaratdigim Key-in value-su ne olacaq? sadaladigim bu 2 elementin qarwiliqlari: */
		private static string ExecutablePath { get; } = Assembly.GetExecutingAssembly().Location;                                    // KeyName
		private static string ExecutableName { get; } = Path.GetFileNameWithoutExtension(typeof(MainClass).Assembly.GetName().Name); // KeyValue

		/* Emeliyyati cancel etmek ucun lazimi deyiwenler: */
		private static readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource(); /* 'CancellationTokenSource' obyekti yaradiriq, hasiki CancellationToken-leri, dolayi yolla tasklari dayandiracaq  */
		private static readonly CancellationToken cancelToken = cancelTokenSource.Token; /* 'cancelTokenSource' ile elaqeli yeni bir 'CancellationToken' obyekti elde edirem 'Token' propertysinin komeyile */

		static void Main()
		{
			if (!HelperMethods.isAnotherInstanceWorking(ExecutableName))
			{
				Environment.Exit(0);
				return;
			}

#if TEST
			HelperMethods.CustomizeConsole();
#endif

			#region Argument: -del
			/* App-e artiq ehtiyacimiz yoxdursa '-del' arqumentiyle acib eyni anda hem reyestrdan celd sile hemde prosesi sonlandira bilerik. */
			string[] args = Environment.GetCommandLineArgs();
			if (args.Contains("-del") == true)
			{
                cancelTokenSource.Cancel();

                HelperMethods.RemoveFromStartup(ExecutableName);
                HelperMethods.StartProcess("taskkill.exe", $"/im {Process.GetCurrentProcess().ProcessName} /f /t", out _);

                Environment.Exit(0);
			}
			else
			{
				HelperMethods.AddToStartup(ExecutableName, ExecutablePath);

				HelperWifiActions.CheckOverallStatus
				(
					cancelToken: cancelToken,
					Interval: 1.5d,
					URL: URL
				);
			}
            #endregion Argument: -del
        }
    }
}
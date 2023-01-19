using System;
using System.IO;
using System.Reflection;
using System.Threading;
using MainProject.Helpers;
using Microsoft.Win32;
using System.Linq;

namespace MainProject
{
    class MainClass
	{
		/* ISP-min dns serveri her hansi bir sebebe(cokme ve s.) gore iwlemeye biler, bu sebeble request etmek istediyim endpointin bir bawa ip adresini verirem url olaraq */
		private static string URL { get { return "142.250.189.164"; } } /* google.com */

		/* Reyestrda hansi adla Key yaradacam ve yaratdigim Key-in value-su ne olacaq? sadaladigim bu 2 elementin qarwiliqlari: */
		private static string ExecutablePath { get; } = Assembly.GetExecutingAssembly().Location;									 // KeyName
        private static string ExecutableName { get; } = Path.GetFileNameWithoutExtension(typeof(MainClass).Assembly.GetName().Name); // KeyValue

        /* Hazirda cancel etmirem metodu, daha sonra ehtiyac olar deye yazmiwam. */
        private static readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource(); /* 'CancellationTokenSource' obyekti yaradiriq, hasiki CancellationToken-leri, dolayi yolla tasklari dayandiracaq  */
		private static readonly CancellationToken cancelToken = cancelTokenSource.Token; /* 'cancelTokenSource' ile elaqeli yeni bir 'CancellationToken' obyekti elde edirem 'Token' propertysinin komeyile */

		static void Main()
		{
			string[] args = Environment.GetCommandLineArgs();
			if(args.Contains("-del") == true)
			{
				HelperMethods.RemoveFromStartup(ExecutableName);

				return;
			}
			else
			{
                HelperMethods.AddToStartup(ExecutableName, ExecutablePath);

                HelperWifiActions.CheckOverallStatus
                (
                    cancelToken: cancelToken,
                    Interval: 1d,
                    URL: URL
                );
            }
		}
	}
}

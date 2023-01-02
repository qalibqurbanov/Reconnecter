using System.Threading;

namespace MainProject
{
    struct MainClass
	{
		/* ISP-min dns serveri kelle mayallaq gede biler, bu sebeble request etmek istediyim endpointin bir bawa ip adresini verirem url olaraq */
		private static string URL { get { return "142.250.189.164"; } } /* google.com */

		/* Hazirda cancel etmirem metodu, daha sonra ehtiyac olar deye yazmiwam. */
		private static CancellationTokenSource cancelTokenSource = new CancellationTokenSource(); /* 'CancellationTokenSource' obyekti yaradiriq, hasiki CancellationToken-leri, dolayi yolla tasklari dayandiracaq  */
		private static CancellationToken cancelToken = cancelTokenSource.Token; /* 'cancelTokenSource' ile elaqeli yeni bir 'CancellationToken' obyekti elde edirem 'Token' propertysinin komeyile */

		static void Main()
		{
			Helpers.CheckOverallStatus(cancelToken, 1d, URL);
		}
	}
}
using Xunit; 
using System.Linq;
using Xunit.Abstractions;
using MainProject.Helpers;
using System.Net.NetworkInformation;

namespace MainProject.UnitTests
{
    public class MiniTests
    {
        private readonly ITestOutputHelper _output;
        public MiniTests(ITestOutputHelper output) => this._output = output;

        [Fact]
        public void TEST_GetConnectedWifiSsid()
        {
            /* Arrange: */
            string SSID = string.Empty;

            /* Act: */
            SSID = WifiHelpers.GetConnectedWifiSsid();

            /* Print result: */
            _output.WriteLine(SSID);

            /* Assert: */
            Assert.NotNull(SSID);
        }

        [Fact]
        public void TEST_GetNetworkInterface()
        {
            /* Arrange: */
            NetworkInterface activeInterface = default!;

            /* Act: */
            activeInterface = NetworkInterface.GetAllNetworkInterfaces()?.FirstOrDefault
            (net =>
                net.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                net.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                net.OperationalStatus == OperationalStatus.Up &&
                net.Name.StartsWith("vEthernet") == false
            )!;

            /* Print result: */
            if(activeInterface != null)
                _output.WriteLine($"[{activeInterface.Name}] : {activeInterface.Description}");

            /* Assert: */
            Assert.NotNull(activeInterface);
        }
    }
}
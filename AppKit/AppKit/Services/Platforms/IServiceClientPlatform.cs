namespace AdMaiora.AppKit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum NetworkConnection
    {
        NotConnected,
        WifiConnection,
        MobileConnection,
        Others
    }

    public interface IServiceClientPlatform
    {
        NetworkConnection GetNetworkConnection();

        bool IsNetworkAvailable();
    }
}

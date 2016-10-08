namespace AdMaiora.AppKit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;

    using SystemConfiguration;

    public class ServiceClientPlatformiOS : IServiceClientPlatform
    {
        public NetworkConnection GetNetworkConnection()
        {
            NetworkReachability nr = new NetworkReachability(null, IPAddress.Parse("8.8.8.8"));
            NetworkReachabilityFlags nf;
            bool success = nr.TryGetFlags(out nf);
            if (!success)
                return NetworkConnection.NotConnected;

            bool isReachable = (nf & NetworkReachabilityFlags.Reachable) != 0;
            bool needsConnection = (nf & NetworkReachabilityFlags.ConnectionRequired) != 0;
            if (!isReachable || needsConnection)
                return NetworkConnection.NotConnected;

            if ((nf & NetworkReachabilityFlags.IsWWAN) != 0)
                return NetworkConnection.MobileConnection;

            if ((nf & NetworkReachabilityFlags.IsDirect) != 0)
                return NetworkConnection.MobileConnection;

            return NetworkConnection.Others;

        }

        public bool IsNetworkAvailable()
        {
            NetworkConnection nc = GetNetworkConnection();
            return nc != NetworkConnection.NotConnected;
        }
    }
}
namespace AdMaiora.AppKit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Android.App;
    using Android.Content;
    using Android.Net;
    using Android.Telephony;

    public class ServiceClientPlatformAndroid : IServiceClientPlatform
    {
        public NetworkConnection GetNetworkConnection()
        {
            ConnectivityManager cm = (ConnectivityManager)Application.Context.GetSystemService(Application.ConnectivityService);

            NetworkInfo ni = cm.ActiveNetworkInfo;
            if (ni == null || !ni.IsConnected)
                return NetworkConnection.NotConnected;

            if (ni.Type == ConnectivityType.Wifi)
                return NetworkConnection.WifiConnection;

            if (ni.Type == ConnectivityType.Mobile)
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
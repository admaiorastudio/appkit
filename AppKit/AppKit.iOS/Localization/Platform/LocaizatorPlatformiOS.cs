namespace AdMaiora.AppKit.Localization
{
    using System;
    using System.Reflection;

    using Foundation;

    public class LocaizatorPlatformiOS : ILocalizatorPlatform
    {
        public Assembly[] GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public string GetDeviceCulture()
        {
            return NSLocale.PreferredLanguages[0];
        }
    }
}
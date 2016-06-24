namespace AdMaiora.AppKit.Localization
{
    using System;
    using System.Reflection;

    using Java.Util;

    public class LocalizatorPlatformAndroid : ILocalizatorPlatform
    {
        public Assembly[] GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public string GetDeviceCulture()
        {
            return Locale.Default.ToString().Replace('_', '-');
        }
    }
}
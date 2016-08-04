namespace AdMaiora.AppKit.Localization
{
    using System;
    using System.Reflection;
    using System.Globalization;

    using AdMaiora.AppKit.IO;

    using Foundation;

    public class LocalizatorPlatformiOS : ILocalizatorPlatform
    {
        public FileSystem GetFileSystem()
        {
            return new FileSystem(new FileSystemPlatformiOS());
        }

        public string GetDeviceUICulture()
        {
            return NSLocale.PreferredLanguages[0];            
        }

        public CultureInfo[] GetInstalledCultures()
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures);
        }

        public Assembly[] GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}
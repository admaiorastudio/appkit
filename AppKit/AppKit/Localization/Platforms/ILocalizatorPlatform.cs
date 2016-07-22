namespace AdMaiora.AppKit.Localization
{
    using System;
    using System.Reflection;
    using System.Globalization;

    using AdMaiora.AppKit.IO;

    public interface ILocalizatorPlatform
    {
        FileSystem GetFileSystem();

        string GetDeviceUICulture();

        CultureInfo[] GetInstalledCultures();

        Assembly[] GetAssemblies();
    }
}

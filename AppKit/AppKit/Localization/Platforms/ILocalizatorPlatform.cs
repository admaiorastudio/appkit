namespace AdMaiora.AppKit.Localization
{
    using System.Reflection;

    public interface ILocalizatorPlatform
    {
        string GetDeviceCulture();
        Assembly[] GetAssemblies();
    }
}

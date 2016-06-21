namespace AdMaiora.AppKit.Utils
{
    using System;

    public class LoggerPlatformAndroid : ILoggerPlatform
    {
        public void ConsoleWriteLine(string tag, string message)
        {
            Android.Util.Log.Debug(tag, message);
        }
    }
}
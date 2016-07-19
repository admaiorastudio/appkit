namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.IO;

    using AdMaiora.AppKit.IO;

    public class LoggerPlatformAndroid : ILoggerPlatform
    {
        public FileSystem GetFileSystem()
        {
            return new FileSystem(new FileSystemPlatformAndroid()); 
        }

        public void ConsoleWriteLine(string tag, string message)
        {
            Android.Util.Log.Debug(tag, message);
        }
    }
}
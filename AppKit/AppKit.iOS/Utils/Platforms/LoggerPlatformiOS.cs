namespace AdMaiora.AppKit.Utils
{
    using System;

    public class LoggerPlatformiOS : ILoggerPlatform
    {
        public void ConsoleWriteLine(string tag, string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
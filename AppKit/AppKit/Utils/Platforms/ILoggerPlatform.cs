namespace AdMaiora.AppKit.Utils
{
    using System;

    public interface ILoggerPlatform
    {
        void ConsoleWriteLine(string tag, string message);
    }
}

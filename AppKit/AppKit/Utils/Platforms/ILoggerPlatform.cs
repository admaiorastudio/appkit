namespace AdMaiora.AppKit.Utils
{
    using System;

    using AdMaiora.AppKit.IO;

    public interface ILoggerPlatform
    {
        FileSystem GetFileSystem();

        void ConsoleWriteLine(string tag, string message);
    }
}

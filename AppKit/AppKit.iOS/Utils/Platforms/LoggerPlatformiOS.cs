namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.IO;

    using AdMaiora.AppKit.IO;

    public class LoggerPlatformiOS : ILoggerPlatform
    {
        public FileSystem GetFileSystem()
        {
            return new FileSystem(new FileSystemPlatformiOS()); 
        }

        public void ConsoleWriteLine(string tag, string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
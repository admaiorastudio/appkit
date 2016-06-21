namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.IO;
    using System.Text;

    using AdMaiora.AppKit.IO;

    public class Logger
    {
        #region Constants and Fields

        public static string LogTag = String.Empty;
        public static FileUri LogPath = null;

        public static bool EchoInConsole = false;

        // Max log file size in Byte
        private static ulong LogMaxSize = 4 * 1024 * 1024;

        private string _locker = "_lock_";

        private ILoggerPlatform _loggerPlatform;
        private IFileSystemPlatform _fileSystemPlatform;

        #endregion

        #region Construcor

        public Logger(IFileSystemPlatform fileSystemPlatform, ILoggerPlatform loggerPlatform)
        {
            _fileSystemPlatform = fileSystemPlatform;
            _loggerPlatform = loggerPlatform;
        }

        #endregion

        #region Public Methods

        public void Info(string message, params object[] prms)
        {
            Write("INFO", String.Format(message, prms), null);
        }

        public void Info(string message)
        {
            Write("INFO", message, null);
        }

        public void Error(string message, params object[] prms)
        {
            Write("Error", String.Format(message, prms), null);
        }

        public void Error(string error, Exception ex = null)
        {
            Write("ERROR", error, ex);
        }

        private void Write(string tag, string message, Exception exception = null)
        {

            lock (_locker)
            {
                if (Logger.LogPath == null)
                    throw new InvalidOperationException("Unable to log. Define a log path first!");

                FileUri logFileUri = Logger.LogPath;
                if (_fileSystemPlatform.FileExists(logFileUri))
                {
                    if (_fileSystemPlatform.GetFileSize(logFileUri) > LogMaxSize)
                        _fileSystemPlatform.DeleteFile(logFileUri);
                }

                using (Stream stream = _fileSystemPlatform.OpenFile(logFileUri, UniversalFileMode.Append, UniversalFileAccess.Write, UniversalFileShare.None))
                {
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(String.Format("[{0:dd/MM/yy HH:mm:ss} - {1}]: {2}", DateTime.Now, tag, message));

                        Exception ex = exception;
                        if (ex != null)
                        {
                            sb.AppendLine(ex.Message);
                            sb.AppendLine();
                            sb.AppendLine(ex.StackTrace);
                            sb.AppendLine();
                        }

                        sw.WriteLine(sb.ToString());

#if DEBUG
                        if(Logger.EchoInConsole)
                            _loggerPlatform.ConsoleWriteLine(tag, message);
#endif
                    }
                }
            }
        }

        #endregion
    }
}

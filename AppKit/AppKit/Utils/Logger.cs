namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.IO;
    using System.Text;

    using AdMaiora.AppKit.IO;

    public class Logger
    {
        #region Constants and Fields
                       
        private ILoggerPlatform _loggerPlatform;

        private FileSystem _fileSystem;

        private FileUri _logUri;
        private string _logTag;

        // Max log file size in Byte
        private ulong _maxLogSize;

        private string _locker = "_lock_";

        #endregion

        #region Constructors

        public Logger(ILoggerPlatform loggerPlatform)
        {
            _loggerPlatform = loggerPlatform;
            _fileSystem = loggerPlatform.GetFileSystem();

            _logTag = "APPKIT";

            // Default is 4 mb
            _maxLogSize = 4 * 1024 * 1024;
        }

        #endregion

        #region Properties

        public FileUri LogUri
        {
            get
            {
                if (_logUri == null)
                    throw new InvalidOperationException("You must set a valid log file uri.");

                return _logUri;
            }
            set
            {
                _logUri = value;
            }
        }

        public string LogTag
        {
            get
            {
                return _logTag;
            }
            set
            {
                _logTag = value;
            }

        }

        public ulong MaxLogsize
        {
            get
            {
                return _maxLogSize;
            }
            set
            {
                ulong minSize = (1 * 1024 * 1024) / 2;
                _maxLogSize = Math.Max(minSize, value);
            }
        }

        public bool EchoInConsole
        {
            get;
            set;
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
                FileUri logFileUri = this.LogUri;
                if (_fileSystem.FileExists(logFileUri))
                {
                    if (_fileSystem.GetFileSize(logFileUri) > this.MaxLogsize)
                        _fileSystem.DeleteFile(logFileUri);
                }

                using (Stream stream = _fileSystem.OpenFile(logFileUri, UniversalFileMode.Append, UniversalFileAccess.Write, UniversalFileShare.None))
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
                        if(this.EchoInConsole)
                            _loggerPlatform.ConsoleWriteLine(tag, message);
#endif
                    }
                }
            }
        }

        #endregion
    }
}

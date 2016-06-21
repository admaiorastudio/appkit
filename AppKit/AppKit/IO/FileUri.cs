namespace AdMaiora.AppKit.IO
{
    using System;

    public enum StorageLocation
    {
        Bundle,
        Internal,
        External
    }

    public class FileUri
    {
        #region Constants and Fields

        protected string _uri;

        protected string _root;

        protected StorageLocation _location;

        protected string _absolutePath;
        protected string _relativePath;

        private IFileUriPlatform _fileSystem;

        #endregion

        #region Constructors and Destructors

        public FileUri(string uri)
        {
            _uri = uri;
            if (_uri == "/")
                _uri = String.Empty;

            if (String.IsNullOrWhiteSpace(_uri)
                || !_uri.Contains("://"))
            {
                throw new InvalidOperationException(
                    "The URI you provide is invalid. Please provide an uri in this form 'location://relative/path/to/your/folder/or/file'");
            }

            string[] locations = Enum.GetNames(typeof(StorageLocation));
            bool hasValidLocation = false;
            foreach (string location in locations)
            {
                _root = String.Concat(location, "://").ToLower();

                if (uri.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
                {
                    hasValidLocation = true;

                    _location =
                        (StorageLocation)Enum.Parse(typeof(StorageLocation), location);

                    break;
                }
            }

            if (!hasValidLocation)
                throw new InvalidOperationException("The URI you provide is invalid.");

            _relativePath = _uri.Substring(_root.Length, _uri.Length - _root.Length);
            _absolutePath = _fileSystem.GetAbsolutePath(Location, _relativePath);
        }

        public FileUri(string path, StorageLocation location)
            : this(String.Concat(location.ToString().ToLower(), "://", path))
        {

        }

        #endregion

        #region Properties

        public string Uri
        {
            get
            {
                return _uri;
            }
        }

        public string AbsolutePath
        {
            get
            {
                return _absolutePath;
            }
        }

        public string RelativePath
        {
            get
            {
                return _relativePath;
            }
        }

        public StorageLocation Location
        {
            get
            {
                return _location;
            }
        }

        #endregion
    }
}

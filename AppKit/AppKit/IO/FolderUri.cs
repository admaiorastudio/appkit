namespace AdMaiora.AppKit.IO
{
    using System;
    using System.IO;

    public class FolderUri : FileUri
    {
        #region Constructors and Destructors

        public FolderUri(string uri)
            : base(StripFileName(uri))
        {
        }

        public FolderUri(string path, StorageLocation location)
            : this(String.Concat(location.ToString().ToLower(), "://", path))
        {
        }

        #endregion

        #region Methods

        private static string StripFileName(string uri)
        {
            string fileExtension = Path.GetExtension(uri);
            if (String.IsNullOrWhiteSpace(fileExtension))
                return uri;

            string fileName = Path.GetFileName(uri);
            if (!String.IsNullOrWhiteSpace(fileName))
                uri = uri.Replace(fileName, String.Empty);

            return uri;
        }

        #endregion
    }
}

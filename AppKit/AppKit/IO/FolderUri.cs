namespace AdMaiora.AppKit.IO
{
    using System;
    using System.IO;

    public class FolderUri : FileUri
    {
        #region Constructors and Destructors

        internal FolderUri(IFileSystemPlatform fileSystemPlatform, string uri)
            : base(fileSystemPlatform, StripFileName(uri))
        {
        }

        internal FolderUri(IFileSystemPlatform fileSystemPlatform, string path, StorageLocation location)
            : this(fileSystemPlatform, String.Concat(location.ToString().ToLower(), "://", path))
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

namespace AdMaiora.AppKit.IO
{
    using System;
    using System.IO;

    using Foundation;

    public class FileUriPlatformiOS : IFileUriPlatform
    {
        public string GetAbsolutePath(StorageLocation location, string path)
        {
            switch (location)
            {
                case StorageLocation.Bundle:
                    return NSBundle.MainBundle.PathForResource(path.Substring(0, path.Length - 4), Path.GetExtension(path));
                case StorageLocation.Internal:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), path);
                case StorageLocation.External:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), path);
            }

            return null;
        }
    }
}
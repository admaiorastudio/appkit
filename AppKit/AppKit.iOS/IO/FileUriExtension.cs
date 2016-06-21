namespace AdMaiora.AppKit.IO
{
    using System;

    using Foundation;

    public static class FileUriExtensions
    {
        public static NSUrl ToNSUrl(this FileUri uri)
        {
            switch (uri.Location)
            {
                case StorageLocation.Bundle:
                case StorageLocation.Internal:
                case StorageLocation.External:
                    return NSUrl.FromFilename(uri.AbsolutePath);

                default:
                    throw new InvalidOperationException("Invalid FileUri type");
            }
        }
    }
}
namespace AdMaiora.AppKit.IO
{
    using System;

    public static class FileUriExtensions
    {
        public static Android.Net.Uri ToUri(this FileUri uri)
        {
            switch (uri.Location)
            {
                case StorageLocation.Bundle:
                    return Android.Net.Uri.Parse(uri.AbsolutePath.Replace("bundle://", "file:///android_asset/"));

                case StorageLocation.Internal:
                    return Android.Net.Uri.FromFile(new Java.IO.File(uri.AbsolutePath));

                case StorageLocation.External:
                    return Android.Net.Uri.FromFile(new Java.IO.File(uri.AbsolutePath));

                default:
                    throw new InvalidOperationException("Invalid FileUri type");
            }

        }
    }
}
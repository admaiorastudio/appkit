namespace AdMaiora.AppKit.IO
{
    using System.IO;

    public class FileUriPlatformAndroid : IFileUriPlatform
    {
        string IFileUriPlatform.GetAbsolutePath(StorageLocation location, string path)
        {
            switch (location)
            {
                case StorageLocation.Bundle:
                    return Path.Combine(Directory.GetCurrentDirectory(), path);
                case StorageLocation.Internal:
                    return Path.Combine(Android.App.Application.Context.FilesDir.Path, path);
                case StorageLocation.External:
                    return Path.Combine(Android.App.Application.Context.GetExternalFilesDir(null).Path, path);
            }

            return null;
        }
    }
}
namespace AdMaiora.AppKit.IO
{
    using System;

    public interface IFileUriPlatform
    {
        string GetAbsolutePath(StorageLocation location, string path);
    }
}

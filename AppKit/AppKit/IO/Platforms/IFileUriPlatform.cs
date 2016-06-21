namespace AdMaiora.AppKit.IO
{
    public interface IFileUriPlatform
    {
        string GetAbsolutePath(StorageLocation location, string path);
    }
}

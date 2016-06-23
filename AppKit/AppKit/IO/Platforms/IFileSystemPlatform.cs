namespace AdMaiora.AppKit.IO
{
    using System.IO;

    public interface IFileSystemPlatform
    {
        ulong GetAvailableDiskSpace(FolderUri uri);

        ulong GetFileSize(FileUri uri);

        bool FolderExists(FolderUri uri);

        void CreateFolder(FolderUri uri);

        void DeleteFolder(FolderUri uri);

        bool FileExists(FileUri uri);

        void CopyFile(FileUri sourceUri, FileUri destinationUri, bool overwrite);

        void MoveFile(FileUri sourceUri, FileUri destinationUri);

        void DeleteFile(FileUri uri);

        Stream OpenFile(FileUri uri, UniversalFileMode mode, UniversalFileAccess access, UniversalFileShare share);

        string GetAbsolutePath(StorageLocation location, string path);
    }
}

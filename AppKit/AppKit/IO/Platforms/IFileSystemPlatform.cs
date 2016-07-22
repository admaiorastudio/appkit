namespace AdMaiora.AppKit.IO
{
    using System;
    using System.IO;

    public interface IFileSystemPlatform
    {
        ulong GetAvailableDiskSpace(FolderUri uri);

        bool FolderExists(FolderUri uri);

        void CreateFolder(FolderUri uri);

        void DeleteFolder(FolderUri uri);

        string[] GetFolderFiles(FolderUri uri, string searchPattern, bool recursive);

        bool FileExists(FileUri uri);

        void CopyFile(FileUri sourceUri, FileUri destinationUri, bool overwrite);

        void MoveFile(FileUri sourceUri, FileUri destinationUri);

        void DeleteFile(FileUri uri);

        Stream OpenFile(FileUri uri, UniversalFileMode mode, UniversalFileAccess access, UniversalFileShare share);

        ulong GetFileSize(FileUri uri);

        UniversalFileInfo GetFileInfo(FileUri uri);

        string GetAbsolutePath(StorageLocation location, string path);
    }
}

namespace AdMaiora.AppKit.IO
{
    using System.IO;

    using Foundation;

    public class FileSystemPlatformiOS : IFileSystemPlatform
    {
        public void CopyFile(FileUri sourceUri, FileUri destinationUri, bool overwrite)
        {
            File.Copy(sourceUri.AbsolutePath, destinationUri.AbsolutePath, overwrite);
        }

        public void CreateFolder(FolderUri uri)
        {
            Directory.CreateDirectory(uri.AbsolutePath);
        }

        public void DeleteFile(FileUri uri)
        {
            File.Delete(uri.AbsolutePath);
        }

        public void DeleteFolder(FolderUri uri)
        {
            Directory.Delete(uri.AbsolutePath, true);
        }

        public bool FileExists(FileUri uri)
        {
            return File.Exists(uri.AbsolutePath);
        }

        public bool FolderExists(FolderUri uri)
        {
            return Directory.Exists(uri.AbsolutePath);
        }

        public ulong GetAvailableDiskSpace(FolderUri uri)
        {
            ulong totalFreeSpace = 0;

            // Use below as suggested by Miguel de Icaza :)
            NSFileSystemAttributes values = NSFileManager.DefaultManager.GetFileSystemAttributes(uri.AbsolutePath);
            totalFreeSpace = values.FreeSize;

            return totalFreeSpace;
        }

        public ulong GetFileSize(FileUri uri)
        {
            if (FileExists(uri))
            {
                FileInfo fi = new FileInfo(uri.AbsolutePath);
                return (ulong)fi.Length;
            }

            return 0;
        }

        public void MoveFile(FileUri sourceUri, FileUri destinationUri)
        {
            File.Move(sourceUri.AbsolutePath, destinationUri.AbsolutePath);
        }

        public Stream OpenFile(FileUri uri, UniversalFileMode mode, UniversalFileAccess access, UniversalFileShare share)
        {
            return File.Open(uri.AbsolutePath, (FileMode)mode, (FileAccess)access, (FileShare)share);
        }
    }
}
namespace AdMaiora.AppKit.IO
{
    using System;
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
            if (!FileExists(uri))
                return 0;
            
            FileInfo fi = new FileInfo(uri.AbsolutePath);
            return (ulong)fi.Length;
        }

        public void MoveFile(FileUri sourceUri, FileUri destinationUri)
        {
            File.Move(sourceUri.AbsolutePath, destinationUri.AbsolutePath);
        }

        public Stream OpenFile(FileUri uri, UniversalFileMode mode, UniversalFileAccess access, UniversalFileShare share)
        {
            return File.Open(uri.AbsolutePath, (FileMode)mode, (FileAccess)access, (FileShare)share);
        }

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

        public UniversalFileInfo GetFileInfo(FileUri uri)
        {
            if (!FileExists(uri))
                throw new InvalidOperationException("File doesn't exists.");

            FileInfo fi = new FileInfo(uri.AbsolutePath);
            return new UniversalFileInfo
            {
                CreationTime = fi.CreationTime,
                LastAccessTime = fi.LastAccessTime,
                LastWriteTime = fi.LastWriteTime,
                Length = (ulong)fi.Length
            };
        }

        public string[] GetFolderFiles(FolderUri uri, string searchPattern, bool recursive)
        {
            return 
                Directory.GetFiles(uri.AbsolutePath, searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }
    }
}
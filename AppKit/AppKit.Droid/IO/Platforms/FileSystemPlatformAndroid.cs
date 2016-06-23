namespace AdMaiora.AppKit.IO
{
    using System;
    using System.IO;
    using System.Linq;

    using Android.App;
    using Android.OS;

    public class FileSystemPlatformAndroid : IFileSystemPlatform
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
            switch (uri.Location)
            {
                case StorageLocation.Bundle:

					try
					{
						using(var stream = new StreamReader(uri.AbsolutePath))
						{	
							if(stream == null)
								return false;

							return true;
						}
					}
					catch
					{
						return false;
					}

                case StorageLocation.Internal:
                    return File.Exists(uri.AbsolutePath);
                case StorageLocation.External:
                    return File.Exists(uri.AbsolutePath);
                default:
                    return false;
            }
        }

        public bool FolderExists(FolderUri uri)
        {
            return Directory.Exists(uri.AbsolutePath);
        }

        public ulong GetAvailableDiskSpace(FolderUri uri)
        {
            try
            {
                Java.Lang.Process proc =
                    Java.Lang.Runtime.GetRuntime().Exec(String.Format("df {0}", uri.AbsolutePath));

                proc.WaitFor();

                var resi = proc.InputStream;
                var rdr = new StreamReader(resi);
                string str = rdr.ReadToEnd();

                string[] lines = str.Split('\n');
                if (lines.Length < 2)
                    throw new InvalidOperationException("Unable to get size from shell.");

                string[] entries = lines[1]
                .Split(' ')
                    .Where(e => !String.IsNullOrWhiteSpace(e))
                        .ToArray();

                string entry = entries[3];

                ulong value = (ulong)Int32.Parse(entry.Substring(0, entry.Length - 1));
                string unit = entry.Substring(entry.Length - 1, 1);

                switch (unit)
                {
                    // Value is in bytes
                    case "B":
                        return value;

                    // Value is in Kbytes
                    case "K":
                        return value * 1024;

                    // Value is in Mbytes
                    case "M":
                        return value * 1024 * 1024;

                    // Value is in Gbytes
                    case "G":
                        return value * 1024 * 1024 * 1024;

                    default:
                        throw new InvalidOperationException("Unknown size unit.");
                }

            }
            catch (Exception ex)
            {
                StatFs stats = new StatFs(uri.AbsolutePath);
                return (ulong)(stats.AvailableBlocks * stats.BlockSize);
            }
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
            switch (uri.Location)
            {
                case StorageLocation.Bundle:
                    return Application.Context.Assets.Open(uri.RelativePath);
                case StorageLocation.Internal:
                    return File.Open(uri.AbsolutePath, (FileMode)mode, (FileAccess)access, (FileShare)share);
                case StorageLocation.External:
                    return File.Open(uri.AbsolutePath, (FileMode)mode, (FileAccess)access, (FileShare)share);
                default:
                    return null;
            }
        }

        public string GetAbsolutePath(StorageLocation location, string path)
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
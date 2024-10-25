#if UNITY_EDITOR
using System;
using System.IO;
using System.Runtime.InteropServices;
using Mapify.Editor.Native;

namespace Mapify.Editor.Utils
{
    public static class EditorFileUtil
    {
        private static readonly string[] LINUX_TRASH_PATHS = { "~/.local/share/Trash/files", "~/.Trash/files" };

        public static void MoveToTrash(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            bool isDirectory = (attr & FileAttributes.Directory) == FileAttributes.Directory;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Shell32.ForceMoveToRecycleBin(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string destFileName = Path.Combine(GetLinuxTrashPath(), Path.GetFileName(path));
                if (Directory.Exists(destFileName))
                    destFileName += $"-{Path.GetRandomFileName()}";
                if (isDirectory)
                    Directory.Move(path, destFileName);
                else
                    File.Move(path, destFileName);
            }
            else
            {
                throw new InvalidOperationException("Unsupported OS!");
            }
        }

        private static string GetLinuxTrashPath()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                throw new InvalidOperationException("Cannot get Linux trash path on non-Linux operating systems!");

            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            foreach (string trashPath in LINUX_TRASH_PATHS)
            {
                string fullTrashPath = trashPath.Replace("~", homeDir);
                if (Directory.Exists(fullTrashPath))
                    return fullTrashPath;
            }

            throw new DirectoryNotFoundException("Failed to find trash directory!");
        }
    }
}
#endif

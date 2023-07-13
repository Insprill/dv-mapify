#if UNITY_EDITOR
using System;
using System.Runtime.InteropServices;

namespace Mapify.Editor.Native
{
    /// <summary>
    ///     Taken and modified from https://stackoverflow.com/a/3282481/12156745
    /// </summary>
    public static class Shell32
    {
        /// <summary>
        ///     Possible flags for the SHFileOperation method.
        /// </summary>
        [Flags]
        private enum FileOperations : ushort
        {
            /// <summary>
            ///     Do not show a dialog during the process
            /// </summary>
            FOF_SILENT = 0x0004,
            /// <summary>
            ///     Do not ask the user to confirm selection
            /// </summary>
            FOF_NOCONFIRMATION = 0x0010,
            /// <summary>
            ///     Delete the file to the recycle bin.  (Required flag to send a file to the bin
            /// </summary>
            FOF_ALLOWUNDO = 0x0040,
            /// <summary>
            ///     Suppress errors, if any occur during the process.
            /// </summary>
            FOF_NOERRORUI = 0x0400,
            /// <summary>
            ///     Warn if files are too big to fit in the recycle bin and will need
            ///     to be deleted completely.
            /// </summary>
            FOF_WANTNUKEWARNING = 0x4000
        }

        /// <summary>
        ///     File Operation Function Type for SHFileOperation
        /// </summary>
        private enum FileOperationType : uint
        {
            /// <summary>
            ///     Delete (or recycle) the objects
            /// </summary>
            FO_DELETE = 0x0003
        }

        /// <summary>
        ///     SHFILEOPSTRUCT for SHFileOperation from COM
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEOPSTRUCT
        {
            public readonly IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            public readonly string pTo;
            public FileOperations fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public readonly bool fAnyOperationsAborted;
            public readonly IntPtr hNameMappings;
            public readonly string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        /// <summary>
        ///     Send file to recycle bin
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        /// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
        private static bool Send(string path, FileOperations flags)
        {
            try
            {
                SHFILEOPSTRUCT fs = new SHFILEOPSTRUCT {
                    wFunc = FileOperationType.FO_DELETE,
                    pFrom = path + '\0' + '\0',
                    fFlags = FileOperations.FOF_ALLOWUNDO | flags
                };
                SHFileOperation(ref fs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Send file to recycle bin. Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool MoveToRecycleBin(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                throw new InvalidOperationException("Cannot call Windows-native method on Linux!");
            return Send(path, FileOperations.FOF_NOCONFIRMATION | FileOperations.FOF_WANTNUKEWARNING);
        }

        /// <summary>
        ///     Send file silently to recycle bin. Suppress dialog, Suppress errors, delete if too large.
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool ForceMoveToRecycleBin(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                throw new InvalidOperationException("Cannot call Windows-native method on Linux!");
            return Send(path, FileOperations.FOF_NOCONFIRMATION | FileOperations.FOF_NOERRORUI | FileOperations.FOF_SILENT);
        }
    }
}
#endif

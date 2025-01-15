﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport("shell32")]
        internal static extern IntPtr SHAppBarMessage(AppBarMessages dwMessage, ref APPBARDATA pData);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern bool SHObjectProperties(IntPtr hwnd, uint shopObjectType, string pszObjectName, string pszPropertyPage);

        [DllImport("shell32.dll", SetLastError = true, EntryPoint = "#2", CharSet = CharSet.Auto)]
        internal static extern uint SHChangeNotifyRegister(IntPtr hWnd, SHChangeNotifyRegisterFlags fSources, SHChangeNotifyEvents fEvents, uint wMsg, int cEntries, ref SHChangeNotifyEntry pFsne);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SHGetPathFromIDList(IntPtr pIDL, StringBuilder strPath);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        internal static extern int SHGetImageList(SHImageLists iImageList, ref Guid riid, out IImageList ppv);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        internal static extern void SHChangeNotify(SHChangeNotifyEvents wEventId, SHChangeNotifyFlags uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}

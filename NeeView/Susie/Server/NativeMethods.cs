﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Susie.Server
{
    internal static class NativeMethods
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public extern static IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32")]
        public extern static bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public extern static IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32")]
        public extern static IntPtr LocalLock(IntPtr hMem);

        [DllImport("kernel32")]
        public extern static bool LocalUnlock(IntPtr hMem);

        [DllImport("kernel32")]
        public extern static IntPtr LocalFree(IntPtr hMem);

        [DllImport("kernel32")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        public extern static UIntPtr LocalSize(IntPtr hMem);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public extern static int GetShortPathName(string longPath, StringBuilder shortPathBuffer, int bufferSize);

        // ショートパス名を求める
        public static string GetShortPathName(string longPath)
        {
            int bufferSize = Math.Max(longPath.Length + 8, 260);
            StringBuilder shortPathBuffer = new StringBuilder(bufferSize);
            NativeMethods.GetShortPathName(longPath, shortPathBuffer, bufferSize);
            string shortPath = shortPathBuffer.ToString();
            return shortPath;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapFileHeader
    {
        public ushort bfType;
        public uint bfSize;
        public ushort bfReserved1;
        public ushort bfReserved2;
        public uint bfOffBits;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }
}

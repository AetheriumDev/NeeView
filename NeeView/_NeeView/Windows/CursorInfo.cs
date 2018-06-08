﻿// from https://github.com/takanemu/WPFDragAndDropSample

// WIN32APIの高DPI対応
// from http://grabacr.net/archives/1105

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NeeView.Windows
{
    /// <summary>
    /// マウスカーソル座標取得(WIN32)
    /// </summary>
    public static class CursorInfo
    {
        internal static class NativeMethods
        {
            /// <summary>
            /// 現在のマウスカーソルのスクリーン座標を得ます
            /// </summary>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetCursorPos(out POINT pt);

            /// <summary>
            /// 画面上にある指定された点を、スクリーン座標からクライアント座標へ変換します。
            /// </summary>
            [DllImport("user32.dll")]
            public static extern int ScreenToClient(IntPtr hwnd, ref POINT pt);

            /// <summary>
            /// 座標
            /// </summary>
            public struct POINT
            {
                public Int32 X;
                public Int32 Y;
            }
        }

        /// <summary>
        /// 現在のマウスカーソル座標を取得
        /// </summary>
        /// <param name="visual">ハンドルを取得するためのビジュアル要素</param>
        /// <returns></returns>
        public static Point GetNowPosition(Visual visual)
        {
            NativeMethods.GetCursorPos(out NativeMethods.POINT point);

            var source = HwndSource.FromVisual(visual) as HwndSource;
            var hwnd = source.Handle;

            NativeMethods.ScreenToClient(hwnd, ref point);

            var dpiScaleFactor = GetDpiScaleFactor(visual);
            return new Point(point.X / dpiScaleFactor.X, point.Y / dpiScaleFactor.Y);
        }


        /// <summary>
        /// 現在のマウスカーソルのスクリーン座標を取得
        /// </summary>
        /// <returns></returns>
        public static Point GetNowScreenPosition()
        {
            NativeMethods.GetCursorPos(out NativeMethods.POINT point);

            var dpiScaleFactor = GetDpiScaleFactor(App.Current.MainWindow);
            return new Point(point.X / dpiScaleFactor.X, point.Y / dpiScaleFactor.Y);
        }


        /// <summary>
        /// 現在の <see cref="T:System.Windows.Media.Visual"/> から、DPI 倍率を取得します。
        /// </summary>
        /// <returns>
        /// X 軸 および Y 軸それぞれの DPI 倍率を表す <see cref="T:System.Windows.Point"/>
        /// 構造体。取得に失敗した場合、(1.0, 1.0) を返します。
        /// </returns>
        public static Point GetDpiScaleFactor(System.Windows.Media.Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source != null && source.CompositionTarget != null)
            {
                return new Point(
                    source.CompositionTarget.TransformToDevice.M11,
                    source.CompositionTarget.TransformToDevice.M22);
            }

            return new Point(1.0, 1.0);
        }
    }
}

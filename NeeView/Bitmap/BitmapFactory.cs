﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{

    /// <summary>
    /// Bitmap生成
    /// </summary>
    public class BitmapFactory
    {
        private readonly DefaultBitmapFactory _default = new();
        private readonly MagicScalerBitmapFactory _magicScaler = new();


        public BitmapImage CreateBitmapSource(Stream stream, BitmapInfo? info, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            // by MagicScaler
            if (!size.IsEmpty && setting.Mode == BitmapCreateMode.HighQuality)
            {
                try
                {
                    return _magicScaler.Create(stream, info, size, setting.ProcessImageSettings);
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();
                    Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                }
            }

            // by Default
            return _default.Create(stream, info, size, token);
        }

        public void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting, CancellationToken token)
        {
            // by MagicScaler
            if (!size.IsEmpty && setting.Mode == BitmapCreateMode.HighQuality)
            {
                try
                {
                    _magicScaler.CreateImage(stream, info, outStream, size, format, quality, setting.ProcessImageSettings);
                    return;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();
                    Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                }
            }

            // by Default
            _default.CreateImage(stream, info, outStream, size, format, quality, token);
        }
    }

}

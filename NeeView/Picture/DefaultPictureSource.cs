﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
#if false
    public class DefaultPictureSource : PictureSource
    {
        private static readonly BitmapFactory _bitmapFactory = new();
        private readonly PictureNamedStreamSource _streamSource;

        public DefaultPictureSource(ArchiveEntry entry, PictureInfo? pictureInfo, PictureSourceCreateOptions createOptions) : base(entry, pictureInfo, createOptions)
        {
            _streamSource = new PictureNamedStreamSource(entry);
        }

        public override long GetMemorySize()
        {
            return _streamSource.GetMemorySize();
        }

        public override PictureInfo CreatePictureInfo(CancellationToken token)
        {
            if (this.PictureInfo != null) return this.PictureInfo;

            token.ThrowIfCancellationRequested();

            var pictureInfo = new PictureInfo();

            using (var stream = _streamSource.CreateStream(token))
            {
                stream.Seek(0, SeekOrigin.Begin);
                var bitmapInfo = BitmapInfo.Create(stream);
                pictureInfo.BitmapInfo = bitmapInfo;
                var originalSize = bitmapInfo.IsTranspose ? bitmapInfo.GetPixelSize().Transpose() : bitmapInfo.GetPixelSize();
                pictureInfo.OriginalSize = originalSize;

                var maxSize = bitmapInfo.IsTranspose ? Config.Current.Performance.MaximumSize.Transpose() : Config.Current.Performance.MaximumSize;
                var size = (Config.Current.Performance.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : Size.Empty;
                pictureInfo.Size = size.IsEmpty ? originalSize : size;
                pictureInfo.AspectSize = bitmapInfo.IsTranspose ? bitmapInfo.GetAspectSize().Transpose() : bitmapInfo.GetAspectSize();

                pictureInfo.Decoder = _streamSource.Decoder ?? ".NET BitmapImage";
                pictureInfo.BitsPerPixel = bitmapInfo.BitsPerPixel;
                pictureInfo.Metadata = bitmapInfo.Metadata;

                this.PictureInfo = pictureInfo;
            }

            return this.PictureInfo;
        }

        public override ImageSource CreateImageSource(Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using (var stream = new WrappingStream(_streamSource.CreateStream(token)))
            {
                if (setting.IsKeepAspectRatio && !size.IsEmpty)
                {
                    size = new Size(size.Width, 0);
                }

                var bitmapSource = _bitmapFactory.CreateBitmapSource(stream, PictureInfo?.BitmapInfo, size, setting, token);

                // 色情報とBPP設定。
                this.PictureInfo?.SetPixelInfo(bitmapSource);

                return bitmapSource;
            }
        }


        public override byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using (var stream = _streamSource.CreateStream(token))
            {
                using (var outStream = new MemoryStream())
                {
                    _bitmapFactory.CreateImage(stream, PictureInfo?.BitmapInfo, outStream, size, format, quality, setting, token);
                    return outStream.ToArray();
                }
            }
        }


        public override byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token)
        {
            ////Debug.WriteLine($"## CreateThumbnail: {this.ArchiveEntry}");

            token.ThrowIfCancellationRequested();

            Size size;
            BitmapInfo? bitmapInfo;
            if (PictureInfo != null)
            {
                size = PictureInfo.Size;
                bitmapInfo = PictureInfo.BitmapInfo;
            }
            else
            {
                using (var stream = _streamSource.CreateStream(token))
                {
                    bitmapInfo = BitmapInfo.Create(stream);
                    size = bitmapInfo.IsTranspose ? bitmapInfo.GetPixelSize().Transpose() : bitmapInfo.GetPixelSize();
                }
            }

            size = ThumbnailProfile.GetThumbnailSize(size);
            var setting = profile.CreateBitmapCreateSetting(bitmapInfo?.Metadata?.IsOriantationEnabled == true);
            return CreateImage(size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }

        public override Size FixedSize(Size size)
        {
            Debug.Assert(PictureInfo != null);

            var maxWixth = Math.Max(this.PictureInfo.Size.Width, Config.Current.Performance.MaximumSize.Width);
            var maxHeight = Math.Max(this.PictureInfo.Size.Height, Config.Current.Performance.MaximumSize.Height);
            var maxSize = new Size(maxWixth, maxHeight);
            return size.Limit(maxSize);
        }

    }
#endif
}

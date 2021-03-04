﻿using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public static class BitmapMetadataAccessorFactory
    {
        public static BitmapMetadataAccessor Create(BitmapMetadata meta)
        {
            if (meta is null)
            {
                return null;
            }

            switch (meta.Format)
            {
                case "tiff": return new TiffMetadataAccessor(meta);
                case "jpg": return new JpgMetadataAccessor(meta);
                case "png": return new PngMetadataAccessor(meta);
                case "gif": return new GifMetadataAccessor(meta);
            }

            Debug.WriteLine($"BitmapMetadataAccessor: not supprot BitmapMetadata.Format: {meta.Format}");
            return null;
        }
    }

}

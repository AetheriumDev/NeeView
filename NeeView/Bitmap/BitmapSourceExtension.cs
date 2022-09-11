﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public static class BitmapSourceExtension
    {
        // from http://www.nminoru.jp/~nminoru/programming/bitcount.html
        public static int BitCount(int bits)
        {
            bits = (bits & 0x55555555) + (bits >> 1 & 0x55555555);
            bits = (bits & 0x33333333) + (bits >> 2 & 0x33333333);
            bits = (bits & 0x0f0f0f0f) + (bits >> 4 & 0x0f0f0f0f);
            bits = (bits & 0x00ff00ff) + (bits >> 8 & 0x00ff00ff);
            return (bits & 0x0000ffff) + (bits >> 16 & 0x0000ffff);
        }

        // from http://www.nminoru.jp/~nminoru/programming/bitcount.html
        public static int BitNTZ(int bits)
        {
            return BitCount((~bits) & (bits - 1));
        }

        // GetOneColor()のサポートフォーマット
        private static readonly PixelFormat[] s_supportedFormats = new PixelFormat[]
        {
            PixelFormats.Bgra32,
            PixelFormats.Bgr32,
            PixelFormats.Bgr24,
            PixelFormats.Bgr555,
            PixelFormats.Bgr565,
            PixelFormats.Gray8,
            PixelFormats.Gray4,
            PixelFormats.Gray2,
        };

        // GetOneColor()のサポートフォーマット (インデックスカラー)
        private static readonly PixelFormat[] s_supportedIndexFormats = new PixelFormat[]
        {
            PixelFormats.Indexed8,
            PixelFormats.Indexed4,
            PixelFormats.Indexed2,
            PixelFormats.Indexed1,
        };


        // 有効BitsPerPixelを取得する
        public static int GetSourceBitsPerPixel(this BitmapSource bmp)
        {
            if (bmp == null) return 0;
            return bmp.Format.BitsPerPixel;
        }


        // 画像の最初の1ピクセルのカラーを取得
        public static Color GetOneColor(this BitmapSource bmp)
        {
            if (bmp == null) return Colors.Black;

            // 1pixel取得
            var pixels = new int[1];
            bmp.CopyPixels(new System.Windows.Int32Rect(0, 0, 1, 1), pixels, 4, 0);

            // ビットマスクを適用して要素の値を取得する
            var elements = new List<byte>();
            foreach (PixelFormatChannelMask channelMask in bmp.Format.Masks)
            {
                int bits = 0;
                int index = 0;

                foreach (byte myByte in channelMask.Mask)
                {
                    bits |= (myByte << (index++ * 8));
                }

                int shift = BitNTZ(bits);

                elements.Add((byte)((pixels[0] & bits) >> shift));
            }

            var color = new Color();

            if (s_supportedFormats.Contains(bmp.Format))
            {
                color.B = elements[0];
                color.G = (elements.Count >= 2) ? elements[1] : elements[0];
                color.R = (elements.Count >= 3) ? elements[2] : elements[0];
                color.A = 0xFF; // elements[3];
            }
            else if (s_supportedIndexFormats.Contains(bmp.Format))
            {
                color = bmp.Palette.Colors[elements[0]];
                color.A = 0xFF;
            }
            else
            {
                Debug.WriteLine("GetOneColor: No supprot format: " + bmp.Format.ToString());
                color = Colors.Black;
            }

            return color;
        }
    }
}

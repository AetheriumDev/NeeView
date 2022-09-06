﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// サムネイル.
    /// Jpegで保持し、必要に応じてBitmapSourceを生成
    /// </summary>
    public class Thumbnail : BindableBase, IThumbnail, IDisposable
    {
        /// <summary>
        /// 開発用：キャッシュ読み込み無効
        /// </summary>
        public static bool DebugIgnoreCache { get; set; }


        /// <summary>
        /// 有効判定
        /// </summary>
        internal bool IsValid => (_image != null);

        /// <summary>
        /// 変更イベント
        /// </summary>
        public event EventHandler? Changed;

        /// <summary>
        /// 参照イベント
        /// </summary>
        public event EventHandler? Touched;


        /// <summary>
        /// Jpeg化された画像
        /// </summary>
        private byte[]? _image;
        public byte[]? Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    if (Image != null)
                    {
                        Changed?.Invoke(this, EventArgs.Empty);
                        Touched?.Invoke(this, EventArgs.Empty);
                        RaisePropertyChanged("");
                    }
                }
            }
        }

        /// <summary>
        /// ユニークイメージ？
        /// </summary>
        public bool IsUniqueImage => _image == null || (_image != _emptyImage && _image != _mediaImage && _image != _folderImage);

        /// <summary>
        /// 標準イメージ？
        /// バナーでの引き伸ばし許可
        /// </summary>
        public bool IsNormalImage => _image == null || (_image != _mediaImage && _image != _folderImage);

        /// <summary>
        /// View用Bitmapプロパティ
        /// </summary>
        public ImageSource? ImageSource => CreateBitmap();

        public double Width => ImageSource is BitmapSource bitmap ? bitmap.PixelWidth : ImageSource != null ? ImageSource.Width : 0.0;
        public double Height => ImageSource is BitmapSource bitmap ? bitmap.PixelHeight : ImageSource != null ? ImageSource.Height : 0.0;


        /// <summary>
        /// View用Bitmapの背景プロパティ
        /// </summary>
        public Brush Background
        {
            get
            {
                if (_image == _mediaImage)
                {
                    return _mediaBackground;
                }
                else
                {
                    return Brushes.Transparent;
                }
            }
        }

        /// <summary>
        /// 寿命間利用シリアルナンバー
        /// </summary>
        public int LifeSerial { get; set; }

        /// <summary>
        /// キャッシュ使用
        /// </summary>
        public bool IsCacheEnabled { get; set; }

        /// <summary>
        /// キャシュ用ヘッダ
        /// </summary>
        public ThumbnailCacheHeader? _header { get; set; }


        /// <summary>
        /// キャッシュを使用してサムネイル生成を試みる
        /// </summary>
        internal async Task InitializeAsync(ArchiveEntry entry, string? appendix, CancellationToken token)
        {
            if (IsValid || !IsCacheEnabled) return;

#if DEBUG
            if (DebugIgnoreCache)
            {
                Image = null;
                return;
            }
#endif
            // NOTE: ディレクトリは更新日をサイズとする
            var length = entry.IsDirectory ? entry.LastWriteTime.ToBinary() : entry.Length;

            _header = new ThumbnailCacheHeader(entry.SystemPath, length, appendix, Config.Current.Thumbnail.GetThumbnailImageGenerateHash());
            var image = await ThumbnailCache.Current.LoadAsync(_header, token);
            ////Debug.WriteLine($"ThumbnailCache.Load: {_header.Key}: {(image == null ? "Miss" : "Hit!")}");
            Image = image;
        }

        /// <summary>
        /// 画像データから初期化
        /// </summary>
        /// <param name="source"></param>
        internal void Initialize(byte[]? image)
        {
            if (IsValid) return;

            Image = image ?? _emptyImage;

            SaveCacheAsync();
        }



        /// <summary>
        /// サムネイル基本タイプから初期化
        /// </summary>
        /// <param name="type"></param>
        internal void Initialize(ThumbnailType type)
        {
            switch (type)
            {
                default:
                case ThumbnailType.Empty:
                    Image = _emptyImage;
                    break;
                case ThumbnailType.Media:
                    Image = _mediaImage;
                    break;
                case ThumbnailType.Folder:
                    Image = _folderImage;
                    break;
            }
        }

        /// <summary>
        /// キャッシュに保存
        /// </summary>
        internal void SaveCacheAsync()
        {
            if (!IsCacheEnabled || _header == null) return;
            if (_image == null || _image == _emptyImage || _image == _mediaImage || _image == _folderImage) return;

            ThumbnailCache.Current.EntrySaveQueue(_header, _image);
        }

        /// <summary>
        /// image無効
        /// </summary>
        public void Clear()
        {
            // 通知は不要なので直接パラメータ変更
            _image = null;
        }

        /// <summary>
        /// Touch
        /// </summary>
        public void Touch()
        {
            Touched?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// ImageSource取得
        /// </summary>
        /// <returns></returns>
        public ImageSource? CreateBitmap()
        {
            if (_image is null) return null;

            Touched?.Invoke(this, EventArgs.Empty);
            if (_image == _emptyImage)
            {
                return EmptyImageSource;
            }
            else if (_image == _mediaImage)
            {
                return MediaBitmapSource;
            }
            else if (_image == _folderImage)
            {
                return FolderBitmapSource;
            }
            else
            {
                return DecodeFromImageData(_image);
            }
        }


        /// <summary>
        /// ImageData to BitmapSource
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private BitmapSource DecodeFromImageData(byte[] image)
        {
            using (var stream = new MemoryStream(image, false))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
        }

        /// <summary>
        /// イメージ初期化
        /// UIスレッドで実行すること。
        /// </summary>
        public static void InitializeBasicImages()
        {
            Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);

            var imageN = EmptyImageSource;
            var image0 = EmptyBitmapSource;
            var image1 = MediaBitmapSource;
        }

        /// <summary>
        /// Empty Image Key
        /// </summary>
        public static byte[] _emptyImage = System.Text.Encoding.ASCII.GetBytes("EMPTY!");


        private static ImageSource? _emptyImageSource;
        public static ImageSource EmptyImageSource
        {
            get
            {
                if (_emptyImageSource == null)
                {
                    _emptyImageSource = MainWindow.Current.Resources["thumbnail_default"] as ImageSource
                        ?? throw new InvalidOperationException("Cannot found resource");
                }
                return _emptyImageSource;
            }
        }

        /// <summary>
        /// EmptyBitmapSource property.
        /// </summary>
        private static BitmapSource? _emptyBitmapSource;
        public static BitmapSource EmptyBitmapSource
        {
            get
            {
                if (_emptyBitmapSource == null)
                {
                    _emptyBitmapSource = CreatetResourceBitmapImage("/Resources/Empty.png");
                }
                return _emptyBitmapSource;
            }
        }

        /// <summary>
        /// Media Image Key
        /// </summary>
        public static byte[] _mediaImage = System.Text.Encoding.ASCII.GetBytes("MEDIA!");

        public static SolidColorBrush _mediaBackground = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A));

        /// <summary>
        /// MediaBitmapSource
        /// </summary>
        private static BitmapSource? _mediaBitmapSource;
        public static BitmapSource MediaBitmapSource
        {
            get
            {
                if (_mediaBitmapSource == null)
                {
                    _mediaBitmapSource = CreatetResourceBitmapImage("/Resources/Media.png");
                }
                return _mediaBitmapSource;
            }
        }

        private static BitmapImage CreatetResourceBitmapImage(string path)
        {
            var uri = new Uri("pack://application:,,," + path);
            var bitmap = new BitmapImage(uri);
            bitmap.Freeze();

            return bitmap;
        }


        public static byte[] _folderImage = System.Text.Encoding.ASCII.GetBytes("FOLDER!");

        private BitmapSource? _folderBitmapSource;
        public BitmapSource FolderBitmapSource
        {
            get
            {
                if (_folderBitmapSource == null)
                {
                    _folderBitmapSource = FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(256.0) ?? EmptyBitmapSource;
                }
                return _folderBitmapSource;
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _image = null;
                    Changed = null;
                    Touched = null;
                    ResetPropertyChanged();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }


    /// <summary>
    /// サムネイル種類
    /// </summary>
    public enum ThumbnailType
    {
        Unique,
        Empty,
        Media,
        Folder,
    }
}

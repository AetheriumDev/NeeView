﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.IO.Search;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class Page : IDisposable, INotifyPropertyChanged, IPageContentLoader, IPageThumbnailLoader, IHasPage, IRenameable, ISearchItem
    {
        private int _index;
        private readonly PageContent _content;
        private bool _isVisible;
        private bool _isMarked;
        private bool _disposedValue;

        public Page(PageContent content) : this(content, "", content.ArchiveEntry.EntryFullName)
        {
        }

        public Page(PageContent content, string bookPath, string entryName)
        {
            BookPath = bookPath;
            EntryName = entryName;

            _content = content;
            _content.ContentChanged += Content_ContentChanged;
            _content.SizeChanged += Content_SizeChanged;

            _thumbnailSource = PageThumbnailFactory.Create(_content);
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? ContentChanged;

        [Subscribable]
        public event EventHandler? SizeChanged;



        public bool IsLoaded => _content.IsLoaded;

        public ArchiveEntry ArchiveEntry => _content.ArchiveEntry;

        public PageContent Content => _content;


        // 登録番号
        public int EntryIndex { get; set; }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                _content.Index = value;
            }
        }

        // TODO: 表示番号と内部番号のずれ
        public int IndexPlusOne => Index + 1;

        // ページ名 : エントリ名
        public string EntryName { get; }

        // ページ名：ファイル名のみ
        public string EntryLastName => ArchiveEntry.EntryLastName;

        // ページ名：スマートパス
        public string EntrySmartName => Prefix == null ? EntryName : EntryName[Prefix.Length..];

        // ページ名：フルパス名 (リンクはそのまま)
        public string EntryFullName => ArchiveEntry.EntryFullName;

        // ページ名：システムパス (リンクは実体に変換済)
        public string SystemPath => ArchiveEntry.SystemPath;

        // ページ名：スマート名用プレフィックス
        public string? Prefix { get; set; }

        // ブックのパス
        public string BookPath { get; }

        // ファイル情報：ファイル作成日
        public DateTime CreationTime => ArchiveEntry != null ? ArchiveEntry.CreationTime : default;

        // ファイル情報：最終更新日
        public DateTime LastWriteTime => ArchiveEntry != null ? ArchiveEntry.LastWriteTime : default;

        // ファイル情報：ファイルサイズ
        public long Length => ArchiveEntry.Length;

        // コンテンツ幅
        public double Width => Size.Width;

        // コンテンツ高
        public double Height => Size.Height;

        /// <summary>
        /// コンテンツサイズ
        /// </summary>
        public Size Size => _content.Size;

        /// <summary>
        /// コンテンツカラー
        /// </summary>
        public Color Color => _content.Color;

        /// <summary>
        /// ページの種類
        /// </summary>
        public PageType PageType => _content.PageType;

        /// <summary>
        /// ブックとして開くことができる
        /// </summary>
        public bool IsBook => _content.IsBook;

        // 表示中?
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        public bool IsMarked
        {
            get { return _isMarked; }
            set { SetProperty(ref _isMarked, value); }
        }


        /// <summary>
        /// 要求状態
        /// </summary>
        public PageContentState State
        {
            get { return _content.State; }
            set { _content.State = value; }
        }

        /// <summary>
        /// 削除済フラグ
        /// </summary>
        public bool IsDeleted => ArchiveEntry.IsDeleted;


        #region Thumbnail

        private readonly PageThumbnail _thumbnailSource;


        /// <summary>
        /// サムネイル
        /// </summary>
        public Thumbnail Thumbnail => _thumbnailSource.Thumbnail;

        public bool IsThumbnailValid => _thumbnailSource.Thumbnail.IsValid;

        /// <summary>
        /// サムネイル読み込み
        /// </summary>
        public async Task<ImageSource?> LoadThumbnailAsync(CancellationToken token)
        {
            if (_disposedValue) return null;

            try
            {
                token.ThrowIfCancellationRequested();
                await _thumbnailSource.LoadAsync(token);
                return this.Thumbnail?.CreateImageSource();
            }
            catch
            {
                // nop.
                return null;
            }
        }

        #endregion


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _content.ContentChanged -= Content_ContentChanged;
                    _content.SizeChanged -= Content_SizeChanged;
                    _content.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Content_ContentChanged(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(Content));
            RaisePropertyChanged(nameof(Color));
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Content_SizeChanged(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(Size));
            SizeChanged?.Invoke(this, EventArgs.Empty);
        }

        // TODO: これをPageのメソッドとして公開するのは？
        public async Task LoadContentAsync(CancellationToken token)
        {
            if (_disposedValue) return;

            await _content.LoadAsync(token);
        }

        // TODO: PageLoader管理と競合している問題
        public void Unload()
        {
            if (_disposedValue) return;

            _content.Unload();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }


        public override string ToString()
        {
            return $"Page:{Index}, {EntryFullName}";
        }

        public Page? GetPage()
        {
            return this;
        }

        public SearchValue GetValue(SearchPropertyProfile profile, string? parameter, CancellationToken token)
        {
            switch (profile.Name)
            {
                case "text":
                    return new StringSearchValue(GetDispName(Config.Current.PageList.Format));
                case "date":
                    return new DateTimeSearchValue(LastWriteTime);
                case "size":
                    return new IntegerSearchValue(Length);
                case "playlist":
                    return new BooleanSearchValue(IsMarked);
                case "meta":
                    return new StringSearchValue(PageMetadataTools.GetValueString(this, parameter, token));
                case "rating":
                    return new IntegerSearchValue(PageMetadataTools.GetRating(this, token));
                default:
                    throw new NotSupportedException($"Not supported SearchProperty: {profile.Name}");
            }
        }

        public string GetDispName(PageNameFormat format)
        {
            return format switch
            {
                PageNameFormat.Smart => GetSmartFullName(),
                PageNameFormat.NameOnly => EntryLastName,
                _ => EntryName,
            };
        }

        #region Page functions


        // ページ名：ソート用分割
        public string[] GetEntryFullNameTokens()
        {
            return LoosePath.Split(EntryName);
        }

        // ページ名：プレフィックスを除いたフルパス
        public string GetSmartFullName()
        {
            return EntrySmartName.Replace("\\", " > ");
        }

        public string GetSmartDirectoryName()
        {
            return LoosePath.GetDirectoryName(EntrySmartName).Replace("\\", " > ");
        }

        // ファイルの場所を取得
        public string GetFilePlace()
        {
            return ArchiveEntry.GetFileSystemPath() ?? ArchiveEntry.Archiver.GetPlace();
        }

        // フォルダーを開く、で取得するパス
        public string GetFolderOpenPlace()
        {
            if (ArchiveEntry.Archiver is FolderArchive)
            {
                return GetFilePlace();
            }
            else
            {
                return GetFolderPlace();
            }
        }

        // フォルダーの場所を取得
        public string GetFolderPlace()
        {
            return ArchiveEntry.Archiver.GetSourceFileSystemPath();
        }

        /// <summary>
        /// can delete?
        /// </summary>
        public bool CanDelete()
        {
            return ArchiveEntry.CanDelete();
        }

        /// <summary>
        /// delete
        /// </summary>
        public async Task<bool> DeleteAsync()
        {
            return await ArchiveEntry.DeleteAsync();
        }

        public string GetRenameText()
        {
            return ArchiveEntry.GetRenameText();
        }

        public bool CanRename()
        {
            return ArchiveEntry.CanRename();
        }

        public async Task<bool> RenameAsync(string name)
        {
            if (_disposedValue) return false;

            var isSuccess = await ArchiveEntry.RenameAsync(name);
            RaiseNamePropertyChanged();
            FileInformation.Current.Update(); // TODO: 伝達方法がよろしくない
            return isSuccess;
        }

        private void RaiseNamePropertyChanged()
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(EntryName));
            RaisePropertyChanged(nameof(EntryLastName));
            RaisePropertyChanged(nameof(EntrySmartName));
            RaisePropertyChanged(nameof(EntryFullName));
            RaisePropertyChanged(nameof(SystemPath));
        }

        public string GetMetaValue(string key, CancellationToken token)
        {
            return PageMetadataTools.GetValueString(this, key.ToLower(), token);
        }

        public Dictionary<string, string> GetMetaValueMap(CancellationToken token)
        {
            return PageMetadataTools.GetValueStringMap(this, token);
        }

        #endregion
    }

}

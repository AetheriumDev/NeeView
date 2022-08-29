﻿using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookPageViewer : BindableBase, IDisposable
    {
        private BookSource _book;
        private BookPageViewSetting _setting;


        // 表示ページコンテキスト
        private ViewContentSourceCollection _viewPageCollection = new ViewContentSourceCollection();

        // リソースを保持しておくページ
        private List<Page> _keepPages = new List<Page>();

        // JOBリクエスト
        private PageContentJobClient _jobClient = new PageContentJobClient("View", JobCategories.PageViewContentJobCategory);

        // メモリ管理
        private BookMemoryService _bookMemoryService;

        // 先読み
        private BookAhead _ahead;

        // コンテンツ生成
        private BookPageViewGenerater? _pageViewGenerater;

        // 処理中フラグ
        private bool _isBusy;

        // ブックのビュー更新カウンター
        private BookPageCounter _viewCounter = new BookPageCounter();


        public BookPageViewer(BookSource book, BookMemoryService memoryService, BookPageViewSetting setting)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _bookMemoryService = memoryService;
            _setting = setting;

            foreach (var page in _book.Pages)
            {
                page.Loaded += Page_Loaded;
            }

            _ahead = new BookAhead(_bookMemoryService);
            _ahead.AddPropertyChanged(nameof(BookAhead.IsBusy), (s, e) => UpdateIsBusy());
        }


        // 設定変更
        public event EventHandler? SettingChanged;

        public IDisposable SubscribeSettingChanged(EventHandler handler)
        {
            SettingChanged += handler;
            return new AnonymousDisposable(() => SettingChanged -= handler);
        }

        // 表示コンテンツ変更
        // 表示の更新を要求
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs>? ViewContentsChanged;

        public IDisposable SubscribeViewContentsChanged(EventHandler<ViewContentSourceCollectionChangedEventArgs> handler)
        {
            ViewContentsChanged += handler;
            return new AnonymousDisposable(() => ViewContentsChanged -= handler);
        }

        // 先読みコンテンツ変更
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs>? NextContentsChanged;

        public IDisposable SubscribeNextContentsChanged(EventHandler<ViewContentSourceCollectionChangedEventArgs> handler)
        {
            NextContentsChanged += handler;
            return new AnonymousDisposable(() => NextContentsChanged -= handler);
        }

        // ページ終端を超えて移動しようとした
        // 次の本への移動を要求
        public event EventHandler<PageTerminatedEventArgs>? PageTerminated;

        public IDisposable SubscribePageTerminated(EventHandler<PageTerminatedEventArgs> handler)
        {
            PageTerminated += handler;
            return new AnonymousDisposable(() => PageTerminated -= handler);
        }



        // 横長ページを分割する
        public bool IsSupportedDividePage
        {
            get { return _setting.IsSupportedDividePage; }
            set { if (_setting.IsSupportedDividePage != value) { _setting.IsSupportedDividePage = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 最初のページは単独表示
        public bool IsSupportedSingleFirstPage
        {
            get { return _setting.IsSupportedSingleFirstPage; }
            set { if (_setting.IsSupportedSingleFirstPage != value) { _setting.IsSupportedSingleFirstPage = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 最後のページは単独表示
        public bool IsSupportedSingleLastPage
        {
            get { return _setting.IsSupportedSingleLastPage; }
            set { if (_setting.IsSupportedSingleLastPage != value) { _setting.IsSupportedSingleLastPage = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 横長ページは２ページとみなす
        public bool IsSupportedWidePage
        {
            get { return _setting.IsSupportedWidePage; }
            set { if (_setting.IsSupportedWidePage != value) { _setting.IsSupportedWidePage = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 右開き、左開き
        public PageReadOrder BookReadOrder
        {
            get { return _setting.BookReadOrder; }
            set { if (_setting.BookReadOrder != value) { _setting.BookReadOrder = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 単ページ/見開き
        public PageMode PageMode
        {
            get { return _setting.PageMode; }
            set { if (_setting.PageMode != value) { _setting.PageMode = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }


        // 処理中フラグ
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }


        // 表示されるページ番号(スライダー用)
        public int DisplayIndex { get; set; }

        // 表示ページ変更回数
        public int PageChangeCount { get; private set; }

        // 終端ページ表示
        public bool IsPageTerminated { get; private set; }

        // TODO: このパラメータだけ公開するのは微妙。
        public ViewContentSourceCollection ViewPageCollection => _viewPageCollection;



        // 処理中フラグ更新
        private void UpdateIsBusy()
        {
            IsBusy = _ahead.IsBusy || (_pageViewGenerater != null ? _pageViewGenerater.IsBusy : false);
        }

        // 動画用：外部から終端イベントを発行
        public void RaisePageTerminatedEvent(object? sender, int direction)
        {
            if (_disposedValue) return;

            PageTerminated?.Invoke(sender, new PageTerminatedEventArgs(direction));
        }

        // 表示ページ番号
        public int GetViewPageIndex() => _viewPageCollection.Range.Min.Index;

        // 表示ページ
        public Page? GetViewPage() => _book.Pages.GetPage(_viewPageCollection.Range.Min.Index);

        // 表示ページ群
        public List<Page> GetViewPages() => _viewPageCollection.Collection.Select(e => e.Page).ToList();

        // 先読み許可フラグ
        private bool AllowPreLoad()
        {
            return Config.Current.Performance.PreLoadSize > 0;
        }

        // 表示ページ再読込
        public async Task RefreshViewPageAsync(object? sender, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_disposedValue) return;

            var range = new PageRange(_viewPageCollection.Range.Min, 1, PageMode.Size());
            await UpdateViewPageAsync(sender, range, token);
        }

        // 表示ページ移動
        public async Task MoveViewPageAsync(object? sender, int step, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_disposedValue) return;

            var viewRange = _viewPageCollection.Range;

            var direction = step < 0 ? -1 : 1;

            var pos = Math.Abs(step) == PageMode.Size() ? viewRange.Next(direction) : viewRange.Move(step);
            if (pos < _book.Pages.FirstPosition() && !viewRange.IsContains(_book.Pages.FirstPosition()))
            {
                pos = new PagePosition(0, direction < 0 ? 1 : 0);
            }
            else if (pos > _book.Pages.LastPosition() && !viewRange.IsContains(_book.Pages.LastPosition()))
            {
                pos = new PagePosition(_book.Pages.Count - 1, direction < 0 ? 1 : 0);
            }

            var range = new PageRange(pos, direction, PageMode.Size());

            await UpdateViewPageAsync(sender, range, token);
        }

        // 表示ページ更新
        public async Task UpdateViewPageAsync(object? sender, PageRange viewPageRange, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_disposedValue) return;

            // ページ終端を越えたか判定
            if (viewPageRange.Position < _book.Pages.FirstPosition())
            {
                PageTerminated?.Invoke(sender, new PageTerminatedEventArgs(-1));
                return;
            }
            else if (viewPageRange.Position > _book.Pages.LastPosition())
            {
                PageTerminated?.Invoke(sender, new PageTerminatedEventArgs(+1));
                return;
            }

            // ページ数０の場合は表示コンテンツなし
            if (_book.Pages.Count == 0)
            {
                ViewContentsChanged?.Invoke(sender, new ViewContentSourceCollectionChangedEventArgs(_book.Path, new ViewContentSourceCollection()));
                return;
            }

            // view pages
            var viewPages = new List<Page>();
            for (int i = 0; i < PageMode.Size(); ++i)
            {
                var page = _book.Pages[_book.Pages.ClampPageNumber(viewPageRange.Position.Index + viewPageRange.Direction * i)];
                if (!viewPages.Contains(page))
                {
                    viewPages.Add(page);
                }
            }

            // pre load
            _ahead.Clear();
            var aheadPageRange = CreateAheadPageRange(viewPageRange);
            var aheadPages = CreatePagesFromRange(aheadPageRange, viewPages);

            var loadPages = viewPages.Concat(aheadPages).Distinct().ToList();

            // update content lock
            var unloadPages = _keepPages.Except(viewPages).ToList();
            foreach (var page in unloadPages)
            {
                page.State = PageContentState.None;
            }
            foreach (var (page, index) in viewPages.ToTuples())
            {
                page.State = PageContentState.View;
            }
            _keepPages = loadPages;

            // update contents
            this.PageChangeCount++;
            this.IsPageTerminated = viewPageRange.Max >= _book.Pages.LastPosition();

            // wait load time (max 5sec.)
            var timeout = (BookProfile.Current.CanPrioritizePageMove() && _viewCounter.Counter != 0) ? 100 : 5000;

            _pageViewGenerater?.Dispose();
            _pageViewGenerater = new BookPageViewGenerater(sender, _book, _setting, viewPageRange, aheadPageRange, _viewCounter);
            _pageViewGenerater.ViewContentsChanged += (s, e) =>
            {
                Interlocked.Exchange(ref _viewPageCollection, e.ViewPageCollection);
                this.DisplayIndex = e.ViewPageCollection.Range.Min.Index;
                ViewContentsChanged?.Invoke(s, e);
            };
            _pageViewGenerater.NextContentsChanged += (s, e) =>
                NextContentsChanged?.Invoke(s, e);
            _pageViewGenerater.AddPropertyChanged(nameof(_pageViewGenerater.IsBusy), (s, e) =>
                UpdateIsBusy());

            _bookMemoryService.SetReference(viewPages.First().Index);
            _jobClient.Order(viewPages);
            _ahead.Order(aheadPages);

            UpdateIsBusy();
            _pageViewGenerater.UpdateNextContents();

            try
            {
                // wait load (max 5sec.)
                await _pageViewGenerater.WaitVisibleAsync(timeout, token);
            }
            catch (TimeoutException)
            {
                _pageViewGenerater.UpdateViewContents();
                _pageViewGenerater.UpdateNextContents();
            }
        }


        /// <summary>
        /// ページロード完了イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            var page = sender as Page;
            if (page is null) return;

            _bookMemoryService.AddPageContent(page);

            _ahead.OnPageLoaded(this, new PageChangedEventArgs(page));

            ////if (!BookProfile.Current.CanPrioritizePageMove()) return;

            _pageViewGenerater?.UpdateNextContents();
        }

        /// <summary>
        /// 先読みページ範囲を求める
        /// </summary>
        /// <returns>先読みページ範囲。ブック終端をふまえた2範囲を返す</returns>
        private List<PageRange> CreateAheadPageRange(PageRange source)
        {
            if (!AllowPreLoad() || Config.Current.Performance.PreLoadSize < 1)
            {
                return new List<PageRange>() { PageRange.Empty, PageRange.Empty };
            }

            PageRange range0 = CreateAheadPageRange(source, source.Direction, Config.Current.Performance.PreLoadSize);

            PageRange range1 = PageRange.Empty;
            if (range0.PageSize < Config.Current.Performance.PreLoadSize)
            {
                var size = Config.Current.Performance.PreLoadSize - range0.PageSize;
                range1 = CreateAheadPageRange(source, source.Direction * -1, size);
            }

            return new List<PageRange>() { range0, range1 };
        }

        private PageRange CreateAheadPageRange(PageRange source, int direction, int size)
        {
            int index = source.Next(direction).Index;
            var pos0 = new PagePosition(index, 0);
            var pos1 = new PagePosition(_book.Pages.ClampPageNumber(index + (size - 1) * direction), 0);
            var range = _book.Pages.IsValidPosition(pos0) ? new PageRange(pos0, pos1) : PageRange.Empty;

            return range;
        }

        /// <summary>
        /// ページ範囲からページ列を生成
        /// </summary>
        /// <param name="ranges"></param>
        /// <param name="excepts">除外するページ</param>
        /// <returns></returns>
        private List<Page> CreatePagesFromRange(List<PageRange> ranges, List<Page> excepts)
        {
            return ranges.Select(e => CreatePagesFromRange(e, excepts))
                .SelectMany(e => e)
                .ToList();
        }

        private List<Page> CreatePagesFromRange(PageRange range, List<Page> excepts)
        {
            if (range.IsEmpty())
            {
                return new List<Page>();
            }

            return Enumerable.Range(0, range.PageSize)
                .Select(e => range.Position.Index + e * range.Direction)
                .Where(e => 0 <= e && e < _book.Pages.Count)
                .Select(e => _book.Pages[e])
                .Except(excepts)
                .ToList();
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.ResetPropertyChanged();
                    this.PageTerminated = null;
                    this.ViewContentsChanged = null;
                    this.NextContentsChanged = null;

                    _ahead.Dispose();
                    _pageViewGenerater?.Dispose();

                    _jobClient.Dispose();
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
}

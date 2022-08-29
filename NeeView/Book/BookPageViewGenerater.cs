﻿using NeeLaboratory.ComponentModel;
using NeeLaboratory.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ViewPageCollection Generate Process
    /// </summary>
    public class BookPageViewGenerater : BindableBase, IDisposable
    {


        private BookSource _book;
        private BookPageViewSetting _setting;

        private object? _sender;
        private PageRange _viewRange;
        private PageRange _nextRange;
        private PageRange _contentRange;
        private int _contentCount;

        private CancellationTokenSource _cancellationTokenSource;
        private object _lock = new object();
        private SemaphoreSlim _semaphore;
        private bool _isBusy = true;
        private ManualResetEventSlim _visibleEvent = new ManualResetEventSlim();
        private BookPageCounter _viewCounter;

        public BookPageViewGenerater(object? sender, BookSource book, BookPageViewSetting setting, PageRange viewPageRange, List<PageRange> aheadPageRanges, BookPageCounter viewCounter)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _semaphore = new SemaphoreSlim(0);

            _book = book;
            _setting = setting;
            _viewCounter = viewCounter;

            _sender = sender;
            _viewRange = viewPageRange;
            _nextRange = viewPageRange;

            // NextContentsChanged発行範囲を先読み含めて前後10ページまでに制限
            var clampOffset = PagePosition.GetValue(10);
            _contentRange = viewPageRange
                    .Truncate()
                    .Add(aheadPageRanges)
                    .Clamp(viewPageRange.Min - clampOffset, viewPageRange.Max + clampOffset);

            _ = WorkerAsync(_cancellationTokenSource.Token);
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



        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }



        public void UpdateNextContents()
        {
            if (_disposedValue) return;

            _semaphore.Release();
        }

        private async ValueTask WorkerAsync(CancellationToken token)
        {
            try
            {
                await UpdateNextContentsAsync(token);
                ////Debug.WriteLine($"> RunUpdateViewContents: done.");
            }
            catch (Exception)
            {
                ////Debug.WriteLine($"> RunUpdateViewContents: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async ValueTask UpdateNextContentsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            while (true)
            {
                IsBusy = false;

                token.ThrowIfCancellationRequested();
                await _semaphore.WaitAsync(token);

                IsBusy = true;

                while (true)
                {
                    ViewContentSourceCollection collection;

                    lock (_lock)
                    {
                        // get next collecton.
                        collection = CreateViewPageCollection(_nextRange);

                        // if out of range, return;
                        if (collection.Collection.Count == 0 || !_contentRange.IsContains(new PagePosition(collection.Range.Last.Index, 0)))
                        {
                            return;
                        }

                        // if collection is not valid, break;
                        if (!collection.IsValid)
                        {
                            break;
                        }

                        // update next range.
                        _nextRange = GetNextRange(collection.Range);
                    }

                    // NOTE: 先行リサイズ処理要求
                    token.ThrowIfCancellationRequested();
                    NextContentsChanged?.Invoke(_sender, new ViewContentSourceCollectionChangedEventArgs(_book.Path, collection) { IsForceResize = (_contentCount == 0) });

                    if (Interlocked.Increment(ref _contentCount) == 1)
                    {
                        // NOTE: 表示処理
                        ////Debug.WriteLine($"UpdateNextContentsInner: ViewContentChanged");
                        token.ThrowIfCancellationRequested();
                        UpdateViewContentsInner(_sender, collection, _viewCounter.Increment() == 1);
                    }
                }
            }
        }

        public void UpdateViewContents()
        {
            ViewContentSourceCollection collection;
            lock (_lock)
            {
                if (_contentCount > 0)
                {
                    return;
                }

                collection = CreateViewPageCollection(_viewRange);
            }

            ////Debug.WriteLine($"UpdateViewContents: ViewContentChanged");
            UpdateViewContentsInner(_sender, collection, false);
        }

        private void UpdateViewContentsInner(object? sender, ViewContentSourceCollection collection, bool isFirst)
        {
            ////var source = collection.Collection[0];
            ////Debug.WriteLine($"UpdateViewContentsInner: Name={source.Page.EntryName}, Type={source.GetContentType()}");

            var args = new ViewContentSourceCollectionChangedEventArgs(_book.Path, collection)
            {
                IsForceResize = true,
                IsFirst = isFirst
            };
            ViewContentsChanged?.Invoke(sender, args);

            _visibleEvent.Set();
        }

        public async ValueTask WaitVisibleAsync(int millisecondsTimeout, CancellationToken token)
        {
            using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _cancellationTokenSource.Token))
            {
                await _visibleEvent.WaitHandle.AsTask().WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout), tokenSource.Token);
            }
        }


        private PageRange GetNextRange(PageRange previous)
        {
            // 先読みコンテンツ領域計算
            var position = previous.Next();
            var direction = previous.Direction;
            var range = new PageRange(position, direction, _setting.PageMode.Size());

            return range;
        }


        // ページのワイド判定
        private bool IsWide(Page page)
        {
            return page.Width > page.Height * Config.Current.Book.WideRatio;
        }

        // 見開きモードでも単独表示するべきか判定
        private bool IsSoloPage(int index)
        {
            if (_setting.IsSupportedSingleFirstPage && index == 0) return true;
            if (_setting.IsSupportedSingleLastPage && index == _book.Pages.Count - 1) return true;
            if (_book.Pages[index].PageType == PageType.Folder) return true;
            if (_setting.IsSupportedWidePage && IsWide(_book.Pages[index])) return true;
            return false;
        }

        // 分割モード有効判定
        private bool IsEnableDividePage(int index)
        {
            return (_setting.PageMode == PageMode.SinglePage && !_book.IsMedia && _setting.IsSupportedDividePage && IsWide(_book.Pages[index]));
        }

        // 表示コンテンツソースと、それに対応したコンテキスト作成
        private ViewContentSourceCollection CreateViewPageCollection(PageRange source)
        {
            var infos = new List<PagePart>();

            {
                PagePosition position = source.Position;
                for (int id = 0; id < _setting.PageMode.Size(); ++id)
                {
                    if (!_book.Pages.IsValidPosition(position) || _book.Pages[position.Index] == null) break;

                    int size = 2;
                    if (IsEnableDividePage(position.Index))
                    {
                        size = 1;
                    }
                    else
                    {
                        position = new PagePosition(position.Index, 0);
                    }

                    infos.Add(new PagePart(position, size, _setting.BookReadOrder));
                    position = position + ((source.Direction > 0) ? size : -1);
                }
            }

            // 見開き補正
            if (_setting.PageMode == PageMode.WidePage && infos.Count >= 2)
            {
                if (IsSoloPage(infos[0].Position.Index) || IsSoloPage(infos[1].Position.Index))
                {
                    infos = infos.GetRange(0, 1);
                }
            }

            // コンテンツソース作成
            var list = new List<ViewContentSource>();
            foreach (var v in infos)
            {
                var viewContentSource = new ViewContentSource(_book.Pages[v.Position.Index], v);
                list.Add(viewContentSource);
            }

            // 並び順補正
            if (source.Direction < 0 && infos.Count >= 2)
            {
                list.Reverse();
                infos.Reverse();
            }

            // ダミーページ挿入
            if (Config.Current.Book.IsInsertDummyPage && list.Count == 1 && _setting.PageMode == PageMode.WidePage)
            {
                var mainSource = list[0];
                bool isSoloPage = IsSoloPage(mainSource.Page.Index);
                bool isFirstPage = mainSource.Page == _book.Pages.FirstOrDefault();
                bool isLastPage = mainSource.Page == _book.Pages.LastOrDefault();

                if (isSoloPage || (isFirstPage && isLastPage))
                {
                }
                else if (isFirstPage)
                {
                    list.Insert(0, new ViewContentSource(mainSource.Page, mainSource.PagePart, true));
                }
                else if (isLastPage)
                {
                    list.Add(new ViewContentSource(mainSource.Page, mainSource.PagePart, true));
                }
            }

            // 左開き
            if (_setting.BookReadOrder == PageReadOrder.LeftToRight)
            {
                list.Reverse();
            }

            // 単一ソースならコンテンツは１つにまとめる
            if (infos.Count == 2 && infos[0].Position.Index == infos[1].Position.Index)
            {
                var position = new PagePosition(infos[0].Position.Index, 0);
                list.Clear();
                list.Add(new ViewContentSource(_book.Pages[position.Index], new PagePart(position, 2, _setting.BookReadOrder)));
            }

            // 新しいコンテキスト
            var context = new ViewContentSourceCollection(new PageRange(infos, source.Direction), list);
            return context;
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_semaphore")]
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    ResetPropertyChanged();
                    ViewContentsChanged = null;
                    NextContentsChanged = null;
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
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

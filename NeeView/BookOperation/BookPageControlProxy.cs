﻿using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookPageControlProxy : IBookPageControl, IDisposable
    {
        private IBookPageControl? _source;
        private bool _disposedValue;

        public BookPageControlProxy()
        {
        }

        public event EventHandler? PagesChanged;
        public event EventHandler<PageRangeChangedEventArgs>? SelectedRangeChanged;

        public IReadOnlyList<Page> Pages => _source?.Pages ?? new List<Page>();
        public IReadOnlyList<Page> SelectedPages => _source?.SelectedPages ?? new List<Page>();
        public PageRange SelectedRange => _source?.SelectedRange ?? new PageRange();


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Detach();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void SetSource(IBookPageControl? source)
        {
            if (_source == source) return;

            Detach();
            Attach(source);
        }

        private void Attach(IBookPageControl? source)
        {
            Debug.Assert(_source is null);

            _source = source;
            if (_source is null) return;

            _source.PagesChanged += Source_PageListChanged;
            _source.SelectedRangeChanged += Source_SelectedItemChanged;

            PagesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Detach()
        {
            if (_source is null) return;

            _source.PagesChanged -= Source_PageListChanged;
            _source.SelectedRangeChanged -= Source_SelectedItemChanged;
            _source.Dispose();
            _source = null;
        }

        private void Source_PageListChanged(object? sender, EventArgs e)
        {
            PagesChanged?.Invoke(sender, e);
        }

        private void Source_SelectedItemChanged(object? sender, PageRangeChangedEventArgs e)
        {
            SelectedRangeChanged?.Invoke(sender, e);
        }


        #region IBookPageMoveControl


        public void MoveToFirst(object? sender)
        {
            _source?.MoveToFirst(sender);
        }

        public void MoveTo(object? sender, int index)
        {
            _source?.MoveTo(sender, index);
        }

        public void MoveToRandom(object? sender)
        {
            _source?.MoveToRandom(sender);
        }

        public void MoveToLast(object? sender)
        {
            _source?.MoveToLast(sender);
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            _source?.MoveNextFolder(sender, isShowMessage);
        }

        public void MoveNextOne(object? sender)
        {
            _source?.MoveNextOne(sender);
        }

        public void MoveNext(object? sender)
        {
            _source?.MoveNext(sender);
        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _source?.ScrollToNextFrame(sender, parameter);
        }

        public void MoveNextSize(object? sender, int size)
        {
            _source?.MoveNextSize(sender, size);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            _source?.MovePrevFolder(sender, isShowMessage);
        }

        public void MovePrevOne(object? sender)
        {
            _source?.MovePrevOne(sender);
        }

        public void MovePrev(object? sender)
        {
            _source?.MovePrev(sender);
        }

        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _source?.ScrollToPrevFrame(sender, parameter);
        }

        public void MovePrevSize(object? sender, int size)
        {
            _source?.MovePrevSize(sender, size);
        }

        #endregion IBookPageMoveControl

        #region IBookPageActionControl

        public bool CanDeleteFile()
        {
            return _source?.CanDeleteFile() ?? false;
        }

        public bool CanExport()
        {
            return _source?.CanExport() ?? false;
        }

        public bool CanOpenFilePlace()
        {
            return _source?.CanOpenFilePlace() ?? false;
        }

        public void CopyToClipboard(CopyFileCommandParameter parameter)
        {
            _source?.CopyToClipboard(parameter);
        }

        public Task DeleteFileAsync()
        {
            return _source?.DeleteFileAsync() ?? Task.CompletedTask;
        }

        public void Export(ExportImageCommandParameter parameter)
        {
            _source?.Export(parameter);
        }

        public void ExportDialog(ExportImageAsCommandParameter parameter)
        {
            _source?.ExportDialog(parameter);
        }

        public void OpenApplication(OpenExternalAppCommandParameter parameter)
        {
            _source?.OpenApplication(parameter);
        }

        public void OpenFilePlace()
        {
            _source?.OpenFilePlace();
        }

        public bool CanDeleteFile(List<Page> pages)
        {
            return _source?.CanDeleteFile(pages) ?? false;
        }

        public Task DeleteFileAsync(List<Page> pages)
        {
            return _source?.DeleteFileAsync(pages) ?? Task.CompletedTask;
        }

        #endregion IBookPageActionControl
    }

}
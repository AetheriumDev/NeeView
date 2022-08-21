﻿using NeeView.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public struct PageHistoryUnit : IEquatable<PageHistoryUnit>
    {
        public static PageHistoryUnit Empty = new PageHistoryUnit("", "");

        public PageHistoryUnit(string bookAddress, string pageName)
        {
            BookAddress = bookAddress;
            PageName = pageName;
        }

        public string BookAddress { get; private set; }
        public string PageName { get; private set; }

        public string EntryFullName => LoosePath.Combine(BookAddress, PageName);


        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(BookAddress) && string.IsNullOrEmpty(PageName);
        }

        public override bool Equals(object? obj)
        {
            if (obj is PageHistoryUnit other)
            {
                return this.Equals(other);
            }
            return false;
        }

        public bool Equals(PageHistoryUnit other)
        {
            return (BookAddress == other.BookAddress) && (PageName == other.PageName);
        }

        public override int GetHashCode()
        {
            return BookAddress.GetHashCode() ^ PageName.GetHashCode();
        }

        public static bool operator ==(PageHistoryUnit lhs, PageHistoryUnit rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PageHistoryUnit lhs, PageHistoryUnit rhs)
        {
            return !(lhs.Equals(rhs));
        }
    }

    /// <summary>
    /// BookHubの履歴。
    /// 本を開いた順番そのままを記録している。
    /// </summary>
    public class PageHistory
    {
        static PageHistory() => Current = new PageHistory();
        public static PageHistory Current { get; }


        private const int _historyCapacity = 100;
        private readonly HistoryLimitedCollection<PageHistoryUnit> _history = new HistoryLimitedCollection<PageHistoryUnit>(_historyCapacity);


        public PageHistory()
        {
            _history.Changed += (s, e) => Changed?.Invoke(s, e);

            BookHub.Current.ViewContentsChanged += BookHub_ViewContentsChanged;
        }

        
        public event EventHandler? Changed;


        private void BookHub_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            var viewPages = e.ViewPageCollection?.Collection.Where(x => x != null).Select(x => x.Page).ToList();

            PageHistoryUnit pageHistoryUnit;
            if (viewPages != null && viewPages.Count > 0)
            {
                var page = viewPages.Select(p => (p.Index, p)).Min().Item2;
                pageHistoryUnit = new PageHistoryUnit(e.BookAddress, page.EntryName);
            }
            else
            {
                // NOTE: 空白ページを登録することで直前ページを正常に参照できるようにする
                pageHistoryUnit = PageHistoryUnit.Empty;
            }

            Add(sender, pageHistoryUnit);
        }


        public void Add(object? sender, PageHistoryUnit unit)
        {
            // NOTE: 履歴操からの操作では履歴を変更しない
            if (sender == this) return;

            _history.TrimEnd(PageHistoryUnit.Empty);

            if (unit != _history.GetCurrent())
            {
                _history.Add(unit);
            }
        }

        public bool CanMoveToPrevious()
        {
            return _history.CanPrevious();
        }

        public void MoveToPrevious()
        {
            if (!_history.CanPrevious()) return;

            var query = _history.GetPrevious();
            LoadPage(query);
            _history.Move(-1);
        }

        public bool CanMoveToNext()
        {
            return _history.CanNext();
        }

        public void MoveToNext()
        {
            if (!_history.CanNext()) return;

            var query = _history.GetNext();
            LoadPage(query);
            _history.Move(+1);
        }

        public void MoveToHistory(KeyValuePair<int, PageHistoryUnit> item)
        {
            var query = _history.GetHistory(item.Key);
            LoadPage(query);
            _history.SetCurrent(item.Key + 1);
        }

        private void LoadPage(PageHistoryUnit unit)
        {
            if (unit.IsEmpty()) return;

            if (BookOperation.Current.Address == unit.BookAddress)
            {
                BookOperation.Current.JumpPageWithPath(this, unit.EntryFullName);
            }
            else
            {
                var option = BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe;
                BookHub.Current.RequestLoad(this, unit.BookAddress, unit.PageName, option, true);
            }
        }

        internal List<KeyValuePair<int, PageHistoryUnit>> GetHistory(int direction, int size)
        {
            return _history.GetHistory(direction, size);
        }
    }
}


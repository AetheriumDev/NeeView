﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using NeeView.Windows;

namespace NeeView
{
    public partial class BookmarkListView : UserControl, IHasFolderListBox
    {
        private readonly BookmarkListViewModel? _vm;


        public BookmarkListView(FolderList model)
        {
            InitializeComponent();

            this.FolderTree.Model = new BookmarkFolderTreeModel(model);

            _vm = new BookmarkListViewModel(model);
            this.Root.DataContext = _vm;

            model.FolderTreeFocus += FolderList_FolderTreeFocus;
        }


        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            if (_vm is null) return;

            base.OnDpiChanged(oldDpi, newDpi);
            _vm.SetDpiScale(newDpi);
        }

        /// <summary>
        /// フォルダーツリーへのフォーカス要求
        /// </summary>
        private void FolderList_FolderTreeFocus(object? sender, EventArgs e)
        {
            if (_vm is null) return;
            if (!_vm.Model.FolderListConfig.IsFolderTreeVisible) return;

            this.FolderTree.FocusSelectedItem();
        }

        private void BookmarkListView_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void Root_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_vm is null) return;

            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (_vm.IsLRKeyEnabled() && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }
        }

        private void Grid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (_vm is null) return;

            if (e.WidthChanged)
            {
                _vm.Model.AreaWidth = e.NewSize.Width;
            }
            if (e.HeightChanged)
            {
                _vm.Model.AreaHeight = e.NewSize.Height;
            }
        }

        public void SetFolderListBoxContent(FolderListBox content)
        {
            this.ListBoxContent.Content = content;
        }
    }
}

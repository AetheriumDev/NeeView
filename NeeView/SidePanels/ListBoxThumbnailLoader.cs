﻿using NeeLaboratory.Linq;
using NeeView.Windows.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ListBoxのページサムネイルを読み込む機能
    /// </summary>
    public class ListBoxThumbnailLoader
    {
        private readonly IPageListPanel _panel;
        private readonly PageThumbnailJobClient _jobClient;

        public ListBoxThumbnailLoader(IPageListPanel panelListBox, PageThumbnailJobClient jobClient)
        {
            _panel = panelListBox;
            _jobClient = jobClient;

            _panel.PageCollectionListBox.Loaded += ListBox_Loaded; ;
            _panel.PageCollectionListBox.IsVisibleChanged += ListBox_IsVisibleChanged;
            _panel.PageCollectionListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged));
        }

        private void ListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Load();
            }
            else
            {
                Unload();
            }
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        public void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Load();
        }

        public void Load()
        {
            if (!_panel.IsThumbnailVisibled)
            {
                return;
            }

            if (!_panel.PageCollectionListBox.IsVisible)
            {
                return;
            }

            var listBoxItems = VisualTreeUtility.FindVisualChildren<ListBoxItem>(_panel.PageCollectionListBox);
            if (listBoxItems == null || listBoxItems.Count <= 0)
            {
                return;
            }

            // 有効な ListBoxItem 収集
            var items = _panel.CollectPageList(listBoxItems.Select(i => i.DataContext)).ToList();

            var pages = items
                .Select(e => e.GetPage())
                .WhereNotNull()
                .ToList();

            if (pages.Any())
            {
                _jobClient?.Order(pages);
            }

            ////Debug.WriteLine($"ThumbLoad: {pages.Count}");
        }

        public void Unload()
        {
            _jobClient?.CancelOrder();
        }
    }
}

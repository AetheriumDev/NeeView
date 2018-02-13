﻿using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Configure
{
    /// <summary>
    /// 設定ウィンドウのページ
    /// </summary>
    public class SettingPage : BindableBase
    {
        private UIElement _content;
        private bool _isSelected;

        public SettingPage(string header)
        {
            this.Header = header;
        }

        public SettingPage(string header, List<SettingItem> items)
            : this(header)
        {
            this.Items = items;
        }

        public SettingPage(string header, List<SettingItem> items, params SettingPage[] children)
            : this(header, items)
        {
            this.Children = children.Where(e => e != null).ToList();
        }

        /// <summary>
        /// ページ名
        /// </summary>
        public string Header { get; private set; }

        /// <summary>
        /// 子ページ
        /// </summary>
        public List<SettingPage> Children { get; protected set; }

        /// <summary>
        /// 項目
        /// </summary>
        public List<SettingItem> Items { get; protected set; }

        /// <summary>
        /// TreeViewで、このノードが選択されているか
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { if (_isSelected != value) { _isSelected = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 表示コンテンツ
        /// </summary>
        public UIElement Content
        {
            get { return _content ?? (_content = CreateContent()); }
        }

        /// <summary>
        /// スクロールビュー？
        /// </summary>
        public bool IsScrollEnabled { get; set; } = true;


        /// <summary>
        /// 表示ページ。
        /// コンテンツがない場合、子のページを返す
        /// </summary>
        public SettingPage DispPage
        {
            get { return (this.Items != null) ? this : this.Children?.FirstOrDefault(); }
        }

        //
        private UIElement CreateContent()
        {
            if (this.Items == null)
            {
                return null;
            }

            var dockPanel = new DockPanel();
            dockPanel.MinWidth = 256;

            foreach (var item in this.Items)
            {
                var itemContent = item.CreateContent();
                if (itemContent != null)
                {
                    DockPanel.SetDock(itemContent, Dock.Top);
                    dockPanel.Children.Add(itemContent);
                }
            }

            if (this.IsScrollEnabled)
            {
                dockPanel.Margin = new Thickness(20, 0, 20, 20);
                dockPanel.LastChildFill = false;

                var scrollViewer = new ScrollViewer();
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                scrollViewer.Content = dockPanel;
                return scrollViewer;
            }
            else
            {
                dockPanel.Margin = new Thickness(20, 0, 0, 0);
                return dockPanel;
            }
        }
    }

}

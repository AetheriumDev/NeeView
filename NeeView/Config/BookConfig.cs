﻿using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Text;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;

namespace NeeView
{
    public class BookConfig : BindableBase
    {
        public static StringCollection DefaultExcludes { get; } = new StringCollection("__MACOSX;.DS_Store");


        private double _wideRatio = 1.0;
        private StringCollection _excludes = (StringCollection)DefaultExcludes.Clone();
        private bool _isMultiplePageMove = true;
        private PageEndAction _pageEndAction;
        private bool _isNotifyPageLoop;
        private bool _isConfirmRecursive;
        private double _contentSpace = -1.0;
        private string? _terminalSound;
        private bool _isAutoRecursive = false;
        private bool _isSortFileFirst;
        private double _bookPageSize = 300.0;
        private bool _resetPageWhenRandomSort;
        private bool _isInsertDummyPage;


        /// <summary>
        /// 横長画像判定用比率
        /// </summary>
        [PropertyMember]
        public double WideRatio
        {
            get { return _wideRatio; }
            set { SetProperty(ref _wideRatio, value); }
        }

        /// <summary>
        /// 除外フォルダー
        /// </summary>
        [PropertyMember]
        public StringCollection Excludes
        {
            get { return _excludes; }
            set { SetProperty(ref _excludes, value); }
        }

        // 2ページコンテンツの隙間
        [DefaultValue(-1.0)]
        [PropertyRange(-32, 32, TickFrequency = 1)]
        public double ContentsSpace
        {
            get { return _contentSpace; }
            set { SetProperty(ref _contentSpace, value); }
        }

        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        [PropertyMember]
        public bool IsPrioritizePageMove { get; set; } = true;

        /// <summary>
        /// ページ移動命令重複許可
        /// </summary>
        [PropertyMember]
        public bool IsMultiplePageMove
        {
            get { return _isMultiplePageMove; }
            set { SetProperty(ref _isMultiplePageMove, value); }
        }

        // ページ終端でのアクション
        [PropertyMember]
        public PageEndAction PageEndAction
        {
            get { return _pageEndAction; }
            set { SetProperty(ref _pageEndAction, value); }
        }

        [PropertyMember]
        public bool IsNotifyPageLoop
        {
            get { return _isNotifyPageLoop; }
            set { SetProperty(ref _isNotifyPageLoop, value); }
        }

        [PropertyPath(Filter = "Wave|*.wav")]
        public string? TerminalSound
        {
            get { return _terminalSound; }
            set { SetProperty(ref  _terminalSound , string.IsNullOrWhiteSpace(value) ? null : value); }
        }

        // 再帰を確認する
        [PropertyMember]
        public bool IsConfirmRecursive
        {
            get { return _isConfirmRecursive; }
            set { SetProperty(ref _isConfirmRecursive, value); }
        }

        // 自動再帰
        [PropertyMember]
        public bool IsAutoRecursive
        {
            get { return _isAutoRecursive; }
            set { SetProperty(ref _isAutoRecursive, value); }
        }

        // ファイル並び順、ファイル優先
        [PropertyMember]
        public bool IsSortFileFirst
        {
            get { return _isSortFileFirst; }
            set { SetProperty(ref _isSortFileFirst, value); }
        }

        // ブックページ画像サイズ
        [PropertyRange(100.0, 600.0, TickFrequency = 10.0, IsEditable = true, Format = "{0} × {0}")]
        public double BookPageSize
        {
            get { return _bookPageSize; }
            set { SetProperty(ref _bookPageSize, Math.Max(value, 64.0)); }
        }

        // ランダムソートでページをリセット
        [PropertyMember]
        public bool ResetPageWhenRandomSort
        {
            get { return _resetPageWhenRandomSort; }
            set { SetProperty(ref _resetPageWhenRandomSort, value); }
        }

        // ダミーページの挿入
        [PropertyMember]
        public bool IsInsertDummyPage
        {
            get { return _isInsertDummyPage; }
            set { SetProperty(ref _isInsertDummyPage, value); }
        }

    }
}

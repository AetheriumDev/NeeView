﻿using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Text;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace NeeView
{
    public class BookConfig : BindableBase
    {
        public static StringCollection DefaultExcludes { get; } = new StringCollection("__MACOSX;.DS_Store");

        private double _wideRatio = 1.0;
        private StringCollection _excludes = (StringCollection)DefaultExcludes.Clone();
        private PageEndAction _pageEndAction;
        private bool _isPrioritizeBookMove = false;
        private bool _isPrioritizePageMove = true;
        private bool _isReadyToPageMove;
        private bool _isNotifyPageLoop;
        private bool _isConfirmRecursive;
        private double _contentSpace = -1.0;
        private double _frameSpace = -1.0;
        private string? _terminalSound;
        private bool _isAutoRecursive = false;
        private bool _isSortFileFirst;
        private bool _resetPageWhenRandomSort;
        private bool _isInsertDummyPage;
        private bool _isInsertDummyFirstPage = false;
        private bool _isInsertDummyLastPage = true;
        private Color _dummyPageColor = Colors.White;
        private bool _isPanorama;
        private PageFrameOrientation _orientation = PageFrameOrientation.Horizontal;


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

        /// <summary>
        /// フレームの接続をパノラマにする
        /// </summary>
        [PropertyMember]
        public bool IsPanorama
        {
            get { return _isPanorama; }
            set { SetProperty(ref _isPanorama, value); }
        }

        /// <summary>
        /// フレームの並び方向
        /// </summary>
        [PropertyMember]
        public PageFrameOrientation Orientation
        {
            get { return _orientation; }
            set { SetProperty(ref _orientation, value); }
        }

        // 2ページコンテンツの隙間
        [DefaultValue(-1.0)]
        [PropertyRange(-32, 32, TickFrequency = 1)]
        public double ContentsSpace
        {
            get { return _contentSpace; }
            set { SetProperty(ref _contentSpace, value); }
        }

        // フレームの間隔
        [DefaultValue(-1.0)]
        [PropertyRange(-32, 32, TickFrequency = 1)]
        public double FrameSpace
        {
            get { return _frameSpace; }
            set { SetProperty(ref _frameSpace, value); }
        }

        /// <summary>
        /// ブック移動優先設定
        /// </summary>
        [PropertyMember]
        public bool IsPrioritizeBookMove
        {
            get { return _isPrioritizeBookMove; }
            set { SetProperty(ref _isPrioritizeBookMove, value); }
        }

        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        [PropertyMember]
        public bool IsPrioritizePageMove
        {
            get { return _isPrioritizePageMove; }
            set { SetProperty(ref _isPrioritizePageMove, value); }
        }

        /// <summary>
        /// 表示準備ができてからページを移動する
        /// </summary>
        [PropertyMember]
        public bool IsReadyToPageMove
        {
            get { return _isReadyToPageMove; }
            set { SetProperty(ref _isReadyToPageMove, value); }
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
            set { SetProperty(ref _terminalSound, string.IsNullOrWhiteSpace(value) ? null : value); }
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

        [PropertyMember]
        public bool IsInsertDummyFirstPage
        {
            get { return _isInsertDummyFirstPage; }
            set { SetProperty(ref _isInsertDummyFirstPage, value); }
        }

        [PropertyMember]
        public bool IsInsertDummyLastPage
        {
            get { return _isInsertDummyLastPage; }
            set { SetProperty(ref _isInsertDummyLastPage, value); }
        }

        // ダミーページ色
        [PropertyMember]
        public Color DummyPageColor
        {
            get { return _dummyPageColor; }
            set { SetProperty(ref _dummyPageColor, value); }
        }

        #region Obsolete

        /// <summary>
        /// ページ移動命令重複許可
        /// </summary>
        [Obsolete("no used"), Alternative(null, 40, ScriptErrorLevel.Info)] // ver.40
        [JsonIgnore]
        public bool IsMultiplePageMove
        {
            get { return true; }
            set { }
        }

        // ブックページ画像サイズ
        [Obsolete("no used"), Alternative(null, 40, ScriptErrorLevel.Info)] // ver.40
        [JsonIgnore]
        public double BookPageSize
        {
            get { return 300.0; }
            set { }
        }

        #endregion
    }
}

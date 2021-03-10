﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class HistoryConfig : BindableBase, IHasPanelListItemStyle
    {
        private PanelListItemStyle _panelListItemStyle = PanelListItemStyle.Content;
        private bool _isSaveHistory = true;
        private bool _isKeepFolderStatus = true;
        private bool _isKeepSearchHistory = true;
        private bool _isInnerArchiveHistoryEnabled = true;
        private bool _isUncHistoryEnabled = true;
        private bool _isForceUpdateHistory;
        private int _historyEntryPageCount = 0;
        private int _limitSize = -1;
        private TimeSpan _limitSpan;
        private bool _isCurrentFolder;
        private bool _isAutoCleanupEnabled;

        [JsonInclude, JsonPropertyName(nameof(HistoryFilePath))]
        public string _historyFilePath;


        [PropertyMember]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        // 履歴データの保存
        [PropertyMember]
        public bool IsSaveHistory
        {
            get { return _isSaveHistory; }
            set { SetProperty(ref _isSaveHistory, value); }
        }

        // 履歴データの保存場所
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "JSON|*.json")]
        public string HistoryFilePath
        {
            get { return _historyFilePath ?? SaveData.DefaultHistoryFilePath; }
            set { SetProperty(ref _historyFilePath, (string.IsNullOrWhiteSpace(value) || value.Trim() == SaveData.DefaultHistoryFilePath) ? null : value.Trim()); }
        }

        // フォルダーリストの情報記憶
        [PropertyMember]
        public bool IsKeepFolderStatus
        {
            get { return _isKeepFolderStatus; }
            set { SetProperty(ref _isKeepFolderStatus, value); }
        }

        // 検索履歴の情報記憶
        [PropertyMember]
        public bool IsKeepSearchHistory
        {
            get { return _isKeepSearchHistory; }
            set { SetProperty(ref _isKeepSearchHistory, value); }
        }

        /// <summary>
        /// アーカイブ内アーカイブの履歴保存
        /// </summary>
        [PropertyMember]
        public bool IsInnerArchiveHistoryEnabled
        {
            get { return _isInnerArchiveHistoryEnabled; }
            set { SetProperty(ref _isInnerArchiveHistoryEnabled, value); }
        }

        /// <summary>
        /// UNCパスの履歴保存
        /// </summary>
        [PropertyMember]
        public bool IsUncHistoryEnabled
        {
            get { return _isUncHistoryEnabled; }
            set { SetProperty(ref _isUncHistoryEnabled, value); }
        }

        /// <summary>
        /// 履歴閲覧でも履歴登録日を更新する
        /// </summary>
        [PropertyMember]
        public bool IsForceUpdateHistory
        {
            get { return _isForceUpdateHistory; }
            set { SetProperty(ref _isForceUpdateHistory, value); }
        }

        /// <summary>
        /// 何回ページを切り替えたら履歴登録するか
        /// </summary>
        [PropertyMember]
        public int HistoryEntryPageCount
        {
            get { return _historyEntryPageCount; }
            set { SetProperty(ref _historyEntryPageCount, value); }
        }

        // 履歴制限
        [PropertyMember]
        public int LimitSize
        {
            get { return _limitSize; }
            set { SetProperty(ref _limitSize, value); }
        }

        // 履歴制限(時間)
        [PropertyMember]
        public TimeSpan LimitSpan
        {
            get { return _limitSpan; }
            set { SetProperty(ref _limitSpan, value); }
        }

        // ブックのあるフォルダーのみ
        [PropertyMember]
        public bool IsCurrentFolder
        {
            get { return _isCurrentFolder; }
            set { SetProperty(ref _isCurrentFolder, value); }
        }

        // 履歴の自動削除
        [PropertyMember]
        public bool IsAutoCleanupEnabled
        {
            get { return _isAutoCleanupEnabled; }
            set { SetProperty(ref _isAutoCleanupEnabled, value); }
        }


    }
}


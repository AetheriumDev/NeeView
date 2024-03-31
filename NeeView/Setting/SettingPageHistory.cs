﻿using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Windows;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: History
    /// </summary>
    public class SettingPageHistory : SettingPage
    {
        public SettingPageHistory() : base(Properties.TextResources.GetString("SettingPage.History"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageHistoryPageView(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.History.General"));
            section.Children.Add(new SettingItemIndexValue<int>(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.HistoryEntryPageCount)), new HistoryEntryPageCount(), true));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsInnerArchiveHistoryEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsUncHistoryEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsForceUpdateHistory))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsAutoCleanupEnabled))));
            section.Children.Add(new SettingItemButton(Properties.TextResources.GetString("SettingPage.History.GeneralDelete"), Properties.TextResources.GetString("SettingPage.History.GeneralDeleteButton"), RemoveHistory));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.History.GeneralLimit"), Properties.TextResources.GetString("SettingPage.History.GeneralLimit.Remarks"));
            section.Children.Add(new SettingItemIndexValue<int>(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.LimitSize)), new HistoryLimitSize(), false));
            section.Children.Add(new SettingItemIndexValue<TimeSpan>(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.LimitSpan)), new HistoryLimitSpan(), false));
            this.Items.Add(section);
        }

        #region Commands

        private RelayCommand<UIElement>? _RemoveHistory;
        public RelayCommand<UIElement> RemoveHistory
        {
            get { return _RemoveHistory = _RemoveHistory ?? new RelayCommand<UIElement>(RemoveHistory_Executed); }
        }

        private void RemoveHistory_Executed(UIElement? element)
        {
            BookHistoryCollection.Current.Clear();

            var dialog = new MessageDialog("", Properties.TextResources.GetString("HistoryDeletedDialog.Title"));
            if (element != null)
            {
                dialog.Owner = Window.GetWindow(element);
            }
            dialog.ShowDialog();
        }

        #endregion

        #region IndexValues

        #region IndexValue

        /// <summary>
        /// 履歴登録開始テーブル
        /// </summary>
        public class HistoryEntryPageCount : IndexIntValue
        {
            private static readonly List<int> _values = new()
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 50, 100,
            };

            public HistoryEntryPageCount() : base(_values)
            {
                IsValueSyncIndex = false;
            }

            public HistoryEntryPageCount(int value) : base(_values)
            {
                IsValueSyncIndex = false;
                Value = value;
            }

            public override string ValueString => $"{Value} {Properties.TextResources.GetString("Word.Page")}";
        }

        #endregion

        /// <summary>
        /// 履歴サイズテーブル
        /// </summary>
        public class HistoryLimitSize : IndexIntValue
        {
            private static readonly List<int> _values = new()
            {
                0, 1, 10, 20, 50, 100, 200, 500, 1000, -1
            };

            public HistoryLimitSize() : base(_values)
            {
            }

            public HistoryLimitSize(int value) : base(_values)
            {
                Value = value;
            }

            public override string ValueString => Value == -1 ? Properties.TextResources.GetString("Word.NoLimit") : Value.ToString();
        }

        /// <summary>
        /// 履歴期限テーブル
        /// </summary>
        public class HistoryLimitSpan : IndexTimeSpanValue
        {
            private static readonly List<TimeSpan> _values = new() {
                TimeSpan.FromDays(1),
                TimeSpan.FromDays(2),
                TimeSpan.FromDays(3),
                TimeSpan.FromDays(7),
                TimeSpan.FromDays(15),
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(100),
                TimeSpan.FromDays(365),
                default,
            };

            public HistoryLimitSpan() : base(_values)
            {
            }

            public HistoryLimitSpan(TimeSpan value) : base(_values)
            {
                Value = value;
            }

            public override string ValueString => Value == default ? Properties.TextResources.GetString("Word.NoLimit") : Properties.TextResources.GetFormatString("Word.DaysAgo", Value.Days);
        }

        #endregion
    }


    /// <summary>
    /// Setting: HistoryPageView
    /// </summary>
    public class SettingPageHistoryPageView : SettingPage
    {
        public SettingPageHistoryPageView() : base(Properties.TextResources.GetString("SettingPage.History.PageViewRecord"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.History.PageViewRecord"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageViewRecorder, nameof(PageViewRecorderConfig.IsSavePageViewRecord))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageViewRecorder, nameof(PageViewRecorderConfig.PageViewRecordFilePath))) { IsStretch = true });

            this.Items = new List<SettingItem>() { section };
        }
    }
}

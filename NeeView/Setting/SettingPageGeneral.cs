﻿using NeeLaboratory.Collection;
using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: General
    /// </summary>
    public class SettingPageGeneral : SettingPage
    {
        public SettingPageGeneral() : base(Properties.TextResources.GetString("SettingPage.General"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageStartUp(),
                new SettingPageSaveData(),
                new SettingPageMemoryAndPerformance(),
                new SettingPageThumbnail(),
                new SettingPageNotify(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.General"));
            var cultureMap = Properties.TextResources.LanguageResource.Cultures.ToKeyValuePairList(e => e.Name, e => e.NativeName);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.Language), new PropertyMemberElementOptions() { StringMap = cultureMap })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.DateTimeFormat))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.General.FileAccess"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsFileWriteAccessEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsRemoveConfirmed))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsRemoveWantNukeWarning))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.General.Search"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsIncrementalSearchEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.SearchHistorySize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsKeepSearchHistory))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.General.Environment"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsNaturalSortEnabled))));
            if (!Environment.IsAppxPackage)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsNetworkEnabled))));
            }
            if (Environment.IsZipLikePackage)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(ExplorerContextMenu.Current, nameof(ExplorerContextMenu.IsEnabled))));
            }
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.WebBrowser))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.TextEditor))) { IsStretch = true });
            this.Items.Add(section);
        }
    }


    /// <summary>
    /// Setting: StartUp
    /// </summary>
    public class SettingPageStartUp : SettingPage
    {
        public SettingPageStartUp() : base(Properties.TextResources.GetString("SettingPage.General.Boot"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.General.Boot"), Properties.TextResources.GetString("SettingPage.General.Boot.Remarks"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsSplashScreenEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsMultiBootEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreWindowPlacement))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsRestoreAeroSnapPlacement)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreWindowPlacement))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreFullScreen)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreWindowPlacement))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsOpenLastBook))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsOpenLastFolder))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsAutoPlaySlideShow))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreSecondWindowPlacement))));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: SaveData
    /// </summary>
    public class SettingPageSaveData : SettingPage
    {
        public SettingPageSaveData() : base(Properties.TextResources.GetString("SettingPage.General.SaveData"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.General.SaveDataTypes"), Properties.TextResources.GetString("SettingPage.General.SaveDataTypes.Remarks"));

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsSaveHistory))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.HistoryFilePath)))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.History, nameof(HistoryConfig.IsSaveHistory))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.IsSaveBookmark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.BookmarkFilePath)))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Bookmark, nameof(BookmarkConfig.IsSaveBookmark))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Playlist, nameof(PlaylistConfig.PlaylistFolder)))
            {
                IsStretch = true,
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsSyncUserSetting))));
  
            if (!Environment.IsAppxPackage)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsSettingBackup))));
            }

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ThumbnailCacheFilePath))) { IsStretch = true });

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.TemporaryDirectory))) { IsStretch = true });

            if (Environment.ConfigType == "Debug" || (Environment.IsUseLocalApplicationDataFolder && !Environment.IsAppxPackage))
            {
                section.Children.Add(new SettingItemButton(Properties.TextResources.GetString("SettingPage.General.SaveDataRemove"), Properties.TextResources.GetString("SettingItem.Remove"), RemoveAllData) { Tips = Properties.TextResources.GetString("SettingItem.Remove.Remarks"), });
            }

            this.Items = new List<SettingItem>() { section };
        }

        #region Commands

        private RelayCommand<UIElement>? _RemoveAllData;
        public RelayCommand<UIElement> RemoveAllData
        {
            get { return _RemoveAllData = _RemoveAllData ?? new RelayCommand<UIElement>(RemoveAllData_Executed); }
        }

        private void RemoveAllData_Executed(UIElement? element)
        {
            var window = element != null ? Window.GetWindow(element) : null;
            Environment.RemoveApplicationData(window);
        }

        #endregion
    }


    /// <summary>
    /// Setting: MemoryAndPerformance
    /// </summary>
    public class SettingPageMemoryAndPerformance : SettingPage
    {
        public SettingPageMemoryAndPerformance() : base(Properties.TextResources.GetString("SettingPage.MemoryAndPerformance"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.MemoryAndPerformance"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.CacheMemorySize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.PreLoadSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.JobWorkerSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.MaximumSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.IsLimitSourceSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.IsLoadingPageVisible))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.PreExtractSolidSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ThumbnailBookCapacity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ThumbnailPageCapacity))));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: Thumbnail
    /// </summary>
    public class SettingPageThumbnail : SettingPage
    {
        public SettingPageThumbnail() : base(Properties.TextResources.GetString("SettingPage.Thumbnail"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Thumbnail.Cache"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.IsCacheEnabled))));
            section.Children.Add(new SettingItemIndexValue<TimeSpan>(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.CacheLimitSpan)), new CacheLimitSpan(), false));
            section.Children.Add(new SettingItemButton(Properties.TextResources.GetString("SettingPage.Thumbnail.CacheClear"), Properties.TextResources.GetString("SettingPage.Thumbnail.CacheClearButton"), RemoveCache));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Thumbnail.Advance"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.Format))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.Quality))));
#if USE_WINRT
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.IsVideoThumbnailEnabled))) { IsEnabled = new IsEnabledPropertyValue(ThumbnailConfig.IsVideoThumbnailSupported)});
#endif
            this.Items.Add(section);
        }

        #region Commands

        private RelayCommand<UIElement>? _RemoveCache;
        public RelayCommand<UIElement> RemoveCache
        {
            get { return _RemoveCache = _RemoveCache ?? new RelayCommand<UIElement>(RemoveCache_Executed); }
        }

        private void RemoveCache_Executed(UIElement? element)
        {
            try
            {
                ThumbnailCache.Current.Remove();

                var dialog = new MessageDialog("", Properties.TextResources.GetString("CacheDeletedDialog.Title"));
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, Properties.TextResources.GetString("CacheDeletedFailedDialog.Title"));
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
        }

        #endregion

        /// <summary>
        /// 履歴期限テーブル
        /// </summary>
        public class CacheLimitSpan : IndexTimeSpanValue
        {
            private static readonly List<TimeSpan> _values = new() {
                TimeSpan.FromDays(2),
                TimeSpan.FromDays(3),
                TimeSpan.FromDays(7),
                TimeSpan.FromDays(15),
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(100),
                TimeSpan.FromDays(365),
                default,
            };

            public CacheLimitSpan() : base(_values)
            {
            }

            public CacheLimitSpan(TimeSpan value) : base(_values)
            {
                Value = value;
            }

            public override string ValueString => Value == default ? Properties.TextResources.GetString("Word.NoLimit") : Properties.TextResources.GetFormatString("Word.DaysAgo", Value.Days);
        }
    }


    /// <summary>
    /// Setting: Notify
    /// </summary>
    public class SettingPageNotify : SettingPage
    {
        public SettingPageNotify() : base(Properties.TextResources.GetString("SettingPage.Notify"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Notify.Display"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.NoticeShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.BookNameShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.CommandShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.GestureShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.NowLoadingShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.ViewTransformShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.IsOriginalScaleShowMessage))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.IsEmptyMessageEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.IsBusyMarkEnabled))));

            this.Items = new List<SettingItem>() { section };
        }
    }

}

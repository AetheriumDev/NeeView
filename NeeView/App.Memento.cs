﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public partial class App : Application, INotifyPropertyChanged
    {
        // ここでのパラメータは値の保持のみを行う。機能は提供しない。

        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember, DefaultValue(false)]
            public bool IsMultiBootEnabled { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsSaveFullScreen { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSaveWindowPlacement { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsNetworkEnabled { get; set; }

            [Obsolete("no used"), DataMember(EmitDefaultValue = false)]
            public bool IsDisableSave { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSaveHistory { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string? HistoryFilePath { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSaveBookmark { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string? BookmarkFilePath { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSavePagemark { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string? PagemarkFilePath { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsIgnoreImageDpi { get; set; }

            [Obsolete("no used"), DataMember(EmitDefaultValue = false), DefaultValue(false)]
            public bool IsIgnoreWindowDpi { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsRestoreSecondWindow { get; set; }

            [Obsolete("no used"), DataMember(Name = "WindowChromeFrame", EmitDefaultValue = false)]
            public WindowChromeFrameV1 WindowChromeFrameV1 { get; set; }

            [DataMember(Name = "WindowChromeFrameV2"), DefaultValue(WindowChromeFrame.WindowFrame)]
            public WindowChromeFrame WindowChromeFrame { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double AutoHideDelayTime { get; set; }

            [DataMember, DefaultValue(0.0)]
            public double AutoHideDelayVisibleTime { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsOpenLastBook { get; set; }

            [DataMember, DefaultValue("")]
            public string? DownloadPath { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsSettingBackup { get; set; }

            [DataMember]
            public string? Language { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSplashScreenEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSyncUserSetting { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string? TemporaryDirectory { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string? CacheDirectory { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string? CacheDirectoryOld { get; set; }

            [DataMember, DefaultValue(AutoHideFocusLockMode.LogicalTextBoxFocusLock)]
            public AutoHideFocusLockMode AutoHideFocusLockMode { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsAutoHideKeyDownDelay { get; set; }

            [DataMember, DefaultValue(32.0)]
            public double AutoHideHitTestMargin { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();

                this.Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            }

#pragma warning disable CS0618

            [OnDeserialized]
            public void OnDeserialized(StreamingContext c)
            {
                // before ver.34
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    WindowChromeFrame = WindowChromeFrameV1 == WindowChromeFrameV1.None ? WindowChromeFrame.None : WindowChromeFrame.WindowFrame;
                }
            }

#pragma warning restore CS0618

            public void RestoreConfig(Config config)
            {
                // ver 37.0
                config.StartUp.IsMultiBootEnabled = IsMultiBootEnabled;
                config.StartUp.IsRestoreFullScreen = IsSaveFullScreen;
                config.StartUp.IsRestoreWindowPlacement = IsSaveWindowPlacement;
                config.StartUp.IsRestoreSecondWindowPlacement = IsRestoreSecondWindow;
                config.System.IsNetworkEnabled = IsNetworkEnabled;
                config.StartUp.IsOpenLastBook = IsOpenLastBook;
                config.System.Language = Language == "Japanese" ? "ja" : "en";
                config.StartUp.IsSplashScreenEnabled = IsSplashScreenEnabled;
                config.History.IsSaveHistory = IsSaveHistory;
                config.History.HistoryFilePath = HistoryFilePath ?? "";
                config.Bookmark.IsSaveBookmark = IsSaveBookmark;
                config.Bookmark.BookmarkFilePath = BookmarkFilePath ?? "";
#pragma warning disable CS0618
                if (config.PagemarkLegacy != null)
                {
                    config.PagemarkLegacy.IsSavePagemark = IsSavePagemark;
                    config.PagemarkLegacy.PagemarkFilePath = PagemarkFilePath ?? "";
                }
#pragma warning restore CS0618
                config.System.IsSettingBackup = IsSettingBackup;
                config.System.IsSyncUserSetting = IsSyncUserSetting;
                config.System.TemporaryDirectory = TemporaryDirectory = ""; ;
                config.Thumbnail.ThumbnailCacheFilePath = CacheDirectory != null ? Path.Combine(CacheDirectory, ThumbnailCache.ThumbnailCacheFileName) : "";
                config.System.IsIgnoreImageDpi = IsIgnoreImageDpi;
                config.AutoHide.AutoHideDelayTime = AutoHideDelayTime;
                config.AutoHide.AutoHideDelayVisibleTime = AutoHideDelayVisibleTime;
                config.AutoHide.AutoHideFocusLockMode = AutoHideFocusLockMode;
                config.AutoHide.IsAutoHideKeyDownDelay = IsAutoHideKeyDownDelay;
                config.AutoHide.AutoHideHitTestHorizontalMargin = AutoHideHitTestMargin;
                config.AutoHide.AutoHideHitTestVerticalMargin = AutoHideHitTestMargin;
                config.System.DownloadPath = DownloadPath ?? "";
            }
        }

        #endregion

    }
}

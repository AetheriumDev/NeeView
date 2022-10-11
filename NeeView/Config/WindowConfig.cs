﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    public class WindowConfig : BindableBase
    {
        private bool _isTopmost = false;
        private double _maximizeWindowGapWidth = 8.0;
        private WindowStateEx _state;
        private bool _isCaptionEmulateInFullScreen;
        private bool _mouseActivateAndEat;
        private bool _isAeroSnapPlacementEnabled = true;
        private bool _isAutoHideInNormal = false;
        private bool _isAutoHideInMaximized = false;
        private bool _IsAutoHideInFullScreen = true;


        [PropertyMember]
        public bool IsTopmost
        {
            get { return _isTopmost; }
            set { SetProperty(ref _isTopmost, value); }
        }

        [PropertyMember]
        public bool IsCaptionEmulateInFullScreen
        {
            get { return _isCaptionEmulateInFullScreen; }
            set { SetProperty(ref _isCaptionEmulateInFullScreen, value); }
        }

        [Obsolete("no used"), Alternative(null, 40)] // ver.40
        [JsonIgnore]
        [PropertyRange(0, 16, TickFrequency = 1, IsEditable = true), DefaultValue(8.0)]
        public double MaximizeWindowGapWidth
        {
            get { return _maximizeWindowGapWidth; }
            set { SetProperty(ref _maximizeWindowGapWidth, value); }
        }

        [PropertyMember]
        public bool MouseActivateAndEat
        {
            get { return _mouseActivateAndEat; }
            set { SetProperty(ref _mouseActivateAndEat, value); }
        }

        /// <summary>
        /// ウィンドウ状態
        /// </summary>
        [PropertyMember]
        public WindowStateEx State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        /// <summary>
        /// エアロスナップのウィンドウ座標を保存
        /// </summary>
        [PropertyMember]
        public bool IsRestoreAeroSnapPlacement
        {
            get { return _isAeroSnapPlacementEnabled; }
            set { SetProperty(ref _isAeroSnapPlacementEnabled, value); }
        }

        [PropertyMember]
        public bool IsAutoHideInNormal
        {
            get { return _isAutoHideInNormal; }
            set { SetProperty(ref _isAutoHideInNormal, value); }
        }

        [PropertyMember]
        public bool IsAutoHidInMaximized
        {
            get { return _isAutoHideInMaximized; }
            set { SetProperty(ref _isAutoHideInMaximized, value); }
        }

        [PropertyMember]
        public bool IsAutoHideInFullScreen
        {
            get { return _IsAutoHideInFullScreen; }
            set { SetProperty(ref _IsAutoHideInFullScreen, value); }
        }


        #region HiddenParameters

        /// <summary>
        /// フルスクリーンから復帰するウィンドウ状態
        /// </summary>
        [PropertyMapIgnore]
        public WindowStateEx LastState { get; set; }

        /// <summary>
        /// 復元ウィンドウ座標
        /// </summary>
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public WindowPlacement? WindowPlacement { get; set; }

        #endregion HiddenParameters


        #region Obsolete

        [Obsolete("no used"), Alternative("Whindow.Border in the custom theme file", 39, IsFullName = true)] // ver.39
        [JsonIgnore]
        public WindowChromeFrame WindowChromeFrame
        {
            get { return WindowChromeFrame.None; }
            set { }
        }

        [Obsolete("no used"), Alternative(null, 39)] // ver.39
        [JsonIgnore]
        public bool IsCaptionVisible
        {
            get { return false; }
            set { }
        }

        #endregion Obsolete
    }

    [Obsolete("no used")] // ver.39
    public enum WindowChromeFrame
    {
        [AliasName]
        None,

        [AliasName]
        WindowFrame,
    }

}

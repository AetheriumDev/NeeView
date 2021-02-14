﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class WindowTitleConfig : BindableBase
    {
        private const string WindowTitleFormat1Default = "$Book ($Page / $PageMax) - $FullName";
        private const string WindowTitleFormat2Default = "$Book ($Page / $PageMax) - $FullNameL | $NameR";
        private const string WindowTitleFormatMediaDefault = "$Book";
        private string _windowTitleFormat1;
        private string _windowTitleFormat2;
        private string _windowTitleFormatMedia;


        /// <summary>
        /// ウィンドウタイトルフォーマット 1P用
        /// </summary>
        [PropertyMember]
        public string WindowTitleFormat1
        {
            get { return _windowTitleFormat1 ?? WindowTitleFormat1Default; }
            set { SetProperty(ref _windowTitleFormat1, CleanUpTitleFormat(value, WindowTitleFormat1Default)); }
        }

        /// <summary>
        /// ウィンドウタイトルフォーマット 2P用
        /// </summary>
        [PropertyMember]
        public string WindowTitleFormat2
        {
            get { return _windowTitleFormat2 ?? WindowTitleFormat2Default; }
            set { SetProperty(ref _windowTitleFormat2, CleanUpTitleFormat(value, WindowTitleFormat2Default)); }
        }

        /// <summary>
        /// ウィンドウタイトルフォーマット メディア用
        /// </summary>
        [PropertyMember]
        public string WindowTitleFormatMedia
        {
            get { return _windowTitleFormatMedia ?? WindowTitleFormatMediaDefault; }
            set { SetProperty(ref _windowTitleFormatMedia, CleanUpTitleFormat(value, WindowTitleFormatMediaDefault)); }
        }


        private string CleanUpTitleFormat(string source, string defaultFormat)
        {
            return string.IsNullOrEmpty(source) ? defaultFormat : source;
        }
    }
}



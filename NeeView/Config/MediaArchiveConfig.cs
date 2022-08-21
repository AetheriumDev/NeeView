﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class MediaArchiveConfig : BindableBase
    {
        public static FileTypeCollection DefaultSupportFileTypes { get; } = new FileTypeCollection(".asf;.avi;.mp4;.mkv;.mov;.wmv");


        private bool _isEnabled = true;
        private FileTypeCollection _supportFileTypes = (FileTypeCollection)DefaultSupportFileTypes.Clone();
        private double _pageSeconds = 10.0;
        private double _mediaStartDelaySeconds = 0.5;
        private bool _isMuted;
        private double _volume = 0.5;
        private bool _isRepeat;


        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        [PropertyMember]
        public FileTypeCollection SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }

        [PropertyMember]
        public double PageSeconds
        {
            get { return _pageSeconds; }
            set { SetProperty(ref _pageSeconds, value); }
        }

        [PropertyMember]
        public double MediaStartDelaySeconds
        {
            get { return _mediaStartDelaySeconds; }
            set { SetProperty(ref _mediaStartDelaySeconds, value); }
        }

        [PropertyMember]
        public bool IsMuted
        {
            get { return _isMuted; }
            set { SetProperty(ref _isMuted, value); }
        }

        [PropertyMember]
        public double Volume
        {
            get { return _volume; }
            set { SetProperty(ref _volume, value); }
        }

        [PropertyMember]
        public bool IsRepeat
        {
            get { return _isRepeat; }
            set { SetProperty(ref _isRepeat, value); }
        }

    }
}

﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class SlideShowConfig : BindableBase
    {
        private double _slideShowInterval = 5.0;
        private bool _isCancelSlideByMouseMove = true;
        private bool _isSlideShowByLoop = true;
        private bool _isTimerVisible;

        /// <summary>
        /// スライドショーの表示間隔(秒)
        /// </summary>
        [PropertyMember(HasDecimalPoint = true)]
        public double SlideShowInterval
        {
            get { return _slideShowInterval; }
            set { SetProperty(ref _slideShowInterval, value); }
        }

        /// <summary>
        /// カーソルでスライドを止める.
        /// </summary>
        [PropertyMember]
        public bool IsCancelSlideByMouseMove
        {
            get { return _isCancelSlideByMouseMove; }
            set { SetProperty(ref _isCancelSlideByMouseMove, value); }
        }

        /// <summary>
        /// ループ再生フラグ
        /// </summary>
        [PropertyMember]
        public bool IsSlideShowByLoop
        {
            get { return _isSlideShowByLoop; }
            set { SetProperty(ref _isSlideShowByLoop ,value); } 
        }

        /// <summary>
        /// 時間表示
        /// </summary>
        [PropertyMember]
        public bool IsTimerVisible
        {
            get { return _isTimerVisible; }
            set { SetProperty(ref _isTimerVisible, value); }
        }

    }
}

﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using NeeLaboratory.ComponentModel;
using NeeView.Windows;

namespace NeeView
{
    /// <summary>
    /// SidePanelFrame ViewModel
    /// </summary>
    public class SidePanelFrameViewModel : BindableBase
    {
        private bool _isAutoHide;
        private SidePanelFrame _model;


        public SidePanelFrameViewModel(SidePanelFrame model, LeftPanelViewModel left, RightPanelViewModel right)
        {
            _model = model;
            _model.VisibleAtOnceRequest += Model_VisibleAtOnceRequest;

            MainLayoutPanelManager = CustomLayoutPanelManager.Current;

            Left = left;
            Left.PropertyChanged += Left_PropertyChanged;

            Right = right;
            Right.PropertyChanged += Right_PropertyChanged;

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHidePanel),
                (s, e) => RaisePropertyChanged(nameof(Opacity)));

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.Opacity),
                (s, e) => RaisePropertyChanged(nameof(Opacity)));

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsSideBarEnabled),
                (s, e) => RaisePropertyChanged(nameof(IsSideBarVisible)));

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsLimitPanelWidth),
                (s, e) => RaisePropertyChanged(nameof(IsLimitPanelWidth)));

            MainLayoutPanelManager.DragBegin +=
                (s, e) => DragBegin(this, EventArgs.Empty);
            MainLayoutPanelManager.DragEnd +=
                (s, e) => DragEnd(this, EventArgs.Empty);

            SidePanelIconDescriptor = new SidePanelIconDescriptor(this);
        }


        public event EventHandler? PanelVisibilityChanged;


        public SidePanelIconDescriptor SidePanelIconDescriptor { get; }

        public bool IsSideBarVisible
        {
            get => Config.Current.Panels.IsSideBarEnabled;
            set => Config.Current.Panels.IsSideBarEnabled = value;
        }

        public double Opacity
        {
            get => MainWindowModel.Current.CanHidePanel ? Config.Current.Panels.Opacity : 1.0;
        }

        public GridLength LeftPanelWidth
        {
            get => new GridLength(this.Left.Width);
            set => this.Left.Width = value.Value;
        }

        public GridLength RightPanelWidth
        {
            get => new GridLength(this.Right.Width);
            set => this.Right.Width = value.Value;
        }

        public bool IsLeftPanelActived
        {
            get => this.Left.IsPanelActived;
        }

        public bool IsRightPanelActived
        {
            get => this.Right.IsPanelActived;
        }

        public bool IsLimitPanelWidth
        {
            get => Config.Current.Panels.IsLimitPanelWidth;
            set => Config.Current.Panels.IsLimitPanelWidth = value;
        }


        /// <summary>
        /// パネル表示リクエスト
        /// </summary>
        private void Model_VisibleAtOnceRequest(object? sender, VisibleAtOnceRequestEventArgs e)
        {
            VisibleAtOnce(e.Key);
        }

        /// <summary>
        /// パネルを一度だけ表示
        /// </summary>
        public void VisibleAtOnce(string key)
        {
            if (Left.SelectedItemContains(key))
            {
                Left.VisibleOnce();
            }
            else if (Right.SelectedItemContains(key))
            {
                Right.VisibleOnce();
            }
        }

        public bool IsAutoHide
        {
            get { return _isAutoHide; }
            set
            {
                if (_isAutoHide != value)
                {
                    _isAutoHide = value;
                    this.Left.IsAutoHide = value;
                    this.Right.IsAutoHide = value;
                    RaisePropertyChanged();
                    PanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public SidePanelFrame Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public SidePanelViewModel Left { get; private set; }

        public SidePanelViewModel Right { get; private set; }

        public App App => App.Current;

        public AutoHideConfig AutoHideConfig => Config.Current.AutoHide;

        public CustomLayoutPanelManager MainLayoutPanelManager { get; private set; }


        /// <summary>
        /// ドラッグ開始イベント処理.
        /// 強制的にパネル表示させる
        /// </summary>
        public void DragBegin(object? sender, EventArgs e)
        {
            Left.IsDragged = true;
            Right.IsDragged = true;
        }

        /// <summary>
        /// ドラッグ終了イベント処理
        /// </summary>
        public void DragEnd(object? sender, EventArgs e)
        {
            Left.IsDragged = false;
            Right.IsDragged = false;
        }


        /// <summary>
        /// 右パネルのプロパティ変更イベント処理
        /// </summary>
        private void Right_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Right.Width):
                    RaisePropertyChanged(nameof(RightPanelWidth));
                    break;
                case nameof(Right.PanelVisibility):
                    PanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                    break;
                case nameof(Right.IsPanelActived):
                    RaisePropertyChanged(nameof(IsRightPanelActived));
                    break;
            }
        }

        /// <summary>
        /// 左パネルのプロパティ変更イベント処理
        /// </summary>
        private void Left_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Left.Width):
                    RaisePropertyChanged(nameof(LeftPanelWidth));
                    break;
                case nameof(Left.PanelVisibility):
                    PanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                    break;
                case nameof(Left.IsPanelActived):
                    RaisePropertyChanged(nameof(IsLeftPanelActived));
                    break;
            }
        }
    }
}

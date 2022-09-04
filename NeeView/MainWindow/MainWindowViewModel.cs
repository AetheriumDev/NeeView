﻿using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace NeeView
{
    /// <summary>
    /// MainWindow : ViewModel
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        #region SidePanels

        /// <summary>
        /// SidePanelMargin property.
        /// メニュの自動非表示ON/OFFによるサイドパネル上部の余白
        /// </summary>
        public Thickness SidePanelMargin
        {
            get { return _SidePanelMargin; }
            set { if (_SidePanelMargin != value) { _SidePanelMargin = value; RaisePropertyChanged(); } }
        }

        //
        private Thickness _SidePanelMargin;

        //
        private void UpdateSidePanelMargin()
        {
            SidePanelMargin = new Thickness(0, _model.CanHideMenu ? 32 : 0, 0, _model.CanHidePageSlider ? 20 : 0);
        }


        /// <summary>
        /// CanvasWidth property.
        /// キャンバスサイズ。SidePanelから引き渡される
        /// </summary>
        public double CanvasWidth
        {
            get { return _CanvasWidth; }
            set { if (_CanvasWidth != value) { _CanvasWidth = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasWidth;


        /// <summary>
        /// CanvasHeight property.
        /// </summary>
        public double CanvasHeight
        {
            get { return _CanvasHeight; }
            set { if (_CanvasHeight != value) { _CanvasHeight = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasHeight;

        #endregion

        #region Window Icon

        // ウィンドウアイコン：標準
        private ImageSource? _windowIconDefault;

        // ウィンドウアイコン：スライドショー再生中
        private ImageSource? _windowIconPlay;

        // ウィンドウアイコン初期化
        private void InitializeWindowIcons()
        {
            _windowIconDefault = null;
            _windowIconPlay = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Play.ico", UriKind.RelativeOrAbsolute));
        }

        // 現在のウィンドウアイコン取得
        public ImageSource? WindowIcon
            => SlideShow.Current.IsPlayingSlideShow ? _windowIconPlay : _windowIconDefault;

        #endregion

        private MainWindowModel _model;

        private MainViewComponent _viewComponent;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MainWindowViewModel(MainWindowModel model)
        {
            _viewComponent = MainViewComponent.Current;

            MenuAutoHideDescription = new MenuAutoHideDescription(MainWindow.Current.LayerMenuSocket, MainWindow.Current.SidePanelFrame);
            StatusAutoHideDescrption = new StatusAutoHideDescription(MainWindow.Current.LayerStatusArea, MainWindow.Current.SidePanelFrame);
            ThumbnailListusAutoHideDescrption = new StatusAutoHideDescription(MainWindow.Current.LayerThumbnailListSocket, MainWindow.Current.SidePanelFrame);

            // icon
            InitializeWindowIcons();

            // mainwindow model
            _model = model;

            _model.AddPropertyChanged(nameof(_model.CanHideMenu),
                (s, e) => UpdateSidePanelMargin());

            _model.AddPropertyChanged(nameof(_model.CanHidePageSlider),
                (s, e) =>
                {
                    UpdateSidePanelMargin();
                    RaisePropertyChanged(nameof(CanHideThumbnailList));
                });

            _model.AddPropertyChanged(nameof(_model.CanHidePanel),
                (s, e) => UpdateSidePanelMargin());

            _model.FocusMainViewCall += Model_FocusMainViewCall;

            MainWindow.Current.Activated +=
                (s, e) => RaisePropertyChanged(nameof(IsMenuBarActive));

            MainWindow.Current.Deactivated +=
                (s, e) => RaisePropertyChanged(nameof(IsMenuBarActive));


            // SlideShow link to WindowIcon
            SlideShow.Current.AddPropertyChanged(nameof(SlideShow.IsPlayingSlideShow),
                (s, e) => RaisePropertyChanged(nameof(WindowIcon)));

            ThumbnailList.Current.AddPropertyChanged(nameof(CanHideThumbnailList),
                (s, e) => RaisePropertyChanged(nameof(CanHideThumbnailList)));

            BookHub.Current.BookChanged +=
                (s, e) => CommandManager.InvalidateRequerySuggested();

            Environment.LocalApplicationDataRemoved +=
                (s, e) =>
                {
                    SaveData.Current.DisableSave(); // 保存禁止
                    App.Current.MainWindow.Close();
                };

            PageTitle.Current.SubscribePropertyChanged(nameof(PageTitle.Title),
                (s, e) => AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(Title))));

            // TODO: アプリの初期化処理で行うべき
            // ダウンロードフォルダー生成
            if (!System.IO.Directory.Exists(Temporary.Current.TempDownloadDirectory))
            {
                System.IO.Directory.CreateDirectory(Temporary.Current.TempDownloadDirectory);
            }
        }



        public event EventHandler? FocusMainViewCall;



        public bool IsClosing { get; set; }

        public string Title => PageTitle.Current.Title;

        // for Binding
        public WindowShape WindowShape => _model.WindowShape;
        public WindowTitle WindowTitle => WindowTitle.Current;
        public PageTitle PageTitle => PageTitle.Current;
        public ThumbnailList ThumbnailList => ThumbnailList.Current;
        public ContentCanvasBrush ContentCanvasBrush => _viewComponent.ContentCanvasBrush;
        public ImageEffect ImageEffect => ImageEffect.Current;
        public MouseInput MouseInput => _viewComponent.MouseInput;
        public InfoMessage InfoMessage => InfoMessage.Current;
        public SidePanelFrame SidePanel => SidePanelFrame.Current;
        public ContentCanvas ContentCanvas => _viewComponent.ContentCanvas;
        public LoupeTransform LoupeTransform => _viewComponent.LoupeTransform;
        public ToastService ToastService => ToastService.Current;
        public App App => App.Current;
        public AutoHideConfig AutoHideConfig => Config.Current.AutoHide;


        public MainWindowModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public bool CanHideThumbnailList
        {
            get
            {
                return ThumbnailList.CanHideThumbnailList && !Model.CanHidePageSlider;
            }
        }

        /// <summary>
        /// Menu用AutoHideBehavior補足
        /// </summary>
        public BasicAutoHideDescription MenuAutoHideDescription { get; }

        /// <summary>
        /// FilmStrep, Slider 用 AutoHideBehavior 補足
        /// </summary>
        public BasicAutoHideDescription StatusAutoHideDescrption { get; }

        public BasicAutoHideDescription ThumbnailListusAutoHideDescrption { get; }

        public bool IsMenuBarActive => MainWindow.Current.IsActive;


        private void Model_FocusMainViewCall(object? sender, EventArgs e)
        {
            FocusMainViewCall?.Invoke(sender, e);
        }

        /// <summary>
        /// 起動時処理
        /// </summary>
        public void Loaded()
        {
            _model.Loaded();
        }

        public void ContentRendered()
        {
            _model.ContentRendered();
        }

        /// <summary>
        /// ウィンドウがアクティブ化したときの処理
        /// </summary>
        public void Activated()
        {
            if (IsClosing) return;

            RoutedCommandTable.Current.UpdateInputGestures();
        }

        /// <summary>
        /// ウィンドウが非アクティブ化したときの処理
        /// </summary>
        public void Deactivated()
        {
            if (IsClosing) return;

            var async = ArchiverManager.Current.UnlockAllArchivesAsync();
        }
    }
}

﻿using Microsoft.Win32;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using NeeView.Setting;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// Load command.
    /// </summary>
    public class LoadCommand : ICommand
    {
        public static LoadCommand Command { get; } = new LoadCommand();

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !BookHub.Current.IsLoading;
        }

        public void Execute(object? parameter)
        {
            var path = parameter as string;
            if (parameter == null) return;
            BookHub.Current.RequestLoad(this, path, null, BookLoadOption.None, true);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// MainWindow : Model
    /// </summary>
    public class MainWindowModel : BindableBase
    {
        private static MainWindowModel? _current;
        public static MainWindowModel Current => _current ?? throw new InvalidOperationException();


        // パネル表示ロック
        private bool _isPanelVisibleLocked;

        // 古いパネル表示ロック。コマンドでロックのトグルをできるようにするため
        private bool _isPanelVisibleLockedOld;

        private bool _canHidePageSlider;
        private bool _canHidePanel;
        private bool _canHideMenu;

        private volatile EditCommandWindow? _editCommandWindow;

        private readonly MainViewComponent _viewComponent;

        private readonly MainWindowController _windowController;


        public static void Initialize(MainWindowController windowController)
        {
            if (_current is not null) throw new InvalidOperationException();
            _current = new MainWindowModel(windowController);
        }

        private MainWindowModel(MainWindowController windowController)
        {
            if (_current is not null) throw new InvalidOperationException();
            _current = this;

            _windowController = windowController;

            _windowController.SubscribePropertyChanged(nameof(_windowController.AutoHideMode),
                (s, e) =>
                {
                    RefreshCanHidePanel();
                    RefreshCanHidePageSlider();
                    RefreshCanHideMenu();
                });

            _viewComponent = MainViewComponent.Current;

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHideMenuInAutoHideMode), (s, e) =>
            {
                RefreshCanHideMenu();
            });

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHideMenu), (s, e) =>
            {
                RefreshCanHideMenu();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsEnabled), (s, e) =>
            {
                RefreshCanHidePageSlider();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsHidePageSliderInAutoHideMode), (s, e) =>
            {
                RefreshCanHidePageSlider();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsHidePageSlider), (s, e) =>
            {
                RefreshCanHidePageSlider();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsHidePanelInAutoHideMode), (s, e) =>
            {
                RefreshCanHidePanel();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsHidePanel), (s, e) =>
            {
                RefreshCanHidePanel();
            });

            RefreshCanHidePanel();
            RefreshCanHidePageSlider();

            PageViewRecorder.Initialize();
        }


        public event EventHandler? CanHidePanelChanged;

        public event EventHandler? FocusMainViewCall;



        public MainWindowController WindowController => _windowController;


        /// <summary>
        /// メニューを自動非表示するか
        /// </summary>
        public bool CanHideMenu
        {
            get { return _canHideMenu; }
            set { SetProperty(ref _canHideMenu, value); }
        }

        /// <summary>
        /// スライダーを自動非表示するか
        /// </summary>
        public bool CanHidePageSlider
        {
            get { return _canHidePageSlider; }
            set { SetProperty(ref _canHidePageSlider, value); }
        }

        /// <summary>
        /// パネルを自動非表示するか
        /// </summary>
        public bool CanHidePanel
        {
            get { return _canHidePanel; }
            private set
            {
                if (SetProperty(ref _canHidePanel, value))
                {
                    CanHidePanelChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        /// <summary>
        /// パネル表示状態をロックする
        /// </summary>
        public bool IsPanelVisibleLocked
        {
            get { return _isPanelVisibleLocked; }
            set
            {
                if (_isPanelVisibleLocked != value)
                {
                    _isPanelVisibleLocked = value;
                    RaisePropertyChanged();
                    SidePanelFrame.Current.IsVisibleLocked = _isPanelVisibleLocked;
                }
            }
        }


        private void RefreshCanHideMenu()
        {
            CanHideMenu = Config.Current.MenuBar.IsHideMenu || (Config.Current.MenuBar.IsHideMenuInAutoHideMode && _windowController.AutoHideMode);
        }

        private void RefreshCanHidePageSlider()
        {
            CanHidePageSlider = Config.Current.Slider.IsEnabled && (Config.Current.Slider.IsHidePageSlider || (Config.Current.Slider.IsHidePageSliderInAutoHideMode && _windowController.AutoHideMode));
        }

        public void RefreshCanHidePanel()
        {
            CanHidePanel = Config.Current.Panels.IsHidePanel || (Config.Current.Panels.IsHidePanelInAutoHideMode && _windowController.AutoHideMode);
        }

        public bool ToggleHideMenu()
        {
            Config.Current.MenuBar.IsHideMenu = !Config.Current.MenuBar.IsHideMenu;
            return Config.Current.MenuBar.IsHideMenu;
        }

        public bool ToggleHidePageSlider()
        {
            Config.Current.Slider.IsHidePageSlider = !Config.Current.Slider.IsHidePageSlider;
            return Config.Current.Slider.IsHidePageSlider;
        }

        public bool ToggleHidePanel()
        {
            Config.Current.Panels.IsHidePanel = !Config.Current.Panels.IsHidePanel;
            return Config.Current.Panels.IsHidePanel;
        }

        public bool ToggleVisibleAddressBar()
        {
            Config.Current.MenuBar.IsAddressBarEnabled = !Config.Current.MenuBar.IsAddressBarEnabled;
            return Config.Current.MenuBar.IsAddressBarEnabled;
        }

        // 起動時処理
        public void Loaded()
        {
            CustomLayoutPanelManager.Current.Restore();

            // Susie起動
            // TODO: 非同期化できないか？
            SusiePluginManager.Current.Initialize();

            // 履歴読み込み
            SaveData.Current.LoadHistory();

            // ブックマーク読み込み
            SaveData.Current.LoadBookmark();

            // SaveDataSync活動開始
            SaveDataSync.Current.Initialize();

            // 最初のブック、フォルダを開く
            new FirstLoader().Load();

            // 最初のブックマークを開く
            BookmarkFolderList.Current.UpdateItems();

            // オプション指定があればフォルダーリスト表示
            if (!string.IsNullOrEmpty(App.Current.Option.FolderList))
            {
                SidePanelFrame.Current.IsVisibleFolderList = true;
            }

            // スライドショーの自動再生
            if (App.Current.Option.IsSlideShow != null ? App.Current.Option.IsSlideShow == SwitchOption.on : Config.Current.StartUp.IsAutoPlaySlideShow)
            {
                SlideShow.Current.Play();
            }

            // 起動時スクリプトの実行
            if (!string.IsNullOrWhiteSpace(App.Current.Option.ScriptFile))
            {
                ScriptManager.Current.Execute(this, App.Current.Option.ScriptFile, null);
            }
        }

        public void ContentRendered()
        {
            if (Config.Current.History.IsAutoCleanupEnabled)
            {
                Task.Run(() => BookHistoryCollection.Current.RemoveUnlinkedAsync(CancellationToken.None));
            }
        }

        // ダイアログでファイル選択して画像を読み込む
        public void LoadAs()
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = GetDefaultFolder();

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                LoadAs(dialog.FileName);
            }
        }

        public void LoadAs(string path)
        {
            BookHub.Current.RequestLoad(this, path, null, BookLoadOption.None, true);
        }

        // ファイルを開く基準となるフォルダーを取得
        private string GetDefaultFolder()
        {
            // 既に開いている場合、その場所を起点とする
            var book = BookHub.Current.GetCurrentBook();
            if (Config.Current.System.IsOpenbookAtCurrentPlace && book != null)
            {
                return System.IO.Path.GetDirectoryName(book.Path) ?? "";
            }
            else
            {
                return "";
            }
        }


        // 設定ウィンドウを開く
        public void OpenSettingWindow()
        {
            if (Setting.SettingWindow.Current != null)
            {
                if (Setting.SettingWindow.Current.WindowState == WindowState.Minimized)
                {
                    Setting.SettingWindow.Current.WindowState = WindowState.Normal;
                }
                Setting.SettingWindow.Current.Activate();
                return;
            }

            var dialog = new Setting.SettingWindow(new Setting.SettingWindowModel());
            dialog.Owner = App.Current.MainWindow;
            dialog.Width = MathUtility.Clamp(App.Current.MainWindow.ActualWidth - 100, 640, 1280);
            dialog.Height = MathUtility.Clamp(App.Current.MainWindow.ActualHeight - 100, 480, 2048);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Show();
        }

        // 設定ウィンドウを閉じる
        public bool CloseSettingWindow()
        {
            if (Setting.SettingWindow.Current != null)
            {
                Setting.SettingWindow.Current.AllowSave = false;
                Setting.SettingWindow.Current.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        // コマンド設定を開く
        public void OpenCommandParameterDialog(string command)
        {
            var dialog = new EditCommandWindow(command, EditCommandWindowTab.Default);
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            try
            {
                _editCommandWindow = dialog;
                if (_editCommandWindow.ShowDialog() == true)
                {
                    // 設定の同期
                    SaveDataSync.Current.SaveUserSetting(Config.Current.System.IsSyncUserSetting, true);
                }
            }
            finally
            {
                _editCommandWindow = null;
            }
        }

        public void CloseCommandParameterDialog()
        {
            _editCommandWindow?.Close();
            _editCommandWindow = null;
        }


        // バージョン情報を表示する
        public void OpenVersionWindow()
        {
            var dialog = new VersionWindow();
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }


        // 設定ファイルの場所を開く
        public void OpenSettingFilesFolder()
        {
            if (Environment.IsAppxPackage)
            {
                new MessageDialog(Resources.OpenSettingFolderErrorDialog_Message, Resources.OpenSettingFolderErrorDialog_Title).ShowDialog();
                return;
            }

            ExternalProcess.Start("explorer.exe", $"\"{Environment.LocalApplicationDataPath}\"");
        }


        /// <summary>
        /// パネル表示ロック開始
        /// コマンドから呼ばれる
        /// </summary>
        public void EnterVisibleLocked()
        {
            this.IsPanelVisibleLocked = !_isPanelVisibleLockedOld;
            _isPanelVisibleLockedOld = _isPanelVisibleLocked;
        }

        /// <summary>
        /// パネル表示ロック解除
        /// 他の操作をした場所から呼ばれる
        /// </summary>
        public void LeaveVisibleLocked()
        {
            if (_isPanelVisibleLocked)
            {
                _isPanelVisibleLockedOld = true;
                this.IsPanelVisibleLocked = false;
            }
            else
            {
                _isPanelVisibleLockedOld = false;
            }
        }

        /// <summary>
        /// 現在のスライダー方向を取得
        /// </summary>
        /// <returns></returns>
        public bool IsLeftToRightSlider()
        {
            if (PageFrameBoxPresenter.Current.IsMedia)
            {
                return true;
            }
            else
            {
                return !PageSlider.Current.IsSliderDirectionReversed;
            }
        }

        /// <summary>
        /// メインビューにフォーカスを移す。コマンド用
        /// </summary>
        public void FocusMainView(FocusMainViewCommandParameter parameter, bool byMenu)
        {
            if (parameter.NeedClosePanels)
            {
                CustomLayoutPanelManager.Current.LeftDock.SelectedItem = null;
                CustomLayoutPanelManager.Current.RightDock.SelectedItem = null;
                ////SidePanel.Current.CloseAllPanels();
            }

            FocusMainViewCall?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// メインビューにフォーカスを移す
        /// </summary>
        public void FocusMainView()
        {
            FocusMainViewCall?.Invoke(this, EventArgs.Empty);
        }

    }

}

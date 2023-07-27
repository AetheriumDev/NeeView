﻿using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// FileInformationView.xaml の相互作用ロジック
    /// </summary>
    public partial class FileInformationView : UserControl
    {
        public static readonly string DragDropFormat = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(), nameof(FileInformationView));

        #region RoutedCommand

        public static readonly RoutedCommand OpenExplorerCommand = new(nameof(OpenExplorerCommand), typeof(FileInformationView));
        public static readonly RoutedCommand OpenExternalAppCommand = new(nameof(OpenExternalAppCommand), typeof(FileInformationView));
        public static readonly RoutedCommand CopyCommand = new(nameof(CopyCommand), typeof(FileInformationView));
        public static readonly RoutedCommand CopyToFolderCommand = new(nameof(CopyToFolderCommand), typeof(FileInformationView));
        public static readonly RoutedCommand MoveToFolderCommand = new(nameof(MoveToFolderCommand), typeof(FileInformationView));
        public static readonly RoutedCommand OpenDestinationFolderCommand = new(nameof(OpenDestinationFolderCommand), typeof(FileInformationView));
        public static readonly RoutedCommand OpenExternalAppDialogCommand = new(nameof(OpenExternalAppDialogCommand), typeof(FileInformationView));
        public static readonly RoutedCommand PlaylistMarkCommand = new(nameof(PlaylistMarkCommand), typeof(FileInformationView));

        private readonly InformationPageCommandResource _commandResource = new();

        private static void InitializeCommandStatic()
        {
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            PlaylistMarkCommand.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
        }

        private void InitializeCommand()
        {
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExplorerCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyToFolderCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(MoveToFolderCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenDestinationFolderCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppDialogCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(PlaylistMarkCommand));
        }

        #endregion RoutedCommand


        private readonly FileInformationViewModel? _vm;
        private readonly MouseWheelDelta _mouseWheelDelta = new();
        private bool _isFocusRequest;


        static FileInformationView()
        {
            InitializeCommandStatic();
        }

        //public FileInformationView()
        //{
        //}

        public FileInformationView(FileInformation model)
        {
            InitializeComponent();
            InitializeCommand();

            this.ThumbnailListBox.ContextMenuOpening += ThumbnailListBoxItem_ContextMenuOpening;

            _vm = new FileInformationViewModel(model);
            this.DataContext = _vm;

            this.IsVisibleChanged += FileInformationView_IsVisibleChanged;
        }


        private void FileInformationView_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (_isFocusRequest && this.IsVisible)
            {
                this.Focus();
                _isFocusRequest = false;
            }
        }

        public void FocusAtOnce()
        {
            var focused = this.Focus();
            if (!focused)
            {
                _isFocusRequest = true;
            }
        }

        private void ThumbnailListBoxItem_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            if (sender is not ListBoxItem container)
            {
                return;
            }

            var item = (container.Content as FileInformationSource)?.ViewContent?.Page;
            if (item == null)
            {
                return;
            }

            var contextMenu = container.ContextMenu;
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Items.Clear();

            var listBox = this.ThumbnailListBox;
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_AddToPlaylist, Command = PlaylistMarkCommand, IsChecked = _commandResource.PlaylistMark_IsChecked(listBox) });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Explorer, Command = OpenExplorerCommand });
            contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(_commandResource.OpenExternalApp_CanExecute(listBox), OpenExternalAppCommand, OpenExternalAppDialogCommand));
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Copy, Command = CopyCommand });
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.PageListItem_Menu_CopyToFolder, _commandResource.CopyToFolder_CanExecute(listBox), CopyToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.PageListItem_Menu_MoveToFolder, _commandResource.MoveToFolder_CanExecute(listBox), MoveToFolderCommand, OpenDestinationFolderCommand));
        }

        private void ThumbnailListBox_PreviewMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            if (_vm is null) return;

            var delta = _mouseWheelDelta.NotchCount(e);
            if (delta != 0)
            {
                delta = PageSlider.Current.IsSliderDirectionReversed ? delta : -delta;
                _vm.MoveSelectedItem(delta);
            }
            e.Handled = true;
        }

        private void FolderInformationView_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_vm is null) return;

            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (_vm.IsLRKeyEnabled() && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }
        }


        #region DragDrop

        public async Task DragStartBehavior_DragBeginAsync(object? sender, Windows.DragStartEventArgs e, CancellationToken token)
        {
            var pages = this.ThumbnailListBox.SelectedItems.Cast<FileInformationSource>()
                .Select(x => x.ViewContent?.Page)
                .WhereNotNull()
                .ToList();

            if (!pages.Any())
            {
                e.Cancel = true;
                return;
            }

            var isSuccess = await Task.Run(() =>
            {
                try
                {
                    return ClipboardUtility.SetData(e.Data, pages, new CopyFileCommandParameter() { MultiPagePolicy = MultiPagePolicy.All }, token);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            });

            if (!isSuccess)
            {
                e.Cancel = true;
                return;
            }

            // 全てのファイルがファイルシステムであった場合のみ
            if (pages.All(p => p.Entry.IsFileSystem))
            {
                // 右クリックドラッグでファイル移動を許可
                if (Config.Current.System.IsFileWriteAccessEnabled && e.MouseEventArgs.RightButton == MouseButtonState.Pressed)
                {
                    e.AllowedEffects |= DragDropEffects.Move;
                }

                // TODO: ドラッグ終了時にファイル移動の整合性を取る必要がある。
                // しっかり実装するならページのファイルシステムの監視が必要になる。ファイルの追加削除が自動的にページに反映するように。

                // ひとまずドラッグ完了後のページ削除を限定的に行う。
                e.DragEndAction = () => BookOperation.Current.BookControl.ValidateRemoveFile(pages);
            }
        }

        #endregion
    }

    public class InformationPageCommandResource : PageCommandResource
    {
        protected override Page? GetSelectedPage(object sender)
        {
            if (sender is not ListBox listBox) return null;

            return (listBox.SelectedItem as FileInformationSource)?.ViewContent?.Page;
        }

        protected override List<Page>? GetSelectedPages(object sender)
        {
            if (sender is not ListBox listBox) return null;

            return listBox.SelectedItems
                .Cast<FileInformationSource>()
                .Select(e => e.ViewContent?.Page)
                .WhereNotNull()
                .ToList();
        }
    }


}

﻿using NeeLaboratory.Windows.Input;
using NeeView.Windows.Media;
using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    /// FolderTreeView.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderTreeView : UserControl
    {
        private readonly FolderTreeViewModel _vm;
        private CancellationTokenSource _removeUnlinkedCommandCancellationTokenSource = new();

        public FolderTreeView()
        {
            InitializeComponent();

            _vm = new FolderTreeViewModel();
            _vm.SelectedItemChanged += ViewModel_SelectedItemChanged;

            // タッチスクロール操作の終端挙動抑制
            this.TreeView.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.Root.DataContext = _vm;

            this.Loaded += FolderTreeView_Loaded;
            this.Unloaded += FolderTreeView_Unloaded;
        }

        #region Dependency Properties

        public FolderTreeModel Model
        {
            get { return (FolderTreeModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(FolderTreeModel), typeof(FolderTreeView), new PropertyMetadata(null, ModelPropertyChanged));

        private static void ModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FolderTreeView control)
            {
                control.UpdateModel();
            }
        }

        #endregion


        #region Commands

        private RelayCommand? _addQuickAccessCommand;
        public RelayCommand AddQuickAccessCommand
        {
            get
            {
                return _addQuickAccessCommand = _addQuickAccessCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    var item = this.TreeView.SelectedItem;
                    if (item != null)
                    {
                        _vm.AddQuickAccess(item);
                    }
                }
            }
        }

        private RelayCommand? _removeCommand;
        public RelayCommand RemoveCommand
        {
            get
            {
                return _removeCommand = _removeCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case QuickAccessNode quickAccess:
                            _vm.RemoveQuickAccess(quickAccess);
                            break;

                        case RootBookmarkFolderNode rootBookmarkFolder:
                            break;

                        case BookmarkFolderNode bookmarkFolder:
                            _vm.RemoveBookmarkFolder(bookmarkFolder);
                            break;
                    }
                }
            }
        }

        private RelayCommand? _PropertyCommand;
        public RelayCommand PropertyCommand
        {
            get
            {
                return _PropertyCommand = _PropertyCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case QuickAccessNode quickAccessNode:
                            {
                                var work = (QuickAccess)quickAccessNode.QuickAccessSource.Clone();
                                var dialog = new QuickAccessPropertyDialog(work)
                                {
                                    Owner = Window.GetWindow(this),
                                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                };
                                var result = dialog.ShowDialog();
                                if (result == true)
                                {
                                    quickAccessNode.QuickAccessSource.Restore(work.CreateMemento());
                                    quickAccessNode.RefreshAllProperties();
                                }
                            }
                            break;
                    }
                }
            }
        }

        private RelayCommand? _RefreshFolderCommand;
        public RelayCommand RefreshFolderCommand
        {
            get
            {
                return _RefreshFolderCommand = _RefreshFolderCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _vm.RefreshFolder();
                }
            }
        }

        private RelayCommand? _OpenExplorerCommand;
        public RelayCommand OpenExplorerCommand
        {
            get
            {
                return _OpenExplorerCommand = _OpenExplorerCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    if (this.TreeView.SelectedItem is DirectoryNode item)
                    {
                        ExternalProcess.OpenWithExplorer(item.Path);
                    }
                }
            }
        }


        private RelayCommand? _NewFolderCommand;
        public RelayCommand NewFolderCommand
        {
            get
            {
                return _NewFolderCommand = _NewFolderCommand ?? new RelayCommand(Execute);

                async void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case BookmarkFolderNode bookmarkFolderNode:
                            {
                                var newItem = _vm.NewBookmarkFolder(bookmarkFolderNode);
                                if (newItem != null)
                                {
                                    this.TreeView.UpdateLayout();
                                    await RenameBookmarkFolder(newItem);
                                }
                            }
                            break;
                    }
                }
            }
        }


        private RelayCommand? _RenameCommand;
        public RelayCommand RenameCommand
        {
            get
            {
                return _RenameCommand = _RenameCommand ?? new RelayCommand(Execute);

                async void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case QuickAccessNode quickAccessNode:
                            await RenameQuickAccess(quickAccessNode);
                            break;

                        case BookmarkFolderNode bookmarkFolderNode:
                            await RenameBookmarkFolder(bookmarkFolderNode);
                            break;
                    }
                }
            }
        }


        private RelayCommand? _removeUnlinkedCommand;
        public RelayCommand RemoveUnlinkedCommand
        {
            get { return _removeUnlinkedCommand = _removeUnlinkedCommand ?? new RelayCommand(RemoveUnlinkedCommand_Executed); }
        }

        private async void RemoveUnlinkedCommand_Executed()
        {
            // 直前の命令はキャンセル
            _removeUnlinkedCommandCancellationTokenSource?.Cancel();
            _removeUnlinkedCommandCancellationTokenSource = new CancellationTokenSource();
            if (this.TreeView.SelectedItem is RootBookmarkFolderNode)
            {
                await BookmarkCollection.Current.RemoveUnlinkedAsync(_removeUnlinkedCommandCancellationTokenSource.Token);
            }
        }

        private RelayCommand? _addBookmarkCommand;
        public RelayCommand AddBookmarkCommand
        {
            get
            {
                return _addBookmarkCommand = _addBookmarkCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    if (this.TreeView.SelectedItem is BookmarkFolderNode item)
                    {
                        _vm.AddBookmarkTo(item);
                    }
                }
            }
        }

        #endregion


        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _vm.DpiChanged(oldDpi, newDpi);
        }

        private void UpdateModel()
        {
            _vm.Model = Model;
            this.TreeView.ItemsSource = Model?.Root.Children;
        }

        private void FolderTreeView_Loaded(object? sender, RoutedEventArgs e)
        {
            FocusSelectedItem();
        }

        private void FolderTreeView_Unloaded(object? sender, RoutedEventArgs e)
        {
        }

        private async Task RenameQuickAccess(QuickAccessNode item)
        {
            var renamer = new FolderTreeItemRenamer(this.TreeView, null);
            await renamer.RenameAsync(item);
        }

        private async Task RenameBookmarkFolder(BookmarkFolderNode item)
        {
            var renamer = new FolderTreeItemRenamer(this.TreeView, null);
            await renamer.RenameAsync(item);
        }

        public void FocusSelectedItem()
        {
            if (!_vm.IsValid) return;
            if (_vm.Model is null) return;

            if (this.TreeView.SelectedItem == null)
            {
                _vm.SelectRootQuickAccess();
            }

            if (_vm.Model.IsFocusAtOnce)
            {
                _vm.Model.IsFocusAtOnce = false;
                ScrollIntoView(true);
            }
        }

        private void ScrollIntoView(bool isFocus)
        {
            if (!_vm.IsValid) return;
            if (_vm.Model is null) return;

            if (!this.TreeView.IsVisible)
            {
                return;
            }

            var selectedItem = _vm.Model.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            ////Debug.WriteLine("ScrollIntoView:");

            this.TreeView.UpdateLayout();

            ItemsControl? container = this.TreeView;
            var lastContainer = container;
            foreach (var node in selectedItem.Hierarchy.Skip(1))
            {
                if (node.Parent == null)
                {
                    break;
                }

                if (node.Parent.Children is null)
                {
                    break;
                }

                var index = node.Parent.Children.IndexOf(node);
                if (index < 0)
                {
                    break;
                }

                container = ScrollIntoView(container, index);
                if (container == null)
                {
                    break;
                }

                container.UpdateLayout();
                lastContainer = container;
            }

            if (isFocus)
            {
                lastContainer.Focus();
                ////Debug.WriteLine($"FolderTree.Focused: {isFocused}");
            }

            _vm.Model.SelectedItem = selectedItem;

            ////Debug.WriteLine("ScrollIntoView: done.");
        }

        // from https://docs.microsoft.com/ja-jp/dotnet/framework/wpf/controls/how-to-find-a-treeviewitem-in-a-treeview
        // HACK: BindingErrorが出る
        private static TreeViewItem? ScrollIntoView(ItemsControl container, int index)
        {
            // Expand the current container
            if (container is TreeViewItem item && !item.IsExpanded)
            {
                container.SetValue(TreeViewItem.IsExpandedProperty, true);
                container.UpdateLayout();
            }

            // Try to generate the ItemsPresenter and the ItemsPanel.
            // by calling ApplyTemplate.  Note that in the 
            // virtualizing case even if the item is marked 
            // expanded we still need to do this step in order to 
            // regenerate the visuals because they may have been virtualized away.
            container.ApplyTemplate();
            ItemsPresenter? itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
            if (itemsPresenter != null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                // The Tree template has not named the ItemsPresenter, 
                // so walk the descendents and find the child.
                itemsPresenter = VisualTreeUtility.FindVisualChild<ItemsPresenter>(container);
                if (itemsPresenter == null)
                {
                    container.UpdateLayout();
                    itemsPresenter = VisualTreeUtility.FindVisualChild<ItemsPresenter>(container);
                }
            }

            Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

            // Ensure that the generator for this panel has been created.
            _ = itemsHostPanel.Children;

            if (itemsHostPanel is CustomVirtualizingStackPanel virtualizingPanel)
            {
                virtualizingPanel.BringIntoView(index);
                var subContainer = (TreeViewItem?)container.ItemContainerGenerator.ContainerFromIndex(index);
                return subContainer;
            }
            else
            {
                var subContainer = (TreeViewItem?)container.ItemContainerGenerator.ContainerFromIndex(index);
                // Bring the item into view to maintain the 
                // same behavior as with a virtualizing panel.
                subContainer?.BringIntoView();
                return subContainer;
            }
        }


        private void ViewModel_SelectedItemChanged(object? sender, EventArgs e)
        {
            ScrollIntoView(false);
        }

        private void TreeView_SelectedItemChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_vm.IsValid) return;
            if (_vm.Model is null) return;

            _vm.Model.SelectedItem = this.TreeView.SelectedItem as FolderTreeNodeBase;
        }

        private async void TreeView_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_vm.IsValid) return;

            var isVisible = (bool)e.NewValue;
            _vm.IsVisibleChanged(isVisible);
            if (isVisible)
            {
                await Task.Yield();
                FocusSelectedItem();
            }
        }

        private void TreeViewItem_Selected(object? sender, RoutedEventArgs e)
        {
        }

        private void TreeViewItem_MouseRightButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                e.Handled = true;
            }
        }


        private void TreeViewItem_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (!_vm.IsValid) return;

            if (sender is TreeViewItem viewItem)
            {
                if (viewItem.IsSelected)
                {
                    _vm.Decide(viewItem.DataContext);
                }
                e.Handled = true;
            }
        }

        private void TreeViewItem_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!_vm.IsValid) return;

            if (sender is not TreeViewItem viewItem)
            {
                return;
            }

            if (e.Key == Key.Return)
            {
                if (viewItem.IsSelected)
                {
                    _vm.Decide(viewItem.DataContext);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (viewItem.IsSelected)
                {
                    RemoveCommand.Execute(viewItem.DataContext);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.F2)
            {
                if (viewItem.IsSelected)
                {
                    RenameCommand.Execute(viewItem.DataContext);
                }
                e.Handled = true;
            }
        }

        private void TreeViewItem_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            if (sender is not TreeViewItem viewItem)
            {
                return;
            }

            if (!viewItem.IsSelected)
            {
                return;
            }

            var contextMenu = viewItem.ContextMenu;
            contextMenu.Items.Clear();

            switch (viewItem.DataContext)
            {
                case RootQuickAccessNode:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.AddCurrentQuickAccess"), AddQuickAccessCommand));
                    break;

                case QuickAccessNode:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Delete"), RemoveCommand, Key.Delete.ToString()));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Rename"), RenameCommand, Key.F2.ToString()));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Property"), PropertyCommand));
                    break;

                case RootDirectoryNode:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.RefreshFolder"), RefreshFolderCommand));
                    break;

                case DirectoryNode:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Explorer"), OpenExplorerCommand));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.AddQuickAccess"), AddQuickAccessCommand));
                    break;

                case RootBookmarkFolderNode:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.DeleteInvalidBookmark"), RemoveUnlinkedCommand));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewFolder"), NewFolderCommand));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.AddBookmark"), AddBookmarkCommand));
                    break;

                case BookmarkFolderNode:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Delete"), RemoveCommand, Key.Delete.ToString()));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Rename"), RenameCommand, Key.F2.ToString()));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewFolder"), NewFolderCommand));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.AddBookmark"), AddBookmarkCommand));
                    break;

                default:
                    e.Handled = true;
                    break;
            }
        }

        private static MenuItem CreateMenuItem(string header, ICommand command)
        {
            return new MenuItem() { Header = header, Command = command };
        }

        private static MenuItem CreateMenuItem(string header, ICommand command, string inputGestureText)
        {
            return new MenuItem() { Header = header, Command = command, InputGestureText = inputGestureText };
        }

        #region DragDrop

        public async Task DragStartBehavior_DragBeginAsync(object? sender, DragStartEventArgs e, CancellationToken token)
        {
            if (e.DragItem is not TreeViewItem data)
            {
                e.Cancel = true;
                return;
            }

            switch (data.DataContext)
            {
                case QuickAccessNode quickAccess:
                    e.Data.SetData(quickAccess);
                    e.AllowedEffects = DragDropEffects.Copy | DragDropEffects.Move;
                    break;

                case DirectoryNode directory:
                    e.Data.SetFileDropList(new System.Collections.Specialized.StringCollection() { directory.Path });
                    e.AllowedEffects = DragDropEffects.Copy;
                    break;

                //case RootBookmarkFolderNode rootBookmarkFolder:
                //    break;

                case BookmarkFolderNode bookmarkFolder:
                    var bookmarkNodeCollection = new BookmarkNodeCollection() { bookmarkFolder.BookmarkSource };
                    e.Data.SetData(bookmarkNodeCollection);
                    e.AllowedEffects = DragDropEffects.Copy | DragDropEffects.Move;
                    break;

                default:
                    e.Cancel = true;
                    break;
            }

            await Task.CompletedTask;
        }

        private void TreeView_PreviewDragEnter(object? sender, DragEventArgs e)
        {
            TreeView_PreviewDragOver(sender, e);
        }

        private void TreeView_PreviewDragLeave(object? sender, DragEventArgs e)
        {
        }

        private void TreeView_PreviewDragOver(object? sender, DragEventArgs e)
        {
            TreeView_DragDrop(sender, e, false);
            DragDropHelper.AutoScroll(sender, e);
        }

        private void TreeView_Drop(object? sender, DragEventArgs e)
        {
            TreeView_DragDrop(sender, e, true);
        }

        private void TreeView_DragDrop(object? sender, DragEventArgs e, bool isDrop)
        {
            if (!_vm.IsValid) return;

            var treeViewItem = PointToViewItem(this.TreeView, e.GetPosition(this.TreeView));
            if (treeViewItem != null)
            {
                switch (treeViewItem.DataContext)
                {
                    case RootQuickAccessNode:
                        {
                            DropToQuickAccess(sender, e, isDrop, null, e.Data.GetData<QuickAccessNode>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, null, e.Data.GetData<BookmarkNodeCollection>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, null, e.Data.GetData<QueryPathCollection>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, null, e.Data.GetFileDrop());
                            if (e.Handled) return;
                        }
                        break;

                    case QuickAccessNode quickAccessTarget:
                        {
                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, e.Data.GetData<QuickAccessNode>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, e.Data.GetData<BookmarkNodeCollection>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, e.Data.GetData<QueryPathCollection>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, e.Data.GetFileDrop());
                            if (e.Handled) return;
                        }
                        break;

                    case BookmarkFolderNode bookmarkFolderTarget:
                        {
                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, e.Data.GetData<BookmarkNodeCollection>());
                            if (e.Handled) return;

                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, e.Data.GetData<QueryPathCollection>());
                            if (e.Handled) return;

                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, e.Data.GetFileDrop());
                            if (e.Handled) return;
                        }
                        break;
                }
            }
        }

#pragma warning disable IDE0060 // 未使用のパラメーターを削除します

        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, QuickAccessNode? quickAccessTarget, QuickAccessNode? quickAccess)
        {
            if (quickAccessTarget == null || quickAccess == null)
            {
                return;
            }

            if (quickAccess != quickAccessTarget)
            {
                if (isDrop)
                {
                    _vm.MoveQuickAccess(quickAccess, quickAccessTarget);
                }
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, QuickAccessNode? quickAccessTarget, IEnumerable<TreeListNode<IBookmarkEntry>>? bookmarkEntries)
        {
            // QuickAccessは大量操作できないので先頭１項目だけ処理する
            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, bookmarkEntries?.FirstOrDefault());
        }


        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, QuickAccessNode? quickAccessTarget, TreeListNode<IBookmarkEntry>? bookmarkEntry)
        {
            if (_vm.Model is null) return;
            if (bookmarkEntry is null) return;

            if (bookmarkEntry.Value is BookmarkFolder)
            {
                if (isDrop)
                {
                    var query = bookmarkEntry.CreateQuery(QueryScheme.Bookmark);
                    _vm.Model.InsertQuickAccess(quickAccessTarget, query.SimplePath);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }


        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, QuickAccessNode? quickAccessTarget, IEnumerable<QueryPath>? queries)
        {
            // QuickAccessは大量操作できないので先頭１項目だけ処理する
            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, queries?.FirstOrDefault());
        }

        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, QuickAccessNode? quickAccessTarget, QueryPath? query)
        {
            if (query == null) return;
            if (_vm.Model is null) return;

            if ((query.Scheme == QueryScheme.File && (Directory.Exists(query.SimplePath) || IsPlaylistFile(query.SimplePath)))
                || (query.Scheme == QueryScheme.Bookmark && BookmarkCollection.Current.FindNode(query)?.Value is BookmarkFolder))
            {
                if (isDrop)
                {
                    _vm.Model.InsertQuickAccess(quickAccessTarget, query.SimpleQuery);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, QuickAccessNode? quickAccessTarget, string[] fileNames)
        {
            if (_vm.Model is null) return;

            if (fileNames == null)
            {
                return;
            }
            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }

            bool isDropped = false;
            foreach (var fileName in fileNames)
            {
                if (Directory.Exists(fileName) || IsPlaylistFile(fileName))
                {
                    if (isDrop)
                    {
                        _vm.Model.InsertQuickAccess(quickAccessTarget, fileName);
                    }
                    isDropped = true;
                }
            }
            if (isDropped)
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private static bool IsPlaylistFile(string path)
        {
            return File.Exists(path) && PlaylistArchive.IsSupportExtension(path);
        }


#if false
        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, BookmarkFolderNode bookmarkFolder)
        {
            if (bookmarkFolder == null)
            {
                return;
            }

            if (!bookmarkFolderTarget.BookmarkSource.ParentContains(bookmarkFolder.BookmarkSource))
            {
                if (isDrop)
                {
                    BookmarkCollection.Current.MoveToChild(bookmarkFolder.BookmarkSource, bookmarkFolderTarget.BookmarkSource);
                }
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }
#endif

        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, IEnumerable<TreeListNode<IBookmarkEntry>>? bookmarkEntries)
        {
            if (bookmarkEntries == null || !bookmarkEntries.Any())
            {
                return;
            }


            e.Effects = bookmarkEntries.All(x => CanDropToBookmark(bookmarkFolderTarget, x)) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;

            if (isDrop && e.Effects == DragDropEffects.Move)
            {
                foreach (var bookmarkEntry in bookmarkEntries)
                {
                    DropToBookmarkExecute(bookmarkFolderTarget, bookmarkEntry);
                }
            }
        }


        private static bool CanDropToBookmark(BookmarkFolderNode bookmarkFolderTarget, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            if (bookmarkEntry.Value is BookmarkFolder)
            {
                var node = bookmarkFolderTarget.BookmarkSource;
                return !node.Children.Contains(bookmarkEntry) && !node.ParentContains(bookmarkEntry) && node != bookmarkEntry;
            }
            else
            {
                return true;
            }
        }

        private static void DropToBookmarkExecute(BookmarkFolderNode bookmarkFolderTarget, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            BookmarkCollection.Current.MoveToChild(bookmarkEntry, bookmarkFolderTarget.BookmarkSource);
        }

#if false
        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            if (bookmarkEntry == null)
            {
                return;
            }

            e.Effects = CanDropToBookmark(bookmarkFolderTarget, bookmarkEntry) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;

            if (isDrop && e.Effects == DragDropEffects.Move)
            {
                DropToBookmarkExecute(bookmarkFolderTarget, bookmarkEntry);
            }
        }
#endif

        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, IEnumerable<QueryPath>? queries)
        {
            if (queries == null || !queries.Any())
            {
                return;
            }

            foreach (var query in queries)
            {
                DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, query);
            }
        }

        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, QueryPath query)
        {
            if (query == null)
            {
                return;
            }

            if (query.Search == null && query.Scheme == QueryScheme.File)
            {
                if (isDrop)
                {
                    BookmarkCollectionService.AddToChild(bookmarkFolderTarget.BookmarkSource, query);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, string[] fileNames)
        {
            if (fileNames == null)
            {
                return;
            }
            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }


            bool isDropped = false;
            foreach (var fileName in fileNames)
            {
                if (ArchiverManager.Current.IsSupported(fileName, true, true) || System.IO.Directory.Exists(fileName))
                {
                    if (isDrop)
                    {
                        BookmarkCollectionService.AddToChild(bookmarkFolderTarget.BookmarkSource, new QueryPath(fileName));
                    }
                    isDropped = true;
                }
            }
            if (isDropped)
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private static TreeViewItem? PointToViewItem(TreeView treeView, Point point)
        {
            // NOTE: リストアイテム間に隙間がある場合があるので、Y座標をずらして再検証する
            var element = VisualTreeUtility.HitTest<TreeViewItem>(treeView, point) ?? VisualTreeUtility.HitTest<TreeViewItem>(treeView, new Point(point.X, point.Y + 1));
            return element;
        }

#pragma warning restore IDE0060 // 未使用のパラメーターを削除します

        #endregion DragDrop
    }



    public class FolderTreeItemRenamer : TreeViewItemRenamer<FolderTreeNodeBase>
    {
        public FolderTreeItemRenamer(TreeView treeView, IToolTipService? toolTipService) : base(treeView, toolTipService)
        {
        }

        protected override RenameControl CreateRenameControl(TreeView treeView, FolderTreeNodeBase item)
        {
            var control = base.CreateRenameControl(treeView, item);
            control.IsInvalidSeparatorChars = true;
            return control;
        }
    }

}

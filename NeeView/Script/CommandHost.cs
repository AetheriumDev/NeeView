﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#pragma warning disable CA1822

namespace NeeView
{
    public class CommandHost
    {
        private readonly CommandHostStaticResource _resource;
        private readonly ScriptAccessDiagnostics _accessDiagnostics;
        private List<string> _args = new();


        public CommandHost()
        {
            _resource = CommandHostStaticResource.Current;
            _accessDiagnostics = _resource.AccessDiagnostics;

            Config = _resource.ConfigMap.Map;
            Command = _resource.CommandAccessMap;
            Environment = new EnvironmentAccessor();
            Book = new BookAccessor(_accessDiagnostics);
            Bookshelf = new BookshelfPanelAccessor();
            PageList = new PageListPanelAccessor();
            Bookmark = new BookmarkPanelAccessor();
            Playlist = new PlaylistPanelAccessor();
            History = new HistoryPanelAccessor();
            Information = new InformationPanelAccessor();
            Effect = new EffectPanelAccessor();
            Navigator = new NavigatorPanelAccessor();
            ExternalAppCollection = new ExternalAppCollectionAccessor();
        }


        [WordNodeMember(IsAutoCollect = false)]
        public List<string> Args => _args;

        [WordNodeMember(IsAutoCollect = false)]
        public Dictionary<string, object> Values => _resource.Values;

        [WordNodeMember(IsAutoCollect = false)]
        public PropertyMap Config { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public CommandAccessorMap Command { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public EnvironmentAccessor Environment { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public BookAccessor Book { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public BookshelfPanelAccessor Bookshelf { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public PageListPanelAccessor PageList { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public BookmarkPanelAccessor Bookmark { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public PlaylistPanelAccessor Playlist { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public HistoryPanelAccessor History { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public InformationPanelAccessor Information { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public EffectPanelAccessor Effect { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public NavigatorPanelAccessor Navigator { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public ExternalAppCollectionAccessor ExternalAppCollection { get; }

        [WordNodeMember]
        [Obsolete("no used"), Alternative(nameof(Playlist), 39)] // ver.39
        public object? Pagemark
        {
            get
            {
                return _accessDiagnostics.Throw<object>(new NotSupportedException(RefrectionTools.CreatePropertyObsoleteMessage(this.GetType())));
            }
        }


        internal bool IsDirty => Command != _resource.CommandAccessMap;



        internal void SetCancellationToken(CancellationToken cancellationToken)
        {
            Book.SetCancellationToken(cancellationToken);
        }

        internal void SetArgs(List<string> args)
        {
            _args = args;
        }

        [WordNodeMember]
        public void ShowMessage(string message)
        {
            InfoMessage.Current.SetMessage(InfoMessageType.Notify, message);
        }

        [WordNodeMember]
        public void ShowToast(string message)
        {
            ToastService.Current.Show(new Toast(message));
        }

        [WordNodeMember]
        public bool ShowDialog(string title, string message = "", int commands = 0)
        {
            return AppDispatcher.Invoke(() => ShowDialogInner(title, message, commands));
        }

        private bool ShowDialogInner(string title, string message, int commands)
        {
            var dialog = new MessageDialog(message, title);
            switch (commands)
            {
                default:
                    dialog.Commands.Add(UICommands.OK);
                    break;
                case 1:
                    dialog.Commands.Add(UICommands.OK);
                    dialog.Commands.Add(UICommands.Cancel);
                    break;
                case 2:
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    break;
            }
            var result = dialog.ShowDialog(App.Current.MainWindow);
            return result.IsPossible;
        }

        [WordNodeMember]
        public string? ShowInputDialog(string title, string? text = null)
        {
            return AppDispatcher.Invoke(() => ShowInputDialogInner(title, text));
        }

        private static string? ShowInputDialogInner(string title, string? text)
        {
            var component = new InputDialogComponent(text);
            var dialog = new MessageDialog(component, title);
            dialog.Commands.Add(UICommands.OK);
            dialog.Commands.Add(UICommands.Cancel);
            var result = dialog.ShowDialog(App.Current.MainWindow);
            return result.IsPossible ? component.Text : null;
        }


        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            if (node.Children is null) throw new InvalidOperationException();

            node.Children.Add(new WordNode(nameof(Args)));
            node.Children.Add(new WordNode(nameof(Values)));
            node.Children.Add(Config.CreateWordNode(nameof(Config)));
            node.Children.Add(Command.CreateWordNode(nameof(Command)));
            node.Children.Add(Environment.CreateWordNode(nameof(Environment)));
            node.Children.Add(Book.CreateWordNode(nameof(Book)));
            node.Children.Add(Bookshelf.CreateWordNode(nameof(Bookshelf)));
            node.Children.Add(PageList.CreateWordNode(nameof(PageList)));
            node.Children.Add(Bookmark.CreateWordNode(nameof(Bookmark)));
            node.Children.Add(Playlist.CreateWordNode(nameof(Playlist)));
            node.Children.Add(History.CreateWordNode(nameof(History)));
            node.Children.Add(Information.CreateWordNode(nameof(Information)));
            node.Children.Add(Effect.CreateWordNode(nameof(Effect)));
            node.Children.Add(Navigator.CreateWordNode(nameof(Navigator)));
            node.Children.Add(ExternalAppCollection.CreateWordNode(nameof(ExternalAppCollection)));

            return node;
        }


        private class InputDialogComponent : IMessageDialogContentComponent
        {
            private readonly TextBox _textBox;

            public InputDialogComponent(string? text)
            {
                _textBox = new TextBox() { Text = text ?? "", Padding = new Thickness(5.0) };
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            }

            private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Return)
                {
                    Decide?.Invoke(this, EventArgs.Empty);
                    e.Handled = true;
                }
            }

            public event EventHandler? Decide;

            public object Content => _textBox;

            public string Text => _textBox.Text;

            public void OnLoaded(object sender, RoutedEventArgs e)
            {
                _textBox.Focus();
                _textBox.SelectAll();
            }
        }
    }
}

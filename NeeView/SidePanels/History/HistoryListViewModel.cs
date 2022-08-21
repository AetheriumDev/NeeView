﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using NeeLaboratory.Windows.Input;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using NeeLaboratory.ComponentModel;
using System.Threading;
using NeeView.Properties;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class HistoryListViewModel : BindableBase
    {
        private HistoryList _model;


        public HistoryListViewModel(HistoryList model)
        {
            _model = model;
            _model.AddPropertyChanged(nameof(HistoryList.FilterPath), (s, e) => RaisePropertyChanged(nameof(FilterPath)));

            MoreMenuDescription = new HistoryListMoreMenuDescription(this);
        }


        public string FilterPath => string.IsNullOrEmpty(_model.FilterPath) ? Properties.Resources.Word_AllHistory : _model.FilterPath;


        #region MoreMenu

        public HistoryListMoreMenuDescription MoreMenuDescription { get; }

        public class HistoryListMoreMenuDescription : ItemsListMoreMenuDescription
        {
            private HistoryListViewModel _vm;

            public HistoryListMoreMenuDescription(HistoryListViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Create()
            {
                var menu = new ContextMenu();
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleList, PanelListItemStyle.Normal));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleContent, PanelListItemStyle.Content));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleBanner, PanelListItemStyle.Banner));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.History_MoreMenu_IsCurrentFolder, new Binding(nameof(HistoryConfig.IsCurrentFolder)) { Source = Config.Current.History }));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCommandMenuItem(Properties.Resources.History_MoreMenu_DeleteInvalid, _vm.RemoveUnlinkedCommand));
                menu.Items.Add(CreateCommandMenuItem(Properties.Resources.History_MoreMenu_DeleteAll, _vm.RemoveAllCommand));
                return  menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, Config.Current.History);
            }
        }

        #endregion

        #region Commands

        private CancellationTokenSource? _removeUnlinkedCommandCancellationToken;
        private RelayCommand<PanelListItemStyle>? _setListItemStyle;
        private RelayCommand? _removeAllCommand;
        private RelayCommand? _removeUnlinkedCommand;

        /// <summary>
        /// SetListItemStyle command.
        /// </summary>
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            Config.Current.History.PanelListItemStyle = style;
        }

        /// <summary>
        /// RemoveAllCommand command
        /// </summary>
        public RelayCommand RemoveAllCommand
        {
            get { return _removeAllCommand = _removeAllCommand ?? new RelayCommand(RemoveAll_Executed); }
        }

        private void RemoveAll_Executed()
        {
            if (BookHistoryCollection.Current.Items.Any())
            {
                var dialog = new MessageDialog(Resources.HistoryDeleteAllDialog_Message, Resources.HistoryDeleteAllDialog_Title);
                dialog.Commands.Add(UICommands.Delete);
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();
                if (answer != UICommands.Delete) return;
            }

            BookHistoryCollection.Current.Clear();
        }

        /// <summary>
        /// RemoveUnlinkedCommand command.
        /// </summary>
        public RelayCommand RemoveUnlinkedCommand
        {
            get { return _removeUnlinkedCommand = _removeUnlinkedCommand ?? new RelayCommand(RemoveUnlinkedCommand_Executed); }
        }

        private async void RemoveUnlinkedCommand_Executed()
        {
            // 直前の命令はキャンセル
            _removeUnlinkedCommandCancellationToken?.Cancel();
            _removeUnlinkedCommandCancellationToken = new CancellationTokenSource();
            int count = await BookHistoryCollection.Current.RemoveUnlinkedAsync(_removeUnlinkedCommandCancellationToken.Token);
            BookHistoryCollection.Current.ShowRemovedMessage(count);
        }

        #endregion
    }
}

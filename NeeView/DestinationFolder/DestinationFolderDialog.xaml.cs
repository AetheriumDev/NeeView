﻿using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// DestinationFolderDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class DestinationFolderDialog : Window
    {
        private DestinationFolderDialogViewModel _vm;

        public DestinationFolderDialog()
        {
            InitializeComponent();

            _vm = new DestinationFolderDialogViewModel();
            _vm.Owner = this;
            this.DataContext = _vm;

            DragDropHelper.AttachDragOverTerminator(this);

            this.Closed += DestinationFolderDialog_Closed;
        }


        public static void ShowDialog(Window owner)
        {
            var dialog = new DestinationFolderDialog();
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }


        private void DestinationFolderDialog_Closed(object sender, EventArgs e)
        {
            _vm.Decide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _vm.EditCommand.Execute(null);
        }

        private void ItemsListView_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = e.Data.GetFileDrop().Any(x => Directory.Exists(x)) ? DragDropEffects.Copy : DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void ItemsListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                foreach (var path in e.Data.GetFileDrop().Where(x => Directory.Exists(x)))
                {
                    _vm.Add(path);
                }
                e.Handled = true;
            }
        }

        private void ItemsListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Delete)
                {
                    _vm.DeleteCommand.Execute(null);
                }
            }
        }
    }
}

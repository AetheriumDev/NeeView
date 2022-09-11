﻿using System;
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
    /// PageSelectDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSelectDialog : Window
    {
        private readonly PageSelectDialogViewModel _vm;
        private readonly MouseWheelDelta _mouseWheelDelta = new();

        // for designer
        public PageSelectDialog() : this(new PageSelecteDialogModel(5,1,10))
        {
        }


        public PageSelectDialog(PageSelecteDialogModel model)
        {
            InitializeComponent();

            _vm = new PageSelectDialogViewModel(model);
            _vm.Decided += ViewModel_ChangeResult;
            this.DataContext = _vm;

            this.Loaded += PageSelectDialog_Loaded;
            this.ContentRendered += PageSelectDialog_ContentRendered;
        }

        private void ViewModel_ChangeResult(object? sender, PageSelectDialogDecidedEventArgs e)
        {
            this.DialogResult = e.Result;
            this.Close();
        }

        private void PageSelectDialog_Loaded(object? sender, RoutedEventArgs e)
        {
            this.InputValueTextBox.Focus();
        }
        private void PageSelectDialog_ContentRendered(object? sender, EventArgs e)
        {
            this.InputValueTextBox.SelectAll();
        }

        private void PageSelectDialog_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            int turn = _mouseWheelDelta.NotchCount(e);
            _vm.AddValue(-turn);
            this.InputValueTextBox.SelectAll();
        }

        private void PageSelectDialog_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
            }
        }

        private void InputValueTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _vm.DecideCommand.Execute(null);
            }
        }
    }

}

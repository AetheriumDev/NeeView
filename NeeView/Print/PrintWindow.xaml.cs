﻿using NeeView.Windows;
using System;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PrintWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PrintWindow : Window
    {
        private readonly PrintWindowViewModel? _vm;


        public PrintWindow()
        {
            InitializeComponent();
        }

        public PrintWindow(PrintContext context) : this()
        {
            _vm = new PrintWindowViewModel(context);
            this.DataContext = _vm;

            _vm.Close += ViewModel_Close;

            this.Loaded += PrintWindow_Loaded;
            this.Closed += PrintWindow_Closed;
            this.KeyDown += PrintWindow_KeyDown;
        }


        private void PrintWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            this.PrintButton.Focus();
        }

        private void PrintWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }

        /// <summary>
        /// ウィンドウ終了イベント処理
        /// </summary>
        private void PrintWindow_Closed(object? sender, EventArgs e)
        {
            _vm?.Closed();
        }

        /// <summary>
        /// ウィンドウ終了リクエスト処理
        /// </summary>
        private void ViewModel_Close(object? sender, PrintWindowCloseEventArgs e)
        {
            this.DialogResult = e.Result;
            this.Close();
        }
    }
}

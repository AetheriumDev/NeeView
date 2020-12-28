﻿using System;
using System.Collections.Generic;
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

namespace NeeView.Setting
{
    /// <summary>
    /// AddParameterDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AddParameterDialog : Window
    {
        public AddParameterDialog()
        {
            InitializeComponent();

            this.AddButton.Content = Properties.Resources.Word_Add;
            this.CancelButton.Content = Properties.Resources.Word_Cancel;

            this.Loaded += AddParameterDialog_Loaded;
            this.KeyDown += AddParameterDialog_KeyDown;
        }

        private void AddParameterDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        #region DependencyProperties

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(AddParameterDialog), new PropertyMetadata(null));


        public string Input
        {
            get { return (string)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            DependencyProperty.Register("Input", typeof(string), typeof(AddParameterDialog), new PropertyMetadata(null));

        #endregion

        #region Methods

        private void AddParameterDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.InputTextBox.Focus();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.DialogResult = true;
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                Close();
                e.Handled = true;
            }
        }

        #endregion

    }
}

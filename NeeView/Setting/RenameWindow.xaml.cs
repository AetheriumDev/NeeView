﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class RenameWindowParam
    {
        public RenameWindowParam(string text, string defaultText)
        {
            Text = text;
            DefaultText = defaultText;
        }

        public string Text { get; set; }
        public string DefaultText { get; set; }
    }


    /// <summary>
    /// RenameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class RenameWindow : Window, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Property: Text
        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; RaisePropertyChanged(); }
        }
        #endregion

        private RenameWindowParam _param;

        //
        public RenameWindow(RenameWindowParam param)
        {
            _param = param;
            _text = _param.Text;

            InitializeComponent();
            this.DataContext = this;

            this.Loaded += RenameWindow_Loaded;
            this.KeyDown += RenameWindow_KeyDown;
        }

        private void RenameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.NameTextBox.SelectAll();
            this.NameTextBox.Focus();
        }

        private void RenameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Text = _param.DefaultText;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _param.Text = Text;
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

﻿using NeeView.Windows.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Toast.xaml の相互作用ロジック
    /// </summary>
    public partial class ToastCard : UserControl
    {
        public readonly static RoutedCommand CopyCommand = new("CopyCommand", typeof(ToastCard), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.C, ModifierKeys.Control) }));

        private readonly Toast _toast;

        // for Designer
        public ToastCard(): this(new Toast("TEST"))
        {
        }

        public ToastCard(Toast toast)
        {
            InitializeComponent();

            this.Root.PreviewMouseDown += (s, e) => this.Root.Focus();
            this.Root.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Execute));

            _toast = toast;
            Refresh();
        }

        public Toast Toast
        {
            get { return _toast; }
#if false
            private set
            {
                if (_toast != value)
                {
                    _toast = value;
                    Refresh();
                }
            }
#endif
        }

        public bool IsCanceled { get; set; }

        private void Refresh()
        {
            this.Caption.Text = _toast.Caption;
            this.Caption.Visibility = string.IsNullOrEmpty(_toast.Caption) ? Visibility.Collapsed : Visibility.Visible;
            this.Message.IsXHtml = _toast.IsXHtml;
            this.Message.Source = _toast.Message;
            this.ConfirmButton.Content = _toast.ButtonContent;
            this.ConfirmButton.Visibility = _toast.ButtonContent is null ? Visibility.Collapsed : Visibility.Visible;

            this.Icon.Source = _toast.Icon switch
            {
                ToastIcon.Warning => (DrawingImage)this.Resources["tic_warning"],
                ToastIcon.Error => (DrawingImage)this.Resources["tic_error"],
                _ => (DrawingImage)this.Resources["tic_info"],
            };
        }

        private void Copy_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var caption = VisualTreeUtility.CollectElementText(this.Caption);
            var message = VisualTreeUtility.CollectElementText(this.Message);
            Clipboard.SetText(caption + message);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            ToastService.Current.Update();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _toast.RaiseConfirmedEvent();
            IsCanceled = true;
            ToastService.Current.Update();
        }

        // from http://gushwell.ldblog.jp/archives/52279481.html
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ExternalProcess.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}

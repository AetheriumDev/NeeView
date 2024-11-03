﻿using NeeView.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
    public partial class ImageEffectView : UserControl
    {
        private readonly ImageEffectViewModel _vm;
        private bool _isFocusRequest;


        public ImageEffectView(ImageEffect model)
        {
            InitializeComponent();

            _vm = new ImageEffectViewModel(model);
            this.DataContext = _vm;

            this.IsVisibleChanged += ImageEffectView_IsVisibleChanged;

            Debug.WriteLine($"> Create: {nameof(ImageEffectView)}");
        }


        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        private void ImageEffectView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_isFocusRequest && this.IsVisible)
            {
                this.Focus();
                _isFocusRequest = false;
            }
        }

        // フィルターパラメータリセット
        private void Reset(object sender, RoutedEventArgs e)
        {
            _vm.ResetValue();

            this.inspectorF.Refresh();
        }

        public void FocusAtOnce()
        {
            var focused = this.Focus();
            if (!focused)
            {
                _isFocusRequest = true;
            }
        }
    }
}

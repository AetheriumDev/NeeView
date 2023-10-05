﻿using NeeLaboratory.Generators;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// NormalInfoMessageView.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class NormalInfoMessageView : UserControl, INotifyPropertyChanged
    {
        private NormalInfoMessageViewModel? _vm;


        public NormalInfoMessageView()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public NormalInfoMessage Source
        {
            get { return (NormalInfoMessage)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(NormalInfoMessage), typeof(NormalInfoMessageView), new PropertyMetadata(null, SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NormalInfoMessageView control && control.Source != null)
            {
                control.VM = new NormalInfoMessageViewModel(control.Source);
            }
        }

        public NormalInfoMessageViewModel? VM
        {
            get { return _vm; }
            private set { SetProperty(ref _vm, value); }
        }

    }
}

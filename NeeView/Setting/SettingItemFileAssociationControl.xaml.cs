﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace NeeView.Setting
{
    /// <summary>
    /// SettingItemFileAssociationControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemFileAssociationControl : UserControl
    {
        private FileAssociationAccessorCollection? _collection;
        private DisposableCollection _disposables = new();

        public SettingItemFileAssociationControl()
        {
            InitializeComponent();

            this.Loaded += SettingItemFileAssociationControl_Loaded;
            this.Unloaded += SettingItemFileAssociationControl_Unloaded;
        }

        private void SettingItemFileAssociationControl_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            Debug.Assert(window is not null);
            AttachWindowEvents(window);
            _disposables.Add(() => DetachWindowEvents(window));

            var collection = FileAssociationCollectionFactory.Create();
            _collection = new FileAssociationAccessorCollection(collection);

            this.AssociationsPanel.Children.Clear();
            foreach (FileAssociationCategory category in Enum.GetValues<FileAssociationCategory>())
            {
                var control = new SettingItemFileAssociationGroupControl(_collection, category);
                this.AssociationsPanel.Children.Add(control);
            }
        }

        private void AttachWindowEvents(Window window)
        {
            window.Closed += Window_Closed;
            window.Deactivated += Window_Deactivated;
        }

        private void DetachWindowEvents(Window window)
        {
            window.Closed -= Window_Closed;
            window.Deactivated -= Window_Deactivated;
        }

        private void SettingItemFileAssociationControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _disposables.Dispose();
            _disposables.Clear();

            _collection?.Flush();
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            _collection?.Flush();
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            _collection?.Flush();
        }
    }
}

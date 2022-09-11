﻿using NeeLaboratory.Collections.Specialized;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Susie;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    /// <summary>
    /// SusiePluginSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SusiePluginSettingWindow : Window
    {
        private readonly SusiePluginSettingWindowViewModel? _vm;


        public SusiePluginSettingWindow()
        {
            InitializeComponent();

            this.Loaded += SusiePluginSettingWindow_Loaded;
            this.Closed += SusiePluginSettingWindow_Closed;
            this.KeyDown += SusiePluginSettingWindow_KeyDown;
        }

        public SusiePluginSettingWindow(SusiePluginInfo spi) : this()
        {
            _vm = new SusiePluginSettingWindowViewModel(spi);
            this.DataContext = _vm;
        }


        private void SusiePluginSettingWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            this.CloseButton.Focus();
        }

        private void SusiePluginSettingWindow_Closed(object? sender, EventArgs e)
        {
            _vm?.Flush();
        }

        private void SusiePluginSettingWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }


        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConfigButton_Click(object? sender, RoutedEventArgs e)
        {
            _vm?.OpenConfigDialog(this);
        }
    }

    /// <summary>
    /// SusiePluginSettingWindow ViewModel
    /// </summary>
    public class SusiePluginSettingWindowViewModel : BindableBase
    {
        private readonly SusiePluginInfo _spi;

        public SusiePluginSettingWindowViewModel(SusiePluginInfo spi)
        {
            _spi = spi;
            DefaultExtensions = new FileTypeCollection(_spi.DefaultExtension);
            Extensions = new FileTypeCollection(_spi.Extensions);
        }

        public string Name => _spi.Name;

        public string? Version => _spi.PluginVersion;

        public bool IsArchiver => _spi.PluginType == SusiePluginType.Archive;

        public bool IsEnabled
        {
            get { return _spi.IsEnabled; }
            set { _spi.IsEnabled = value; }
        }

        public bool IsCacheEnabled
        {
            get { return _spi.IsCacheEnabled; }
            set { _spi.IsCacheEnabled = value; }
        }

        public bool IsPreExtract
        {
            get { return _spi.IsPreExtract; }
            set { _spi.IsPreExtract = value; }
        }

        public FileTypeCollection DefaultExtensions { get; set; }

        public FileTypeCollection Extensions { get; set; }

        public bool CanOpenConfigDialog => _spi.HasConfigurationDlg;


        public void OpenConfigDialog(Window owner)
        {
            SusiePluginManager.Current.ShowPluginConfigulationDialog(_spi.Name, owner);
        }

        public void Flush()
        {
            _spi.UserExtension = new FileExtensionCollection(Extensions.OneLine);
            Debug.WriteLine($"TODO: Flush UserExtension");
        }
    }

}

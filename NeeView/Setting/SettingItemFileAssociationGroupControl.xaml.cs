﻿using System.Diagnostics;
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
    /// SettingItemFileAssociationGroupControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemFileAssociationGroupControl : UserControl
    {
        private SettingItemFileAssociationGroupViewModel? _vm;


        public SettingItemFileAssociationGroupControl()
        {
            InitializeComponent();
        }

        public SettingItemFileAssociationGroupControl(FileAssociationAccessorCollection collection, FileAssociationCategory category) : this()
        {
            _vm = new SettingItemFileAssociationGroupViewModel(collection, category);
            this.DataContext = _vm;
            this.Loaded += SettingItemFileAssociationGroupControl_Loaded;
            this.Unloaded += SettingItemFileAssociationGroupControl_Unloaded;
        }

        private void SettingItemFileAssociationGroupControl_Loaded(object sender, RoutedEventArgs e)
        {
            _vm?.Attach();
        }

        private void SettingItemFileAssociationGroupControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm?.Detach();
        }
    }
}

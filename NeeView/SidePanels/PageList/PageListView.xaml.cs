﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
    /// PageListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListView : UserControl
    {
        private PageListViewModel _vm;


        //public PageListView()
        //{
        //}

        public PageListView(PageList model)
        {
            InitializeComponent();

            _vm = new PageListViewModel(model);
            this.DockPanel.DataContext = _vm;
        }

        /// <summary>
        /// 履歴戻るボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrevButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(-1, 10);
        }

        /// <summary>
        /// 履歴進むボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(+1, 10);
        }

        #region UI Accessor

        public PageSortMode GetSortMode()
        {
            return (this.PageSortComboBox.SelectedValue is PageSortMode sortMode) ? sortMode : default;
        }

        public void SetSortMode(PageSortMode sortMode)
        {
            this.PageSortComboBox.SetCurrentValue(ComboBox.SelectedValueProperty, sortMode);
        }

        public PageNameFormat GetFormat()
        {
            return (this.FormatComboBox.SelectedValue is PageNameFormat format) ? format : default;
        }

        public void SetFormat(PageNameFormat format)
        {
            this.FormatComboBox.SetCurrentValue(ComboBox.SelectedValueProperty, format);
        }

        #endregion UI Accessor
    }

    public enum PageNameFormat
    {
        [AliasName]
        Smart,

        [AliasName]
        NameOnly,

        [AliasName]
        Raw,
    }


    /// <summary>
    /// 
    /// </summary>
    public class PageNameConverter : IValueConverter
    {
        public Style? SmartTextStyle { get; set; }
        public Style? DefaultTextStyle { get; set; }
        public Style? NameOnlyTextStyle { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var format = (PageNameFormat)value;
                switch (format)
                {
                    default:
                    case PageNameFormat.Raw:
                        return DefaultTextStyle;
                    case PageNameFormat.Smart:
                        return SmartTextStyle;
                    case PageNameFormat.NameOnly:
                        return NameOnlyTextStyle;
                }
            }
            catch { }

            return DefaultTextStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

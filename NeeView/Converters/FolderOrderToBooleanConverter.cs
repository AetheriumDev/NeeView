﻿using System;
using System.Windows.Data;

namespace NeeView
{
    // コンバータ：フォルダーの並びフラグ
    [ValueConversion(typeof(FolderOrder), typeof(bool))]
    public class FolderOrderToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = parameter as string ?? throw new ArgumentException();
            FolderOrder mode0 = (FolderOrder)value;
            FolderOrder mode1 = (FolderOrder)Enum.Parse(typeof(FolderOrder), s);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

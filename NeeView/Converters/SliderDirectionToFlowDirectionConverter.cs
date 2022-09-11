﻿using System;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    // コンバータ：サムネイル方向
    [ValueConversion(typeof(bool), typeof(FlowDirection))]
    public class SliderDirectionToFlowDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isReverse = false; ;
            if (value is bool boolean)
            {
                isReverse = boolean;
            }
            else if (value is string s)
            {
                bool.TryParse(s, out isReverse);
            }

            return isReverse ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

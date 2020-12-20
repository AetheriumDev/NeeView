﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace NeeView
{
    [ValueConversion(typeof(QueryPath), typeof(ImageSource))]
    public class PathToPlaceIconConverter : IMultiValueConverter
    {
        public double Width { get; set; } = 16.0;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) throw new InvalidOperationException();

            if (!(values[0] is QueryPath path))
            {
                return DependencyProperty.UnsetValue;
            }

            if (!(values[1] is double dpiScale))
            {
                return DependencyProperty.UnsetValue;
            }

            return Convert(path)?.GetImageSource(Width * dpiScale);
        }

        public static IImageSourceCollection Convert(QueryPath path)
        {
            if (path != null)
            {
                if (path.Path == null)
                {
                    return new SingleImageSourceCollection(path.Scheme.ToImage());
                }
                else if (path.Scheme == QueryScheme.Bookmark)
                {
                    return new SingleImageSourceCollection(path.Scheme.ToImage());
                }
                else if (path.Scheme == QueryScheme.Pagemark)
                {
                    return new SingleImageSourceCollection(path.Scheme.ToImage());
                }
                else if (path.Search != null)
                {
                    return new SingleImageSourceCollection(MainWindow.Current.Resources["ic_search_24px"] as ImageSource);
                }
                else if (path.Scheme == QueryScheme.File && PlaylistArchive.IsSupportExtension(path.SimplePath))
                {
                    return new SingleImageSourceCollection(MainWindow.Current.Resources["ic_playlist"] as ImageSource);
                }
            }

            return FileIconCollection.Current.CreateDefaultFolderIcon();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

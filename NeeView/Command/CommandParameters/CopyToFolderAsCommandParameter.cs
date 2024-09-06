﻿using NeeView.Windows.Property;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class CopyToFolderAsCommandParameter : CommandParameter
    {
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;
        private int _index;


        /// <summary>
        /// 複数ページのときの動作
        /// </summary>
        [PropertyMember]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { _multiPagePolicy = value; }
        }

        /// <summary>
        /// 選択されたフォルダーの番号。0 は未選択
        /// </summary>
        [PropertyMember(NoteConverter = typeof(IntToDestinationFolderString))]
        public int Index
        {
            get { return _index; }
            set { SetProperty(ref _index, Math.Max(0, value)); }
        }
    }


    public class IntToDestinationFolderString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int index) return "";

            if (index <= 0) return ResourceService.GetString("@Word.SelectionMenu");
            index--;

            var items = Config.Current.System.DestinationFolderCollection;
            if (items.Count <= index) return ResourceService.GetString("@Word.Undefined");

            return items[index].Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

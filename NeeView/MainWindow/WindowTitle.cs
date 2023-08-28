﻿using NeeLaboratory.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// ウィンドウタイトル
    /// </summary>
    public class WindowTitle : BindableBase
    {
        static WindowTitle() => Current = new WindowTitle();
        public static WindowTitle Current { get; }


        private readonly string _defaultWindowTitle;
        private string _title = "";

        private readonly BookHub _bookHub;
        //private readonly MainViewComponent _mainViewComponent;
        private readonly PageFrameBoxPresenter _presenter;
        private readonly TitleStringService _titleStringService;
        private readonly TitleString _titleString;


        public WindowTitle()
        {
            _bookHub = BookHub.Current;
            //_mainViewComponent = MainViewComponent.Current;
            _presenter = MainViewComponent.Current.PageFrameBoxPresenter;
            _titleStringService = TitleStringService.Default;

            _titleString = new TitleString(_titleStringService);
            _titleString.AddPropertyChanged(nameof(TitleString.Title), TitleString_TitleChanged);

            _defaultWindowTitle = $"{Environment.ApplicationName} {Environment.DispVersion}";

            _presenter.ViewContentChanged += (s, e) =>
            {
                UpdateFormat();
            };

            _bookHub.Loading += (s, e) =>
            {
                UpdateTitle();
            };

            Config.Current.WindowTitle.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(WindowTitleConfig.WindowTitleFormat1):
                    case nameof(WindowTitleConfig.WindowTitleFormat2):
                    case nameof(WindowTitleConfig.WindowTitleFormatMedia):
                        UpdateFormat();
                        break;
                }
            };

            UpdateTitle();
        }


        public string Title
        {
            get { return _title; }
            private set { _title = value; RaisePropertyChanged(); }
        }


        private void TitleString_TitleChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateTitle();
        }

        private void UpdateFormat()
        {
            var contents = (_presenter.GetSelectedPageFrameContent()?.ViewContents ?? new List<ViewContent>()).Where(e => !e.Element.IsDummy).ToList();
            var isMedia = contents.FirstOrDefault()?.Element.Page.Entry.Archiver is MediaArchiver == true;

            string format = isMedia
                ? Config.Current.WindowTitle.WindowTitleFormatMedia
                : contents.Count >= 2 ? Config.Current.WindowTitle.WindowTitleFormat2 : Config.Current.WindowTitle.WindowTitleFormat1;

            _titleString.SetFormat(format);
        }

        private void UpdateTitle()
        {
            var address = _bookHub.GetCurrentBook()?.Path;

            if (_bookHub.IsLoading)
            {
                Title = LoosePath.GetFileName(_bookHub.LoadingPath) + " " + Properties.Resources.Notice_LoadingTitle;
            }
            else if (address == null)
            {
                Title = _defaultWindowTitle;
            }
            else if (_presenter.GetSelectedPageFrameContent() == null)
            {
                Title = LoosePath.GetDispName(address);
            }
            else
            {
                Title = _titleString.Title;
            }
        }

    }
}

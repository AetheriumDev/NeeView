﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// 履歴パネル
    /// Type: ControlModel? ViewModelParts?
    /// </summary>
    public class PageListPanel : BindableBase, IPanel
    {
        private readonly PageListView _view;
        private readonly PageListPresenter _presenter;

        public PageListPanel(PageList model)
        {
            _view = new PageListView(model);
            _presenter = new PageListPresenter(_view, model);

            Icon = App.Current.MainWindow.Resources["pic_photo_library_24px"] as ImageSource
                ?? throw new InvalidOperationException("Cannot found resource");
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(PageListPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => Properties.TextResources.GetString("PageList.Title");

        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace { get; set; } = PanelPlace.Right;

        public PageListPresenter Presenter => _presenter;


        public void Refresh()
        {
            // nop.
        }

        public void Focus()
        {
            _presenter.FocusAtOnce();
        }
    }

}

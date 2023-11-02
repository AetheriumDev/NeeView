﻿using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.Setting
{
    /// <summary>
    /// 設定画面 ViewModel
    /// </summary>
    public class SettingWindowViewModel : BindableBase
    {
        private SettingWindowModel _model;


        public SettingWindowViewModel(SettingWindowModel model)
        {
            _model = model;

            _model.SubscribePropertyChanged(nameof(_model.CurrentPage), (s, e) => RaisePropertyChanged(nameof(CurrentPage)));
        }


        public SettingWindowModel Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }

        public SearchBoxModel SearchBoxModel => _model.SearchBoxModel;

        public SettingPage? CurrentPage => _model.CurrentPage;

        public bool IsSearchPageSelected => _model.IsSearchPageSelected;


        public void SelectedItemChanged(SettingPage settingPage)
        {
            _model.SelectedItemChanged(settingPage);
        }
    }
}

﻿using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    // ver 34.0 obsolete.
    [Obsolete]
    public class BookSettingPresenterLegacy : BindableBase
    {
        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public Book.Memento? BookMemento { get; set; }
            [DataMember]
            public Book.Memento? BookMementoDefault { get; set; }
            [DataMember]
            public bool IsUseBookMementoDefault { get; set; }
            [DataMember]
            public BookMementoFilter? HistoryMementoFilter { get; set; }


            public BookSettingPresenter.Memento ToBookSettingPresenter()
            {
                var memento = new BookSettingPresenter.Memento();
                memento.DefaultSetting = BookSettingConfigExtensions.FromBookMement(this.BookMementoDefault);
                if (memento.DefaultSetting != null)
                {
                    memento.DefaultSetting.Page = "";
                }
                memento.LatestSetting = BookSettingConfigExtensions.FromBookMement(this.BookMemento);
                if (memento.LatestSetting != null)
                {
                    memento.LatestSetting.Page = "";
                }

                memento.Generater = new BookSettingPolicyConfig();
                var defaultSelecor = this.IsUseBookMementoDefault ? BookSettingSelectMode.Default : BookSettingSelectMode.Continue;
                var storeSelector = this.IsUseBookMementoDefault ? BookSettingSelectMode.RestoreOrDefault : BookSettingSelectMode.RestoreOrContinue;
                if (this.HistoryMementoFilter != null)
                {
                    memento.Generater.Page = (this.HistoryMementoFilter.Page ? storeSelector : defaultSelecor).ToPageSelectMode();
                    memento.Generater.PageMode = this.HistoryMementoFilter.PageMode ? storeSelector : defaultSelecor;
                    memento.Generater.BookReadOrder = this.HistoryMementoFilter.BookReadOrder ? storeSelector : defaultSelecor;
                    memento.Generater.IsSupportedDividePage = this.HistoryMementoFilter.IsSupportedDividePage ? storeSelector : defaultSelecor;
                    memento.Generater.IsSupportedSingleFirstPage = this.HistoryMementoFilter.IsSupportedSingleFirstPage ? storeSelector : defaultSelecor;
                    memento.Generater.IsSupportedSingleLastPage = this.HistoryMementoFilter.IsSupportedSingleLastPage ? storeSelector : defaultSelecor;
                    memento.Generater.IsSupportedWidePage = this.HistoryMementoFilter.IsSupportedWidePage ? storeSelector : defaultSelecor;
                    memento.Generater.IsRecursiveFolder = this.HistoryMementoFilter.IsRecursiveFolder ? BookSettingSelectMode.RestoreOrDefault : BookSettingSelectMode.Default;
                    memento.Generater.SortMode = this.HistoryMementoFilter.SortMode ? storeSelector : defaultSelecor;
                }

                return memento;
            }
        }
        #endregion
    }
}

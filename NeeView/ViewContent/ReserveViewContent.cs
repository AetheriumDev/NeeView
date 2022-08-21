﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Reserve ViewContent
    /// </summary>
    public class ReserveViewContent : ViewContent
    {
        public ReserveViewContent(MainViewComponent viewComponent, ViewContentSource source, ViewContent old) : base(viewComponent, source)
        {
            this.Size = new Size(480, 680);
            this.Color = old != null ? old.Color : Colors.Black;
        }


        public override bool IsBitmapScalingModeSupported => false;

        public override bool IsViewContent => true;


        private void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            if (this.Source is null) throw new InvalidOperationException();
            this.View = new ViewContentControl(CreateView(this.Source, parameter));
        }

        /// <summary>
        /// 読み込み中ビュー生成
        /// </summary>
        private FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
        {
            var rectangle = new Rectangle();
            rectangle.Fill = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));

            return rectangle;
        }


        public static ViewContent Create(MainViewComponent viewComponent, ViewContentSource source, ViewContent oldViewContent)
        {
            ViewContent viewContent = oldViewContent;
            if (!Config.Current.Performance.IsLoadingPageVisible || oldViewContent.View is null)
            {
                 var newViewContent = new ReserveViewContent(viewComponent, source, oldViewContent);
                newViewContent.Initialize();
                viewContent = newViewContent;
            }

            viewContent.View?.SetMessage(LoosePath.GetFileName(source.Page.EntryName));
            return viewContent;
        }
    }
}

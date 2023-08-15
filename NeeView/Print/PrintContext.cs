﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 印刷コンテキスト
    /// </summary>
    public class PrintContext
    {
        public PrintContext(ViewContent? mainContent, IEnumerable<ViewContent> contents, FrameworkElement view, Transform viewTransform, double viewWidth, double viewHeight, Effect? viewEffect, Brush? background, Brush? backgroundFront)
        {
            MainContent = mainContent;
            Contents = contents;
            View = view;
            ViewTransform = viewTransform;
            ViewWidth = viewWidth;
            ViewHeight = viewHeight;
            ViewEffect = viewEffect;
            Background = background;
            BackgroundFront = backgroundFront;
        }


        /// <summary>
        /// 表示コンテキスト(メインページ)。
        /// 画像印刷での対象
        /// </summary>
        public ViewContent? MainContent { get; set; }

        /// <summary>
        /// メインページの画像
        /// </summary>
#warning not implement yet Printer
        //public ImageSource? RawImage => (MainContent?.Source?.Content as BitmapPageContent)?.ImageSource;
        public ImageSource? RawImage => null;

        /// <summary>
        /// 表示コンテキスト。
        /// 表示印刷での対象
        /// </summary>
        public IEnumerable<ViewContent> Contents { get; set; }

        /// <summary>
        /// 表示コンテキスト余白。ページの隙間
        /// </summary>
        public Thickness ContentMargin { get; set; }

        /// <summary>
        /// 表示エリア
        /// </summary>
        public FrameworkElement View { get; set; }

        /// <summary>
        /// 表示座標行列
        /// </summary>
        public Transform ViewTransform { get; set; }

        /// <summary>
        /// 表示エリアサイズ
        /// </summary>
        public double ViewWidth { get; set; }
        public double ViewHeight { get; set; }

        /// <summary>
        /// エフェクト
        /// </summary>
        public Effect? ViewEffect { get; set; }

        /// <summary>
        /// 背景ブラシ
        /// </summary>
        public Brush? Background { get; set; }

        /// <summary>
        /// 背景ブラシ
        /// </summary>
        public Brush? BackgroundFront { get; set; }
    }
}

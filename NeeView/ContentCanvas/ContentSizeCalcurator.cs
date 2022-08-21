﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// コンテンツ表示サイズ計算結果
    /// </summary>
    public class FixedContentSize
    {
        public FixedContentSize(List<Size> sourceSizeList, List<Size> contentSizeList, double angle, Thickness margin)
        {
            SourceSizeList = sourceSizeList;
            ContentSizeList = contentSizeList;
            ContentAngle = angle;
            ContentsMargin = margin;
        }

        /// <summary>
        /// コンテンツのオリジナルサイズ
        /// </summary>
        public List<Size> SourceSizeList { get; set; }

        /// <summary>
        /// 表示コンテンツサイズ
        /// </summary>
        public List<Size> ContentSizeList { get; set; }

        /// <summary>
        /// 表示コンテンツ角度
        /// </summary>
        public double ContentAngle { get; set; }

        /// <summary>
        /// 表示コンテンツの間隔
        /// </summary>
        public Thickness ContentsMargin { get; set; }

        /// <summary>
        /// コンテンツ全体の幅
        /// </summary>
        public double Width => ContentSizeList[0].Width + ContentSizeList[1].Width + ContentsMargin.Left;

        /// <summary>
        /// コンテンツ全体の高さ
        /// </summary>
        public double Height => Math.Max(ContentSizeList[0].Height, ContentSizeList[1].Height);


        /// <summary>
        /// 表示コンテンツサイズのオリジナル基準スケールを得る。
        /// 複数コンテンツの場合は拡大率が小さいものを基準にする。
        /// </summary>
        /// <returns>倍率</returns>
        public double GetScale()
        {
            var scale0 = (ContentSizeList[0].Width != 0.0) ? ContentSizeList[0].Width / SourceSizeList[0].Width : 0.0;
            var scale1 = (ContentSizeList[1].Width != 0.0) ? ContentSizeList[1].Width / SourceSizeList[1].Width : 0.0;

            if (scale0 == 0.0) return scale1;
            if (scale1 == 0.0) return scale0;
            return Math.Min(scale0, scale1);
        }
    }

    /// <summary>
    /// コンテンツ表示サイズ計算機
    /// </summary>
    public class ContentSizeCalcurator
    {
        private PageStretchMode StretchMode => Config.Current.View.StretchMode;
        private double ContentsSpace => Config.Current.Book.ContentsSpace;
        private AutoRotateType AutoRotateType => Config.Current.View.AutoRotate;
        private bool AllowEnlarge => Config.Current.View.AllowStretchScaleUp;
        private bool AllowReduce => Config.Current.View.AllowStretchScaleDown;



        /// <summary>
        /// コンテンツ表示サイズを計算。
        /// 角度は自動回転から求める。
        /// </summary>
        /// <param name="source">元のコンテンツサイズ</param>
        /// <returns></returns>
        public FixedContentSize GetFixedContentSize(List<Size> source, Size viewSize, AngleResetMode angleResetMode, double defaultAngle, DpiScale dpiScale)
        {
            return GetFixedContentSize(source, viewSize, GetAutoRotateAngle(source, viewSize, angleResetMode, defaultAngle), dpiScale);
        }

        /// <summary>
        /// コンテンツ表示サイズを計算。
        /// </summary>
        /// <param name="source">元のコンテンツサイズ</param>
        /// <param name="viewSize">ビューエリアサイズ</param>
        /// <param name="angle">角度</param>
        /// <returns></returns>
        public FixedContentSize GetFixedContentSize(List<Size> source, Size viewSize, double angle, DpiScale dpiScale)
        {
            // 2ページ表示時は重なり補正を行う
            double offsetWidth = (source[0].Width > 0.5 && source[1].Width > 0.5) ? ContentsSpace : 0.0;

            // Viewにあわせたコンテンツサイズ
            var sizes = CalcContentSize(source, viewSize.Width, viewSize.Height, offsetWidth, angle, dpiScale);

            var contentSizeList = sizes.Select(e => e.IsEmpty ? SizeExtensions.Zero : new Size(e.Width, e.Height)).ToList();
            var result = new FixedContentSize(source, contentSizeList, angle, new Thickness(offsetWidth, 0, 0, 0));
            return result;
        }

        /// <summary>
        /// 自動回転角度を計算
        /// </summary>
        /// <param name="source">元のコンテンツサイズ</param>
        /// <returns></returns>
        public double GetAutoRotateAngle(List<Size> source, Size viewSize, AngleResetMode angleResetMode, double defaultAngle)
        {
            switch (angleResetMode)
            {
                case AngleResetMode.None:
                    return defaultAngle;

                case AngleResetMode.ForceAutoRotate:
                    return this.AutoRotateType.ToAngle();

                default:
                case AngleResetMode.Normal:
                    return CheckAutoRotate(GetContentSize(source), viewSize).ToAngle();
            }
        }

        // 自動回転する？
        private AutoRotateType CheckAutoRotate(Size contentSize, Size viewSize)
        {
            if (Config.Current.View.AutoRotate == AutoRotateType.None) return AutoRotateType.None;

            if (viewSize.Height <= 0.0) return AutoRotateType.None;
            var viewRatio = viewSize.Width / viewSize.Height;

            if (contentSize.IsEmptyOrZero()) return AutoRotateType.None;
            var contentRatio = contentSize.Width / contentSize.Height;

            // NOTE: サイズ指定に問題が生じるため、マージンはなし
            double margin = 0.0;

            var isAutoRotated = viewRatio >= 1.0 ? contentRatio < (1.0 - margin) : contentRatio > (1.0 + margin);
            return isAutoRotated ? Config.Current.View.AutoRotate : AutoRotateType.None;
        }

        //
        private Size GetContentSize(List<Size> source)
        {
            var c0 = source[0];
            var c1 = source[1];

            double rate0 = 1.0;
            double rate1 = 1.0;

            // 2ページ合わせたコンテンツの表示サイズを求める

            // どちらもImageでない
            if (c0.Width < 0.1 && c1.Width < 0.1)
            {
                return new Size(1.0, 1.0);
            }
            else if (c1.IsZero())
            {
                return c0;
            }
            // オリジナルサイズ
            else if (this.StretchMode == PageStretchMode.None)
            {
                return new Size(c0.Width + c1.Width, Math.Max(c0.Height, c1.Height));
            }
            else
            {
                if (c0.Width == 0) c0 = c1;
                if (c1.Width == 0) c1 = c0;

                // 高さを 高い方に合わせる
                if (c0.Height > c1.Height)
                {
                    rate1 = c0.Height / c1.Height;
                }
                else
                {
                    rate0 = c1.Height / c0.Height;
                }

                // 高さをあわせたときの幅の合計
                return new Size(c0.Width * rate0 + c1.Width * rate1, c0.Height * rate0);
            }
        }

        // ストレッチモードに合わせて各コンテンツのスケールを計算する。BaseScaleを適用
        private Size[] CalcContentSize(List<Size> source, double width, double height, double margin, double angle, DpiScale dpiScale)
        {
            var sizes = CalcContentSizeBase(source, width, height, margin, angle, dpiScale);

            if (Config.Current.View.IsBaseScaleEnabled)
            {
                return sizes.Select(e => e.Multi(Config.Current.View.BaseScale)).ToArray();
            }
            else
            {
                return sizes;
            }
        }

        // ストレッチモードに合わせて各コンテンツのスケールを計算する
        private Size[] CalcContentSizeBase(List<Size> source, double width, double height, double margin, double angle, DpiScale dpiScale)
        {
            if (width < 1.0) width = 1.0;
            if (height < 1.0) height = 1.0;

            var c0 = source[0];
            var c1 = source[1];

            var dpiRate = 1.0 / dpiScale.DpiScaleX;
            var d0 = c0.IsEmpty ? c0 : c0.Multi(dpiRate);
            var d1 = c1.IsEmpty ? c1 : c1.Multi(dpiRate);
            var originalSize = new Size[] { d0, d1 };

            // オリジナルサイズ
            if (this.StretchMode == PageStretchMode.None)
            {
                return originalSize;
            }

            double rate0 = 1.0;
            double rate1 = 1.0;

            // 2ページ合わせたコンテンツの表示サイズを求める
            Size content;

            // どちらもImageでない
            if (c0.Width < 0.1 && c1.Width < 0.1)
            {
                return originalSize;
            }
            else if (c1.IsZero())
            {
                content = c0;
            }
            else
            {
                if (c0.Width == 0) c0 = c1;
                if (c1.Width == 0) c1 = c0;

                // 高さを 高い方に合わせる
                if (c0.Height > c1.Height)
                {
                    rate1 = c0.Height / c1.Height;
                }
                else
                {
                    rate0 = c1.Height / c0.Height;
                }

                // 高さをあわせたときの幅の合計
                content = new Size(c0.Width * rate0 + c1.Width * rate1, c0.Height * rate0);
            }

            var marginSign = margin < 0.0 ? -1.0 : 1.0;
            var marginSize = new Size(margin * marginSign, 0.0);

            // 回転反映
            {
                var rotate = new Matrix();
                rotate.Rotate(angle);

                var rect = new Rect(content);
                rect.Transform(rotate);
                content = new Size(rect.Width, rect.Height);

                var marginRect = new Rect(marginSize);
                marginRect.Transform(rotate);
                marginSize = new Size(marginRect.Width, marginRect.Height);
            }

            // ビューエリアサイズに合わせる場合のスケール
            double rateW = (width - marginSize.Width * marginSign) / content.Width;
            double rateH = (height - marginSize.Height * marginSign) / content.Height;

            // 拡大制限
            if (!AllowEnlarge)
            {
                if (rateW > 1.0) rateW = 1.0;
                if (rateH > 1.0) rateH = 1.0;
            }
            // 縮小制限
            if (!AllowReduce)
            {
                if (rateW < 1.0) rateW = 1.0;
                if (rateH < 1.0) rateH = 1.0;
            }

            // 面積をあわせる
            if (this.StretchMode == PageStretchMode.UniformToSize)
            {
                var viewSize = width * height;
                var contentSize = content.Width * content.Height;
                var rate = Math.Sqrt(viewSize / contentSize);
                if (rate > 1.0 && !AllowEnlarge) rate = 1.0;
                if (rate < 1.0 && !AllowReduce) rate = 1.0;
                rate0 *= rate;
                rate1 *= rate;
            }
            // 高さを合わせる
            else if (this.StretchMode == PageStretchMode.UniformToVertical)
            {
                rate0 *= rateH;
                rate1 *= rateH;
            }
            // 幅を合わせる
            else if (this.StretchMode == PageStretchMode.UniformToHorizontal)
            {
                rate0 *= rateW;
                rate1 *= rateW;
            }
            // 枠いっぱいに広げる
            else if (this.StretchMode == PageStretchMode.UniformToFill)
            {
                if (rateW > rateH)
                {
                    rate0 *= rateW;
                    rate1 *= rateW;
                }
                else
                {
                    rate0 *= rateH;
                    rate1 *= rateH;
                }
            }
            // 枠に収めるように広げる
            else
            {
                if (rateW < rateH)
                {
                    rate0 *= rateW;
                    rate1 *= rateW;
                }
                else
                {
                    rate0 *= rateH;
                    rate1 *= rateH;
                }
            }

            // 幅が整数になるように補正
            if (c0.Width > 0.0)
            {
                rate0 = Math.Truncate(c0.Width * rate0) / c0.Width;
            }
            if (c1.Width > 0.0)
            {
                rate1 = Math.Truncate(c1.Width * rate1) / c1.Width;
            }

            var s0 = new Size(c0.Width * rate0, c0.Height * rate0);
            var s1 = new Size(c1.Width * rate1, c1.Height * rate1);
            return new Size[] { s0, s1 };
        }
    }
}

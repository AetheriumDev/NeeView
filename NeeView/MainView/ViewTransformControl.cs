﻿using NeeLaboratory;
using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 表示のTransform操作
    /// </summary>
    // TODO: PageFrames.ViewTransformControl と名前が競合
    public class ViewTransformControl : IViewTransformControl
    {
        private PageFrameBoxPresenter _presenter;


        public ViewTransformControl(PageFrameBoxPresenter presenter)
        {
            _presenter = presenter;
        }



        // 水平スクロールの正方向
        // TODO: どうやって取得？どこから取得？
        public double ViewHorizontalDirection => Config.Current.BookSetting.BookReadOrder == PageReadOrder.LeftToRight ? 1.0 : -1.0;



        public void ScaleDown(ViewScaleCommandParameter parameter)
        {
            var control = GetDragTransform(Config.Current.View.ScaleCenter == DragControlCenter.Cursor);
            if (control is null) return;

            var scaleDelta = parameter.Scale;
            var isSnap = parameter.IsSnapDefaultScale;
            Debug.Assert(scaleDelta >= 0.0);
            var scale = control.Context.BaseScale / (1.0 + scaleDelta);

            // TODO: 100%となるスケール。表示の100%にするかソースの100%にするかで変わってくる
            var originalScale = 1.0;

            if (isSnap)
            {
                if (Config.Current.Notice.IsOriginalScaleShowMessage && originalScale > 0.0)
                {
                    // original scale 100% snap
                    if (control.Context.BaseScale * originalScale > 1.01 && scale * originalScale < 1.01)
                    {
                        scale = 1.0 / originalScale;
                    }
                }
                else
                {
                    // visual scale 100% snap
                    if (control.Context.BaseScale > 1.01 && scale < 1.01)
                    {
                        scale = 1.0;
                    }
                }
            }

            control.DoScale(scale, TimeSpan.Zero);
        }

        public void ScaleUp(ViewScaleCommandParameter parameter)
        {
            var control = GetDragTransform(Config.Current.View.ScaleCenter == DragControlCenter.Cursor);
            if (control is null) return;

            var scaleDelta = parameter.Scale;
            var isSnap = parameter.IsSnapDefaultScale;
            Debug.Assert(scaleDelta >= 0.0);
            var scale = control.Context.BaseScale * (1.0 + scaleDelta);

            // TODO: 100%となるスケール。表示の100%にするかソースの100%にするかで変わってくる
            var originalScale = 1.0;

            if (isSnap)
            {
                if (Config.Current.Notice.IsOriginalScaleShowMessage && originalScale > 0.0)
                {
                    // original scale 100% snap
                    if (control.Context.BaseScale * originalScale < 0.99 && scale * originalScale > 0.99)
                    {
                        scale = 1.0 / originalScale;
                    }
                }
                else
                {
                    // visual scale 100% snap
                    if (control.Context.BaseScale < 0.99 && scale > 0.99)
                    {
                        scale = 1.0;
                    }
                }
            }

            control.DoScale(scale, TimeSpan.Zero);
        }


        public void ViewRotateLeft(ViewRotateCommandParameter parameter)
        {
            Rotate(-parameter.Angle, parameter.IsStretch);
        }

        public void ViewRotateRight(ViewRotateCommandParameter parameter)
        {
            Rotate(parameter.Angle, parameter.IsStretch);
        }

        private void Rotate(double angle, bool isStretch)
        {
            var control = GetDragTransform(Config.Current.View.RotateCenter == DragControlCenter.Cursor);
            if (control is null) return;

            // スナップ値を下限にする
            if (Math.Abs(angle) < Config.Current.View.AngleFrequency)
            {
                angle = Config.Current.View.AngleFrequency * Math.Sign(angle);
            }

            control.DoRotate(MathUtility.NormalizeLoopRange(control.Context.BaseAngle + angle, -180, 180), TimeSpan.Zero);

            if (isStretch)
            {
                Stretch();
            }
        }


        public void Stretch(bool ignoreViewOrigin = false)
        {
            _presenter.Stretch(ignoreViewOrigin);
        }




        public void ScrollLeft(ViewScrollCommandParameter parameter)
        {
            var control = GetDragTransform(false);
            if (control is null) return;

            var rate = parameter.Scroll;
            var span = TimeSpan.FromSeconds(parameter.ScrollDuration);

            var old = control.Point;
            control.DoMove(new Vector(control.Context.ViewRect.Width * rate, 0), span);

            if (parameter.AllowCrossScroll && control.Point.X == old.X)
            {
                control.DoMove(new Vector(0, control.Context.ViewRect.Height * rate * ViewHorizontalDirection), span);
            }
        }

        public void ScrollRight(ViewScrollCommandParameter parameter)
        {
            var control = GetDragTransform(false);
            if (control is null) return;

            var rate = parameter.Scroll;
            var span = TimeSpan.FromSeconds(parameter.ScrollDuration);

            var old = control.Point;
            control.DoMove(new Vector(control.Context.ViewRect.Width * -rate, 0), span);

            if (parameter.AllowCrossScroll && control.Point.X == old.X)
            {
                control.DoMove(new Vector(0, control.Context.ViewRect.Height * -rate * ViewHorizontalDirection), span);
            }
        }

        public void ScrollDown(ViewScrollCommandParameter parameter)
        {
            var control = GetDragTransform(false);
            if (control is null) return;

            var rate = parameter.Scroll;
            var span = TimeSpan.FromSeconds(parameter.ScrollDuration);

            var old = control.Point;
            control.DoMove(new Vector(0, control.Context.ViewRect.Height * -rate), span);

            if (parameter.AllowCrossScroll && control.Point.Y == old.Y)
            {
                control.DoMove(new Vector(control.Context.ViewRect.Width * -rate * ViewHorizontalDirection, 0), span);
            }
        }

        public void ScrollUp(ViewScrollCommandParameter parameter)
        {
            var control = GetDragTransform(false);
            if (control is null) return;

            var rate = parameter.Scroll;
            var span = TimeSpan.FromSeconds(parameter.ScrollDuration);

            var old = control.Point;
            control.DoMove(new Vector(0, control.Context.ViewRect.Height * rate), span);

            if (parameter.AllowCrossScroll && control.Point.Y == old.Y)
            {
                control.DoMove(new Vector(control.Context.ViewRect.Width * rate * ViewHorizontalDirection, 0), span);
            }
        }

        public void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter)
        {
            _presenter.ScrollToNext(LinkedListDirection.Next, parameter);
        }

        public void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter)
        {
            _presenter.ScrollToNext(LinkedListDirection.Previous, parameter);
        }

        public void NextScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _presenter.ScrollToNextFrame(LinkedListDirection.Next, parameter, parameter.LineBreakStopMode, parameter.EndMargin);
        }

        public void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _presenter.ScrollToNextFrame(LinkedListDirection.Previous, parameter, parameter.LineBreakStopMode, parameter.EndMargin);
        }


        public void FlipHorizontal(bool isFlip)
        {
            var transform = GetDragTransform(Config.Current.View.FlipCenter == DragControlCenter.Cursor);
            transform?.DoFlipHorizontal(isFlip, TimeSpan.Zero);
        }

        public void FlipVertical(bool isFlip)
        {
            var transform = GetDragTransform(Config.Current.View.FlipCenter == DragControlCenter.Cursor);
            transform?.DoFlipVertical(isFlip, TimeSpan.Zero);
        }

        public void ToggleFlipHorizontal()
        {
            var transform = GetDragTransform(Config.Current.View.FlipCenter == DragControlCenter.Cursor);
            transform?.DoFlipHorizontal(!transform.IsFlipHorizontal, TimeSpan.Zero);
        }

        public void ToggleFlipVertical()
        {
            var transform = GetDragTransform(Config.Current.View.FlipCenter == DragControlCenter.Cursor);
            transform?.DoFlipVertical(!transform.IsFlipVertical, TimeSpan.Zero);
        }



        private DragTransform? GetDragTransform(bool isPointed)
        {
            var context = _presenter.CreateDragTransformContext(isPointed, false);
            if (context is null) return null;
            return new DragTransform(context);
        }


        public void ResetContentSizeAndTransform()
        {
            _presenter.ResetTransform();
        }
    }

#if false
    public class ViewTransformControl : IViewTransformControl
    {
        private readonly MainViewComponent _viewComponent;
        private readonly ScrollPageController _scrollPageControl;

        public ViewTransformControl(MainViewComponent viewComponent, ScrollPageController scrollPageControl)
        {
            _viewComponent = viewComponent;
            _scrollPageControl = scrollPageControl;
        }

        public void FlipHorizontal(bool isFlip)
        {
            _viewComponent.DragTransformControl.FlipHorizontal(isFlip);
        }

        public void FlipVertical(bool isFlip)
        {
            _viewComponent.DragTransformControl.FlipVertical(isFlip);
        }

        public void ToggleFlipHorizontal()
        {
            _viewComponent.DragTransformControl.ToggleFlipHorizontal();
        }

        public void ToggleFlipVertical()
        {
            _viewComponent.DragTransformControl.ToggleFlipVertical();
        }

        public void ResetContentSizeAndTransform()
        {
            _viewComponent.ContentCanvas.ResetContentSizeAndTransform(new ResetTransformCondition(true));
        }

        public void ViewRotateLeft(ViewRotateCommandParameter parameter)
        {
            _viewComponent.ContentCanvas.ViewRotateLeft(parameter);
        }

        public void ViewRotateRight(ViewRotateCommandParameter parameter)
        {
            _viewComponent.ContentCanvas.ViewRotateRight(parameter);
        }

        public void ScaleDown(ViewScaleCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScaleDown(parameter.Scale, parameter.IsSnapDefaultScale, _viewComponent.ContentCanvas.MainContentScale);
        }

        public void ScaleUp(ViewScaleCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScaleUp(parameter.Scale, parameter.IsSnapDefaultScale, _viewComponent.ContentCanvas.MainContentScale);
        }

        public void ScrollUp(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollUp(parameter);
        }

        public void ScrollDown(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollDown(parameter);
        }

        public void ScrollLeft(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollLeft(parameter);
        }

        public void ScrollRight(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollRight(parameter);
        }


        public void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter)
        {
            _scrollPageControl.ScrollNTypeUp(parameter);
        }

        public void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter)
        {
            _scrollPageControl.ScrollNTypeDown(parameter);
        }

        public void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _scrollPageControl.PrevScrollPage(sender, parameter);
        }

        public void NextScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _scrollPageControl.NextScrollPage(sender, parameter);
        }

    }
#endif
}

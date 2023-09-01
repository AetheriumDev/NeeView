﻿using System;
using System.Windows;
using NeeLaboratory;
using NeeView;

namespace NeeView
{
    public class HoverDragAction : DragAction
    {
        public HoverDragAction()
        {
            DragKey = new DragKey("MiddleButton");
            DragActionCategory = DragActionCategory.Point;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ActionControl(context, this);
        }



        private class ActionControl : DragActionControl
        {
            private MouseConfig _mouseConfig;
            private Point _basePoint;


            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _mouseConfig = Config.Current.Mouse;

                // TODO: ViewTransform か ContentTransform かの区別はこれでいいのか？
                if (context.Transform is PageFrames.ViewTransformControl)
                {
                    _basePoint = context.Transform.Point - (Vector)context.ContentCenter;
                }
                else
                {
                    _basePoint = default;
                }
            }


            public override void Execute()
            {
                Context.UpdateRect();
                HoverScroll(Context.Last, TimeSpan.FromSeconds(_mouseConfig.HoverScrollDuration));
            }

            /// <summary>
            /// Hover scroll
            /// </summary>
            /// <param name="point">point in sender</param>
            /// <param name="span">scroll time</param>
            private void HoverScroll(Point point, TimeSpan span)
            {
                var rateX = point.X / Context.ViewRect.Width * -2.0;
                var rateY = point.Y / Context.ViewRect.Height * -2.0;
                HoverScroll(rateX, rateY, span);
            }

            /// <summary>
            /// Hover scroll
            /// </summary>
            /// <param name="rateX">point.X rate in sender [-1.0, 1.0]</param>
            /// <param name="rateY">point.Y rate in sender [-1.0, 1.0]</param>
            /// <param name="span">scroll time</param>
            private void HoverScroll(double rateX, double rateY, TimeSpan span)
            {
                // TODO: StaticFrame のみ？
                // ブラウザのようなスクロール(AutoScroll)は別機能

                var x = Math.Max(Context.ContentRect.Width - Context.ViewRect.Width, 0.0) * rateX.Clamp(-0.5, 0.5);
                var y = Math.Max(Context.ContentRect.Height - Context.ViewRect.Height, 0.0) * rateY.Clamp(-0.5, 0.5);
                var pos = new Point(_basePoint.X + x, _basePoint.Y + y);

                Context.Transform.SetPoint(pos, span);
            }
        }
    }
}
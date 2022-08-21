﻿using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// タッチ通常入力状態
    /// </summary>
    public class TouchInputNormal : TouchInputBase
    {
        /// <summary>
        /// 押されている？
        /// </summary>
        private bool _isTouchDown;

        /// <summary>
        /// 現在のタッチデバイス
        /// </summary>
        private TouchContext? _touch;

        //
        private TouchInputGesture _gesture;


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputNormal(TouchInputContext context, TouchInputGesture gesture) : base(context)
        {
            _gesture = gesture;
        }

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            ////Debug.WriteLine("TouchState: Normal");
            _isTouchDown = false;
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
        }


        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            // マルチタッチでドラッグへ
            if (_context.TouchMap.Count >= 2)
            {
                SetState(TouchInputState.Drag, _touch);
                return;
            }

            _context.TouchMap.TryGetValue(e.StylusDevice, out _touch);
            if (_touch == null) return;

            _isTouchDown = true;
            _context.Sender.Focus();

            e.Handled = true;
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusUp(object sender, StylusEventArgs e)
        {
            if (!_isTouchDown) return;
            if (e.StylusDevice != _touch?.StylusDevice) return;

            // タッチジェスチャー判定
            ExecuteTouchGesture(sender, e);

            // その後の操作は全て無効
            _isTouchDown = false;
        }


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (!_isTouchDown) return;
            if (e.StylusDevice != _touch?.StylusDevice) return;

            var point = e.GetPosition(_context.Sender);

            var touchStart = _context.TouchMap[e.StylusDevice].StartPoint;
            var deltaX = Math.Abs(point.X - touchStart.X);
            var deltaY = Math.Abs(point.Y - touchStart.Y);

            // drag check
            if (deltaX > Config.Current.Touch.GestureMinimumDistance || deltaY > Config.Current.Touch.GestureMinimumDistance)
            {
                SetState(Config.Current.Touch.DragAction);
            }
        }

        public override void OnStylusInAirMove(object sender, StylusEventArgs e)
        {
            if (Config.Current.Mouse.IsHoverScroll)
            {
                HoverScroll(sender, e);
            }
        }

        /// <summary>
        /// ホバースクロール
        /// </summary>
        private void HoverScroll(object sender, StylusEventArgs e)
        {
            if (_context.DragTransformControl is null) return;

            var point = e.GetPosition(_context.Sender);
            _context.DragTransformControl.HoverScroll(point);
        }

        //
        public override void OnStylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            if (!_isTouchDown) return;
            if (e.StylusDevice != _touch?.StylusDevice) return;

            if (e.SystemGesture == SystemGesture.HoldEnter || e.SystemGesture == SystemGesture.RightDrag)
            {
                SetState(Config.Current.Touch.HoldAction);
            }
        }

        //
        private void SetState(TouchAction action)
        {
            if (_touch is null) return;

            switch (action)
            {
                case TouchAction.Drag:
                    SetState(TouchInputState.Drag, _touch);
                    break;
                case TouchAction.MouseDrag:
                    SetState(TouchInputState.MouseDrag, _touch);
                    break;
                case TouchAction.Gesture:
                    SetState(TouchInputState.Gesture, _touch);
                    break;
                case TouchAction.Loupe:
                    SetState(TouchInputState.Loupe, _touch);
                    break;
            }
        }

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public TouchAction DragAction { get; set; }
            [DataMember]
            public TouchAction HoldAction { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Touch.DragAction = DragAction;
                config.Touch.HoldAction = HoldAction;
            }
        }

        #endregion

    }


}

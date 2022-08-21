﻿using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウスジェスチャー
    /// </summary>
    public class MouseInputGesture : MouseInputBase
    {
        /// <summary>
        /// 入力トラッカー
        /// </summary>
        private MouseGestureSequenceTracker _gesture;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public MouseInputGesture(MouseInputContext context) : base(context)
        {
            _gesture = new MouseGestureSequenceTracker();
            _gesture.GestureProgressed += (s, e) => GestureProgressed?.Invoke(this, new MouseGestureEventArgs(_gesture.Sequence));
        }


        /// <summary>
        /// ジェスチャー進捗通知
        /// </summary>
        public event EventHandler<MouseGestureEventArgs>? GestureProgressed;

        /// <summary>
        /// ジェスチャー確定通知
        /// </summary>
        public event EventHandler<MouseGestureEventArgs>? GestureChanged;


        //
        public void Reset()
        {
            _gesture.Reset(_context.StartPoint);
        }


        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            if (sender.Cursor != Cursors.None)
            {
                sender.Cursor = null;
            }
            ////Reset();

            _gesture.Reset(_context.StartPoint);
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
        }

        public override void OnCaptureOpened(FrameworkElement sender)
        {
            MouseInputHelper.CaptureMouse(this, sender);
        }

        public override void OnCaptureClosed(FrameworkElement sender)
        {
            MouseInputHelper.ReleaseMouseCapture(this, sender);
        }

        /// <summary>
        /// ボタン押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
        {
            UpdateState(sender, e);
            if (e.Handled) return;

            // 右ボタンのみジェスチャー終端として認識
            if (e.ChangedButton == MouseButton.Left && _gesture.Sequence.Count > 0)
            {
                // ジェスチャーコマンド実行
                _gesture.Sequence.Add(MouseGestureDirection.Click);
                var args = new MouseGestureEventArgs(_gesture.Sequence);
                GestureChanged?.Invoke(sender, args);
            }

            // ジェスチャー解除
            ResetState();
        }

        /// <summary>
        /// ボタン離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
        {
            // ジェスチャーコマンド実行
            if (_gesture.Sequence.Count > 0)
            {
                var args = new MouseGestureEventArgs(_gesture.Sequence);
                GestureChanged?.Invoke(sender, args);
                e.Handled = args.Handled;
            }

            // ジェスチャー解除
            ResetState();
        }

        /// <summary>
        /// ホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            // ホイール入力確定
            MouseWheelChanged?.Invoke(sender, e);

            // ジェスチャー解除
            if (e.Handled)
            {
                ResetState();
            }
        }

        /// <summary>
        /// 水平ホイール処理
        /// </summary>
        public override void OnMouseHorizontalWheel(object? sender, MouseWheelEventArgs e)
        {
            // ホイール入力確定
            MouseHorizontalWheelChanged?.Invoke(sender, e);

            // ジェスチャー解除
            if (e.Handled)
            {
                ResetState();
            }
        }

        private void UpdateState(object? sender, MouseEventArgs e)
        {
            // ジェスチャー認識前に他のドラッグに切り替わったら処理を切り替える
            if (_gesture.Sequence.Count > 0) return;

            var action = DragActionTable.Current.GetActionType(new DragKey(CreateMouseButtonBits(e), Keyboard.Modifiers));
            switch (action ?? "")
            {
                case "":
                    break;
                case DragActionTable.GestureDragActionName:
                    break;
                default:
                    SetState(MouseInputState.Drag, e);
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(object? sender, MouseEventArgs e)
        {
            UpdateState(sender, e);
            if (e.Handled) return;

            var point = e.GetPosition(_context.Sender);

            _gesture.Move(point);
        }

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember(Name = "GestureMinimumDistanceX"), DefaultValue(30.0)]
            public double GestureMinimumDistance { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Mouse.GestureMinimumDistance = GestureMinimumDistance;
            }
        }

        #endregion

    }
}

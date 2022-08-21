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
    /// タッチジェスチャー
    /// </summary>
    public class TouchInputGesture : TouchInputBase
    {
        /// <summary>
        /// ジェスチャー入力
        /// </summary>
        private MouseGestureSequenceTracker _gesture;

        /// <summary>
        /// 監視するデバイス
        /// </summary>
        private TouchContext? _touch;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public TouchInputGesture(TouchInputContext context) : base(context)
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



        public void Reset()
        {
            if (_touch == null) return;
            _gesture.Reset(_touch.StartPoint);
        }


        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            ////Debug.WriteLine("TouchState: Gesture");

            _touch = parameter as TouchContext ?? throw new InvalidOperationException("parameter must be TouchContext");

            MouseInputHelper.CaptureMouse(this, sender);
            if (sender.Cursor != Cursors.None)
            {
                sender.Cursor = null;
            }
            Reset();
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            MouseInputHelper.ReleaseMouseCapture(this, sender);
        }

        /// <summary>
        /// ボタン押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            if (e.Handled) return;

            // マルチタッチはドラッグへ
            SetState(TouchInputState.Drag, _touch);
        }

        /// <summary>
        /// ボタン離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusUp(object sender, StylusEventArgs e)
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
        /// タッチ移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (e.Handled) return;
            if (e.StylusDevice != _touch?.StylusDevice) return;

            var point = e.GetPosition(_context.Sender);

            _gesture.Move(point);
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember(Name = "GestureMinimumDistanceX"), DefaultValue(16.0)]
            public double GestureMinimumDistance { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Touch.GestureMinimumDistance = GestureMinimumDistance;

            }
        }

        #endregion
    }
}

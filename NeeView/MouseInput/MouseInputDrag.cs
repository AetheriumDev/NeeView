﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{

    /// <summary>
    /// ドラッグ操作
    /// </summary>
    public class MouseInputDrag : MouseInputBase
    {
        readonly IDragTransformControl _dragTransformControl;

        public MouseInputDrag(MouseInputContext context) : base(context)
        {
            if (context.DragTransformControl is null) throw new InvalidOperationException();
            _dragTransformControl = context.DragTransformControl;
        }


        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            sender.Cursor = Cursors.Hand;

            _dragTransformControl.ResetState();
            _dragTransformControl.UpdateState(CreateMouseButtonBits(), Keyboard.Modifiers, ToDragCoord(_context.StartPoint), _context.StartTimestamp, DragActionUpdateOptions.None);
        }

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

        public override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
        {
            // nop.
        }

        public override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
        {
            OnMouseMove(sender, e);

            // ドラッグ解除
            if (CreateMouseButtonBits(e) == MouseButtonBits.None)
            {
                ResetState();
            }
        }

        public override void OnMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            // コマンド実行
            MouseWheelChanged?.Invoke(sender, e);

            // ドラッグ解除
            if (e.Handled)
            {
                ResetState();
            }
        }

        public override void OnMouseHorizontalWheel(object? sender, MouseWheelEventArgs e)
        {
            // コマンド実行
            MouseHorizontalWheelChanged?.Invoke(sender, e);

            // ドラッグ解除
            if (e.Handled)
            {
                ResetState();
            }
        }

        public override void OnMouseMove(object? sender, MouseEventArgs e)
        {
            _dragTransformControl.UpdateState(CreateMouseButtonBits(e), Keyboard.Modifiers, ToDragCoord(e.GetPosition(_context.Sender)), e.Timestamp, DragActionUpdateOptions.None);
        }
    }
}

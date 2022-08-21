﻿using NeeLaboratory;
using NeeView.Windows;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

// TODO: 関数が大きすぎる？細分化を検討

namespace NeeView
{
    public static class PointExtensions
    {
        // 開発用：座標表示
        public static string ToIntString(this Point point)
        {
            return $"({(int)point.X},{(int)point.Y})";
        }
    }

    // 回転、拡大操作の中心
    public enum DragControlCenter
    {
        [AliasName]
        View,

        [AliasName]
        Target,

        [AliasName]
        Cursor,
    }

    // 値変化の原因
    public enum TransformActionType
    {
        None,
        Reset,
        Angle,
        Scale,
        FlipHorizontal,
        FlipVertical,
        LoupeScale,
        Touch,
        Navigate,
    }

    // 変化通知イベントの引数
    public class TransformEventArgs : EventArgs
    {
        public TransformEventArgs(TransformActionType actionType)
        {
            this.ActionType = actionType;
        }

        /// <summary>
        /// 変化したもの
        /// </summary>
        public TransformActionType ActionType { get; set; }
    }


    /// <summary>
    /// ドラッグ操作
    /// </summary>
    public class DragTransformControl
    {
        #region Fields

        /// <summary>
        /// 操作領域
        /// </summary>
        private FrameworkElement _sender;

        /// <summary>
        /// 操作エレメント
        /// </summary>
        private FrameworkElement _target;

        /// <summary>
        /// 操作エレメント変換パラメータ
        /// </summary>
        private DragTransform _transform;


        private Point _defaultPosition;

        private double _defaultAngle;

        private double _defaultScale;

        // X方向の移動制限フラグ
        private bool _lockMoveX;

        // Y方向の移動制限フラグ
        private bool _lockMoveY;

        private AreaSelectAdorner? _adorner;

        #endregion

        #region Constructors

        public DragTransformControl(DragTransform transform, FrameworkElement sender, FrameworkElement target)
        {
            _sender = sender;
            _target = target;
            _transform = transform;

            _sender.SizeChanged += Sender_SizeChanged;
            UpdateCoordCenter();

            _sender.Loaded += (s, e) =>
            {
                _adorner = _adorner ?? new AreaSelectAdorner(_sender);
            };
        }

        #endregion

        #region Properties

        public FrameworkElement SenderElement => _sender;
        public FrameworkElement TargetElement => _target;

        // 開始時の基準
        public DragViewOrigin ViewOrigin { get; set; }

        // 水平スクロールの正方向
        public double ViewHorizontalDirection { get; set; } = 1.0;

        #endregion

        #region StateMachine

        private bool _isMouseButtonDown;

        public void ResetState()
        {
            _isMouseButtonDown = false;
            _action = null;
            _adorner?.Detach();
        }

        /// <summary>
        /// Change State
        /// </summary>
        /// <param name="buttons">マウスボタンの状態</param>
        /// <param name="keys">装飾キーの状態</param>
        /// <param name="pos">マウス座標(_context.Sender)</param>
        public void UpdateState(MouseButtonBits buttons, ModifierKeys keys, Point point)
        {
            if (_isMouseButtonDown)
            {
                StateDrag(buttons, keys, point);
            }
            else
            {
                StateIdle(buttons, keys, point);
            }
        }

        private void StateIdle(MouseButtonBits buttons, ModifierKeys keys, Point point)
        {
            if (buttons != MouseButtonBits.None)
            {
                InitializeDragParameter(point);
                _isMouseButtonDown = true;

                StateDrag(buttons, keys, point);
            }
        }

        private void StateDrag(MouseButtonBits buttons, ModifierKeys keys, Point point)
        {
            if (buttons == MouseButtonBits.None)
            {
                // excep end action
                _endPoint = GetCenterCoordVector(point);
                _action?.ExecuteEnd(this, new DragTransformActionArgs(_startPoint, _endPoint));

                _isMouseButtonDown = false;
                return;
            }

            var dragKey = new DragKey(buttons, keys);
            if (!dragKey.IsValid) return;

            // update action
            if (_action == null || _action.DragKey != dragKey)
            {
                var action = DragActionTable.Current.GetAction(dragKey);

                if (action != _action && action?.IsDummy == false)
                {
                    _adorner?.Detach();
                    _action = action;
                    InitializeDragParameter(point);
                }
            }

            // exec action
            _endPoint = GetCenterCoordVector(point);
            _action?.Execute(this, new DragTransformActionArgs(_startPoint, _endPoint));
        }

        #endregion

        #region Methods

        private void Sender_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateCoordCenter();
        }

        private void UpdateCoordCenter()
        {
            _coordCenter = new Vector(_sender.ActualWidth * 0.5, _sender.ActualHeight * 0.5);
            ////Debug.WriteLine($"CoordCenter: { ((Point)_coordCenter).ToIntString()}");
        }

        // ドラッグでビュー操作設定の更新
        public void SetMouseDragSetting(int direction, DragViewOrigin origin, PageReadOrder order)
        {
            if (origin == DragViewOrigin.None)
            {
                origin = Config.Current.View.IsViewStartPositionCenter
                    ? DragViewOrigin.Center
                    : order == PageReadOrder.LeftToRight
                        ? DragViewOrigin.LeftTop
                        : DragViewOrigin.RightTop;

                ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop) ? 1.0 : -1.0;
            }
            else
            {
                ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop || origin == DragViewOrigin.LeftBottom) ? 1.0 : -1.0;
            }
        }

        // 初期化
        // コンテンツ切り替わり時等
        public void Reset(bool isResetScale, bool isResetAngle, bool isResetFlip, double angle, bool ignoreViewOrigin)
        {
            _lockMoveX = Config.Current.View.IsLimitMove;
            _lockMoveY = Config.Current.View.IsLimitMove;

            if (isResetAngle)
            {
                _transform.SetAngle(angle, TransformActionType.Reset);
            }
            if (isResetScale)
            {
                _transform.SetScale(1.0, TransformActionType.Reset);
            }
            if (isResetFlip)
            {
                _transform.IsFlipHorizontal = false;
                _transform.IsFlipVertical = false;
            }

            if (ignoreViewOrigin || ViewOrigin == DragViewOrigin.Center)
            {
                _transform.SetPosition(new Point(0, 0));
            }
            else
            {
                // レイアウト更新
                _transform.SetPosition(new Point(0, 0));

                _sender.UpdateLayout();
                var area = GetArea();
                var pos = new Point(0, 0);
                var move = new Vector(0, 0);
                if (area.Target.Height > area.View.Height)
                {
                    var verticalDirection = (ViewOrigin == DragViewOrigin.LeftTop || ViewOrigin == DragViewOrigin.RightTop) ? 1.0 : -1.0;
                    move.Y = (area.Target.Height - area.View.Height + 1) * 0.5 * verticalDirection;
                }
                if (area.Target.Width > area.View.Width)
                {
                    var horizontalDirection = (ViewOrigin == DragViewOrigin.LeftTop || ViewOrigin == DragViewOrigin.LeftBottom) ? 1.0 : -1.0;
                    move.X = (area.Target.Width - area.View.Width + 1) * 0.5 * horizontalDirection;
                }

                if (move.X != 0 || move.Y != 0)
                {
                    var limitedPos = pos + GetLimitMove(area, move);
                    _transform.SetPosition(limitedPos);
                }
            }

            _defaultPosition = _transform.Position;
            _defaultScale = _transform.Scale;
            _defaultAngle = _transform.Angle;
        }

        // 移動量限界計算
        private Vector GetLimitMove(DragArea area, Vector move)
        {
            var margin = new Point(
                area.Target.Width < area.View.Width ? 0 : area.Target.Width - area.View.Width,
                area.Target.Height < area.View.Height ? 0 : area.Target.Height - area.View.Height);

            if (move.X < 0 && area.Target.Left + move.X < -margin.X)
            {
                move.X = -margin.X - area.Target.Left;
                if (move.X > 0) move.X = 0;
            }
            else if (move.X > 0 && area.Target.Right + move.X > area.View.Width + margin.X)
            {
                move.X = area.View.Width + margin.X - area.Target.Right;
                if (move.X < 0) move.X = 0;
            }
            if (move.Y < 0 && area.Target.Top + move.Y < -margin.Y)
            {
                move.Y = -margin.Y - area.Target.Top;
                if (move.Y > 0) move.Y = 0;
            }
            else if (move.Y > 0 && area.Target.Bottom + move.Y > area.View.Height + margin.Y)
            {
                move.Y = area.View.Height + margin.Y - area.Target.Bottom;
                if (move.Y < 0) move.Y = 0;
            }

            return move;
        }

        /// <summary>
        /// 表示コンテンツのエリア情報取得
        /// </summary>
        /// <returns></returns>
        private DragArea GetArea()
        {
            return new DragArea(_sender, _target);
        }

        // ビューエリアサイズ変更に追従する
        public void SnapView()
        {
            if (!Config.Current.View.IsLimitMove) return;

            // レイアウト更新
            _sender.UpdateLayout();

            var area = GetArea();
            _transform.SetPosition(area.SnapView(_transform.Position));
        }

        #endregion

        #region LookAt method

        /// <summary>
        /// ターゲット座標で移動 (ナビゲーター用)
        /// </summary>
        /// <param name="point">ターゲット座標</param>
        public void LookAt(Point point)
        {
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_transform.ScaleX, _transform.ScaleY));
            transformGroup.Children.Add(new RotateTransform(_transform.Angle));

            var pos = transformGroup.Transform(point);
            _transform.SetPosition(pos);
        }

        #endregion LookAt method

        #region Hover scroll method

        /// <summary>
        /// Hover scroll
        /// </summary>
        /// <param name="point">point in sender</param>
        public void HoverScroll(Point point)
        {
            HoverScroll(point, TimeSpan.FromSeconds(Config.Current.Mouse.HoverScrollDuration));
        }

        /// <summary>
        /// Hover scroll
        /// </summary>
        /// <param name="point">point in sender</param>
        /// <param name="span">scroll time</param>
        public void HoverScroll(Point point, TimeSpan span)
        {
            var rateX = (0.5 - point.X / _sender.ActualWidth) * 2.0;
            var rateY = (0.5 - point.Y / _sender.ActualHeight) * 2.0;
            HoverScroll(rateX, rateY, span);
        }

        /// <summary>
        /// Hover scroll
        /// </summary>
        /// <param name="rateX">point.X rate in sender [-1.0, 1.0]</param>
        /// <param name="rateY">point.Y rate in sender [-1.0, 1.0]</param>
        /// <param name="span">scroll time</param>
        public void HoverScroll(double rateX, double rateY, TimeSpan span)
        {
            var targetRect = new Rect(0.0, 0.0, _target.ActualWidth, _target.ActualHeight);
            var targetViewRect = _transform.TransformCalc.TransformBounds(targetRect);

            var x = Math.Max(targetViewRect.Width - _sender.ActualWidth, 0.0) * MathUtility.Clamp(rateX, -0.5, 0.5);
            var y = Math.Max(targetViewRect.Height - _sender.ActualHeight, 0.0) * MathUtility.Clamp(rateY, -0.5, 0.5);
            var pos = new Point(x, y);

            var easing = span == TimeSpan.Zero ? TranslateTransformEasing.Direct : TranslateTransformEasing.Animation;
            _transform.SetPosition(pos, easing, span);
        }

        #endregion Hover scroll method

        #region Scroll method

        // スクロール↑コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollUp(ViewScrollCommandParameter parameter)
        {
            var rate = parameter.Scroll;
            var span = TimeSpan.FromSeconds(parameter.ScrollDuration);

            UpdateLock();
            if (!_lockMoveY)
            {
                DoMove(new Vector(0, _sender.ActualHeight * rate), span);
            }
            else if (parameter.AllowCrossScroll)
            {
                DoMove(new Vector(_sender.ActualWidth * rate * ViewHorizontalDirection, 0), span);
            }
        }

        // スクロール↓コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollDown(ViewScrollCommandParameter parameter)
        {
            var rate = parameter.Scroll;
            var span = TimeSpan.FromSeconds(parameter.ScrollDuration);

            UpdateLock();
            if (!_lockMoveY)
            {
                DoMove(new Vector(0, _sender.ActualHeight * -rate), span);
            }
            else if (parameter.AllowCrossScroll)
            {
                DoMove(new Vector(_sender.ActualWidth * -rate * ViewHorizontalDirection, 0), span);
            }
        }

        // スクロール←コマンド
        // 横方向にスクロールできない場合、縦方向にスクロールする
        public void ScrollLeft(ViewScrollCommandParameter parameter)
        {
            var rate = parameter.Scroll;
            var span = TimeSpan.FromSeconds(parameter.ScrollDuration);

            UpdateLock();
            if (!_lockMoveX)
            {
                DoMove(new Vector(_sender.ActualWidth * rate, 0), span);
            }
            else if (parameter.AllowCrossScroll)
            {
                DoMove(new Vector(0, _sender.ActualHeight * rate * ViewHorizontalDirection), span);
            }
        }

        // スクロール→コマンド
        // 横方向にスクロールできない場合、縦方向にスクロールする
        public void ScrollRight(ViewScrollCommandParameter parameter)
        {
            var rate = parameter.Scroll;
            var span = TimeSpan.FromSeconds(parameter.ScrollDuration);

            UpdateLock();
            if (!_lockMoveX)
            {
                DoMove(new Vector(_sender.ActualWidth * -rate, 0), span);
            }
            else if (parameter.AllowCrossScroll)
            {
                DoMove(new Vector(0, _sender.ActualHeight * -rate * ViewHorizontalDirection), span);
            }
        }

        #endregion Scroll method

        #region NScroll method

        private const double _nscrollCountThreshold = 0.9;
        private RepeatLimiter _repeatLimiter = new RepeatLimiter();

        /// <summary>
        /// N字スクロール
        /// </summary>
        /// <param name="direction">次方向:+1 / 前方向:-1</param>
        /// <param name="bookReadDirection">右開き:+1 / 左開き:-1</param>
        /// <param name="parameter">N字スクロールコマンドパラメータ</param>
        /// <returns>スクロールしたか</returns>
        public bool ScrollN(int direction, int bookReadDirection, bool isLieBreakStop, IScrollNTypeParameter parameter)
        {
            var endMargin = (parameter is IScrollNTypeEndMargin e) ? e.EndMargin : 0.0;
            var lineBreakStopTime = isLieBreakStop ? parameter.LineBreakStopTime : 0.0;
            return ScrollN(direction, bookReadDirection, parameter.ScrollType, parameter.Scroll, parameter.ScrollDuration, lineBreakStopTime, endMargin);
        }

        /// <summary>
        /// N字スクロール
        /// </summary>
        /// <param name="direction">次方向:+1 / 前方向:-1</param>
        /// <param name="bookReadDirection">右開き:+1 / 左開き:-1</param>
        /// <param name="scrollType">スクロールのタイプ</param>
        /// <param name="minScroll">最小移動距離</param>
        /// <param name="rate">移動距離の割合</param>
        /// <param name="sec">移動時間。アニメーション時間</param>
        /// <param name="endMargin">終端判定マージン</param>
        /// <returns>スクロールしたか</returns>
        private bool ScrollN(int direction, int bookReadDirection, NScrollType scrollType, double rate, double sec, double lineBreakStopTime, double endMargin)
        {
            var isLimit = _repeatLimiter.IsLimit((int)(lineBreakStopTime * 1000.0));
            _repeatLimiter.Reset();

            var delta = GetNScrollDelta(direction, bookReadDirection, scrollType, rate, endMargin);
            if (delta.IsZero())
            {
                return false;
            }

            // line braek stop
            if (isLimit && IsNScrollLineBreak(scrollType, delta))
            {
                return true;
            }

            DoMove(delta, TimeSpan.FromSeconds(sec));
            return true;
        }

        // N字スクロール：移動量から改行であるかを判定
        private bool IsNScrollLineBreak(NScrollType scrollType, Vector v)
        {
            return scrollType != NScrollType.Diagonal && v.X != 0.0 && v.Y != 0.0;
        }

        // N字スクロール：スクロール距離を計算
        private Vector GetNScrollDelta(int direction, int bookReadDirection, NScrollType scrollType, double rate, double endMergin)
        {
            var area = GetArea();

            switch (scrollType)
            {
                case NScrollType.NType:
                    return GetNTypeScrollDelta(area, direction, bookReadDirection, rate, endMergin);

                case NScrollType.ZType:
                    return GetZTypeScrollDelta(area, direction, bookReadDirection, rate, endMergin);

                case NScrollType.Diagonal:
                default:
                    return GetDiagonalScrollDelta(area, direction, bookReadDirection, rate, endMergin);
            }
        }

        private Vector GetNTypeScrollDelta(DragArea area, int direction, int bookReadDirection, double rate, double endMergin)
        {
            var delta = SnapZero(GetNScrollVertical(area, direction, bookReadDirection, rate), endMergin);
            UpdateLock();
            if (_lockMoveY || delta.Y == 0.0)
            {
                delta = SnapZero(GetNScrollNewLineVertical(area, direction, bookReadDirection, rate), endMergin);
            }
            return delta;
        }

        private Vector GetZTypeScrollDelta(DragArea area, int direction, int bookReadDirection, double rate, double endMergin)
        {
            var delta = SnapZero(GetNScrollHorizontal(area, direction, bookReadDirection, rate), endMergin);
            UpdateLock();
            if (_lockMoveX || delta.X == 0.0)
            {
                delta = SnapZero(GetNScrollNewLineHorizontal(area, direction, bookReadDirection, rate), endMergin);
            }
            return delta;
        }

        private Vector GetDiagonalScrollDelta(DragArea area, int direction, int bookReadDirection, double rate, double endMergin)
        {
            var deltaX = GetNScrollHorizontal(area, direction, bookReadDirection, rate);
            var deltaY = GetNScrollVertical(area, direction, bookReadDirection, rate);
            return SnapZero(new Vector(deltaX.X, deltaY.Y), endMergin);
        }

        private Vector GetNScrollHorizontal(DragArea area, int direction, int bookReadDirection, double rate)
        {
            var delta = new Vector();

            if (direction * bookReadDirection > 0)
            {
                delta.X = SnapZero(GetNScrollHorizontalToLeft(area, rate));
            }
            else
            {
                delta.X = SnapZero(GetNScrollHorizontalToRight(area, rate));
            }

            return delta;
        }

        private Vector GetNScrollVertical(DragArea area, int direction, int bookReadDirection, double rate)
        {
            var delta = new Vector();

            if (direction > 0)
            {
                delta.Y = SnapZero(GetNScrollVerticalToBottom(area, rate));
            }
            else
            {
                delta.Y = SnapZero(GetNScrollVerticalToTop(area, rate));
            }

            return delta;
        }

        // N字スクロール改行：水平方向
        private Vector GetNScrollNewLineHorizontal(DragArea area, int direction, int bookReadDirection, double rate)
        {
            var delta = new Vector();

            var canHorizontalScroll = area.Over.Width > 0.0;
            var rateY = canHorizontalScroll ? 1.0 : rate;
            if (direction > 0)
            {
                delta.Y = SnapZero(GetNScrollVerticalToBottom(area, rateY));
            }
            else
            {
                delta.Y = SnapZero(GetNScrollVerticalToTop(area, rateY));
            }

            if (delta.Y != 0.0)
            {
                if (direction * bookReadDirection > 0)
                {
                    delta.X = SnapZero(GetNScrollHorizontalMoveToRight(area));
                }
                else
                {
                    delta.X = SnapZero(GetNScrollHorizontalMoveToLeft(area));
                }
            }

            return delta;
        }


        // N字スクロール改行：垂直方向
        private Vector GetNScrollNewLineVertical(DragArea area, int direction, int bookReadDirection, double rate)
        {
            var delta = new Vector();

            var canVerticalScroll = area.Over.Height > 0.0;
            var rateX = canVerticalScroll ? 1.0 : rate;
            if (direction * bookReadDirection > 0)
            {
                delta.X = SnapZero(GetNScrollHorizontalToLeft(area, rateX));
            }
            else
            {
                delta.X = SnapZero(GetNScrollHorizontalToRight(area, rateX));
            }

            if (delta.X != 0.0)
            {
                if (direction > 0)
                {
                    delta.Y = SnapZero(GetNScrollVerticalMoveToTop(area));
                }
                else
                {
                    delta.Y = SnapZero(GetNScrollVerticalMoveToBottom(area));
                }
            }

            return delta;
        }

        /// <summary>
        /// Snap zero.
        /// if abs(x) < 1.0, then 0.0
        /// </summary>
        private double SnapZero(double value, double margin = 1.0)
        {
            return (-margin < value && value < margin) ? 0.0 : value;
        }

        private Vector SnapZero(Vector v, double mergin)
        {
            if (v.IsZero())
            {
                return v;
            }

            if (v.LengthSquared < mergin * mergin)
            {
                return new Vector();
            }

            return v;
        }


        // N字スクロール：上方向スクロール距離取得
        private double GetNScrollVerticalToTop(DragArea area, double rate)
        {
            if (area.Over.Top < 0.0)
            {
                double dy = Math.Abs(area.Over.Top);
                var n = (int)(dy / (area.View.Height * rate) + _nscrollCountThreshold);
                if (n > 1)
                {
                    dy = Math.Min(dy / n, dy);
                }
                return dy;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：下方向スクロール距離取得
        private double GetNScrollVerticalToBottom(DragArea area, double rate)
        {
            if (area.Over.Bottom > 0.0)
            {
                double dy = Math.Abs(area.Over.Bottom);
                var n = (int)(dy / (area.View.Height * rate) + _nscrollCountThreshold);
                if (n > 1)
                {
                    dy = Math.Min(dy / n, dy);
                }
                return -dy;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：上端までの移動距離取得
        private double GetNScrollVerticalMoveToTop(DragArea area)
        {
            return Math.Abs(area.Over.Top);
        }

        // N字スクロール：下端までの移動距離取得
        private double GetNScrollVerticalMoveToBottom(DragArea area)
        {
            return -Math.Abs(area.Over.Bottom);
        }

        // N字スクロール：左端までの移動距離取得
        private double GetNScrollHorizontalMoveToLeft(DragArea area)
        {
            return Math.Abs(area.Over.Left);
        }

        // N字スクロール：右端までの移動距離取得
        private double GetNScrollHorizontalMoveToRight(DragArea area)
        {
            return -Math.Abs(area.Over.Right);
        }

        // N字スクロール：左方向スクロール距離取得
        private double GetNScrollHorizontalToLeft(DragArea area, double rate)
        {
            if (area.Over.Left < 0.0)
            {
                double dx = Math.Abs(area.Over.Left);
                var n = (int)(dx / (area.View.Width * rate) + _nscrollCountThreshold);
                if (n > 1)
                {
                    dx = Math.Min(dx / n, dx);
                }
                return dx;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：右方向スクロール距離取得
        private double GetNScrollHorizontalToRight(DragArea area, double rate)
        {
            if (area.Over.Right > 0.0)
            {
                double dx = Math.Abs(area.Over.Right);
                var n = (int)(dx / (area.View.Width * rate) + _nscrollCountThreshold);
                if (n > 1)
                {
                    dx = Math.Min(dx / n, dx);
                }
                return -dx;
            }
            else
            {
                return 0.0;
            }
        }

        #endregion NScroll method

        #region Scale method
        // 拡大コマンド
        public void ScaleUp(double scaleDelta, bool isSnap, double originalScale)
        {
            InitializeDragParameter(Mouse.GetPosition(_sender));

            var scale = _baseScale * (1.0 + scaleDelta);

            if (isSnap)
            {
                if (Config.Current.Notice.IsOriginalScaleShowMessage && originalScale > 0.0)
                {
                    // original scale 100% snap
                    if (_baseScale * originalScale < 0.99 && scale * originalScale > 0.99)
                    {
                        scale = 1.0 / originalScale;
                    }
                }
                else
                {
                    // visual scale 100% snap
                    if (_baseScale < 0.99 && scale > 0.99)
                    {
                        scale = 1.0;
                    }
                }
            }

            DoScale(scale);
        }

        // 縮小コマンド
        public void ScaleDown(double scaleDelta, bool isSnap, double originalScale)
        {
            InitializeDragParameter(Mouse.GetPosition(_sender));

            var scale = _baseScale / (1.0 + scaleDelta);

            if (isSnap)
            {
                if (Config.Current.Notice.IsOriginalScaleShowMessage && originalScale > 0.0)
                {
                    // original scale 100% snap
                    if (_baseScale * originalScale > 1.01 && scale * originalScale < 1.01)
                    {
                        scale = 1.0 / originalScale;
                    }
                }
                else
                {
                    // visual scale 100% snap
                    if (_baseScale > 1.01 && scale < 1.01)
                    {
                        scale = 1.0;
                    }
                }
            }

            DoScale(scale);
        }
        #endregion

        #region Rotate method
        // 回転コマンド
        public void Rotate(double angle)
        {
            // スナップ値を下限にする
            if (Math.Abs(angle) < Config.Current.View.AngleFrequency)
            {
                angle = Config.Current.View.AngleFrequency * Math.Sign(angle);
            }

            InitializeDragParameter(Mouse.GetPosition(_sender));
            DoRotate(NormalizeLoopRange(_baseAngle + angle, -180, 180));
        }
        #endregion

        #region Flip method
        // 反転コマンド
        public void ToggleFlipHorizontal()
        {
            InitializeDragParameter(Mouse.GetPosition(_sender));
            DoFlipHorizontal(!_transform.IsFlipHorizontal);
        }

        // 反転コマンド
        public void FlipHorizontal(bool isFlip)
        {
            InitializeDragParameter(Mouse.GetPosition(_sender));
            DoFlipHorizontal(isFlip);
        }

        // 反転コマンド
        public void ToggleFlipVertical()
        {
            InitializeDragParameter(Mouse.GetPosition(_sender));
            DoFlipVertical(!_transform.IsFlipVertical);
        }

        // 反転コマンド
        public void FlipVertical(bool isFlip)
        {
            InitializeDragParameter(Mouse.GetPosition(_sender));
            DoFlipVertical(isFlip);
        }
        #endregion

        #region Actions

        // Sender座標系でのCenter座標系の基準位置
        private Vector _coordCenter;

        private Point _startPoint;
        private Point _endPoint;
        private Point _rotateCenter;
        private Point _scaleCenter;
        private Point _flipCenter;

        // ドラッグアクション
        private DragAction? _action;

        /// <summary>
        /// Senderの座標をCenter座標系に変換する
        /// </summary>
        private Point GetCenterCoordVector(Point pointInSender)
        {
            return pointInSender - _coordCenter;
        }

        // ドラッグパラメータ初期化
        private void InitializeDragParameter(Point pos)
        {
            _startPoint = GetCenterCoordVector(pos);

            InitializeWindowDragPosition();

            _rotateCenter = GetCenterPosition(Config.Current.View.RotateCenter);
            _scaleCenter = GetCenterPosition(Config.Current.View.ScaleCenter);
            _flipCenter = GetCenterPosition(Config.Current.View.FlipCenter);

            _basePosition = _transform.Position;
            _baseAngle = _transform.Angle;
            _baseScale = _transform.Scale;

            ////Debug.WriteLine($"cur:{PointToString(_startPoint)}, pos:{PointToString(_basePosition)}, scale:{_baseScale}, center:{PointToString(_rotateCenter)}");
        }

        private void InitializeWindowDragPosition()
        {
            var window = Window.GetWindow(_sender);
            if (window is null) return;

            var windowDiff = PointToLogicalScreen(window, new Point(0, 0)) - new Point(window.Left, window.Top);
            _startPointFromWindow = _sender.TranslatePoint(_startPoint, window) + windowDiff;
        }

        private Point GetCenterPosition(DragControlCenter dragControlCenter)
        {
            switch (dragControlCenter)
            {
                case DragControlCenter.View:
                    return new Point(0, 0);
                case DragControlCenter.Target:
                    return _transform.Position;
                case DragControlCenter.Cursor:
                    return _startPoint;
                default:
                    throw new NotImplementedException();
            }
        }

        // 移動制限更新
        // ビューエリアサイズを超える場合、制限をOFFにする
        private void UpdateLock()
        {
            var area = GetArea();

            double margin = 1.1;

            if (_lockMoveX)
            {
                if (area.Over.Left + margin < 0 || area.Over.Right - margin > 0)
                {
                    _lockMoveX = false;
                }
            }
            if (_lockMoveY)
            {
                if (area.Over.Top + margin < 0 || area.Over.Bottom - margin > 0)
                {
                    _lockMoveY = false;
                }
            }
        }

        // 移動制限解除
        private void UnlockMove()
        {
            _lockMoveX = false;
            _lockMoveY = false;
        }

        #endregion

        #region Drag Move

        private Point _basePosition;

        // 移動
        public void DragMove(Point start, Point end)
        {
            DragMoveEx(start, end, 1.0, TimeSpan.Zero);
        }

        // 移動(速度スケール依存)
        public void DragMoveScale(Point start, Point end, double sencitivity)
        {
            var area = GetArea();
            var scaleX = area.Target.Width / area.View.Width;
            var scaleY = area.Target.Height / area.View.Height;
            var scale = (scaleX > scaleY ? scaleX : scaleY) * sencitivity;
            scale = scale < 1.0 ? 1.0 : scale;

            DragMoveEx(start, end, scale, TimeSpan.Zero);
        }

        private void DragMoveEx(Point start, Point end, double scale, TimeSpan span)
        {
            var pos0 = _transform.Position;
            var pos1 = (end - start) * scale + _basePosition;
            var move = pos1 - pos0;

            DoMove(move, span);
        }


        /// <summary>
        /// 移動実行
        /// </summary>
        /// <param name="move">移動量</param>
        /// <param name="span">移動時間</param>
        /// <param name="endMargin">移動終端判定マージン</param>
        /// <returns>実際の移動量</returns>
        private Vector DoMove(Vector move, TimeSpan span)
        {
            var area = GetArea();
            var pos0 = _transform.Position;

            UpdateLock();

            if (_lockMoveX)
            {
                move.X = 0;
            }
            if (_lockMoveY)
            {
                move.Y = 0;
            }

            if (Config.Current.View.IsLimitMove)
            {
                var moveExpectation = move;
                move = GetLimitMove(area, move);
                _basePosition += move - moveExpectation;
            }

            _transform.SetPosition(pos0 + move, TranslateTransformEasing.Animation, span);

            return move;
        }

        #endregion

        #region Drag Angle

        private double _baseAngle;

        // 回転
        public void DragAngle(Point start, Point end)
        {
            var v0 = start - _rotateCenter;
            var v1 = end - _rotateCenter;

            // 回転の基準となるベクトルが得られるまで処理を進めない
            const double minLength = 10.0;
            if (v0.Length < minLength)
            {
                _startPoint = end;
                return;
            }

            double angle = NormalizeLoopRange(_baseAngle + Vector.AngleBetween(v0, v1), -180, 180);

            DoRotate(angle);
        }

        public void DragAngleSlider(Point start, Point end, double sensitivity)
        {
            var angle = NormalizeLoopRange(_baseAngle + (start.X - end.X) * 0.5 * sensitivity, -180, 180);
            DoRotate(angle);
        }

        // 回転実行
        private void DoRotate(double angle)
        {
            if (Config.Current.View.AngleFrequency > 0)
            {
                angle = Math.Floor((angle + Config.Current.View.AngleFrequency * 0.5) / Config.Current.View.AngleFrequency) * Config.Current.View.AngleFrequency;
            }

            _transform.SetAngle(angle, TransformActionType.Angle);

            RotateTransform m = new RotateTransform(_transform.Angle - _baseAngle);
            _transform.SetPosition(_rotateCenter + (Vector)m.Transform((Point)(_basePosition - _rotateCenter)));

            UnlockMove();
        }

        // 角度の正規化
        public static double NormalizeLoopRange(double val, double min, double max)
        {
            if (min >= max) throw new ArgumentException("need min < max");

            if (val >= max)
            {
                return min + (val - min) % (max - min);
            }
            else if (val < min)
            {
                return max - (min - val) % (max - min);
            }
            else
            {
                return val;
            }
        }

        #endregion

        #region Drag Scale

        private double _baseScale;

        // 拡縮スナップ。0で無効;
        public double SnapScale { get; set; } = 0;

        // 拡縮
        public void DragScale(Point start, Point end)
        {
            var v0 = start - _scaleCenter;
            var v1 = end - _scaleCenter;

            // 拡縮の基準となるベクトルが得られるまで処理を進めない
            const double minLength = 32.0;
            if (v0.Length < minLength)
            {
                _startPoint = end;
                return;
            }

            var scale1 = v1.Length / v0.Length * _baseScale;
            DoScale(scale1);
        }

        // 拡縮 (スライダー)
        public void DragScaleSlider(Point start, Point end, double sensitivity)
        {
            var scale1 = System.Math.Pow(2, (end.X - start.X) * 0.01 * sensitivity) * _baseScale;
            DoScale(scale1);
        }

        // 拡縮 (スライダー、中央寄せ)
        public void DragScaleSliderCentered(Point start, Point end, double sensitivity)
        {
            var scale1 = System.Math.Pow(2, (end.X - start.X) * 0.01 * sensitivity) * _baseScale;
            DoScale(scale1, false);

            var len0 = Math.Abs(end.X - start.X);
            var len1 = 200.0;
            var rate = Math.Min(len0 / len1, 1.0);

            var p0 = _scaleCenter;
            var p1 = _basePosition + (p0 - _basePosition) * (_transform.Scale / _baseScale);
            var pa = (Vector)p0 * (1.0 - rate);

            var pos = (Point)(pa + (_basePosition - p1));
            _transform.SetPosition(pos);

            UnlockMove();
        }

        // 拡縮実行
        private void DoScale(double scale, bool withTransform = true)
        {
            if (SnapScale > 0)
            {
                scale = Math.Floor((scale + SnapScale * 0.5) / SnapScale) * SnapScale;
            }

            _transform.SetScale(scale, TransformActionType.Scale);

            if (withTransform)
            {
                var pos0 = _scaleCenter;
                var rate = _transform.Scale / _baseScale;
                _transform.SetPosition(pos0 + (_basePosition - pos0) * rate);

                UnlockMove();
            }
        }

        #endregion

        #region MarqueeZoom

        public void DragMarqueeZoom(Point start, Point end)
        {
            if (_adorner is null) return;

            _adorner.Start = start + _coordCenter;
            _adorner.End = end + _coordCenter;
            _adorner.Attach();
        }

        public void DragMarqueeZoomEnd(Point start, Point end)
        {
            _adorner?.Detach();

            var zoomRect = new Rect(start, end);
            if (zoomRect.Width < 0 || zoomRect.Height < 0) return;

            var area = GetArea();
            var zoomX = area.View.Width / zoomRect.Width;
            var zoomY = area.View.Height / zoomRect.Height;
            var zoom = zoomX < zoomY ? zoomX : zoomY;
            _transform.SetScale(_transform.Scale * zoom, TransformActionType.Scale);

            var zoomCenter = new Point(zoomRect.X + zoomRect.Width * 0.5, zoomRect.Y + zoomRect.Height * 0.5);
            _transform.SetPosition((Point)((_transform.Position - zoomCenter) * zoom));
        }


        #endregion

        #region Drag Flip

        // 左右反転
        public void DragFlipHorizontal(Point start, Point end)
        {
            const double margin = 16;

            if (start.X + margin < end.X)
            {
                DoFlipHorizontal(true);
                start.X = end.X - margin;
            }
            else if (start.X - margin > end.X)
            {
                DoFlipHorizontal(false);
                start.X = end.X + margin;
            }
        }

        // 反転実行
        private void DoFlipHorizontal(bool isFlip)
        {
            if (_transform.IsFlipHorizontal != isFlip)
            {
                _transform.IsFlipHorizontal = isFlip;

                // 角度を反転
                var angle = -NormalizeLoopRange(_transform.Angle, -180, 180);

                _transform.SetAngle(angle, TransformActionType.FlipHorizontal);

                // 座標を反転
                _transform.SetPosition(new Point(_flipCenter.X * 2.0 - _transform.Position.X, _transform.Position.Y));

                UnlockMove();
            }
        }


        // 上下反転
        public void DragFlipVertical(Point start, Point end)
        {
            const double margin = 16;

            if (start.Y + margin < end.Y)
            {
                DoFlipVertical(true);
                start.Y = end.Y - margin;
            }
            else if (start.Y - margin > end.Y)
            {
                DoFlipVertical(false);
                start.Y = end.Y + margin;
            }
        }

        // 反転実行
        private void DoFlipVertical(bool isFlip)
        {
            if (_transform.IsFlipVertical != isFlip)
            {
                _transform.IsFlipVertical = isFlip;

                // 角度を反転
                var angle = 90 - NormalizeLoopRange(_transform.Angle + 90, -180, 180);
                _transform.SetAngle(angle, TransformActionType.FlipVertical);

                // 座標を反転
                _transform.SetPosition(new Point(_transform.Position.X, _flipCenter.Y * 2.0 - _transform.Position.Y));
            }
        }

        #endregion

        #region Drag Window

        private Point _startPointFromWindow;

        // ウィンドウ移動
        public void DragWindowMove(Point start, Point end)
        {
            var window = Window.GetWindow(_sender);
            if (window is null) return;

            if (window.WindowState == WindowState.Normal)
            {
                var pos = PointToLogicalScreen(_sender, end) - _startPointFromWindow;
                window.Left = pos.X;
                window.Top = pos.Y;
            }
        }

        /// <summary>
        /// 座標を論理座標でスクリーン座標に変換
        /// </summary>
        private Point PointToLogicalScreen(Visual visual, Point point)
        {
            var pos = visual.PointToScreen(point); // デバイス座標

            if (Window.GetWindow(visual) is IDpiScaleProvider dpiProvider)
            {
                var dpi = dpiProvider.GetDpiScale();
                pos.X = pos.X / dpi.DpiScaleX;
                pos.Y = pos.Y / dpi.DpiScaleY;
            }
            return pos;
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember]
            public bool IsOriginalScaleShowMessage { get; set; }
            [DataMember]
            public bool IsKeepScale { get; set; }
            [DataMember]
            public bool IsKeepAngle { get; set; }
            [DataMember]
            public bool IsKeepFlip { get; set; }
            [DataMember]
            public bool IsViewStartPositionCenter { get; set; }
            [DataMember]
            public DragControlCenter DragControlRotateCenter { get; set; }
            [DataMember]
            public DragControlCenter DragControlScaleCenter { get; set; }
            [DataMember]
            public DragControlCenter DragControlFlipCenter { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsControlCenterImage { get; set; }


            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                // before 34.0
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    var center = IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
                    DragControlRotateCenter = center;
                    DragControlScaleCenter = center;
                    DragControlFlipCenter = center;
                }
#pragma warning restore CS0612
            }

            public void RestoreConfig(Config config)
            {
                config.Notice.IsOriginalScaleShowMessage = IsOriginalScaleShowMessage;

                config.View.RotateCenter = DragControlRotateCenter;
                config.View.ScaleCenter = DragControlScaleCenter;
                config.View.FlipCenter = DragControlFlipCenter;
                config.View.IsKeepScale = IsKeepScale;
                config.View.IsKeepAngle = IsKeepAngle;
                config.View.IsKeepFlip = IsKeepFlip;
                config.View.IsViewStartPositionCenter = IsViewStartPositionCenter;
            }
        }

        #endregion

    }


    /// <summary>
    /// N字スクロールパラメータ
    /// </summary>
    public interface IScrollNTypeParameter
    {
        /// <summary>
        /// スクロールの種類
        /// </summary>
        NScrollType ScrollType { get; set; }

        /// <summary>
        /// スクロール移動量の割合
        /// </summary>
        double Scroll { get; set; }

        /// <summary>
        /// スクロール時間
        /// </summary>
        double ScrollDuration { get; set; }

        /// <summary>
        /// 改行遅延時間
        /// </summary>
        double LineBreakStopTime { get; set; }
    }

    public interface IScrollNTypeEndMargin
    {
        /// <summary>
        /// 終端判定マージン
        /// </summary>
        double EndMargin { get; set; }
    }

}

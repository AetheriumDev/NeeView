﻿using System;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    public interface IPageFrameTransform : IScaleControl, IAngleControl, IPointControl, IFlipControl
    {
        //public double Scale { get; }
        //public double Angle { get; }
        //public Point Point { get; }
        //public bool IsFlipHorizontal { get; }
        //public bool IsFlipVertical { get; }

        public Transform Transform { get; }
        public Transform TransformView { get; }

        //public void SetScale(double value, TimeSpan span);
        //public void SetAngle(double value, TimeSpan span);
        //public void SetPoint(Point value, TimeSpan span);
        //public void SetFlipHorizontal(bool value, TimeSpan span);
        //public void SetFlipVertical(bool value, TimeSpan span);
    }

    public class DummyPageFrameTransform : IPageFrameTransform
    {
        public double Scale => 1.0;
        public double Angle => 0.0;
        public Point Point => default;
        public bool IsFlipHorizontal => false;
        public bool IsFlipVertical => false;

        public Transform Transform => Transform.Identity;
        public Transform TransformView => Transform.Identity;


        public void SetAngle(double value, TimeSpan span)
        {
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
        }

        public void SetPoint(Point value, TimeSpan span)
        {
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
        }

        public void SetScale(double value, TimeSpan span)
        {
        }
    }
}

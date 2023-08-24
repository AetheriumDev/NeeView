﻿using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using System;
using System.Diagnostics;

namespace NeeView.PageFrames
{
    public interface INotifyTransformChanged
    {
        event TransformChangedEventHandler? TransformChanged;
    }


    public enum TransformChangeSource
    {
        Logic,
        ViewControl,
    }

    public enum TransformCategory
    {
        Content,
        View,
        Loupe,
    }

    public enum TransformAction
    {
        Point,
        Angle,
        Scale,
        FlipHorizontal,
        FlipVertical,
    }

    public class TransformChangedEventArgs : EventArgs
    {
        public TransformChangedEventArgs(ITransformControlObject source, TransformCategory category, TransformAction action)
        {
            switch (action)
            {
                case TransformAction.Scale:
                    Debug.Assert(source is IScaleControl);
                    break;
                case TransformAction.Angle:
                    Debug.Assert(source is IAngleControl);
                    break;
                case TransformAction.Point:
                    Debug.Assert(source is IPointControl);
                    break;
                case TransformAction.FlipHorizontal:
                case TransformAction.FlipVertical:
                    Debug.Assert(source is IFlipControl);
                    break;
                default:
                    throw new ArgumentException($"Source has not action: {action}");
            }

            Source = source;
            Category = category;
            Action = action;
        }

        public TransformChangedEventArgs(TransformChangedEventArgs source)
        {
            Source = source.Source;
            Category = source.Category;
            Action = source.Action;
        }


        public ITransformControlObject Source { get; set; }
        public TransformCategory Category { get; }
        public TransformAction Action { get; }
    }

    /// <summary>
    /// 基準スケールを保持した <see cref="TransformChangedEventArgs"/>
    /// </summary>
    public class OriginalScaleTransformChangedEventArgs : TransformChangedEventArgs
    {
        public OriginalScaleTransformChangedEventArgs(TransformChangedEventArgs source, double originalScale) :
            base(source)
        {
            OriginalScale = originalScale;
        }

        public double OriginalScale { get; } = 1.0;
    }


    public delegate void TransformChangedEventHandler(object? sender, TransformChangedEventArgs e);
}
﻿//#define LOCAL_DEBUG

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NeeView.Threading;

namespace NeeView
{
    public class ImageViewContentStrategy : IDisposable, IViewContentStrategy, IHasImageSource, IHasScalingMode
    {
        private readonly ViewContent _viewContent;
        private readonly InstantDelayAction _delayAction = new();
        private ImageContentControl? _imageControl;
        private bool _disposedValue;
        private BitmapScalingMode? _scalingMode;
        private readonly object _lock = new object();

        public ImageViewContentStrategy(ViewContent viewContent)
        {
            _viewContent = viewContent;
        }


        public ImageSource? ImageSource => _imageControl?.ImageSource;

        public BitmapScalingMode? ScalingMode
        {
            get { return _scalingMode; }
            set
            {
                if (_scalingMode != value)
                {
                    _scalingMode = value;
                    if (_imageControl != null)
                    {
                        _imageControl.ScalingMode = _scalingMode;
                    }
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _delayAction.Dispose();
                    _imageControl?.Dispose();
                    _imageControl = null;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public void OnSourceChanged()
        {
            if (_disposedValue) return;

            _delayAction.Request(
                () => _viewContent.RequestLoadViewSource(CancellationToken.None),
                TimeSpan.FromMilliseconds(200)
            );
        }

        public FrameworkElement CreateLoadedContent(object data)
        {
            var viewData = data as ImageViewData ?? throw new InvalidOperationException();

            Trace($"Create={_viewContent.Page}, {_imageControl is not null}");

            lock (_lock)
            {
                _imageControl?.Dispose();
                _imageControl = null;

                if (viewData.ImageSource is DrawingImage)
                {
                    _imageControl = new CropImageContentControl(_viewContent.Element, viewData.ImageSource, _viewContent.ViewContentSize, _viewContent.BackgroundSource);
                }
                else
                {
                    _imageControl = new BrushImageContentControl(_viewContent.Element, viewData.ImageSource, _viewContent.ViewContentSize, _viewContent.BackgroundSource);
                }
                _imageControl.ScalingMode = ScalingMode;
                return _imageControl;
            }
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}: {string.Format(s, args)}");
        }
    }
}

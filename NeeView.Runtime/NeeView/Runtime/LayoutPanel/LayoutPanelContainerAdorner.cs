﻿using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanelContainerAdorner : Adorner
    {
        private readonly AdornerLayer _layer;
        private bool _isAttached;
        private Point _start;
        private Point _end;


        public LayoutPanelContainerAdorner(UIElement adornedElement) : base(adornedElement)
        {
            this.IsHitTestVisible = false;
            this.UseLayoutRounding = true;
            this.SnapsToDevicePixels = true;

            _layer = AdornerLayer.GetAdornerLayer(adornedElement);
        }


        public Point Start
        {
            get { return _start; }
            set { if (_start != value) { _start = value; Update(); } }
        }

        public Point End
        {
            get { return _end; }
            set { if (_end != value) { _end = value; Update(); } }
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0x80, 0x80));
            var renderPen = new Pen(new SolidColorBrush(Colors.LightGray), 0.0);

            var rect = new Rect(Start, End);
            drawingContext.DrawRectangle(renderBrush, renderPen, rect);
        }

        public void Attach()
        {
            if (_layer != null && !_isAttached)
            {
                _layer.Add(this);
                _isAttached = true;
            }
        }

        public void Detach()
        {
            if (_layer != null && _isAttached)
            {
                _layer.Remove(this);
                _isAttached = false;
            }
        }

        private void Update()
        {
            _layer?.Update();
        }
    }
}

﻿using NeeLaboratory.Windows.Input;
using NeeView.Windows;
using NeeView.Windows.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Runtime.LayoutPanel
{
    /// <summary>
    /// LayoutPanelContainer.xaml の相互作用ロジック
    /// </summary>
    public partial class LayoutPanelContainer : UserControl
    {
        private LayoutPanelContainerAdorner? _adorner;
        private readonly LayoutPanelManager _manager;


        // NOTE: Designer用
        public LayoutPanelContainer()
        {
            InitializeComponent();
            this.DataContext = this;

            _manager = new LayoutPanelManager();
        }

        public LayoutPanelContainer(LayoutPanelManager manager, LayoutPanel layoutPanel)
        {
            InitializeComponent();
            this.DataContext = this;

            _manager = manager;
            LayoutPanel = layoutPanel;

            this.FloatingMenuItem.Header = manager.Resources["Floating"];
            this.DockingMenuItem.Header = manager.Resources["Docking"];
            this.CloseMenuItem.Header = manager.Resources["Close"];

            this.Loaded += LayoutPanelContainer_Loaded;
        }


        public LayoutPanel LayoutPanel
        {
            get { return (LayoutPanel)GetValue(LayoutPanelProperty); }
            set { SetValue(LayoutPanelProperty, value); }
        }

        public static readonly DependencyProperty LayoutPanelProperty =
            DependencyProperty.Register("LayoutPanel", typeof(LayoutPanel), typeof(LayoutPanelContainer), new PropertyMetadata(null));



        public IDragDropDescriptor DragDropDescriptor
        {
            get { return (IDragDropDescriptor)GetValue(DescriptorProperty); }
            set { SetValue(DescriptorProperty, value); }
        }

        public static readonly DependencyProperty DescriptorProperty =
            DependencyProperty.Register("Descriptor", typeof(IDragDropDescriptor), typeof(LayoutPanelContainer), new PropertyMetadata(null));


        private void LayoutPanelContainer_Loaded(object sender, RoutedEventArgs e)
        {
            _adorner = _adorner ?? new LayoutPanelContainerAdorner(this);

            this.PreviewDragOver += LayoutPanelContainer_PreviewDragOver;
            this.PreviewDragEnter += LayoutPanelContainer_PreviewDragEnter;
            this.PreviewDragLeave += LayoutPanelContainer_PreviewDragLeave;
            this.Drop += LayoutPanelContainer_Drop;
            this.AllowDrop = true;
        }


        public void Snap()
        {
            // TODO: WindowPlacement情報破棄 はダメ。復元できるように。
            LayoutPanel.WindowPlacement = WindowPlacement.None;
        }


        private void OpenWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var point = this.PointToScreen(new Point(0.0, 0.0));
            _manager.OpenWindow(LayoutPanel, new WindowPlacement(WindowState.Normal, (int)point.X + 32, (int)point.Y + 32, (int)ActualWidth, (int)ActualHeight));
        }

        private void ClosePanelCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            _manager.StandAlone(LayoutPanel);
            _manager.Close(LayoutPanel);
        }

        #region DragDrop

        private void DragBegin(object sender, DragStartEventArgs e)
        {
            _manager.RaiseDragBegin();
        }

        private void DragEnd(object sender, EventArgs e)
        {
            _manager.RaiseDragEnd();
        }

        private void LayoutPanelContainer_Drop(object sender, DragEventArgs e)
        {
            _adorner?.Detach();

            var content = (LayoutPanel)e.Data.GetData(typeof(LayoutPanel));
            if (content is null)
            {
                return;
            }

            e.Handled = true;

            if (content == this.LayoutPanel)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            var dock = GetLayoutDockFromPosY(e.GetPosition(this).Y, this.ActualHeight);

            if (this.Parent is LayoutDockPanel dockPanel)
            {
                // 挿入位置
                var list = dockPanel.ItemsSource;
                var index = list.IndexOf(this.LayoutPanel);
                if (index < 0) throw new InvalidOperationException();

                if (list.Contains(content))
                {
                    // list内での移動
                    var oldIndex = list.IndexOf(content);
                    var newIndex = index + ((oldIndex < index) ? -1 : 0) + ((dock == Dock.Bottom) ? 1 : 0);
                    list.Move(oldIndex, newIndex);
                }
                else
                {
                    // 管理からいったん削除
                    _manager.Remove(content);

                    // GridLengthの補正
                    var gridLength = new GridLength(LayoutPanel.GridLength.Value * 0.5, GridUnitType.Star);
                    LayoutPanel.GridLength = gridLength;
                    content.GridLength = gridLength;

                    // 登録
                    var newIndex = index + ((dock == Dock.Bottom) ? 1 : 0);
                    list.Insert(newIndex, content);
                }
            }
        }


        private void LayoutPanelContainer_PreviewDragOver(object sender, DragEventArgs e)
        {
            var content = (LayoutPanel)e.Data.GetData(typeof(LayoutPanel));
            if (content is null)
            {
                return;
            }

            if (content == this.LayoutPanel)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (_adorner != null)
            {
                var dock = GetLayoutDockFromPosY(e.GetPosition(this).Y, this.ActualHeight);
                switch (dock)
                {
                    case Dock.Top:
                        _adorner.Start = new Point(0, 0);
                        _adorner.End = new Point(this.ActualWidth, this.ActualHeight * 0.5);
                        break;

                    case Dock.Bottom:
                        _adorner.Start = new Point(0, this.ActualHeight * 0.5);
                        _adorner.End = new Point(this.ActualWidth, this.ActualHeight);
                        break;

                    default:
                        throw new NotSupportedException();
                }

                _adorner.Attach();
            }

            ////Debug.WriteLine($"AllowDrag:Move");

            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }


        private void LayoutPanelContainer_PreviewDragLeave(object sender, DragEventArgs e)
        {
            var content = (LayoutPanel)e.Data.GetData(typeof(LayoutPanel));
            if (content is null)
            {
                return;
            }

            if (content == this.LayoutPanel)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }

            _adorner?.Detach();
            e.Handled = true;
        }

        private void LayoutPanelContainer_PreviewDragEnter(object sender, DragEventArgs e)
        {
            LayoutPanelContainer_PreviewDragOver(sender, e);
        }

        private static Dock GetLayoutDockFromPosY(double y, double height)
        {
            return (y < height * 0.5) ? Dock.Top : Dock.Bottom;
        }

        #endregion DragDrop

    }

    public interface IDragDropDescriptor
    {
        void DragBegin();
        void DragEnd();
    }

    public interface ILayoutPanelContainerDecorator
    {
        void Decorate(LayoutPanelContainer container, Button closeButton);
    }
}

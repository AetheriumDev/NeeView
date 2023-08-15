﻿using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Media.Imaging.Metadata;
using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView
{

    public class FileInformationSource : BindableBase
    {
        private List<FileInformationRecord>? _properties;


        public FileInformationSource(Page page)
        {
            this.Page = page;

            Update();
        }


        public Page Page { get; private set; }

        public BitmapPageContent? BitmapContent => Page?.Content as BitmapPageContent;

        public PictureInfo? PictureInfo => BitmapContent?.PictureInfo;

        public BitmapMetadataDatabase? Metadata => PictureInfo?.Metadata;

        public double IconMaxSize => 96.0;

        public FrameworkElement? Icon => CreateIcon();

        public List<FileInformationRecord>? Properties
        {
            get { return _properties; }
            set { SetProperty(ref _properties, value); }
        }

        public GpsLocation? GpsLocation { get; private set; }


        public static List<FileInformationRecord> CreatePropertiesTemplate()
        {
            return new List<FileInformationRecord>(InformationKeyExtensions.DefaultKeys
                .Select(e => new FileInformationRecord(e, null)));
        }

        public void Update()
        {
            this.GpsLocation = CreateGpsLocate();
            this.Properties = CreateProperties();
        }

        public List<FileInformationRecord> CreateProperties()
        {
            var factory = new InformationValueFactory(new InformationValueSource(Page, BitmapContent, Metadata));
            var defaults = InformationKeyExtensions.DefaultKeys.Select(e => new FileInformationRecord(e, factory.Create(e)));
            var extras = factory.GetExtraMap().Select(e => new FileInformationRecord(e.Key, InformationGroup.Extras, e.Value));
            return defaults.Concat(extras).ToList();
        }

        private GpsLocation? CreateGpsLocate()
        {
            if (Metadata != null && Metadata[BitmapMetadataKey.GPSLatitude] is ExifGpsDegree lat && Metadata[BitmapMetadataKey.GPSLongitude] is ExifGpsDegree lon)
            {
                return new GpsLocation(lat, lon);
            }
            return null;
        }


        public FrameworkElement? CreateIcon()
        {
#warning 未実装
            return null;
#if false
            if (BitmapContent?.ImageSource != null)
            {
                return CreateBitmapContentIcon(BitmapContent);
            }
            else if (Page?.Entry != null)
            {
                var entry = Page.Entry;

                if (entry.IsDirectory)
                {
                    if (entry.IsFileSystem)
                    {
                        return CreateSymbolFolderIcon();
                    }
                    else
                    {
                        return CreateSymbolIcon("/Archive");
                    }
                }
                else
                {
                    return CreateSymbolIcon(LoosePath.GetExtension(entry.EntryName).ToUpper());
                }
            }

            return null;
#endif
        }

        private FrameworkElement? CreateBitmapContentIcon(BitmapPageContent bitmapContent)
        {
#warning 未実装
            return null;
#if false
            if (bitmapContent?.ImageSource is null) return null;

            var length = bitmapContent.Size.Width > bitmapContent.Size.Height ? bitmapContent.Size.Width : bitmapContent.Size.Height;
            var retio = IconMaxSize / length;

            var image = new Image()
            {
                Source = bitmapContent.ImageSource,
                Width = bitmapContent.Size.Width * retio,
                Height = bitmapContent.Size.Height * retio,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                UseLayoutRounding = true,
                Effect = new DropShadowEffect()
                {
                    ShadowDepth = 2.0,
                    Opacity = 0.5
                },
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            return image;
#endif
        }

        private FrameworkElement CreateSymbolFolderIcon()
        {
            var imageSource = FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(256.0);

            var image = new Image()
            {
                Source = imageSource,
                Width = 64.0,
                Height = 64.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                UseLayoutRounding = true,
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            return CreateSymbolIcon(image);
        }

        private FrameworkElement CreateSymbolIcon(string text)
        {
            var border = new Border()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = IconMaxSize,
                Child = new TextBlock()
                {
                    Text = text,
                    FontSize = 20.0,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5),
                },
            };
            return CreateSymbolIcon(border);
        }

        private FrameworkElement CreateSymbolIcon(UIElement content)
        {
            var border = new Border()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = IconMaxSize,
                Height = IconMaxSize,
                Background = new SolidColorBrush(Color.FromArgb(0x10, 0x80, 0x80, 0x80)),
                Child = content,
            };

            return border;
        }


#warning 未実装
        public bool CanOpenPlace()
        {
            return false;
            //return ViewContent?.FolderPlace != null;
        }

        public void OpenPlace()
        {
            //var place = ViewContent?.Page?.GetFolderOpenPlace();
            //if (!string.IsNullOrWhiteSpace(place))
            //{
            //    ExternalProcess.Start("explorer.exe", "/select,\"" + place + "\"");
            //}
        }

        public bool CanOpenMap()
        {
            return GpsLocation != null;
        }

        public void OpenMap()
        {
            GpsLocation?.OpenMap(Config.Current.Information.MapProgramFormat);
        }

    }
}

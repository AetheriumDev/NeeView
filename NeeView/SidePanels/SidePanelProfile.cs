﻿using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class SidePanelProfile
    {
        public void Initialize()
        {
            FontParameters.Current.AddPropertyChanged(nameof(FontParameters.DefaultFontName),
                (s, e) => ValidatePanelListItemProfile());

            FontParameters.Current.AddPropertyChanged(nameof(FontParameters.PaneFontSize),
                (s, e) => ValidatePanelListItemProfile());

            ValidatePanelListItemProfile();
        }

        public static string GetDecoratePlaceName(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Config.Current.Panels.IsDecoratePlace ? LoosePath.GetPlaceName(s) : s;
        }

        private static void ValidatePanelListItemProfile()
        {
            Config.Current.Panels.ContentItemProfile.UpdateTextHeight();
            Config.Current.Panels.BannerItemProfile.UpdateTextHeight();
            Config.Current.Panels.ThumbnailItemProfile.UpdateTextHeight();
        }

    }

}

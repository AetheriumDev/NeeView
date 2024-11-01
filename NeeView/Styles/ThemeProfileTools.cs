﻿using System;
using System.Text.Json;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using System.Diagnostics;
using System.Xml;
using System.Globalization;

namespace NeeView
{
    public static class ThemeProfileTools
    {
        public static void Save(ThemeProfile themeProfile, string path)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(themeProfile, UserSettingTools.GetSerializerOptions());
            System.IO.File.WriteAllBytes(path, json);
        }

        public static void SaveFromContent(string contentPath, string path)
        {
            var uri = new Uri(contentPath, UriKind.Relative);
            var info = Application.GetRemoteStream(uri);

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                info.Stream.CopyTo(fileStream);
            }
        }

        private static ThemeProfile? Load(Stream stream)
        {
            return JsonSerializer.Deserialize<ThemeProfile>(stream, UserSettingTools.GetSerializerOptions());
        }

        public static ThemeProfile LoadFromContent(string contentPath)
        {
            var uri = new Uri(contentPath, UriKind.Relative);
            var info = Application.GetRemoteStream(uri);
            if (info is null) throw new FileNotFoundException($"No such theme: {contentPath}");
            var profile = Load(info.Stream);
            if (profile is null) throw new FormatException($"Wrong theme profile format: {contentPath}");
            return profile;
        }

        public static ThemeProfile LoadFromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var profile = Load(stream);
                if (profile is null) throw new FormatException($"Wrong theme profile format: {path}");
                return profile;
            }
        }

        public static ThemeProfile Merge(ThemeProfile baseProfile, ThemeProfile overwriteProfile)
        {
            var profile = (ThemeProfile)baseProfile.Clone();

            foreach (var pair in overwriteProfile.Colors)
            {
                profile[pair.Key] = pair.Value;
            }

            return profile;
        }

        [Conditional("DEBUG")]
        public static void SaveColorsXaml(ThemeProfile themeProfile, string path)
        {
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace nsx = "http://schemas.microsoft.com/winfx/2006/xaml";
            var xdoc = new XDocument();
            var root = new XElement(ns + "ResourceDictionary", new XAttribute("xmlns", ns), new XAttribute(XNamespace.Xmlns + "x", nsx));

            xdoc.Add(root);
            foreach (var pair in themeProfile.Colors)
            {
                var node = new XElement(ns + "SolidColorBrush",
                    new XAttribute(nsx + "Key", pair.Key),
                    new XAttribute("Color", themeProfile.GetColor(pair.Key, 1.0).ToString(CultureInfo.InvariantCulture)));
                root.Add(node);
            }

            Debug.Write(xdoc);

            using (var xw = XmlWriter.Create(path, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true, IndentChars = "    " }))
            {
                xdoc.Save(xw);
            }
        }
    }
}

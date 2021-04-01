﻿using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Text.Json;

namespace NeeView
{
    public enum ThemeColorType
    {
        Default,
        Color,
        Link,
    }


    [TypeConverter(typeof(ThemeColorTypeConverter))]
    [JsonConverter(typeof(ThemeColorJsonConverter))]
    public class ThemeColor
    {
        public ThemeColor()
        {
            ThemeColorType = ThemeColorType.Default;
        }

        public ThemeColor(Color color, double opacity)
        {
            ThemeColorType = ThemeColorType.Color;
            Color = color;
            Opacity = opacity;
        }

        public ThemeColor(string link, double opacity)
        {
            ThemeColorType = ThemeColorType.Link;
            Link = link;
            Opacity = opacity;
        }


        public ThemeColorType ThemeColorType { get; private set; }
        public Color Color { get; private set; }
        public string Link { get; private set; }
        public double Opacity { get; private set; } = 1.0;

        public override string ToString()
        {
            switch (ThemeColorType)
            {
                case ThemeColorType.Default:
                    return "";
                case ThemeColorType.Color:
                    return DecorateOpacityString(Color.ToString());
                case ThemeColorType.Link:
                    return DecorateOpacityString(Link);
            }

            throw new InvalidOperationException();
        }

        private string DecorateOpacityString(string s)
        {
            if (Opacity == 1.0) return s;

            return s + ":" + Opacity.ToString("F2");
        }

        public static ThemeColor Parse(string s)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return new ThemeColor();
                }
                else
                {
                    var tokens = s.Split('/');
                    var token = tokens[0];
                    var opacity = (tokens.Length > 1) ? double.Parse(tokens[1]) : 1.0;

                    if (token.IndexOf('.') >= 0)
                    {
                        return new ThemeColor(token, opacity);
                    }
                    else
                    {
                        return new ThemeColor((Color)ColorConverter.ConvertFromString(token), opacity);
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new FormatException($"{ex.Message}: \"{s}\"", ex);
            }
        }
    }


    public class ThemeColorTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return ThemeColor.Parse(value as string);
        }
    }


    public sealed class ThemeColorJsonConverter : JsonConverter<ThemeColor>
    {
        public override ThemeColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ThemeColor.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, ThemeColor value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

}

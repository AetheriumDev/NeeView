﻿using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Windows.Property
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyMemberAttribute : Attribute
    {
        public string? Name;
        public string? Title;
        public string? Tips;
        public bool IsVisible = true;
        public string? EmptyMessage;

        public PropertyMemberAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyRangeAttribute : PropertyMemberAttribute
    {
        public double Minimum;
        public double Maximum;
        public double TickFrequency;
        public bool IsEditable;
        public string? Format;
        public string? RangeProperty;

        public PropertyRangeAttribute(double min, double max)
        {
            Minimum = min;
            Maximum = max;
        }
    }

    /// <summary>
    /// double range: percent format
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyPercentAttribute : PropertyRangeAttribute
    {
        public PropertyPercentAttribute() : base(0.0, 1.0)
        {
        }

        public PropertyPercentAttribute(double min, double max) : base(min, max)
        {
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyPathAttribute : PropertyMemberAttribute
    {
        public FileDialogType FileDialogType;
        public string? Filter;
        public string? Note;
        public string? DefaultFileName;

        public PropertyPathAttribute() : base()
        {
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyStringsAttribute : PropertyMemberAttribute
    {
        public string[]? Strings;

        public PropertyStringsAttribute() : base()
        {
        }
    }

    public static class PropertyMemberAttributeExtensions
    {
        private static string GetResourceKey(PropertyInfo property, string? postfix = null)
        {
            return $"@{property.DeclaringType?.Name}.{property.Name}{postfix}";
        }

        public static string GetPropertyName(PropertyInfo property, PropertyMemberAttribute? attribute)
        {
            if (attribute is null)
            {
                return property.Name;
            }

            var resourceKey = attribute.Name ?? GetResourceKey(property);
            var resourceValue = ResourceService.GetResourceString(resourceKey, true);

#if DEBUG
            if (resourceValue is null)
            {
                Debug.WriteLine($"Error: PropertyName not found: {resourceKey}");
            }
#endif

            return resourceValue ?? resourceKey;
        }

        public static string GetPropertyName(PropertyInfo property)
        {
            return GetPropertyName(property, property.GetCustomAttribute<PropertyMemberAttribute>());
        }

        public static string? GetPropertyTips(PropertyInfo property, PropertyMemberAttribute? attribute)
        {
            if (attribute is null)
            {
                return null;
            }

            var resourceKey = attribute.Tips ?? GetResourceKey(property, ".Remarks");
            var resourceValue = ResourceService.GetResourceString(resourceKey, true);

            return resourceValue;
        }

        public static string? GetPropertyTips(PropertyInfo property)
        {
            return GetPropertyTips(property, property.GetCustomAttribute<PropertyMemberAttribute>());
        }

        public static string? GetPropertyTitle(PropertyInfo property, PropertyMemberAttribute? attribute)
        {
            if (attribute is null)
            {
                return null;
            }

            var resourceKey = attribute.Title ?? GetResourceKey(property, ".Title");
            var resourceValue = ResourceService.GetResourceString(resourceKey, true);

            return resourceValue;
        }

        public static string? GetPropertyTitle(PropertyInfo property)
        {
            return GetPropertyTitle(property, property.GetCustomAttribute<PropertyMemberAttribute>());
        }
    }
}

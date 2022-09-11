﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    /// <summary>
    /// プロパティで構成されたアクセスマップ
    /// </summary>
    public class PropertyMap : PropertyMapNode, INotifyPropertyChanged, IEnumerable<KeyValuePair<string, PropertyMapNode>>
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private static readonly PropertyMapConverter _defaultConverter;
        private static readonly PropertyMapOptions _defaultOptions;

        static PropertyMap()
        {
            _defaultConverter = new PropertyMapDefaultConverter();

            _defaultOptions = new PropertyMapOptions();
            _defaultOptions.Converters.Add(new PropertyMapEnumConverter());
            _defaultOptions.Converters.Add(new PropertyMapSizeConverter());
            _defaultOptions.Converters.Add(new PropertyMapPointConverter());
            _defaultOptions.Converters.Add(new PropertyMapColorConverter());
            _defaultOptions.Converters.Add(new PropertyMapFileTypeCollectionConverter());
            _defaultOptions.Converters.Add(new PropertyMapStringCollectionConverter());
        }


        private readonly object _source;
        private readonly Dictionary<string, PropertyMapNode> _items;
        private readonly PropertyMapOptions _options;
        private readonly IAccessDiagnostics _accessDiagnostics;


        public PropertyMap(object source, IAccessDiagnostics? accessDiagnostics, string prefix)
            : this(source, accessDiagnostics, prefix, null, null)
        {
        }

        public PropertyMap(object source, IAccessDiagnostics? accessDiagnostics, string prefix, string? label, PropertyMapOptions? options)
        {
            _source = source;
            _accessDiagnostics = accessDiagnostics ?? new DefaultAccessDiagnostics();
            _options = options ?? _defaultOptions;

            var type = _source.GetType();

            _items = new Dictionary<string, PropertyMapNode>();
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(e => e.Name))
            {
                if (property.GetCustomAttribute(typeof(PropertyMapIgnoreAttribute)) != null) continue;

                var nameAttribute = (PropertyMapNameAttribute?)property.GetCustomAttribute(typeof(PropertyMapNameAttribute));
                var key = nameAttribute?.Name ?? property.Name;
                var converter = _options.Converters.FirstOrDefault(e => e.CanConvert(property.PropertyType));

                var obsolete = (ObsoleteAttribute?)property.GetCustomAttribute(typeof(ObsoleteAttribute));
                if (obsolete != null)
                {
                    ////Debug.WriteLine($"[OBSOLETE] {prefix}.{key}: {obsolete.Message}");
                    var alternative = property.GetCustomAttribute<AlternativeAttribute>();
                    _items.Add(key, new PropertyMapObsolete(prefix + "." + key, property.PropertyType, obsolete.Message, obsolete, alternative));
                }
                else if (converter == null && property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    var propertyValue = property.GetValue(_source) ?? throw new InvalidOperationException();
                    var labelAttribute = (PropertyMapLabelAttribute?)property.GetCustomAttribute(typeof(PropertyMapLabelAttribute));
                    var newPrefix = prefix + "." + key;
                    var newLabel = labelAttribute != null ? label + ResourceService.GetString(labelAttribute.Label) + ": " : label;
                    _items.Add(key, new PropertyMap(propertyValue, _accessDiagnostics, newPrefix, newLabel, options));
                }
                else
                {
                    _items.Add(key, new PropertyMapSource(source, property, converter ?? _defaultConverter, label));
                }
            }
        }

        public object? this[string key]
        {
            get { return GetValue(_items[key]); }
            set { SetValue(_items[key], value); RaisePropertyChanged(key); }
        }

        internal bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        internal PropertyMapNode GetNode(string key)
        {
            return _items[key];
        }

        internal object? GetValue(PropertyMapNode node)
        {
            if (node is PropertyMapObsolete obsolete)
            {
                return _accessDiagnostics.Throw(new NotSupportedException(obsolete.CreateObsoleteMessage()), obsolete.PropertyType);
            }
            else if (node is PropertyMapSource source)
            {
                return AppDispatcher.Invoke(() => source.Read(_options));
            }
            else
            {
                return node;
            }
        }

        internal void SetValue(PropertyMapNode node, object? value)
        {
            if (node is PropertyMapObsolete obsolete)
            {
                _accessDiagnostics.Throw(new NotSupportedException(obsolete.CreateObsoleteMessage()));
            }
            else if (node is PropertyMapSource source)
            {
                AppDispatcher.Invoke(() => source.Write(value, _options));
            }
            else
            {
                throw new InvalidOperationException();
            }
        }



        /// <summary>
        /// 外部からのプロパティの追加
        /// </summary>
        internal void AddProperty(object source, string propertyName, string? memberName = null)
        {
            var type = source.GetType();
            var property = type.GetProperty(propertyName);
            if (property is null) throw new ArgumentException("not support property name", nameof(propertyName));
            var converter = _options.Converters.FirstOrDefault(e => e.CanConvert(property.PropertyType)) ?? _defaultConverter;

            _items.Add(memberName ?? propertyName, new PropertyMapSource(source, property, converter, null));
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = new WordNode(name);
            if (_items.Any())
            {
                node.Children = new List<WordNode>();
                foreach (var item in _items)
                {
                    switch (item.Value)
                    {
                        case PropertyMap propertyMap:
                            node.Children.Add(propertyMap.CreateWordNode(item.Key));
                            break;

                        case PropertyMapObsolete _:
                            break;

                        default:
                            node.Children.Add(new WordNode(item.Key));
                            break;
                    }
                }
            }
            return node;
        }

        internal string CreateHelpHtml(string prefix)
        {
            string s = "";

            foreach (var item in _items)
            {
                var name = prefix + "." + item.Key;
                if (item.Value is PropertyMap subMap)
                {
                    s += subMap.CreateHelpHtml(name);
                }
                else if (item.Value is PropertyMapObsolete)
                {
                }
                else
                {
                    string type = "";
                    string description = "";
                    if (item.Value is PropertyMapSource valueItem)
                    {
                        (type, description) = valueItem.CreateHelpHtml();
                    }
                    s += $"<tr><td>{name}</td><td>{type}</td><td>{description}</td></tr>\r\n";
                }
            }

            return s;
        }

        public IEnumerator<KeyValuePair<string, PropertyMapNode>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

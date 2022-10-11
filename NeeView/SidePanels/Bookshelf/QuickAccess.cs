﻿using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class QuickAccess : BindableBase, ICloneable
    {
        private string? _path;

        [JsonInclude, JsonPropertyName(nameof(Name))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? _name;


        public QuickAccess()
        { 
        }

        public QuickAccess(string path)
        {
            _path = path;
        }

        [NotNull]
        public string? Path
        {
            get { return _path ?? ""; }
            set
            {
                if (SetProperty(ref _path, value))
                {
                    RaisePropertyChanged(nameof(Name));
                    RaisePropertyChanged(nameof(Detail));
                }
            }
        }

        [JsonIgnore]
        [NotNull]
        public string? Name
        {
            get
            {
                return _name ?? DefaultName;
            }
            set
            {
                var name = value?.Trim();
                SetProperty(ref _name, string.IsNullOrEmpty(name) || name == DefaultName ? null : name); 
            }
        }

        public string DefaultName
        {
            get
            {
                var query = new QueryPath(_path);

                var name = query.DispName;
                if (PlaylistArchive.IsSupportExtension(name))
                {
                    name = LoosePath.GetFileNameWithoutExtension(name);
                }

                if (query.Search != null)
                {
                    name += $" ({query.Search})";
                }

                return name;
            }
        }

        public string Detail
        {
            get
            {
                var query = new QueryPath(_path);
                return query.SimplePath + (query.Search != null ? $"\n{Properties.Resources.Word_SearchWord}: {query.Search}" : null);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            return (QuickAccess)MemberwiseClone();
        }


        #region Memento
        [Memento]
        public class Memento
        {
            public string? Path { get; set; }
            public string? Name { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Path = _path;
            memento.Name = _name;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            Path = memento.Path;
            Name = memento.Name;
        }

        #endregion

    }
}

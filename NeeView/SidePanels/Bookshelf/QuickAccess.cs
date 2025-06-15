﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NeeView
{
    public class QuickAccess : BindableBase, ICloneable, IQuickAccessEntry
    {
        private string? _path;

        [JsonInclude, JsonPropertyName(nameof(Name))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? _name;


        public QuickAccess() : this(null, null)
        { 
        }

        public QuickAccess(string? path) : this(null, path)
        {
        }

        public QuickAccess(string? name, string? path)
        {
            _name = name;
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
                var name = GetValidateName(value?.Trim());
                SetProperty(ref _name, string.IsNullOrEmpty(name) || name == DefaultName ? null : name); 
            }
        }

        public string? RawName => _name;

        public string DefaultName
        {
            get
            {
                var query = new QueryPath(_path);

                var name = query.DisplayName;
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
                return query.SimplePath + (query.Search != null ? $"\n{Properties.TextResources.GetString("Word.SearchWord")}: {query.Search}" : null);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            var clone = new QuickAccess();
            clone.Restore(CreateMemento());
            return clone;
        }

        public static string GetValidateName(string? name)
        {
            if (name is null) return "";
            return name.Trim().Replace('/', '_').Replace('\\', '_');
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

        public string GetRenameText()
        {
            return Name;
        }

        public bool CanRename()
        {
            return true;
        }

        public async ValueTask<bool> RenameAsync(string name)
        {
            Name = name;
            await ValueTask.CompletedTask;
            return true;
        }

        #endregion
    }

}

﻿using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeeView
{
    public interface IBookmarkEntry : IHasName 
    {
    }


    public class Bookmark : BindableBase, IBookmarkEntry
    {
        private string _path;
        private BookMementoUnit? _unit;


        public Bookmark(string path)
        {
            _path = path;
        }

        public Bookmark(BookMementoUnit unit)
        {
            _path = unit.Path;
            _unit = unit;
        }


        public string Path
        {
            get { return _path; }
            set
            {
                if (SetProperty(ref _path, value))
                {
                    _unit = null;
                    RaisePropertyChanged(null);
                }
            }
        }

        public DateTime EntryTime { get; set; }

        public string Name => Unit.Memento.Name;

        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Set(Path); }
            private set { _unit = value; }
        }

        public bool IsEqual(IBookmarkEntry entry)
        {
            return entry is Bookmark bookmark && this.Name == bookmark.Name && this.Path == bookmark.Path;
        }

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }

    }

}

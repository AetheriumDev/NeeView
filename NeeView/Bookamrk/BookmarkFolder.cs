﻿using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    public class BookmarkFolder : BindableBase, IBookmarkEntry, ICloneable
    {
        private string? _name;


        public string? Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }


        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool IsEqual(IBookmarkEntry entry)
        {
            return entry is BookmarkFolder folder && this.Name == folder.Name;
        }
    }


    public class BookmarkEmpty : IBookmarkEntry, ICloneable
    {
        public string? Name => "";

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

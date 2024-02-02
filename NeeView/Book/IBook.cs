﻿using NeeLaboratory.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    // TODO: いろいろ精査
    public partial interface IBook
    {
        [Subscribable]
        public event EventHandler? PagesChanged;

        public BookMemoryService BookMemoryService { get; }

        public string Path { get; }

        public IReadOnlyList<Page> Pages { get; }

        public BookMemento Memento { get; }
    }

}

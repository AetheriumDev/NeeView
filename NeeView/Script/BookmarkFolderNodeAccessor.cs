﻿using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public record class BookmarkFolderNodeAccessor : NodeAccessor
    {
        private readonly BookmarkFolderNode _node;
        private readonly BookmarkFolderNodeSource _value;

        public BookmarkFolderNodeAccessor(FolderTreeModel model, BookmarkFolderNode node) : base(model, node)
        {
            _node = node;
            _value = new BookmarkFolderNodeSource(_node);
        }


        [WordNodeMember(AltName = "@BookmarkFolderNodeSource")]
        [ReturnType(typeof(BookmarkFolderNodeSource))]
        public override object? Value => _value;


        protected override string GetName() => _node.DispName;

        [ReturnType(typeof(BookmarkFolderNodeAccessor))]
        public override NodeAccessor Add(IDictionary<string, object?>? parameter)
        {
            return base.Add(parameter);
        }

        [WordNodeMember(AltName = nameof(Add))]
        [ReturnType(typeof(BookmarkFolderNodeAccessor))]
        public override NodeAccessor Insert(int index, IDictionary<string, object?>? parameter)
        {
            return base.Add(parameter);
        }


        internal static WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, typeof(BookmarkFolderNodeAccessor));
            return node;
        }
    }

}

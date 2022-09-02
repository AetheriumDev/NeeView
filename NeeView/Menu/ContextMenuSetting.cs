﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    public class MenuNode
    {
        public MenuNode(string? name, MenuElementType menuElementType, string? commandName)
        {
            Name = name;
            MenuElementType = menuElementType;
            CommandName = commandName;
        }

        public string? Name { get; set; }

        public MenuElementType MenuElementType { get; set; }

        public string? CommandName { get; set; }

        public List<MenuNode>? Children { get; set; }


        public IEnumerable<MenuNode> GetEnumerator()
        {
            yield return this;

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var subChild in child.GetEnumerator())
                    {
                        yield return subChild;
                    }
                }
            }
        }
    }


    public class ContextMenuManager : ContextMenuSetting
    {
        static ContextMenuManager() => Current = new ContextMenuManager();
        public static ContextMenuManager Current { get; }


        public MenuNode? CreateContextMenuNode()
        {
            return SourceTreeRaw?.CreateMenuNode();
        }

        public List<object> CreateContextMenuItems()
        {
            return SourceTree.CreateContextMenuItems();
        }

        internal void Resotre(MenuNode? contextMenuNode)
        {
            var sourceTree = contextMenuNode != null ? MenuTree.CreateMenuTree(contextMenuNode) : null;
            sourceTree?.Validate();
            SourceTree = sourceTree;
        }
    }


    [DataContract]
    public class ContextMenuSetting : BindableBase
    {
        private ContextMenu? _contextMenu;
        private bool _isDarty = true;

        [DataMember]
        private MenuTree? _sourceTree;

        [DataMember]
        public int _Version { get; set; } = Environment.ProductVersionNumber;


        public ContextMenuSetting()
        {
        }

        public ContextMenuSetting(MenuTree? source)
        {
            _sourceTree = source;
        }


        public ContextMenu? ContextMenu
        {
            get
            {
                _contextMenu = this.IsDarty ? SourceTree.CreateContextMenu() : _contextMenu;
                _isDarty = false;
                return _contextMenu;
            }
        }

        public MenuTree? SourceTreeRaw
        {
            get => _sourceTree;
            set => _sourceTree = value;
        }

        [NotNull]
        public MenuTree? SourceTree
        {
            get { return _sourceTree ?? MenuTree.CreateDefault(); }
            set
            {
                _sourceTree = value;
                _contextMenu = null;
                _isDarty = true;
                RaisePropertyChanged();
            }
        }

        public bool IsDarty
        {
            get { return _isDarty || _contextMenu == null; }
            set { _isDarty = value; }
        }

        public ContextMenuSetting Clone()
        {
            return new ContextMenuSetting(_sourceTree?.Clone());
        }

        public void Validate()
        {
            _sourceTree?.Validate();
        }


        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_sourceTree == null) return;

            // before 37.0
            if (_Version < Environment.GenerateProductVersionNumber(37, 0, 0))
            {
                foreach (var node in _sourceTree)
                {
                    if (node.MenuElementType == MenuElementType.Command)
                    {
                        if (node.CommandName is not null && CommandTable.Memento.RenameMap_37_0_0.TryGetValue(node.CommandName, out string? newName))
                        {
                            node.CommandName = newName;
                        }
                    }
                }
            }
        }
    }
}

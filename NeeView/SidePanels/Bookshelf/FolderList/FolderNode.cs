﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 一時的にフォルダーリスト階層を作る。巡回移動用
    /// </summary>
    /// HACK: パスをQueryPathで管理
    public class FolderNode
    {
        // Fields

        private object _lock = new object();

        private bool _isParentValid;
        private bool _isChildrenValid;
        private FolderCollection _collection;


        // Constructors

        public FolderNode(FolderNode parent, string name, FolderItem content)
        {
            Parent = parent;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Content = content;
            _isParentValid = _parent?.Content != null;
        }

        public FolderNode(FolderCollection collection, FolderItem content)
        {
            if (collection is FolderSearchCollection) throw new ArgumentException("collection is not folder entry");

            var parent = new FolderNode(null, collection.Place.FullPath, null);
            parent.Children = collection.Items
                .Where(e => !e.IsEmpty())
                .Select(e => new FolderNode(parent, e.Name, e) { Place = collection.Place.FullPath })
                .ToList();
            parent._isChildrenValid = true;

            parent._collection = collection;

            // 重複名はターゲットで区別する
            var index = parent.Children.FindIndex(e => e.Name == content.Name && e.Content.TargetPath == content.TargetPath);
            if (index < 0) throw new ArgumentException("collection dont have content");

            this.Parent = parent;
            this.Place = collection.Place.FullPath;
            this.Name = content.Name;
            this.Content = content;
            _isParentValid = true;

            parent.Children[index] = this;
        }


        // Properties

        private FolderNode _parent;
        public FolderNode Parent
        {
            get => _isParentValid ? _parent : throw new InvalidOperationException("parent not initialize");
            private set => _parent = value;
        }

        private List<FolderNode> _children;
        public List<FolderNode> Children
        {
            get => _isChildrenValid ? _children : throw new InvalidOperationException("children not initialize");
            private set => _children = value;
        }

        private string _place;
        public string Place
        {
            get { return _parent != null ? _parent.FullName : _place; }
            set { _place = value; }
        }

        public string Name { get; set; }

        public string FullName => _parent != null ? LoosePath.Combine(_parent.FullName, Name) : Name;

        public FolderItem Content { get; set; }


        // Methods

        public async Task<FolderNode> GetParent(CancellationToken token)
        {
            if (!_isParentValid)
            {
                var parent = await CreateParent(token);
                if (parent != null)
                {
                    var name = LoosePath.GetFileName(FullName, parent.FullName);
                    var index = parent.Children.FindIndex(e => e.Name == name); // 重複名は区別できていない
                    if (index < 0) throw new KeyNotFoundException();

                    lock (_lock)
                    {
                        if (!_isParentValid)
                        {
                            this.Parent = parent;
                            this.Place = null;
                            this.Content = Content ?? parent.Children[index].Content;
                            parent.Children[index] = this;
                            _isParentValid = true;
                        }
                    }
                }
                else
                {
                    _isParentValid = true;
                }
            }

            return Parent;
        }

        private async Task<FolderNode> CreateParent(CancellationToken token)
        {
            if (_isParentValid) return Parent;

            FolderNode parent;
            string name;

            // normal folder
            {
                var directory = _collection?.GetParentQuery().FullPath ?? LoosePath.GetDirectoryName(Name);
                if (string.IsNullOrEmpty(directory))
                {
                    Parent = null;
                    _isParentValid = true;
                    return null;
                }

                parent = new FolderNode(null, directory, null);
                name = LoosePath.GetFileName(Name);
            }

            // 子の生成
            await parent.GetChildren(token);

            return parent;
        }


        public async Task<List<FolderNode>> GetChildren(CancellationToken token)
        {
            if (!_isChildrenValid)
            {
                var children = await CreateChildren(token);

                lock (_lock)
                {
                    if (!_isChildrenValid)
                    {
                        _isChildrenValid = true;
                        Children = children;
                    }
                }

                token.ThrowIfCancellationRequested();
            }

            return Children;
        }

        private async Task<List<FolderNode>> CreateChildren(CancellationToken token)
        {
            if (_isChildrenValid) return Children;

            if (Content != null && !Content.CanOpenFolder())
            {
                return new List<FolderNode>();
            }

            var path = new QueryPath(FullName);
            if (Content != null && Content.Attributes.HasFlag(FolderItemAttribute.Shortcut))
            {
                path = Content.TargetPath;
            }

            using (var collection = await BookshelfFolderList.Current.FolderCollectionFactory.CreateFolderCollectionAsync(path, false, token))
            {
                var children = collection.Items
                    .Where(e => !e.IsEmpty())
                    .Select(e => new FolderNode(this, e.Name, e) { Place = collection.Place.FullPath })
                    .ToList();

                return children;
            }
        }


        public async Task<FolderNode> CruisePrev(CancellationToken token)
        {
            var parent = await GetParent(token);
            if (parent == null) return null;

            // 兄の末裔
            var brother = await parent.GetChildren(token);
            var index = brother.IndexOf(this);
            if (index - 1 >= 0)
            {
                return await brother[index - 1].CruiseDescendant(token);
            }

            // 親
            await parent.GetParent(token); // コンテンツ確定
            return parent;
        }

        private async Task<FolderNode> CruiseDescendant(CancellationToken token)
        {
            var children = await GetChildren(token);
            if (children.Count > 0)
            {
                return await children.Last().CruiseDescendant(token);
            }
            else
            {
                return this;
            }
        }


        public async Task<FolderNode> CruiseNext(CancellationToken token)
        {
            // 長男
            var children = await GetChildren(token);
            if (children.Count > 0)
            {
                return children.First();
            }

            return await CruiseNextUp(token);
        }

        private async Task<FolderNode> CruiseNextUp(CancellationToken token)
        {
            var parent = await GetParent(token);
            if (parent == null) return null;

            // 弟
            var brother = await parent.GetChildren(token);
            var index = brother.IndexOf(this);
            if (index + 1 < brother.Count)
            {
                return brother[index + 1];
            }

            // 親へ
            return await parent.CruiseNextUp(token);
        }
    }
}

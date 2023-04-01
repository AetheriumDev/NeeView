﻿using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// ブックマークリスト用のブックマーク登録管理
    /// </summary>
    public static class BookmarkCollectionService
    {
        /// <summary>
        /// 現在開いているフォルダーリストの場所を優先してブックマークを追加する
        /// </summary>
        public static void Add(QueryPath query)
        {
            if (!BookmarkFolderList.Current.AddBookmark(query, false))
            {
                AddToChild(BookmarkCollection.Current.Items, query);
            }
        }

        public static TreeListNode<IBookmarkEntry>? AddToChild(TreeListNode<IBookmarkEntry> parent, QueryPath query)
        {
            if (query.Scheme != QueryScheme.File)
            {
                return null;
            }

            // TODO: 重複チェックはBookmarkCollectionで行うようにする?
            var node = parent.Children.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Path == query.SimplePath);
            if (node == null)
            {
                var unit = BookMementoCollection.Current.Set(query.SimplePath);
                var bookmark = new Bookmark(unit);
                node = new TreeListNode<IBookmarkEntry>(bookmark);
                BookmarkCollection.Current.AddToChild(node, parent);
            }

            return node;
        }

        /// <summary>
        /// 現在開いているフォルダーリストを優先してブックマークを削除する
        /// </summary>
        public static bool Remove(QueryPath query)
        {
            if (BookshelfFolderList.Current.FolderCollection is BookmarkFolderCollection bookmarkFolderCollection)
            {
                var node = bookmarkFolderCollection.BookmarkPlace.Children.FirstOrDefault(e => e.IsEqual(query));
                if (node != null)
                {
                    return BookmarkCollection.Current.Remove(node);
                }
            }

            return BookmarkCollection.Current.Remove(BookmarkCollection.Current.FindNode(query));
        }


        /// <summary>
        /// ブックマークの名前変更とそれに伴う統合を行う
        /// </summary>
        public static bool Rename(TreeListNode<IBookmarkEntry> node, string newName)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Value is not BookmarkFolder folder) throw new ArgumentOutOfRangeException(nameof(node));

            newName = BookmarkFolder.GetValidateName(newName);
            var oldName = folder.Name;

            if (string.IsNullOrEmpty(newName))
            {
                return false;
            }

            if (newName != oldName)
            {
                var conflict = node.Parent?.Children.FirstOrDefault(e => e != node && e.Value is BookmarkFolder && e.Value.Name == newName);
                if (conflict != null)
                {
                    var dialog = new MessageDialog(string.Format(Properties.Resources.MergeFolderDialog_Message, newName), Properties.Resources.MergeFolderDialog_Title);
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    var result = dialog.ShowDialog();

                    if (result.Command == UICommands.Yes)
                    {
                        BookmarkCollection.Current.Merge(node, conflict);
                        return true;
                    }
                }
                else
                {
                    folder.Name = newName;
                    BookmarkCollection.Current.RaiseBookmarkChangedEvent(new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, node.Parent, node) { OldName = oldName });
                    return true;
                }
            }

            return false;
        }
    }
}

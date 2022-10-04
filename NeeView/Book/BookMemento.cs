﻿using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView
{
    public partial class Book
    {
        [DataContract]
        public class Memento
        {
            // フォルダーの場所
            [DataMember(Name = "Place", EmitDefaultValue = false)]
            public string Path { get; set; } = "";

            // ディレクトリ？
            [DataMember(EmitDefaultValue = false)]
            public bool IsDirectorty { get; set; }

            // 名前
            public string Name => Path.EndsWith(@":\") ? Path : System.IO.Path.GetFileName(Path);

            // 現在ページ
            [DataMember(EmitDefaultValue = false)]
            public string? Page { get; set; }

            // 1ページ表示 or 2ページ表示
            [DataMember(Name = "PageModeV2")]
            public PageMode PageMode { get; set; }

            // 右開き or 左開き
            [DataMember]
            public PageReadOrder BookReadOrder { get; set; }

            // 横長ページ分割 (1ページモード)
            [DataMember]
            public bool IsSupportedDividePage { get; set; }

            // 最初のページを単独表示 
            [DataMember]
            public bool IsSupportedSingleFirstPage { get; set; }

            // 最後のページを単独表示
            [DataMember]
            public bool IsSupportedSingleLastPage { get; set; }

            // 横長ページを2ページ分とみなす(2ページモード)
            [DataMember]
            public bool IsSupportedWidePage { get; set; } = true;

            // フォルダーの再帰
            [DataMember]
            public bool IsRecursiveFolder { get; set; }

            // ページ並び順
            [DataMember]
            public PageSortMode SortMode { get; set; }

            // 最終アクセス日
            [Obsolete("no used"), DataMember(Order = 12, EmitDefaultValue = false)]
            public DateTime LastAccessTime { get; set; }


            /// <summary>
            /// 複製
            /// </summary>
            public Memento Clone()
            {
                return (Memento)this.MemberwiseClone();
            }

            // 保存用バリデート
            // このmementoは履歴とデフォルト設定の２つに使われるが、デフォルト設定には本の場所やページ等は不要
            public void ValidateForDefault()
            {
                Path = "";
                Page = null;
                IsDirectorty = false;
            }

            // バリデートされたクローン
            public Memento ValidatedClone()
            {
                var clone = this.Clone();
                clone.ValidateForDefault();
                return clone;
            }

            // 値の等価判定
            public bool IsEquals(Memento? other)
            {
                return other is not null &&
                       Path == other.Path &&
                       IsDirectorty == other.IsDirectorty &&
                       Name == other.Name &&
                       Page == other.Page &&
                       PageMode == other.PageMode &&
                       BookReadOrder == other.BookReadOrder &&
                       IsSupportedDividePage == other.IsSupportedDividePage &&
                       IsSupportedSingleFirstPage == other.IsSupportedSingleFirstPage &&
                       IsSupportedSingleLastPage == other.IsSupportedSingleLastPage &&
                       IsSupportedWidePage == other.IsSupportedWidePage &&
                       IsRecursiveFolder == other.IsRecursiveFolder &&
                       SortMode == other.SortMode;
            }
        }
    }
}


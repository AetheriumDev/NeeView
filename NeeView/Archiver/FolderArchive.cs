﻿using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：通常ファイル
    /// ディレクトリをアーカイブとみなしてアクセスする
    /// </summary>
    public class FolderArchive : Archiver
    {
        /// <summary>
        /// ファイル単体のArchiveEntry用アーカイブ
        /// </summary>
        public static FolderArchive StaticArchiver { get; } = new FolderArchive("", null);


        public FolderArchive(string path, ArchiveEntry? source) : base(path, source)
        {
        }


        public override bool IsFileSystem { get; } = true;


        public override string ToString()
        {
            return "Folder";
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // リスト取得
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            // Pathがない場合は汎用アーカイブなのでリスト作成は行わない
            if (string.IsNullOrEmpty(Path))
            {
                Debug.Fail("If there is no Path, it is a general-purpose archive and does not create a list.");
                return new List<ArchiveEntry>();
            }

            token.ThrowIfCancellationRequested();

            int prefixLen = Path.Length;
            var list = new List<ArchiveEntry>();

            var directory = new DirectoryInfo(Path);
            foreach (var info in directory.EnumerateFileSystemInfos())
            {
                token.ThrowIfCancellationRequested();

                if (!FileIOProfile.Current.IsFileValid(info.Attributes))
                {
                    continue;
                }

                var name = info.FullName[prefixLen..].TrimStart('\\', '/');
                var fileInfo = info as FileInfo;
                var isDirectory = info.Attributes.HasFlag(FileAttributes.Directory);

                var entry = new ArchiveEntry(this)
                {
                    IsValid = true,
                    Id = list.Count,
                    RawEntryName = name,
                    Length = isDirectory ? -1 : fileInfo?.Length ?? 0,
                    CreationTime = info.CreationTime,
                    LastWriteTime = info.LastWriteTime,
                };

                if (fileInfo != null && FileShortcut.IsShortcut(fileInfo.Name))
                {
                    var shortcut = new FileShortcut(fileInfo);
                    if (shortcut.IsValid && shortcut.Target is FileInfo target)
                    {
                        entry.Link = target.FullName;
                        entry.Length = target.Length;
                        entry.CreationTime = target.CreationTime;
                        entry.LastWriteTime = target.LastWriteTime;
                    }
                }

                list.Add(entry);
            }

            await Task.CompletedTask;
            return list;
        }


        // ストリームを開く
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            return new FileStream(entry.Link ?? GetFileSystemPath(entry), FileMode.Open, FileAccess.Read);
        }

        // ファイルパス取得
        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            return System.IO.Path.Combine(Path, entry.EntryName);
        }

        // ファイル出力
        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            File.Copy(GetFileSystemPath(entry), exportFileName, isOverwrite);
        }
    }
}

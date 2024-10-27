﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// TODO: 書庫内書庫 ストリームによる多重展開が可能？

namespace NeeView
{
    /// <summary>
    /// アーカイバー：標準Zipアーカイバー
    /// </summary>
    public class ZipArchive : Archive
    {
        private Encoding? _encoding;


        public ZipArchive(string path, ArchiveEntry? source) : base(path, source)
        {
        }


        public override string ToString()
        {
            return Properties.TextResources.GetString("Archiver.Zip");
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        /// <summary>
        /// ZIPヘッダチェック
        /// </summary>
        /// <returns></returns>
        private static bool CheckSignature(Stream stream)
        {
            var pos = stream.Position;

            byte[] signature = new byte[4];
            stream.Read(signature, 0, 4);
            stream.Seek(pos, SeekOrigin.Begin);

            return (BitConverter.ToString(signature, 0) == "50-4B-03-04");
        }


        // エントリーリストを得る
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            FileStream? stream = null;
            try
            {
                stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);

                // ヘッダチェック
                if (!CheckSignature(stream))
                {
                    throw new FormatException(string.Format(Properties.TextResources.GetString("NotZipException.Message"), Path));
                }

                // 文字エンコード取得
                _encoding = GetEncoding(stream);

                // エントリー取得
                stream.Seek(0, SeekOrigin.Begin);
                using (var archiver = new System.IO.Compression.ZipArchive(stream, ZipArchiveMode.Read, false, _encoding))
                {
                    stream = null;

                    for (int id = 0; id < archiver.Entries.Count; ++id)
                    {
                        token.ThrowIfCancellationRequested();

                        var entry = archiver.Entries[id];
                        ZipArchiveEntryHelper.RepairEntryName(entry);

                        var archiveEntry = new ArchiveEntry(this)
                        {
                            IsValid = true,
                            Id = id,
                            Instance = null,
                            RawEntryName = entry.FullName,
                            Length = entry.Length,
                            LastWriteTime = entry.LastWriteTime.LocalDateTime,
                        };

                        if (!entry.IsDirectory())
                        {
                            list.Add(archiveEntry);
                        }
                        else
                        {
                            archiveEntry.Length = -1;
                            directories.Add(archiveEntry);
                        }
                    }

                    // ディレクトリエントリを追加
                    list.AddRange(CreateDirectoryEntries(list.Concat(directories)));
                }
            }
            finally
            {
                stream?.Dispose();
            }

            await Task.CompletedTask;
            return list;
        }

        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token)
        {
            if (entry.Id < 0) throw new ArgumentException("Cannot open this entry: " + entry.EntryName);
            if (entry.IsDirectory) throw new InvalidOperationException("Cannot open directory: " + entry.EntryName);

            using (var archiver = ZipFile.Open(Path, ZipArchiveMode.Read, _encoding))
            {
                ZipArchiveEntry archiveEntry = archiver.Entries[entry.Id];
                ZipArchiveEntryHelper.RepairEntryName(archiveEntry);
                if (!IsValidEntry(entry, archiveEntry)) throw new ValidationException(Properties.TextResources.GetString("InconsistencyException.Message"));

                using (var stream = archiveEntry.Open())
                {
                    var ms = new MemoryStream();
                    await stream.CopyToAsync(ms, token);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms;
                }
            }
        }

        /// <summary>
        /// 実体化可能なエントリ？
        /// </summary>
        public override bool CanRealize(ArchiveEntry entry)
        {
            Debug.Assert(entry.Archive == this);
            return true;
        }

        /// <summary>
        /// エントリをファイルまたはディレクトリにエクスポート
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="exportFileName">エクスポート先のパス</param>
        /// <param name="isOverwrite">上書き許可</param>
        /// <param name="token"></param>
        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            await Task.Run(() =>
            {
                using (var archiver = ZipFile.Open(Path, ZipArchiveMode.Read, _encoding))
                {
                    if (entry.IsDirectory)
                    {
                        ExtractDirectoryEntry(entry, exportFileName, isOverwrite, archiver);
                    }
                    else
                    {
                        ExtractEntry(entry, exportFileName, isOverwrite, archiver);
                    }
                }
            }, token);
        }

        /// <summary>
        /// ファイルエントリのエクスポート
        /// </summary>
        private static void ExtractEntry(ArchiveEntry entry, string exportFileName, bool isOverwrite, System.IO.Compression.ZipArchive archiver)
        {
            if (entry.Id < 0) throw new ArgumentException("Cannot extract this entry: " + entry.EntryName);
            if (entry.IsDirectory) throw new InvalidOperationException("Archive directory: " + entry.EntryName);

            var rawEntry = archiver.Entries[entry.Id].Hotfix();
            Debug.Assert(IsValidEntry(entry, rawEntry));

            rawEntry.Export(exportFileName, isOverwrite);
        }

        /// <summary>
        /// ディレクトリエントリのエクスポート
        /// </summary>
        private static void ExtractDirectoryEntry(ArchiveEntry entry, string exportFileName, bool isOverwrite, System.IO.Compression.ZipArchive archiver)
        {
            if (!entry.IsDirectory) throw new InvalidOperationException("Not archive directory: " + entry.EntryName);

            if (!isOverwrite && Directory.Exists(exportFileName)) throw new IOException($"Directory already exists: {exportFileName}");

            var prefix = CreateEntryPrefix(entry);

            var rawEntries = archiver.CollectEntries(prefix);
            if (rawEntries.Count == 0) throw new InvalidOperationException();

            foreach (var rawEntry in rawEntries)
            {
                var output = rawEntry.CreateExportPath(prefix, exportFileName);
                rawEntry.Export(output, false);
            }
        }

        /// <summary>
        /// ディレクトリエントリ以下のエントリ識別用プレフィックスを作成
        /// </summary>
        /// <param name="entry">ディレクトリエントリ</param>
        /// <returns></returns>
        private static string CreateEntryPrefix(ArchiveEntry entry)
        {
            Debug.Assert(entry.IsDirectory);
            return LoosePath.TrimDirectoryEnd(LoosePath.NormalizeSeparator(entry.RawEntryName));
        }

        /// <summary>
        /// 有効なエントリであるかを判定
        /// </summary>
        /// <param name="entry">調査するエントリ</param>
        /// <param name="zipArchiveEntry">関連付けられているZipArchiveEntry</param>
        /// <remarks>
        /// 同名エントリは正確に判定できない問題がある
        /// </remarks>
        private static bool IsValidEntry(ArchiveEntry entry, ZipArchiveEntry zipArchiveEntry)
        {
            return entry.RawEntryName == zipArchiveEntry.FullName;
        }

        /// <summary>
        /// exists?
        /// </summary>
        public override bool Exists(ArchiveEntry entry)
        {
            // TODO：１つでも削除されるとIDが変更になるため、この実装は間違っている
            // 同名エントリの区別ができれば解決するのだが現状では難しい
            return base.Exists(entry);
        }

        /// <summary>
        /// can delete
        /// </summary>
        /// <exception cref="ArgumentException">Not registered with this archiver.</exception>
        public override bool CanDelete(List<ArchiveEntry> entries)
        {
            if (entries.Any(e => e.Archive != this)) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entries));

            if (!Config.Current.Archive.Zip.IsFileWriteAccessEnabled) return false;
            return entries.All(e => e.Archive == this && e.Archive.IsRoot);
        }

        /// <summary>
        /// delete entries
        /// </summary>
        /// <exception cref="ArgumentException">Not registered with this archiver.</exception>
        public override async Task<bool> DeleteAsync(List<ArchiveEntry> entries)
        {
            if (!IsRoot) throw new ArgumentException("The archive is not a file.");
            if (entries.Any(e => e.Archive != this)) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entries));
            if (!entries.Any()) return false;

            var removes = entries;
            var directories = entries.Where(e => e.IsDirectory);
            if (directories.Any())
            {
                var all = await entries.First().Archive.GetEntriesAsync(CancellationToken.None);
                var children = directories.SelectMany(d => all.Where(e => e.Id >= 0 && e.EntryName.StartsWith(LoosePath.TrimDirectoryEnd(d.EntryName))));
                removes = entries.Concat(children).Where(e => e.Id >= 0).Distinct().ToList();
            }
            Debug.Assert(removes.All(e => e.Id >= 0));

            return await Task.Run(() =>
            {
                var tempFilename = FileIO.CreateUniquePath(Path + ".temp");
                try
                {
                    // NOTE: コピーしたファイルに対して操作と置き換えを行いアーカイブ破壊の可能性を最小限に抑える
                    File.Copy(Path, tempFilename);
                    using (var archive = ZipFile.Open(tempFilename, ZipArchiveMode.Update, _encoding))
                    {
                        ClearEntryCache();
                        var map = removes.Select(e => (Entry: e, ZipArchiveEntry: GetTargetEntry(archive, e))).ToList();
                        foreach (var item in map)
                        {
                            item.ZipArchiveEntry?.Delete();
                            item.Entry.IsDeleted = true;
                        }
                    }
                    File.Replace(tempFilename, Path, null);
                    return true;
                }
                finally
                {
                    if (File.Exists(tempFilename))
                    {
                        File.Delete(tempFilename);
                    }
                }
            });

            static ZipArchiveEntry? GetTargetEntry(System.IO.Compression.ZipArchive archive, ArchiveEntry entry)
            {
                var zipArchiveEntry = archive.Entries[entry.Id];
                ZipArchiveEntryHelper.RepairEntryName(zipArchiveEntry);
                return IsValidEntry(entry, zipArchiveEntry) ? zipArchiveEntry : null;
            }
        }

        /// <summary>
        /// Zipの文字エンコードを取得。既定(UTF8)ならば null を返す。
        /// </summary>
        /// <param name="stream">Zip stream</param>
        /// <returns></returns>
        private Encoding? GetEncoding(Stream stream)
        {
            return Config.Current.Archive.Zip.Encoding switch
            {
                ZipEncoding.Local
                    => Environment.Encoding,
                ZipEncoding.UTF8
                    => null,
                ZipEncoding.Auto
                    => IsUTF8EncodingMaybe(stream) ? null : Environment.Encoding,
                _
                    => null,
            };
        }

        /// <summary>
        /// Zipの文字エンコードがUTF8であるかを判定
        /// </summary>
        /// <param name="stream">Zip stream</param>
        /// <returns></returns>
        private bool IsUTF8EncodingMaybe(Stream stream)
        {
            try
            {
                using var analyzer = new ZipAnalyzer(stream, true);
                return analyzer.IsEncodingUTF8();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        // NOTE：ZipArchiveの項目削除にはいろいろ課題があるため保留
        // - 処理によりIDがずれてしまうためブックを開き直す必要がある -> ファイルと同様の操作感にならない
        //   - 同様の問題は削除処理にも存在するため、ブックのIDのずれを補正する包括的な処理がほしい
        // - ZipArchiveに名前変更機能がないため、削除追加という処理になってしまい、重く日付も変更されてしまう ... 7Zip だとどうだろう？
        // - フォルダーの変更処理。エントリすべてを変更する必要がある...重そうだな
#if false
        /// <summary>
        /// can rename?
        /// </summary>
        public override bool CanRename(ArchiveEntry entry)
        {
            if (entry.Archive != this) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entry));

            return IsRoot && Config.Current.Archive.Zip.IsFileWriteAccessEnabled;
        }

        /// <summary>
        /// rename
        /// </summary>
        public override async Task<bool> RenameAsync(ArchiveEntry entry, string name)
        {
            if (entry.Archive != this) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entry));
            if (!IsRoot) throw new ArgumentException("The archive is not a file.");

            //throw new NotImplementedException();

            var tempFilename = this.Path;

            using (var archive = ZipFile.Open(tempFilename, ZipArchiveMode.Update, _encoding))
            {
                ClearEntryCache();

                var oldEntry = GetTargetEntry(archive, entry);
                if (oldEntry is null) return false;

                var directory = LoosePath.GetDirectoryName(oldEntry.FullName);
                var newEntryName = LoosePath.Combine(directory, name, '/');

                var mem = new MemoryStream();
                using (var stream = oldEntry.Open())
                {
                    await stream.CopyToAsync(mem);
                    await stream.FlushAsync();
                }

                oldEntry.Delete();

                var newEntry = archive.CreateEntry(newEntryName);
                using (var stream = newEntry.Open())
                {
                    mem.Seek(0, SeekOrigin.Begin);
                    await mem.CopyToAsync(stream);
                    await mem.FlushAsync();
                }
                newEntry.LastWriteTime = oldEntry.LastWriteTime;

                entry.RawEntryName = newEntryName;
            }

            // TODO: 現在ブックを開き直す
            return true;

            static ZipArchiveEntry? GetTargetEntry(ZipArchive archive, ArchiveEntry entry)
            {
                var zipArchiveEntry = archive.Entries[entry.Id];
                ZipArchiveEntryHelper.RepairEntryName(zipArchiveEntry);
                return IsValidEntry(entry, zipArchiveEntry) ? zipArchiveEntry : null;
            }
        }
#endif
    }

#if false
    public static class ZipArchiveEntryExtension
    {
        public static bool IsDirectory(this ZipArchiveEntry self)
        {
            var last = self.FullName.Last();
            return (self.Name == "" && (last == '\\' || last == '/'));
        }
    }
#endif
}

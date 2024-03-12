﻿using NeeView.Interop;
using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

// TODO: UI要素の除外

namespace NeeView
{
    /// <summary>
    /// File I/O
    /// </summary>
    public static class FileIO
    {
        /// <summary>
        /// ファイルかディレクトリの存在チェック
        /// </summary>
        public static bool ExistsPath(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        /// <summary>
        /// FileSystemInfoを取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileSystemInfo CreateFileSystemInfo(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists) return directoryInfo;
            else return new FileInfo(path);
        }

        /// <summary>
        /// ファイル上書きチェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isOverwrite"></param>
        /// <exception cref="IOException"></exception>
        public static void CheckOverwrite(string path, bool isOverwrite)
        {
            if (!isOverwrite && File.Exists(path)) throw new IOException($"File already exists: {path}");
        }

        /// <summary>
        /// ファイル上書き前処理
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isOverwrite"></param>
        /// <exception cref="IOException"></exception>
        public static void ReadyOverwrite(string path, bool isOverwrite)
        {
            if (File.Exists(path))
            {
                if (isOverwrite)
                {
                    File.Delete(path);
                }
                else
                {
                    throw new IOException($"File already exists: {path}");
                }
            }
        }

        /// <summary>
        /// パスの衝突を連番をつけて回避
        /// </summary>
        public static string CreateUniquePath(string source)
        {
            if (!ExistsPath(source))
            {
                return source;
            }

            var path = source;

            bool isFile = File.Exists(path);
            var directory = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Cannot get parent directory");
            var filename = isFile ? Path.GetFileNameWithoutExtension(path) : Path.GetFileName(path);
            var extension = isFile ? Path.GetExtension(path) : "";
            int count = 1;

            var regex = new Regex(@"^(.+)\((\d+)\)$");
            var match = regex.Match(filename);
            if (match.Success)
            {
                filename = match.Groups[1].Value.Trim();
                count = int.Parse(match.Groups[2].Value);
            }

            do
            {
                path = Path.Combine(directory, $"{filename} ({++count}){extension}");
            }
            while (ExistsPath(path));

            return path;
        }

        /// <summary>
        /// ディレクトリが親子関係にあるかをチェック
        /// </summary>
        /// <returns></returns>
        public static bool IsSubDirectoryRelationship(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            if (dir1 == dir2) return true;

            var path1 = LoosePath.TrimDirectoryEnd(LoosePath.NormalizeSeparator(dir1.FullName)).ToUpperInvariant();
            var path2 = LoosePath.TrimDirectoryEnd(LoosePath.NormalizeSeparator(dir2.FullName)).ToUpperInvariant();
            if (path1.Length < path2.Length)
            {
                return path2.StartsWith(path1);
            }
            else
            {
                return path1.StartsWith(path2);
            }
        }

        /// <summary>
        /// DirectoryInfoの等価判定
        /// </summary>
        public static bool DirectoryEquals(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            if (dir1 == null && dir2 == null) return true;
            if (dir1 == null || dir2 == null) return false;

            var path1 = LoosePath.NormalizeSeparator(dir1.FullName).TrimEnd(LoosePath.Separators).ToUpperInvariant();
            var path2 = LoosePath.NormalizeSeparator(dir2.FullName).TrimEnd(LoosePath.Separators).ToUpperInvariant();
            return path1 == path2;
        }

       /// <summary>
       /// ファイルロックチェック
       /// </summary>
       /// <param name="file"></param>
       /// <returns></returns>
        public static bool IsFileLocked(FileInfo file, FileShare share = FileShare.None)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, share))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ファイルが読み込み可能になるまで待機
        /// </summary>
        /// <param name="file"></param>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task WaitFileReadableAsync(FileInfo file, TimeSpan timeout, CancellationToken token)
        {
            var time = new TimeSpan();
            var interval = TimeSpan.FromMilliseconds(500); 
            while (IsFileLocked(file, FileShare.Read))
            {
                if (time > timeout) throw new TimeoutException();
                await Task.Delay(interval, token);
                time += interval;
            }
        }


        #region Copy

        /// <summary>
        /// 非同期ファイルコピー
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        /// <param name="isOverwrite"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task CopyFileAsync(string sourceFileName, string destFileName, bool isOverwrite, CancellationToken token)
        {
            var mode = isOverwrite ? FileMode.Create : FileMode.CreateNew;
            using var source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);
            using var destination = new FileStream(destFileName, mode, FileAccess.Write);
            await source.CopyToAsync(destination, token);
        }

        /// <summary>
        /// ファイル、ディレクトリーを指定のフォルダーにコピーする
        /// </summary>
        public static void CopyToFolder(IEnumerable<string> froms, string toDirectory)
        {
            var toDirPath = LoosePath.TrimDirectoryEnd(toDirectory);

            var dir = new DirectoryInfo(toDirPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            ShellFileOperation.Copy(App.Current.MainWindow, froms, toDirPath);
        }

        #endregion Copy

        #region Move

        /// <summary>
        /// ファイル、ディレクトリーを指定のフォルダーに移動する
        /// </summary>
        public static void MoveToFolder(IEnumerable<string> froms, string toDirectory)
        {

            var toDirPath = LoosePath.TrimDirectoryEnd(toDirectory);

            var dir = new DirectoryInfo(toDirPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            ShellFileOperation.Move(App.Current.MainWindow, froms, toDirPath);
        }

        #endregion Move

        #region Delete

        // ファイル削除 (Direct)
        public static void DeleteFile(string filename)
        {
            new FileInfo(filename).Delete();
        }

        /// <summary>
        /// ファイル削除
        /// </summary>
        public static async Task<bool> DeleteAsync(string path)
        {
            return await DeleteAsync(new List<string>() { path });
        }

        /// <summary>
        /// ファイル削除
        /// </summary>
        public static async Task<bool> DeleteAsync(List<string> paths)
        {
            try
            {
                // 開いている本であるならば閉じる
                await BookHubTools.CloseBookAsync(paths);

                // 全てのファイルロックをはずす
                await ArchiverManager.Current.UnlockAllArchivesAsync();

                ShellFileOperation.Delete(Application.Current.MainWindow, paths, Config.Current.System.IsRemoveWantNukeWarning);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        #endregion Delete

        #region Rename

        /// <summary>
        /// ファイル名に無効な文字が含まれているか
        /// </summary>
        public static bool ContainsInvalidFileNameChars(string newName)
        {
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            return invalidCharsIndex >= 0;
        }

        /// <summary>
        /// Rename用変更後ファイル名を生成
        /// </summary>
        public static string? CreateRenameDst(string sourcePath, string newName, bool showConfirmDialog)
        {
            if (sourcePath is null) throw new ArgumentNullException(nameof(sourcePath));
            if (newName is null) throw new ArgumentNullException(nameof(newName));

            newName = newName.Trim().TrimEnd(' ', '.');

            // ファイル名に使用できない
            if (string.IsNullOrWhiteSpace(newName))
            {
                if (showConfirmDialog)
                {
                    var dialog = new MessageDialog(Properties.TextResources.GetString("FileRenameWrongDialog.Message"), Properties.TextResources.GetString("FileRenameErrorDialog.Title"));
                    dialog.ShowDialog();
                }
                return null;
            }

            //ファイル名に使用できない文字
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                if (showConfirmDialog)
                {
                    var invalids = string.Join(" ", newName.Where(e => invalidChars.Contains(e)).Distinct());
                    var dialog = new MessageDialog($"{Properties.TextResources.GetString("FileRenameInvalidDialog.Message")}\n\n{invalids}", Properties.TextResources.GetString("FileRenameErrorDialog.Title"));
                    dialog.ShowDialog();
                }
                return null;
            }

            // ファイル名に使用できない
            var match = new Regex(@"^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])(\.|$)", RegexOptions.IgnoreCase).Match(newName);
            if (match.Success)
            {
                if (showConfirmDialog)
                {
                    var dialog = new MessageDialog($"{Properties.TextResources.GetString("FileRenameWrongDeviceDialog.Message")}\n\n{match.Groups[1].Value.ToUpper()}", Properties.TextResources.GetString("FileRenameErrorDialog.Title"));
                    dialog.ShowDialog();
                }
                return null;
            }

            string src = sourcePath;
            string folder = System.IO.Path.GetDirectoryName(src) ?? throw new InvalidOperationException("Cannot get parent directory");
            string dst = System.IO.Path.Combine(folder, newName);

            // 全く同じ名前なら処理不要
            if (src == dst) return null;

            // 拡張子変更確認
            if (!Directory.Exists(sourcePath))
            {
                var srcExt = System.IO.Path.GetExtension(src);
                var dstExt = System.IO.Path.GetExtension(dst);
                if (string.Compare(srcExt, dstExt, true) != 0)
                {
                    if (showConfirmDialog)
                    {
                        var dialog = new MessageDialog(Properties.TextResources.GetString("FileRenameExtensionDialog.Message"), Properties.TextResources.GetString("FileRenameExtensionDialog.Title"));
                        dialog.Commands.Add(UICommands.Yes);
                        dialog.Commands.Add(UICommands.No);
                        var answer = dialog.ShowDialog();
                        if (answer.Command != UICommands.Yes)
                        {
                            return null;
                        }
                    }
                }
            }

            // 大文字小文字の変換は正常
            if (string.Compare(src, dst, true) == 0)
            {
                // nop.
            }

            // 重複ファイル名回避
            else if (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst))
            {
                string dstBase = dst;
                string dir = System.IO.Path.GetDirectoryName(dst) ?? throw new InvalidOperationException("Cannot get parent directory");
                string name = System.IO.Path.GetFileNameWithoutExtension(dst);
                string ext = System.IO.Path.GetExtension(dst);
                int count = 1;

                do
                {
                    dst = $"{dir}\\{name} ({++count}){ext}";
                }
                while (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst));

                // 確認
                if (showConfirmDialog)
                {
                    var dialog = new MessageDialog(string.Format(Properties.TextResources.GetString("FileRenameConflictDialog.Message"), Path.GetFileName(dstBase), Path.GetFileName(dst)), Properties.TextResources.GetString("FileRenameConflictDialog.Title"));
                    dialog.Commands.Add(new UICommand("@Word.Rename"));
                    dialog.Commands.Add(UICommands.Cancel);
                    var answer = dialog.ShowDialog();
                    if (answer.Command != dialog.Commands[0])
                    {
                        return null;
                    }
                }
            }

            return dst;
        }

        /// <summary>
        /// ファイル名前変更。現在ブックにも反映させる
        /// </summary>
        public static async Task<bool> RenameAsync(string src, string dst, bool restoreBook)
        {
            // 現在の本ならば閉じる
            var closeBookResult = await BookHubTools.CloseBookAsync(src);

            // 全てのファイルロックをはずす
            await ArchiverManager.Current.UnlockAllArchivesAsync();

            // rename main
            var isSuccess = RenameRetry(src, dst);
            if (!isSuccess) return false;

            // 本を開き直す
            if (restoreBook && closeBookResult.IsClosed)
            {
                BookHubTools.RestoreBook(dst, src, closeBookResult.RequestLoadCount);
            }

            return true;
        }

        private static bool RenameRetry(string src, string dst)
        {
            while (true)
            {
                try
                {
                    RenameCore(src, dst);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageDialogResult? answer = null;
                    AppDispatcher.Invoke(() =>
                    {
                        var retryConfirm = new MessageDialog($"{Properties.TextResources.GetString("FileRenameFailedDialog.Message")}\n\n{ex.Message}", Properties.TextResources.GetString("FileRenameFailedDialog.Title"));
                        retryConfirm.Commands.Add(UICommands.Retry);
                        retryConfirm.Commands.Add(UICommands.Cancel);
                        answer = retryConfirm.ShowDialog();
                    });
                    if (answer?.Command == UICommands.Retry)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        /// <summary>
        /// ファイル名変更
        /// </summary>
        /// <param name="src">変更前のパス</param>
        /// <param name="dst">変更後のパス</param>
        /// <exception cref="FileNotFoundException">srcファイルが見つかりません</exception>
        private static void RenameCore(string src, string dst)
        {
            try
            {
                if (System.IO.Directory.Exists(src))
                {
                    System.IO.Directory.Move(src, dst);
                }
                else if (System.IO.File.Exists(src))
                {
                    System.IO.File.Move(src, dst);
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch (IOException) when (string.Compare(src, dst, true) == 0)
            {
                // 大文字小文字の違いだけである場合はWIN32APIで処理する
                // .NET6 では不要？
                NativeMethods.MoveFile(src, dst);
            }
        }

        #endregion Rename
    }

}

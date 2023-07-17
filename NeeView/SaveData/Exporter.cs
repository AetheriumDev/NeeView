﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NeeView
{
    public static class Exporter
    {
        public static void Export(string filename)
        {
            SaveDataSync.Current.SaveAll(false, false);

            try
            {
                // 保存されたファイルをzipにまとめて出力
                using (var archive = new ZipArchive(new FileStream(filename, FileMode.Create, FileAccess.ReadWrite), ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(SaveDataProfile.UserSettingFileName, SaveDataProfile.UserSettingFileName);

                    if (File.Exists(SaveData.HistoryFilePath))
                    {
                        archive.CreateEntryFromFile(SaveData.HistoryFilePath, SaveDataProfile.HistoryFileName);
                    }
                    if (File.Exists(SaveData.BookmarkFilePath))
                    {
                        archive.CreateEntryFromFile(SaveData.BookmarkFilePath, SaveDataProfile.BookmarkFileName);
                    }
                    var playlists = PlaylistHub.GetPlaylistFiles(false);
                    if (playlists.Any())
                    {
                        foreach (var playlist in playlists)
                        {
                            archive.CreateEntryFromFile(playlist, LoosePath.Combine("Playlists", LoosePath.GetFileName(playlist)));
                        }
                    }
                    var themes = ThemeManager.CollectCustomThemes();
                    if (themes.Any())
                    {
                        foreach (var theme in themes)
                        {
                            archive.CreateEntryFromFile(theme.CustomThemeFilePath, LoosePath.Combine("Themes", theme.FileName));
                        }
                    }
                    var scripts = ScriptCommandSourceMap.CollectScripts();
                    if (scripts.Any())
                    {
                        foreach (var script in scripts)
                        {
                            archive.CreateEntryFromFile(script.FullName, LoosePath.Combine("Scripts", script.Name));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 中途半端なファイルは削除
                if (File.Exists(filename))
                {
                    Debug.WriteLine($"Delete {filename}");
                    File.Delete(filename);
                }

                throw;
            }
        }

    }
}

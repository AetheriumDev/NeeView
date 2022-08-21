﻿using System;

namespace NeeView.Susie
{
    public class SusieArchiveEntry
    {
        public SusieArchiveEntry()
        {
            Path = "";
            FileName = "";
        }

        public SusieArchiveEntry(string path, string fileName)
        {
            Path = path;
            FileName = fileName;
        }

        public int Position { get; set; }

        public string Path { get; set; }
        public string FileName { get; set; }

        // ディレクトリ？
        // ファイル名の末尾から判定。これに当てはまらないサイズ0のエントリの場合はどちらとも解釈できる
        public bool IsDirectory { get; set; }

        // 展開後ファイルサイズ
        // 正確でない可能性がある
        public uint FileSize { get; set; }

        // タイムスタンプ
        public DateTime TimeStamp { get; set; }
    }
}

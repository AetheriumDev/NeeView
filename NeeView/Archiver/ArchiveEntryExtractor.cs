﻿using NeeLaboratory.Threading.Tasks;
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
    /// ArchiveEntryExtractor イベント引数
    /// </summary>
    public class ArchiveEntryExtractorEventArgs : EventArgs
    {
        public CancellationToken CancellationToken { get; set; }
    }

    /// <summary>
    /// ArchiveEntryからファイルに展開する。キャンセル可。
    /// </summary>
    public class ArchiveEntryExtractor
    {
        private Task? _action;


        public ArchiveEntryExtractor(ArchiveEntry entry, string path)
        {
            Entry = entry;
            ExtractFileName = path ?? throw new ArgumentNullException(nameof(path));
        }


        /// <summary>
        /// 展開完了イベント
        /// </summary>
        public event EventHandler<ArchiveEntryExtractorEventArgs>? Completed;


        /// <summary>
        /// 元になるArchiveEntry
        /// </summary>
        public ArchiveEntry Entry { get; private set; }

        /// <summary>
        /// 展開ファイルパス
        /// </summary>
        public string ExtractFileName { get; private set; }

        /// <summary>
        /// 処理開始済？
        /// </summary>
        public bool IsActive => _action != null;


        public async Task<string> ExtractAsync(CancellationToken token)
        {
            Exception? innerException = null;

            _action = TaskUtils.ActionAsync((t) =>
            {
                try
                {
                    Entry.ExtractToFile(ExtractFileName, false);
                    //Debug.WriteLine("EXT: Extract done.");
                    Completed?.Invoke(this, new ArchiveEntryExtractorEventArgs() { CancellationToken = t });
                }
                catch (Exception e)
                {
                    innerException = e;
                }
            },
            token);

            await TaskUtils.WaitAsync(_action, token);
            if (innerException != null) throw innerException;

            return ExtractFileName;
        }

        public async Task<string> WaitAsync(CancellationToken token)
        {
            Debug.Assert(_action != null);

            await TaskUtils.WaitAsync(_action, token);

            return ExtractFileName;
        }
    }
}

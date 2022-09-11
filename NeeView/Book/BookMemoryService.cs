﻿using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ブックのメモリ管理
    /// </summary>
    public class BookMemoryService : BindableBase
    {
        private readonly PageContentPool _contentPool = new();
        private readonly PictureSourcePool _pictureSourcePool = new();

        public static long LimitSize => (long)Config.Current.Performance.CacheMemorySize * 1024 * 1024;

        public long TotalSize => _contentPool.TotalSize + _pictureSourcePool.TotalSize;

        public bool IsFull => TotalSize >= LimitSize;


        public void SetReference(int index)
        {
            _contentPool.SetReference(index);
        }

        public void AddPageContent(IHasPageContent content)
        {
            _contentPool.Add(content);

            _contentPool.Cleanup(LimitSize - _pictureSourcePool.TotalSize);
            if (IsFull)
            {
                _pictureSourcePool.Cleanup();
            }

            RaisePropertyChanged("");
        }

        public void AddPictureSource(IHasPictureSource pictureSource)
        {
            _pictureSourcePool.Add(pictureSource);

            RaisePropertyChanged("");
        }

        /// <summary>
        /// OutOfMemory発生時の不活性メモリ開放処理
        /// </summary>
        public void CleanupDeep()
        {
            _contentPool.Cleanup(0);
            _pictureSourcePool.Cleanup();
        }

        public void Clear()
        {
            _contentPool.Clear();
            _pictureSourcePool.Clear();

            RaisePropertyChanged("");
        }
    }
}

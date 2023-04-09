﻿using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// Playlistの名前変更機能
    /// </summary>
    public class PlaylistItemRenamer : ListBoxItemRenamer<PlaylistItem>
    {
        private readonly Func<PlaylistItem, string, bool> _renameFunc;

        public PlaylistItemRenamer(ListBox listBox, IToolTipService? toolTipService, Func<PlaylistItem, string, bool> renameFunc) : base(listBox, toolTipService)
        {
            _renameFunc = renameFunc;
        }

        protected override RenameControl CreateRenameControl(ListBox listBox, PlaylistItem item)
        {
            return new PlaylistItemmRenameControl(listBox, item, _renameFunc);
        }
    }


    public class PlaylistItemmRenameControl : ListBoxItemRenameControl<PlaylistItem>
    {
        private readonly Func<PlaylistItem, string, bool> _renameFunc;

        public PlaylistItemmRenameControl(ListBox listBox, PlaylistItem item, Func<PlaylistItem, string, bool> renameFunc) : base(listBox, item)
        {
            _renameFunc = renameFunc;

            this.IsInvalidSeparatorChars = true;
        }

        protected override async Task<bool> OnRenameAsync(string oldValue, string newValue)
        {
            if (oldValue == newValue) return true;

            var result = _renameFunc(_item, newValue);
            return await Task.FromResult(result);
        }
    }
}

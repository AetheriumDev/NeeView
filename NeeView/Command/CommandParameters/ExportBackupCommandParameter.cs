﻿using NeeView.Windows.Property;

namespace NeeView
{
    public class ExportBackupCommandParameter : CommandParameter
    {
        private string? _fileName;

        [PropertyPath(FileDialogType = Windows.Controls.FileDialogType.SaveFile, Filter = "NeeView BackupFile|*.nvzip")]
        public string FileName
        {
            get { return _fileName ?? ""; }
            set { SetProperty(ref _fileName, value); }
        }
    }
}

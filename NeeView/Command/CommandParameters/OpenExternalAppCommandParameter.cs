﻿using NeeView.Windows.Property;
using System.ComponentModel;

namespace NeeView
{
    public class OpenExternalAppCommandParameter : CommandParameter, INotifyPropertyChanged, IExternalAppParameter
    {
        // コマンドパラメータで使用されるキーワード
        public const string KeyFile = "$File";
        public const string KeyUri = "$Uri";
        public const string DefaultParameter = "\"" + KeyFile + "\"";

        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private string? _command;
        private string _parameter = DefaultParameter;
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;
        private string? _workingDirectory;


        // コマンド
        [PropertyPath(Filter = "EXE|*.exe|All|*.*")]
        public string? Command
        {
            get { return _command; }
            set { SetProperty(ref _command, value); }
        }

        // コマンドパラメータ
        // $File = 渡されるファイルパス
        [PropertyMember]
        public string Parameter
        {
            get { return _parameter; }
            set { SetProperty(ref _parameter, string.IsNullOrWhiteSpace(value) ? DefaultParameter : value); }
        }

        // 作業フォルダー
        [PropertyPath(FileDialogType = Windows.Controls.FileDialogType.Directory)]
        public string? WorkingDirectory
        {
            get { return _workingDirectory; }
            set { SetProperty(ref _workingDirectory, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        // 複数ページのときの動作
        [PropertyMember]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { SetProperty(ref _multiPagePolicy, value); }
        }

        // 圧縮ファイルのときの動作
        [PropertyMember]
        public ArchivePolicy ArchivePolicy
        {
            get { return _archivePolicy; }
            set { SetProperty(ref _archivePolicy, value); }
        }
    }

}

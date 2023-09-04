﻿//using System.Drawing;

using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// 画像ファイル出力
    /// </summary>
    // TODO: スケールをオリジナルにできないか？だがフィルターで求めるサイズにしている可能性も。悩ましい。
    public class ExportImage : BindableBase
    {
        private readonly ExportImageSource _source;

        private IImageExporter _exporter;

        public ExportImage(ExportImageSource source)
        {
            _source = source;

            UpdateExporter();
        }

        public string? ExportFolder { get; set; }

        private ExportImageMode _mode;
        public ExportImageMode Mode
        {
            get { return _mode; }
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    UpdateExporter();
                }
            }
        }

        /// <summary>
        /// ViewImage用：背景を含める
        /// </summary>
        private bool _hasBackground;
        public bool HasBackground
        {
            get { return _hasBackground; }
            set
            {
                if (SetProperty(ref _hasBackground, value))
                {
                    _exporter.HasBackground = _hasBackground;
                    UpdatePreview();
                }
            }
        }

        private FrameworkElement? _preview;
        public FrameworkElement? Preview
        {
            get { return _preview; }
            set { SetProperty(ref _preview, value); }
        }

        private string _imageFormatNote = "";
        public string ImageFormatNote
        {
            get { return _imageFormatNote; }
            set { SetProperty(ref _imageFormatNote, value); }
        }

        public int QualityLevel { get; internal set; }


        private static IImageExporter CreateExporter(ExportImageMode mode, ExportImageSource source, bool hasBackground)
        {
            return mode switch
            {
                ExportImageMode.Original
                    => new OriginalImageExporter(source) { HasBackground = hasBackground },
                ExportImageMode.View
                    => new ViewImageExporter(source) { HasBackground = hasBackground },
                _ 
                    => throw new InvalidOperationException(),
            };
        }


        [MemberNotNull(nameof(_exporter))]
        public void UpdateExporter()
        {
            _exporter = CreateExporter(_mode, _source, _hasBackground);
            UpdatePreview();
        }

        public void UpdatePreview()
        {
            AppDispatcher.BeginInvoke(() =>
            {
                try
                {
                    var content = _exporter.CreateView();
                    if (content is null) throw new InvalidOperationException();
                    Preview = content.View;
                    ImageFormatNote = content.Size.IsEmpty ? "" : $"{(int)content.Size.Width} x {(int)content.Size.Height}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Preview = null;
                    ImageFormatNote = "Error.";
                }
            });
        }


        public void Export(string path, bool isOverwrite)
        {
            path = System.IO.Path.GetFullPath(path);

            _exporter.Export(path, isOverwrite, QualityLevel);
            ExportFolder = System.IO.Path.GetDirectoryName(path);
        }

        public string CreateFileName(ExportImageFileNameMode fileNameMode, ExportImageFormat format)
        {
            var nameMode = fileNameMode == ExportImageFileNameMode.Default
                ? _mode == ExportImageMode.Original ? ExportImageFileNameMode.Original : ExportImageFileNameMode.BookPageNumber
                : fileNameMode;

            var extension = _mode == ExportImageMode.Original
                ? LoosePath.GetExtension(_source.Pages[0].EntryLastName).ToLower()
                : format == ExportImageFormat.Png ? ".png" : ".jpg";

            if (nameMode == ExportImageFileNameMode.Original)
            {
                var filename = LoosePath.ValidFileName(_source.Pages[0].EntryLastName);
                return System.IO.Path.ChangeExtension(filename, extension);
            }
            else
            {
                var bookName = LoosePath.GetFileNameWithoutExtension(_source.BookAddress);

                var indexLabel = _mode != ExportImageMode.Original && _source.Pages.Count > 1
                    ? $"{_source.Pages[0].Index:000}-{_source.Pages[1].Index:000}"
                    : $"{_source.Pages[0].Index:000}";

                return LoosePath.ValidFileName($"{bookName}_{indexLabel}{extension}");
            }
        }
    }
}

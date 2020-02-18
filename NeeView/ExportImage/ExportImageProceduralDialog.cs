﻿using System;
//using System.Drawing;
using System.Windows;

namespace NeeView
{
    public class ExportImageProceduralDialog
    {
        private bool _isInitialized;

        public string ExportFolder { get; set; }

        public ExportImageMode Mode { get; set; }

        public bool HasBackground { get; set; }

        public void Show(ExportImageDialogCommandParameter parameter)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                ExportFolder = parameter.ExportFolder;
            }

            var source = ExportImageSource.Create();

            var exporter = new ExportImage(source);
            exporter.ExportFolder = string.IsNullOrWhiteSpace(ExportFolder) ? System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) : ExportFolder;
            exporter.Mode = Mode;
            exporter.HasBackground = HasBackground;
            exporter.QualityLevel = parameter.QualityLevel;

            var vm = new ExportImageWindowViewModel(exporter);
            var editor = new ExportImageWindow(vm);
            editor.Owner = MainWindow.Current;
            editor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            editor.ShowDialog();

            ExportFolder = exporter.ExportFolder;
            Mode = exporter.Mode;
            HasBackground = HasBackground;
        }
    }
}

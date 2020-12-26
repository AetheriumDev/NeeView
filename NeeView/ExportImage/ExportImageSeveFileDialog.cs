﻿using Microsoft.Win32;
using NeeView.Properties;
using System;
//using System.Drawing;
using System.Linq;
using System.Windows;

namespace NeeView
{
    public class ExportImageSeveFileDialog
    {
        public bool? ShowDialog(Window owner)
        {
            try
            {
                var dialog = CreateSaveFileDialog(FileName, ValidateDirectoryPath(InitialDirectory), CanSelectFormat);
                var result = dialog.ShowDialog(owner);
                FileName = dialog.FileName;
                return result;
            }
            catch (Exception ex)
            {
                new MessageDialog($"{Resources.ImageExportErrorDialog_Message}\n{Resources.WordCause}: {ex.Message}", Resources.ImageExportErrorDialog_Title).ShowDialog();
                return false;
            }
        }

        public string InitialDirectory { get; set; }

        public string FileName { get; set; }

        public bool CanSelectFormat { get; set; }


        private string ValidateDirectoryPath(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                return path;
            }
            else
            {
                return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            }
        }

        public SaveFileDialog CreateSaveFileDialog(string filename, string directory, bool canSelectFormat)
        {
            var dialog = new SaveFileDialog();

            dialog.InitialDirectory = directory;
            dialog.OverwritePrompt = true;

            dialog.AddExtension = true;

            var defaultExt = LoosePath.GetExtension(filename);
            dialog.DefaultExt = defaultExt;

            var fileName = LoosePath.ValidFileName(System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(filename), defaultExt));
            dialog.FileName = fileName;

            if (canSelectFormat)
            {
                var pngExt = new string[] { ".png" };
                var jpgExt = new string[] { ".jpg", ".jpeg", ".jpe", ".jfif" };

                string filter = "PNG|*.png|JPEG|*.jpg;*.jpeg;*.jpe;*.jfif";

                if (pngExt.Contains(defaultExt))
                {
                    dialog.FilterIndex = 1;
                }
                else if (jpgExt.Contains(defaultExt))
                {
                    dialog.FilterIndex = 2;
                }

                dialog.Filter = filter + "|All|*.*";
            }

            return dialog;
        }
    }
}

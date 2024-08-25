﻿using NeeLaboratory.IO;
using NeeView.IO;
using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    // TODO: エクスプローラーの圧縮 zip 内からのドロップ対応
    // TODO: 7zからのドロップ対応

    /// <summary>
    ///  Drop Exception
    ///  ユーザに知らせるべき例外
    /// </summary>
    public class DropException : Exception
    {
        public DropException()
        {
        }

        public DropException(string message)
            : base(message)
        {
        }

        public DropException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


    /// <summary>
    /// Drop Manager
    /// </summary>
    public class ContentDropManager
    {
        static ContentDropManager() => Current = new ContentDropManager();
        public static ContentDropManager Current { get; }

        private ContentDropManager()
        {
        }

        public void SetDragDropEvent(FrameworkElement sender)
        {
            sender.PreviewDragEnter += Element_PreviewDragOver;
            sender.PreviewDragOver += Element_PreviewDragOver;
            sender.Drop += Element_Drop;
        }

        // ドラッグ＆ドロップ前処理
        private void Element_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (CheckDragContent(e.Data))
            {
                e.Effects = NowLoading.Current.IsDispNowLoading ? DragDropEffects.None : DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        // ドラッグ＆ドロップで処理を開始する
        private async void Element_Drop(object sender, DragEventArgs e)
        {
            FocusWindow(sender as DependencyObject);
            await LoadDataObjectAsync(e.Data);
        }

        // ウィンドウフォーカスを得る
        private static void FocusWindow(DependencyObject? dependencyObject)
        {
            if (dependencyObject is null) return;

            var window = Window.GetWindow(dependencyObject);
            window?.Activate();
            (dependencyObject as FrameworkElement)?.Focus();
        }

        // コピー＆ペーストできる？
        public bool CanLoadFromClipboard()
        {
            var data = Clipboard.GetDataObject();
            return data != null && !NowLoading.Current.IsDispNowLoading && CheckDragContent(data);
        }

        // コピー＆ペーストで処理を開始する
        public async void LoadFromClipboard()
        {
            await LoadDataObjectAsync(Clipboard.GetDataObject());
        }

        // データオブジェクトからのロード処理
        private async Task LoadDataObjectAsync(IDataObject data)
        {
            if (NowLoading.Current.IsDispNowLoading || data == null) return;

            try
            {
                var downloadPath = string.IsNullOrWhiteSpace(Config.Current.System.DownloadPath) ? Temporary.Current.TempDownloadDirectory : Config.Current.System.DownloadPath;
                var files = await DropAsync(this, data, downloadPath, (string message) => NeeView.NowLoading.Current.SetLoading(message));
                LoadFiles(files);
            }
            catch (Exception ex)
            {
                BookHub.Current.RequestUnload(this, true, ex.Message ?? Properties.TextResources.GetString("Notice.ContentFailed"));
                NeeView.NowLoading.Current.ResetLoading();
            }
        }

        private void LoadFiles(List<string> files)
        {
            // Import
            if (files.Count == 1)
            {
                var file = files[0];
                if (LoosePath.GetExtension(file) == ".nvzip" && System.IO.File.Exists(file))
                {
                    var param = new ImportBackupCommandParameter() { FileName = file };
                    ExportDataPresenter.Current.Import(param);
                    return;
                }
            }

            // Load Book
            BookHubTools.RequestLoad(this, files);
        }

        // ドロップ受付判定
        private static bool CheckDragContent(IDataObject data)
        {
            if (data.GetDataPresent(PageListBox.DragDropFormat) || data.GetDataPresent(FileInformationView.DragDropFormat)) return false;

            return (data.GetDataPresent(DataFormats.FileDrop, true) || (data.GetDataPresent("FileContents") && data.GetDataPresent("FileGroupDescriptorW")) || data.GetDataPresent(DataFormats.Bitmap) || data.GetDataPresent(typeof(QueryPath)));
        }

        // ファイラーからのドロップ
        private readonly List<DropReciever> _fileDropRecievers = new()
        {
            new DropQueryPath(),
            new DropFileDrop(),
            new DropFileContents(),
            new DropInlineImage(),
            new DropBitmap(),
        };

        // ブラウザからのドロップ
        private readonly List<DropReciever> _browserDropRecievers = new()
        {
            new DropQueryPath(),
            new DropFileContents(),
            new DropInlineImage(),
            new DropFileDropCopy(),
            new DropWebImage(),
            new DropBitmap(),
        };


        // ファイルのドラッグ＆ドロップで処理を開始する
        private async Task<List<string>> DropAsync(object sender, IDataObject data, string downloadPath, Action<string> nowloading)
        {
            var recievers = (data.GetDataPresent("UniformResourceLocator") || data.GetDataPresent("UniformResourceLocatorW"))
                ? _browserDropRecievers : _fileDropRecievers;

            string? errorMessage = null;
            foreach (var reciever in recievers)
            {
                try
                {
                    var path = await reciever.DropAsync(sender, data, downloadPath, nowloading);
                    if (path != null)
                    {
                        Debug.WriteLine("Load by " + reciever.ToString());
                        return path;
                    }
                }
                catch (DropException ex)
                {
                    errorMessage = ex.Message;
                    break;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }

            // データタンプ
            DumpDragData(data);

            //  読み込めなかったエラー表示
            throw new ApplicationException(errorMessage ?? Properties.TextResources.GetString("Notice.ContentFailed"));
        }


        /// <summary>
        /// ドラッグオブジェクトのダンプ
        /// </summary>
        /// <param name="data"></param>
        [Conditional("DEBUG")]
        private static void DumpDragData(System.Windows.IDataObject data)
        {
            Debug.WriteLine("----");
            foreach (var name in data.GetFormats(true))
            {
                try
                {
                    var obj = data.GetData(name);
                    if (obj is System.IO.MemoryStream)
                    {
                        Debug.WriteLine($"<{name}>: {obj} ({(obj as System.IO.MemoryStream)?.Length})");
                    }
                    else if (obj is string)
                    {
                        Debug.WriteLine($"<{name}>: string: {obj}");
                    }
                    else
                    {
                        Debug.WriteLine($"<{name}>: {obj}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"<{name}>: [Exception]: {ex.Message}");
                }
            }
            Debug.WriteLine("----");
        }
    }



    /// <summary>
    /// ドロップ処理基底
    /// </summary>
    public abstract class DropReciever
    {
        /// <summary>
        /// ドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data">データオブジェクト</param>
        /// <param name="downloadPath">ファイル出力パス</param>
        /// <param name="nowloading">NowLoading表示用デリゲート</param>
        /// <returns>得られたファイルパス</returns>
        public abstract Task<List<string>?> DropAsync(object sender, IDataObject data, string downloadPath, Action<string> nowloading);

        /// <summary>
        /// バイナリを画像としてファイルに保存(Async)
        /// </summary>
        public static async Task<string?> DownloadToFileAsync(byte[] buff, string? name, string downloadPath)
        {
            return await Task.Run(() => DownloadToFile(buff, name, downloadPath));
        }

        /// <summary>
        /// バイナリを画像としてファイルに保存
        /// </summary>
        /// <param name="buff">バイナリ</param>
        /// <param name="name">希望ファイル名。現在の実装では無視されます</param>
        /// <param name="downloadPath">保存先フォルダー</param>
        /// <returns>出力されたファイルパスを返す。バイナリが画像データ出なかった場合はnull</returns>
        public static string? DownloadToFile(byte[] buff, string? name, string downloadPath)
        {
            //if (!System.IO.Directory.Exists(downloadPath)) throw new DropException("保存先フォルダーが存在しません");

            // ファイル名は固定
            name = DateTime.Now.ToString("yyyyMMddHHmmss");
            string ext = "";

            // 画像ファイルチェック
            // 対応拡張子に変更する
            var exts = PictureFormat.GetSupportImageExtensions(buff);
            if (exts == null) return null;
            if (!exts.Contains(ext))
            {
                var newExtension = exts[0];
                // 一部拡張子置き換え
                if (newExtension == ".jpeg") newExtension = ".jpg";
                if (newExtension == ".tiff") newExtension = ".tif";
                name = System.IO.Path.ChangeExtension(name, newExtension);
            }

            // ユニークなパスを作成
            string fileName = FileIO.CreateUniquePath(System.IO.Path.Combine(downloadPath, name));

            try
            {
                // 保存
                using (var stream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
                {
                    stream.Write(buff, 0, buff.Length);
                }
            }
            catch (Exception e)
            {
                if (!System.IO.Directory.Exists(downloadPath)) throw new DropException(Properties.TextResources.GetString("Notice.OutputFailed") + "\n" + e.Message, e);
            }

            return fileName;
        }

#if false
        // ファイル名の修正
        private static string ValidateFileName(string name)
        {
            string DefaultName = DateTime.Now.ToString("yyyyMMddHHmmss");

            // nullの場合はデフォルト名
            name = name ?? DefaultName;

            // ファイル名として使用可能な文字列にする
            name = LoosePath.ValidFileName(name);

            // 拡張子取得
            string ext = System.IO.Path.GetExtension(name).ToLower();

            // 名前が長すぎる場合は独自名にする。64文字ぐらい？
            if (string.IsNullOrWhiteSpace(name) || name.Length > 64)
            {
                name = DefaultName + ext;
            }

            return name;
        }
#endif
    }


    /// <summary>
    /// Drop : FileContents
    /// </summary>
    public class DropFileContents : DropReciever
    {
        public override async Task<List<string>?> DropAsync(object sender, IDataObject data, string downloadPath, Action<string> nowloading)
        {
            //
            if (data.GetDataPresent("FileContents") && data.GetDataPresent("FileGroupDescriptorW"))
            {
                var fileNames = new List<string>();
                foreach (var file in FileContents.Get(data))
                {
                    if (file.Bytes == null || file.Bytes.Length <= 0) continue;

                    string? fileName = await DownloadToFileAsync(file.Bytes, file.Name, downloadPath);
                    if (fileName != null) fileNames.Add(fileName);
                }

                if (fileNames.Count > 0)
                {
                    return new List<string>() { fileNames[0] };
                }
            }

            return null;
        }
    }


    /// <summary>
    /// Drop : QueryPath
    /// </summary>
    public class DropQueryPath : DropReciever
    {
        public override async Task<List<string>?> DropAsync(object sender, IDataObject data, string downloadPath, Action<string> nowloading)
        {
            var query = data.GetData(typeof(QueryPath)) as QueryPath;
            if (query != null && query.Search == null && query.Scheme == QueryScheme.File)
            {
                return new List<string>() { query.SimplePath };
            }

            await Task.CompletedTask;
            return null;
        }
    }


    /// <summary>
    /// Drop : FileDrop
    /// </summary>
    public class DropFileDrop : DropReciever
    {
        public override async Task<List<string>?> DropAsync(object sender, IDataObject data, string downloadPath, Action<string> nowloading)
        {
            // File drop
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                if (data.GetData(DataFormats.FileDrop) is not string[] files)
                {
                    return new List<string>();
                }
                else if (files.Length <= 1)
                {
                    return new List<string>() { files[0] };
                }
                else
                {
                    return new List<string>(files);
                }
            }

            await Task.CompletedTask;
            return null;
        }
    }


    /// <summary>
    /// Drop: FileDrop to Copy
    /// </summary>
    public class DropFileDropCopy : DropReciever
    {
        public override async Task<List<string>?> DropAsync(object sender, IDataObject data, string downloadPath, Action<string> nowloading)
        {
            // File drop (from browser)
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                if (data.GetData(DataFormats.FileDrop) is string[] files)
                {
                    var fileNames = new List<string>();
                    foreach (var file in files)
                    {
                        // copy
                        var bytes = await Task.Run(() => System.IO.File.ReadAllBytes(file));

                        string? fileName = await DownloadToFileAsync(bytes, System.IO.Path.GetFileName(file), downloadPath);
                        if (fileName != null) fileNames.Add(fileName);
                    }
                    if (fileNames.Count > 0)
                    {
                        return new List<string>() { fileNames[0] };
                    }
                }
            }

            return null;
        }
    }


    /// <summary>
    /// Drop : Inline Image
    /// インラインデータ(base64)のみ処理
    /// </summary>
    public class DropInlineImage : DropReciever
    {
        public override async Task<List<string>?> DropAsync(object sender, IDataObject data, string downloadPath, Action<string> nowloading)
        {
            if (data.GetDataPresent("HTML Format"))
            {
                var fileNames = new List<string>();
                foreach (var url in HtmlString.CollectImgSrc(data.GetData("HTML Format").ToString()))
                {
                    //data:[<mediatype>][;base64],<data>
                    if (url.StartsWith("data:image/"))
                    {
                        // base64 to binary
                        const string keyword = "base64,";
                        var index = url.IndexOf(keyword);
                        if (index < 0) continue;  // base64の埋め込みのサポート
                        var crypt = url[(index + keyword.Length)..];
                        var bytes = Convert.FromBase64String(crypt);

                        // ファイル化
                        string? fileName = await DownloadToFileAsync(bytes, null, downloadPath);
                        if (fileName != null) fileNames.Add(fileName);
                    }
                }
                if (fileNames.Count > 0)
                {
                    return new List<string>() { fileNames[0] };
                }
            }

            return null;
        }
    }


    /// <summary>
    /// Drop : Download Image
    /// Webからダウンロードする
    /// </summary>
    public class DropWebImage : DropReciever
    {
        public override async Task<List<string>?> DropAsync(object sender, IDataObject data, string downloadPath, Action<string> nowloading)
        {
            // Webアクセス時はNowLoading表示を行う
            nowloading(Properties.TextResources.GetString("Notice.DropContent"));

            using (var client = new System.Net.Http.HttpClient())
            {
                // from HTML format
                if (data.GetDataPresent("HTML Format"))
                {
                    var fileNames = new List<string>();
                    foreach (var url in HtmlString.CollectImgSrc(data.GetData("HTML Format").ToString()))
                    {
                        if (url.StartsWith("http://") || url.StartsWith("https://"))
                        {
                            // download
                            var bytes = await client.GetByteArrayAsync(new Uri(url));

                            // ファイル化
                            string? fileName = await DownloadToFileAsync(bytes, null, downloadPath);
                            if (fileName != null) fileNames.Add(fileName);
                        }
                    }
                    if (fileNames.Count > 0)
                    {
                        return new List<string> { fileNames[0] };
                    }
                }

                // from Text
                if (data.GetDataPresent("UniformResourceLocator") || data.GetDataPresent("UniformResourceLocatorW"))
                {
                    var url = data.GetData(DataFormats.Text).ToString();
                    if (url is not null && (url.StartsWith("http://") || url.StartsWith("https://")))
                    {
                        // download
                        var bytes = await client.GetByteArrayAsync(new Uri(url));

                        // ファイル化
                        var result = DownloadToFile(bytes, null, downloadPath);
                        if (result is null) return null;

                        return new List<string>() { result };
                    }
                }

                return null;
            }
        }
    }


    /// <summary>
    /// Drop : Bitmap
    /// アルファ値はあきらめよ
    /// </summary>
    public class DropBitmap : DropReciever
    {
        public override async Task<List<string>?> DropAsync(object sender, IDataObject data, string downloadPath, Action<string> nowloading)
        {
            if (data.GetDataPresent(DataFormats.Bitmap))
            {
                if (data.GetData(DataFormats.Bitmap) is System.Windows.Interop.InteropBitmap bitmap)
                {
                    var name = DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";

                    // ユニークなパスを作成
                    string fileName = FileIO.CreateUniquePath(System.IO.Path.Combine(downloadPath, name));

                    // アルファ無効
                    var fixedBitmap = new FormatConvertedBitmap(bitmap, System.Windows.Media.PixelFormats.Bgr32, null, 0);

                    // フレーム作成
                    var frame = BitmapFrame.Create(fixedBitmap);
                    frame.Freeze();

                    // 一時ファイルとして保存
                    await Task.Run(() =>
                    {
                        using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(frame);
                            encoder.Save(fs);
                            fs.Close();
                        }
                    });

                    return new List<string>() { fileName };
                }
            }

            return null;
        }
    }
}

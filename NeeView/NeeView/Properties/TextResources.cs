﻿using NeeLaboratory.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace NeeView.Properties
{
    /// <summary>
    /// NeeView.Properties.Resources に代わるテキストリソース
    /// </summary>
    internal class TextResources
    {
        private static readonly Lazy<FileLanguageResource> _languageResource = new(() => new FileLanguageResource(Path.Combine(Environment.AssemblyFolder, "Languages")));
        private static readonly TextResourceExpand _accessor;
        private static bool _initialized;

        public static CultureInfo Culture => Resource.Culture;

        public static FileLanguageResource LanguageResource => _languageResource.Value;

        public static TextResourceManager Resource { get; } = new(LanguageResource);


        static TextResources()
        {
            // テキストリソース取得時にキー文字列サポートと再帰展開を行うアクセサ
            _accessor = new TextResourceExpand(new TextResourceWithInputGesture(Resource));
        }


        public static void Initialize(CultureInfo culture)
        {
            if (_initialized) return;
            _initialized = true;

            LoadCulture(culture);
        }

        public static void LoadCulture(CultureInfo culture)
        {
            Debug.WriteLine($"Load Culture: {culture}");

            Resource.Load(culture);

            // 開発用：テキストの重複チェック
            //CheckDuplicateText();

            Resource.Add(new AppFileSource(new Uri("/Languages/shared.restext", UriKind.Relative)));
            Resource.SetItem("_VersionTag", "");
        }

        /// <summary>
        /// 開発用：テキストの重複チェック
        /// </summary>
        [Conditional("DEBUG")]
        private static void CheckDuplicateText()
        {
            // テキスト重複項目を出力し、シェアテキストにするかの判断材料とする。
            // TODO: 全言語同時にチェックする必要があるので、これはツールで行ったほうがよい。

            var resolved = new List<string>();
            Debug.WriteLine("<ResourceText.Duplicate>");
            foreach (var item in Resource.Map)
            {
                resolved.Add(item.Key);
                var duplicates = Resource.Map.Where(e => e.Value.Text == item.Value.Text && !resolved.Contains(e.Key)).ToList();
                if (duplicates.Any())
                {
                    resolved.AddRange(duplicates.Select(e => e.Key));
                    Debug.WriteLine($"{item.Key}={item.Value.Text}");
                    foreach (var dup in duplicates)
                    {
                        Debug.WriteLine($"  {dup.Key}");
                    }
                }
            }
            Debug.WriteLine("</ResourceText.Duplicate>");
        }

        /// <summary>
        /// 最低限の初期化。設定ファイル読み込み前のエラー等の正常初期化前の処理用。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InitializeMinimum()
        {
            if (!_initialized)
            {
                Initialize(CultureInfo.CurrentCulture);
            }
        }

        public static string GetString(string name)
        {
            InitializeMinimum();
            // TODO: このレベルではフォールバックしない?
            return _accessor.GetResourceString(name)?.String ?? "@" + name;
        }

        public static string? GetStringRaw(string name)
        {
            InitializeMinimum();
            return _accessor.GetResourceString(name)?.String;
        }

        public static string GetCaseString(string name, string pattern)
        {
            InitializeMinimum();
            return _accessor.GetCaseResourceString(name, pattern)?.String ?? "@" + name;
        }

        public static string GetFormatString(string name, object? arg0)
        {
            var pattern = arg0?.ToString() ?? "";
            return string.Format(CultureInfo.InvariantCulture, GetCaseString(name, pattern), arg0);
        }

        public static string? Replace(string s, bool fallback)
        {
            InitializeMinimum();
            return _accessor.Replace(s, fallback);
        }
    }
}

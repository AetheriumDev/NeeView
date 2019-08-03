﻿using Microsoft.Win32;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Explorerのコンテキストメニューに「NeeViewを開く」コマンドを追加。
    /// ユーザー単位の設定になる(HKCU)。
    /// Zip版のみの機能で、インストーラー版はインストール時にHKLMに設定される。
    /// </summary>
    public class ExplorerContextMenu
    {
        static ExplorerContextMenu() => Current = new ExplorerContextMenu();
        public static ExplorerContextMenu Current { get; }

        private const string _keyName = "OpenInNeeView";
        private const string _keyFile = @"*\shell\" + _keyName;
        private const string _keyDirectory = @"Directory\shell\" + _keyName;
        private const string _keyDirectoryBackground = @"Directory\Background\shell\" + _keyName;

        private RegistryKey _root;
        private bool _isEnabled;


        public ExplorerContextMenu()
        {
            _root = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);
            _isEnabled = Exists();
        }


        [PropertyMember("@ParamIsExplorerContextMenuEnabled", Tips = "@ParamIsExplorerContextMenuEnabledTips")]
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (value != _isEnabled)
                {
                    if (value)
                    {
                        Create();
                    }
                    else
                    {
                        Delete();
                    }

                    _isEnabled = Exists();
                }
            }
        }


        private bool Exists()
        {
            return IsExistSubKey(_keyFile);
        }

        public void Create()
        {
            var commandName = "\"" + Assembly.GetEntryAssembly().Location + "\"";
            var label = Properties.Resources.ResourceManager.GetString("WordOpenInNeeView", CultureInfo.CurrentCulture);
            var icon = commandName + ",0";
            var command = commandName + " \"%1\"";
            var commandTypeV = commandName + " \"%V\"";

            CreateSubKey(_keyFile, label, icon, command);
            CreateSubKey(_keyDirectory, label, icon, command);
            CreateSubKey(_keyDirectoryBackground, label, icon, commandTypeV);
        }

        public void Delete()
        {
            DeleteSubKey(_keyFile);
            DeleteSubKey(_keyDirectory);
            DeleteSubKey(_keyDirectoryBackground);
        }

        private bool IsExistSubKey(string keyName)
        {
            try
            {
                var key = _root.OpenSubKey(keyName);
                return key != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private void CreateSubKey(string key, string label, string icon, string command)
        {
            try
            {
                using (var reg = _root.CreateSubKey(key, true))
                {
                    reg.SetValue(null, label);
                    reg.SetValue("Icon", icon);
                }
                using (var reg = _root.CreateSubKey(key + @"\command"))
                {
                    reg.SetValue(null, command);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void DeleteSubKey(string key)
        {
            try
            {
                _root.DeleteSubKeyTree(key);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

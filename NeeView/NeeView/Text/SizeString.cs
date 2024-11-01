﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Text
{
    /// <summary>
    /// "数値x数値" という文字列で数値を表す
    /// </summary>
    public class SizeString
    {
        /// <summary>
        /// フォーマット正規表現
        /// </summary>
        private readonly Regex _regex = new(@"^(\d+)x(\d+)$");

        private string _value;


        public SizeString(string value)
        {
            SetValue(value);
        }


        /// <summary>
        /// "数値x数値"
        /// </summary>
        public string Value
        {
            get { return _value; }
            private set
            {
                if (_value != value)
                {
                    SetValue(value);
                }
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }


        [MemberNotNull(nameof(_value))]
        private void SetValue(string value)
        {
            _value = value;

            var match = _regex.Match(this.Value);
            if (!match.Success) throw new ArgumentException("wrong value format.");
            this.Width = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            this.Height = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 有効判定
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return IsValid(this.Value);
        }

        /// <summary>
        /// 有効判定
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return (_regex.IsMatch(value));
        }

        /// <summary>
        /// 正規化。無効の場合はdefaultValueを適用
        /// </summary>
        /// <param name="defaultValue"></param>
        public void Verify(string defaultValue)
        {
            Debug.Assert(IsValid(defaultValue));

            if (!IsValid())
            {
                this.Value = defaultValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Size ToSize()
        {
            return new Size(Width, Height);
        }
    }
}

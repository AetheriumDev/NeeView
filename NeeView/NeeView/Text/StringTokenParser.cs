﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView.Text
{
    /// <summary>
    /// 自然ソート用に数値の概念を取り入れたEnumerator
    /// </summary>
    public class StringTokenParser : IEnumerable<StringToken>
    {
        private readonly StringNormalizeParser _parser;

        public StringTokenParser(string source)
        {
            _parser = new StringNormalizeParser(source);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

#nullable disable
        // TODO: numsFactoryのインスタンス生成のあつかいを再検討。処理負荷に影響しそう。
        public IEnumerator<StringToken> GetEnumerator()
        {
            int state = 0;
            NumsTokenFactory numsFactory = null;

            foreach (var c in _parser)
            {
                switch (state)
                {
                    case 0:
                        if (KanaEmbedded.IsDigit(c))
                        {
                            numsFactory = new NumsTokenFactory();
                            numsFactory.Add(c);
                            state = 1;
                        }
                        else
                        {
                            yield return new StringToken(c);
                        }
                        break;

                    case 1:
                        if (KanaEmbedded.IsDigit(c))
                        {
                            numsFactory.Add(c);
                        }
                        else if (c == '.')
                        {
                            state = 2;
                        }
                        else
                        {
                            numsFactory.Determine();
                            yield return numsFactory.ToStringToken();
                            yield return new StringToken(c);
                            state = 0;
                        }
                        break;

                    case 2:
                        if (KanaEmbedded.IsDigit(c))
                        {
                            numsFactory.Determine('.');
                            numsFactory.Add(c);
                            state = 1;
                        }
                        else
                        {
                            numsFactory.Determine();
                            yield return numsFactory.ToStringToken();
                            yield return new StringToken('.');
                            yield return new StringToken(c);
                            state = 0;
                        }
                        break;
                }
            }

            // terminate
            switch (state)
            {
                case 0:
                    break;

                case 1:
                    numsFactory.Determine();
                    yield return numsFactory.ToStringToken();
                    break;

                case 2:
                    numsFactory.Determine();
                    yield return numsFactory.ToStringToken();
                    yield return new StringToken('.');
                    break;
            }
        }
#nullable restore

        /// <summary>
        /// 数値のStringToken生成用
        /// </summary>
        public class NumsTokenFactory
        {
            private long _n;
            private readonly List<long> _nums = new(4);
            private readonly List<char> _chars = new(8);

            public void Clear()
            {
                _n = 0;
                _nums.Clear();
                _chars.Clear();
            }

            public void Add(char c)
            {
                Debug.Assert(KanaEmbedded.IsDigit(c));
                _chars.Add(c);
                _n = _n * 10 + (c - '0');
            }

            public void Determine()
            {
                _nums.Add(_n);
                _n = 0;
            }

            public void Determine(char c)
            {
                Debug.Assert(c == '.');
                _chars.Add(c);
                _nums.Add(_n);
                _n = 0;
            }

            public StringToken ToStringToken()
            {
                return new StringToken(_chars, _nums);
            }
        }

    }

}

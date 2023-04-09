using System;
using System.Collections;
using System.Collections.Generic;

namespace CommonCode
{
    [Flags]
    // ReSharper disable InconsistentNaming
    public enum eParseStringOptions
    {
        None = 0,
        AddLastEmptyToken = 1,
        PossibleEmptyTokens = 2
    }

    public class TokenList : IEnumerable<ParseString.Token>
    {
        private ParseString.Token[] myList;

        private TokenList(ParseString.Token[] list)
        {
            myList = list;
        }

        public static implicit operator TokenList(ParseString.Token[] list)
        {
            return new TokenList(list);
        }

        public int Length
        {
            get { return myList.Length; }
        }

        public void Resize(int newSize)
        {
            Array.Resize(ref myList, newSize);

            for (int i = 0; i < myList.Length; i++)
            {
                if (myList[i] == null)
                {
                    myList[i] = new ParseString.Token(string.Empty, i == 0 ? 0 : myList[i - 1].LastPos);
                }
            }
		}

        public ParseString.Token this[int i]
        {
            get { return myList[i]; }
            set { myList[i] = value; }
        }

        public string[] ToArray()
        {
            string[] retValue = new string[Length];
            for (int i = 0; i < Length; i++)
            {
                retValue[i] = this[i].Text;
            }

            return retValue;
        }

        #region Implementation of IEnumerable<out Token>

        public IEnumerator<ParseString.Token> GetEnumerator()
        {
            return ((IEnumerable<ParseString.Token>) myList).GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public static class ParseString
    {
        public class Token
        {
            public Token()
            {
                Text = string.Empty;
                Pos = -1;
            }

            public Token(string text, int pos)
            {
                Text = text;
                Pos = pos;
            }

            public override string ToString()
            {
                return Text;
            }

            public static implicit operator string(Token token)
            {
                return token.Text;
            }

            public string Text;
            public int Pos;

            public int Len
            {
                get { return Text.Length; }
            }

            public int LastPos
            {
                get { return Pos + Len; }
            }
        }

        public static TokenList Parse(Token srcToken, params char[] delimiters)
        {
            TokenList resultTokens = Parse(srcToken.Text, delimiters);

            for (int i = 0; i < resultTokens.Length; i++)
            {
                resultTokens[i].Pos += srcToken.Pos;
            }

            return resultTokens;
        }

        public static TokenList Parse(string srcString, params char[] delimiters)
        {
            return Parse(srcString, eParseStringOptions.None, delimiters);
        }

        public static TokenList Parse(string srcString, eParseStringOptions options, params char[] delimiters)
        {
            bool inWord = false;
            bool inQuotation = false;
            bool hasInQuotation = false;

            if (delimiters.Length == 0)
            {
                delimiters = new char[] { ' ', '\t' };
            }

            if (string.IsNullOrEmpty(srcString))
            {
                return new Token[] {new Token(string.Empty, 0)};
            }

            List<Token> result = new List<Token>();
            result.Add(new Token());

            int resultLength = 1;

            int i;
            for (i = 0; i < srcString.Length; i++)
            {
                char currChar = srcString[i];

                if (Array.IndexOf(delimiters, currChar) == -1 || currChar == '"' || inQuotation)
                {
                    if (!inWord && !inQuotation)
                    {
                        result[resultLength - 1].Pos = i;
                    }

                    result[resultLength - 1].Text = result[resultLength - 1].Text + currChar;

                    if (currChar == '"')
                    {
                        hasInQuotation = inQuotation;
                        inQuotation = !inQuotation;
                    }
                    else
                    {
                        if (!inQuotation)
                        {
                            inWord = true;
                        }
                    }
                }
                else
                {
                    if (inWord || hasInQuotation || (options & eParseStringOptions.PossibleEmptyTokens) == eParseStringOptions.PossibleEmptyTokens)
                    {
                        if (result[resultLength - 1].Pos == -1)
                        {
                            result[resultLength - 1].Pos = i;
                        }

                        result.Add(new Token());
                        resultLength++;

                        inWord = false;
                        hasInQuotation = false;
                    }
                }
            }

            if (srcString.Length > 0 && result[resultLength - 1].Text.Length == 0)
            {
                if ((options & eParseStringOptions.AddLastEmptyToken) == eParseStringOptions.AddLastEmptyToken)
                {
                    result[resultLength - 1].Pos = srcString.Length;
                }
                else
                {
                    result.RemoveAt(resultLength - 1);
                }
            }

            return result.Count != 0
                       ? result.ToArray()
                       : new Token[] {new Token(string.Empty, 0)};
        }
    }
}
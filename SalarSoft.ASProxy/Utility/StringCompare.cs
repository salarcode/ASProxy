//**************************************************************************
// Product Name: ASProxy
// Description:  SalarSoft ASProxy is a free and open-source web proxy
// which allows the user to surf the net anonymously.
//                     
// MPL 1.1 License agreement.
// The contents of this file are subject to the Mozilla Public License
// Version 1.1 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS"
// basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
// License for the specific language governing rights and limitations
// under the License.
// 
// The Original Code is ASProxy (an ASP.NET Proxy).
// 
// The Initial Developer of the Original Code is Salar.Kh (SalarSoft).
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.Kh < salarsoftwares [@] gmail[.]com > (original author)
//
//**************************************************************************

using System.Globalization;
using System.Threading;

namespace SalarSoft.ASProxy
{

    /// <summary>
    /// <para>This class provides a collection of best functions and methods for application hight performance.</para>
    /// <remarks>Important: IgnoreCase methods are always slower than MatchCase methods, because of thier mechanism.</remarks>
    /// </summary>
    public class StringCompare
    {
        private static CompareInfo __CompareInfo = null;

        static StringCompare()
        {
            __CompareInfo = Thread.CurrentThread.CurrentCulture.CompareInfo;
        }

        #region prefix suffix
        public static bool StartsWithIgnoreCase(ref string source, string prefix)
        {
            return __CompareInfo.IsPrefix(source, prefix, CompareOptions.IgnoreCase);
        }
        public static bool StartsWithMatchCase(ref string source, string prefix)
        {
            return __CompareInfo.IsPrefix(source, prefix, CompareOptions.None);
        }
        public static bool EndsWithIgnoreCase(ref string source, string suffix)
        {
            return __CompareInfo.IsSuffix(source, suffix, CompareOptions.IgnoreCase);
        }
        public static bool EndsWithMatchCase(ref string source, string suffix)
        {
            return __CompareInfo.IsSuffix(source, suffix, CompareOptions.None);
        }
        #endregion

        #region IndexOf a string with IgnoreCase option
        public static int IndexOfIgnoreCase(ref string source, string text, int start)
        {
            return __CompareInfo.IndexOf(source, text, start, source.Length - start, CompareOptions.IgnoreCase);
        }
        public static int IndexOfIgnoreCase(ref string source, string text)
        {
            return __CompareInfo.IndexOf(source, text, 0, source.Length, CompareOptions.IgnoreCase);
        }
        public static int IndexOfIgnoreCase(ref string source, char character, int start)
        {
            return __CompareInfo.IndexOf(source, character, start, source.Length - start, CompareOptions.IgnoreCase);
        }
        public static int IndexOfIgnoreCase(ref string source, char character)
        {
            return __CompareInfo.IndexOf(source, character, 0, source.Length, CompareOptions.IgnoreCase);
        }
        #endregion

        #region IndexOf a string without IgnoreCase option
        public static int IndexOfMatchCase(ref string source, string text, int start, int count)
        {
            return __CompareInfo.IndexOf(source, text, start, count, CompareOptions.None);
        }
        public static int IndexOfMatchCase(ref string source, string text, int start)
        {
            return __CompareInfo.IndexOf(source, text, start, source.Length - start, CompareOptions.None);
        }
        public static int IndexOfMatchCase(ref string source, string text)
        {
            return __CompareInfo.IndexOf(source, text, 0, source.Length, CompareOptions.None);
        }
        public static int IndexOfMatchCase(ref string source, char character, int start, int count)
        {
            return __CompareInfo.IndexOf(source, character, start, count, CompareOptions.None);
        }
        public static int IndexOfMatchCase(ref string source, char character, int start)
        {
            return __CompareInfo.IndexOf(source, character, start, source.Length - start, CompareOptions.None);
        }
        public static int IndexOfMatchCase(ref string source, char character)
        {
            return __CompareInfo.IndexOf(source, character, 0, source.Length, CompareOptions.None);
        }
        #endregion

        #region LastIndexOf a string with IgnoreCase option
        public static int LastIndexOfIgnoreCase(ref string source, string text, int start)
        {
            return __CompareInfo.LastIndexOf(source, text, start, start + 1, CompareOptions.IgnoreCase);
        }
        public static int LastIndexOfIgnoreCase(ref string source, string text)
        {
            return __CompareInfo.LastIndexOf(source, text, source.Length - 1, source.Length, CompareOptions.IgnoreCase);
        }
        public static int LastIndexOfIgnoreCase(ref string source, char character, int start)
        {
            return __CompareInfo.LastIndexOf(source, character, start, start + 1, CompareOptions.IgnoreCase);
        }
        public static int LastIndexOfIgnoreCase(ref string source, char character)
        {
            return __CompareInfo.LastIndexOf(source,
                       character,
                       source.Length - 1,
                       source.Length,
                       CompareOptions.IgnoreCase);
        }
        #endregion

        #region LastIndexOf a string without IgnoreCase option
        public static int LastIndexOfMatchCase(ref string source, string text, int start)
        {
            return __CompareInfo.LastIndexOf(source,
                       text,
                       start,
                       source.Length - start,
                       CompareOptions.None);
        }
        public static int LastIndexOfMatchCase(ref string source, string text)
        {
            return __CompareInfo.LastIndexOf(source, text, 0, source.Length, CompareOptions.None);
        }
        public static int LastIndexOfMatchCase(ref string source, char character, int start)
        {
            return __CompareInfo.LastIndexOf(source, character, start, start, CompareOptions.None);
        }
        public static int LastIndexOfMatchCase(ref string source, char character)
        {
            return __CompareInfo.LastIndexOf(source, character, 0, source.Length, CompareOptions.None);
        }
        #endregion

		internal static bool IsAlphabet(string text)
		{
			text = text.Trim();
			foreach (char c in text)
			{
				if (!char.IsLetter(c))
					return false;
			}
			return true;
		}
		internal static bool IsMixedCharacters(string text)
		{
			foreach (char c in text)
			{
				if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
					return true;
			}
			return false;
		}
	}
}
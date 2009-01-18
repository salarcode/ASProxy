using System.Globalization;
using System.Threading;

namespace SalarSoft.ASProxy
{

    /// <summary>
    /// <para>This class provides a collection of best functions and methods for application hight performance.</para>
    /// <remarks>Important: IgnoreCase methods always are slower than MatchCase methods, because of thier mechanism.</remarks>
    /// </summary>
    public class Performance
    {
        private static CompareInfo __CompareInfo = null;
        
        static Performance()
        {
            __CompareInfo = Thread.CurrentThread.CurrentCulture.CompareInfo;
        }

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
		public static int IndexOfMatchCase(ref string source, char character, int start,int count)
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
            return __CompareInfo.LastIndexOf(source, text, start,start+1, CompareOptions.IgnoreCase);
        }
        public static int LastIndexOfIgnoreCase(ref string source, string text)
        {
			return __CompareInfo.LastIndexOf(source, text, source.Length - 1, source.Length, CompareOptions.IgnoreCase);
        }
        public static int LastIndexOfIgnoreCase(ref string source, char character, int start)
        {
            return __CompareInfo.LastIndexOf(source, character, start, start+1, CompareOptions.IgnoreCase);
        }
        public static int LastIndexOfIgnoreCase(ref string source, char character)
        {
			return __CompareInfo.LastIndexOf(source, character, source.Length - 1, source.Length, CompareOptions.IgnoreCase);
        }
        #endregion

		#region LastIndexOf a string without IgnoreCase option
		public static int LastIndexOfMatchCase(ref string source, string text, int start)
        {
            return __CompareInfo.LastIndexOf(source, text, start, source.Length - start, CompareOptions.None);
        }
        public static int LastIndexOfMatchCase(ref string source, string text)
        {
            return __CompareInfo.LastIndexOf(source, text, 0, source.Length, CompareOptions.None);
        }
        public static int LastIndexOfMatchCase(ref string source, char character, int start)
        {
            return __CompareInfo.LastIndexOf(source, character, start,  start, CompareOptions.None);
        }
        public static int LastIndexOfMatchCase(ref string source, char character)
        {
            return __CompareInfo.LastIndexOf(source, character, 0, source.Length, CompareOptions.None);
        }
        #endregion
    }
}
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

using System;
using System.Text;

namespace SalarSoft.ASProxy
{
	public static class WebMethods
	{
		public enum DefaultMethods
		{
			GET = 0,
			POST = 1,
			PUT = 2,
			DELETE = 3,
			TRACE = 4,
			CONNECT = 5,
			OPTIONS = 6
		}

		public const string GET = "GET";
		public const string POST = "POST";
		public const string PUT = "PUT";
		public const string DELETE = "DELETE";
		public const string TRACE = "TRACE";
		public const string CONNECT = "CONNECT";
		public const string OPTIONS = "OPTIONS";


		public static string ValidateMethod(string method, DefaultMethods defaultMethod)
		{
			if (method == null) return method;
			string testMethod = method.Trim().ToUpper();

			if (testMethod == GET)
				return method;
			else if (testMethod == POST)
				return method;
			else if (testMethod == PUT)
				return method;
			else if (testMethod == DELETE)
				return method;
			else if (testMethod == TRACE)
				return method;
			else if (testMethod == CONNECT)
				return method;
			else if (testMethod == OPTIONS)
				return method;
			else
				return defaultMethod.ToString();
		}

		public static string OmitInvalidCharacters(string method)
		{
			StringBuilder strBuilder=new StringBuilder();
			for (int i = 0; i < method.Length; i++)
			{
				char c = method[i];
                if (char.IsLetter(c))
                    strBuilder.Append(c);
            }
			return strBuilder.ToString();
		}

		/// <summary>
		/// Checks web request method type if is POST or not
		/// </summary>
		public static bool IsPostMethod(string method)
		{
			if (method == null) return false;
			if (method.Trim().ToUpper() == POST)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks web request method type
		/// </summary>
		public static bool IsMethod(string method, DefaultMethods methodType)
		{
			if (method == null) return false;
			string t = methodType.ToString().ToLower();
			if (method.Trim().ToLower() == t)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Detects method by starting characters
		/// </summary>
		public static string DetectMethod(string method, DefaultMethods defaultMethods)
		{
			if (method == null) return method;
			string testMethod = method.Trim().ToUpper();

			if (GET.StartsWith(testMethod))
				return GET;
			else if (POST.StartsWith(testMethod))
				return POST;
			else if (PUT.StartsWith(testMethod))
				return PUT;
			else if (DELETE.StartsWith(testMethod))
				return DELETE;
			else if (TRACE.StartsWith(testMethod))
				return TRACE;
			else if (CONNECT.StartsWith(testMethod))
				return CONNECT;
			else if (OPTIONS.StartsWith(testMethod))
				return OPTIONS;
			else
				return defaultMethods.ToString();
		}
	}
}
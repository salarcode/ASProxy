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
// The Initial Developer of the Original Code is Salar.K.
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.K https://github.com/salarcode (original author)
//
//**************************************************************************

using System;
using System.IO.Compression;
using System.Web;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class HttpCompressor
	{
		public static void ApplyCompression(MimeContentType contentType)
		{
			// only text contents
			switch (contentType)
			{
				case MimeContentType.text_html:
				case MimeContentType.text_plain:
				case MimeContentType.text_css:
				case MimeContentType.text_javascript:
					ApplyCompression(HttpContext.Current.Request, HttpContext.Current.Response);
					break;

				case MimeContentType.image_jpeg:
				case MimeContentType.image_gif:
				case MimeContentType.application:
				default:
					break;
			}
		}

		public static void ApplyCompression()
		{
			ApplyCompression(HttpContext.Current.Request, HttpContext.Current.Response);
		}

		public static void ApplyCompression(HttpRequest Request, HttpResponse Response)
		{
			if (IsCompressEnabled(Request) == false)
				return;

			// load encodings from header
			QValueList encodings = new QValueList(Request.Headers["Accept-Encoding"]);

			// get the types we can handle, can be accepted and
			// in the defined client preference
			QValue preferred = encodings.FindPreferred("gzip", "deflate", "identity");

			// if none of the preferred values were found, but the
			// client can accept wildcard encodings, we'll default
			// to Gzip.
			if (preferred.IsEmpty && encodings.AcceptWildcard && encodings.Find("gzip").IsEmpty)
				preferred = new QValue("gzip");


			// handle the preferred encoding
			switch (preferred.Name)
			{
				case "gzip":
					Response.AppendHeader("Content-Encoding", "gzip");
					Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
					AddToCookie(Response, Request, "gzip");
					break;
				case "deflate":
					Response.AppendHeader("Content-Encoding", "deflate");
					Response.Filter = new DeflateStream(Response.Filter, CompressionMode.Compress);
					AddToCookie(Response, Request, "deflate");
					break;
				case "identity":
				default:
					break;
			}
		}

		static bool IsCompressEnabled(HttpRequest request)
		{
			HttpCookie cookie = request.Cookies[Consts.FrontEndPresentation.UserOptionsCookieName];
			if (cookie == null)
				return false;

			string compress = cookie["HttpCompression"];

			if (string.IsNullOrEmpty(compress))
				return false;

			try
			{
				return (Convert.ToBoolean(compress));
			}
			catch
			{
				return false;
			}
		}

		static void AddToCookie(HttpResponse response, HttpRequest request, string encode)
		{
			HttpCookie reqCookie = request.Cookies[Consts.FrontEndPresentation.UserOptionsCookieName];
			if (reqCookie != null)
			{
				string currentEncoding = reqCookie[Consts.FrontEndPresentation.HttpCompressEncoding];
				if (!string.IsNullOrEmpty(currentEncoding) && currentEncoding.ToLower() == encode.ToLower())
				{
					// already saved
					return;
				}
			}


			HttpCookie resCookie = response.Cookies[Consts.FrontEndPresentation.HttpCompressor];
			if (resCookie == null)
			{
				resCookie = new HttpCookie(Consts.FrontEndPresentation.HttpCompressEncoding);
				response.Cookies.Add(resCookie);
			}
			resCookie.Values.Add(Consts.FrontEndPresentation.HttpCompressEncoding, encode);
		}


	}
}
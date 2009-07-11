using System;
using System.IO.Compression;
using System.Web;

namespace SalarSoft.ASProxy
{
	public class HttpCompressor : IHttpModule
	{

		public void Dispose()
		{
		}

		public void Init(HttpApplication context)
		{
			context.BeginRequest += new EventHandler(context_BeginRequest);
		}

		void AddToCookie(HttpResponse response, string encode)
		{
			HttpCookie cookie = response.Cookies[Consts.FrontEndPresentation.HttpCompressorCookieName];
			if (cookie == null)
			{
				cookie = new HttpCookie(Consts.FrontEndPresentation.HttpCompressorCookieName);
				response.Cookies.Add(cookie);
			}
			cookie.Values.Add(Consts.FrontEndPresentation.HttpCompressEncoding, encode);
		}

		bool IsCompressEnabled(HttpRequest request)
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


		void ApplyCompression(HttpRequest Request, HttpResponse Response)
		{
			if (IsCompressEnabled(Request) == false)
				return;

			/// load encodings from header
			QValueList encodings = new QValueList(Request.Headers["Accept-Encoding"]);

			/// get the types we can handle, can be accepted and
			/// in the defined client preference
			QValue preferred = encodings.FindPreferred("gzip", "deflate", "identity");

			/// if none of the preferred values were found, but the
			/// client can accept wildcard encodings, we'll default
			/// to Gzip.
			if (preferred.IsEmpty && encodings.AcceptWildcard && encodings.Find("gzip").IsEmpty)
				preferred = new QValue("gzip");

			// handle the preferred encoding
			switch (preferred.Name)
			{
				case "gzip":
					Response.AppendHeader("Content-Encoding", "gzip");
					Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
					AddToCookie(Response, "gzip");
					break;
				case "deflate":
					Response.AppendHeader("Content-Encoding", "deflate");
					Response.Filter = new DeflateStream(Response.Filter, CompressionMode.Compress);
					AddToCookie(Response, "deflate");
					break;
				case "identity":
				default:
					break;
			}

		}

		void context_BeginRequest(object sender, EventArgs e)
		{
			HttpApplication app = (HttpApplication)sender;
			HttpRequest request = app.Request;
			HttpResponse response = app.Response;

			ApplyCompression(request, response);

		}
	}
}
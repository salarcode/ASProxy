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

		/// <summary>
		/// Add 
		/// </summary>
		/// <param name="response"></param>
		/// <param name="encode"></param>
        void AddToCookie(HttpResponse response, string encode)
        {
            HttpCookie cookie = response.Cookies[GlobalConsts.HttpCompressorCookieMasterName];
            if (cookie == null)
            {
                cookie = new HttpCookie(GlobalConsts.HttpCompressorCookieMasterName);
                response.Cookies.Add(cookie);
            }
            cookie.Values.Add("CompressEncoding", encode);
        }

        bool compressHeader(HttpRequest request)
        {
            HttpCookie cookie = request.Cookies[GlobalConsts.CookieMasterName];
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
                return true;
            }
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            HttpRequest request = app.Request;

            if (compressHeader(request) == false)
                return;

            String acceptEncoding = request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(acceptEncoding))
                return;
            acceptEncoding = acceptEncoding.ToLower();


            if (acceptEncoding.Contains("gzip"))
            {
				// Compression with GZIP
                app.Response.AddHeader("Content-Encoding", "gzip");
                app.Response.Filter = new GZipStream(app.Response.Filter, CompressionMode.Compress);
                AddToCookie(app.Response, "gzip");
            }
            else if (acceptEncoding.Contains("deflate"))
            {
				// Compression with DEFLATE
                app.Response.AddHeader("Content-Encoding", "deflate");
                app.Response.Filter = new DeflateStream(app.Response.Filter, CompressionMode.Compress);
                AddToCookie(app.Response, "deflate");
            }
        }
    }
}
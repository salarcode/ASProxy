using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using SalarSoft.ASProxy.General;

namespace SalarSoft.ASProxy
{
    public class UrlProvider
    {
        public static string UrlPathEncodeSpecial(string str)
        {
            str = HttpUtility.UrlPathEncode(str);
            str = str.Replace("#", "%23");
            return str;
        }



        public static string UrlEncodeSpecialChars(string str)
        {
            //str = UrlEncode(str);
            str = str.Replace("!", "%21");
            str = str.Replace("*", "%2A");
            str = str.Replace("(", "%28");
            str = str.Replace(")", "%29");
            str = str.Replace("-", "%2D");
            str = str.Replace(".", "%2E");
            str = str.Replace("_", "%5F");
            str = str.Replace(@"\", "%5C");
            return str;
        }





        public static void ParseASProxyPageUrl(NameValueCollection collection, out bool decode, out string url)
        {
            decode = true;
            url = "";
            try
            {
                string str = collection[Consts.Query.UrlAddress];
                if (!string.IsNullOrEmpty(str))
                {
                    url = str;
                    object obj = collection[Consts.Query.Decode];
                    if (obj != null)
                    {
                        try
                        {
                            decode = Convert.ToBoolean(Convert.ToInt32(obj));
                        }
                        catch
                        {
                            decode = false;
                        }
                    }
                    else
                        decode = false;
                }
            }
            catch
            {
                url = "";
                decode = false;
            }
        }

        public static string GetASProxyPageUrl(string asproxyPage, string url, bool encodeUrl)
        {
            if (encodeUrl)
                url = EncodeUrl(url);
            return String.Format("{0}?{1}={2}&{3}={4}",
                       asproxyPage,
                       Consts.Query.Decode,
                       Convert.ToSByte(encodeUrl),
                       Consts.Query.UrlAddress,
                       url);
        }


        public static string GetAppAbsolutePath()
        {
            HttpRequest request = HttpContext.Current.Request;
            string result = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath;

            // BUGFIX 5.1: app path should end with slash character (/)
            // Occurs only with subdirectories.
            char last = result[result.Length - 1];
            if (last != '\\' && last != '/')
                result += '/';

            return result;
        }

        public static string GetAppAbsoluteBasePath()
        {
            HttpRequest request = HttpContext.Current.Request;
            return request.Url.Scheme + "://" + request.Url.Authority;
        }

        public static string GetAppAbsolutePath(Uri url)
        {
            return url.Scheme + "://" + url.Authority;
        }

        public static string CorrectInputUrl(string url)
        {
            if (url.IndexOf("://") == -1)
                url = "http://" + url;
            return url;
        }

        public static bool IsASProxyAddressUrlIncluded(NameValueCollection collection)
        {
            string str = collection[Consts.Query.UrlAddress];
            if (string.IsNullOrEmpty(str))
                return false;
            else
                return true;
        }


        static string[] _ClientSideUrls = new string[] { "mailto:", "file://", "javascript:", "vbscript:", "jscript:", "vbs:", "ymsgr:", "data:" };
        public static bool IsClientSitdeUrl(string path)
        {
            path = path.ToLower();
            for (int i = 0; i < _ClientSideUrls.Length; i++)
            {
                if (StringCompare.StartsWithMatchCase(ref path, _ClientSideUrls[i]))
                    return true;
                //if (path.StartsWith(stdurl[i]))
                //    return true;
            }
            return false;
        }

        public static bool IsJavascriptUrl(string path)
        {
            if (StringCompare.StartsWithMatchCase(ref path, "javascript:"))
                return true;
            return false;
        }

        static string[] _NonVirtualUrls = new string[] { "http://", "https://", "mailto:", "ftp://", "file://", "telnet://", "news://", "nntp://", "ldap://", "ymsgr:", "javascript:", "vbscript:", "jscript:", "vbs:", "data:" };
        public static bool IsVirtualUrl(string path)
        {
            path = path.ToLower();
            for (int i = 0; i < _NonVirtualUrls.Length; i++)
            {
                if (StringCompare.StartsWithMatchCase(ref path, _NonVirtualUrls[i]))
                    return false;
                //if (path.StartsWith(stdurl[i]))
                //    return false;
            }
            return true;
        }

        public static string IgnoreInvalidUrlCharctersInHtml(string htmlurl)
        {
            htmlurl = htmlurl.Replace("\n", string.Empty);
            htmlurl = htmlurl.Replace("\r", string.Empty);
            htmlurl = htmlurl.Replace("\t", string.Empty);
            htmlurl = htmlurl.Replace("/./", "/");
            return htmlurl;
        }

        /// <summary>
        /// Add a slash to end of url addresses. http://google.com --> http://google.com/
        /// </summary>
        /// <returns>Slash ended url</returns>
        public static string AddSlashToEnd(string url)
        {
            if (url.Length > 0)
            {
                if (url[url.Length - 1] != '\\' && url[url.Length - 1] != '/')
                {
                    return url + '/';
                }
            }
            return url;
        }

        public static string JoinUrl(string path1, string path2)
        {
            if (!IsVirtualUrl(path2))
                return path1;

            path1 = AddSlashToEnd(path1);

            if (path2.Length > 0)
            {
                if (path2[0] == '/')
                    return path1 + '.' + path2;
            }

            return path1 + path2;
        }

        public static string JoinUrl(string query, string pageUrlWithoutParameters, string pageBasePath, string siteBasePath)
        {
            // chiching page parameter
            if (query.Length == 0)
                return pageBasePath;

            // If the query contains only parameter
            if (query[0] == '?')
            {
                return UrlBuilder.AppendAntoherQueries(pageUrlWithoutParameters, query);// Safe but slow
                //return pageUrlWithoutParameters + query; //
            }

            string tmpPage = query.ToLower();

            // check if the url is a foreign site url like this: http://www.google.com
            if (!IsVirtualUrl(tmpPage))
                return query;
            else
            {
                pageBasePath = AddSlashToEnd(pageBasePath);
                siteBasePath = AddSlashToEnd(siteBasePath);

                // if "page" is a base url like this: /Test/default.aspx
                if (query[0] == '/')
                    return siteBasePath + '.' + query;

                // if "page" is a virtual url like this: SalarSoft/default.aspx
                return pageBasePath + query;
            }
        }

        /// <summary>
        /// Gets URL site base path. E.G. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/dir1/dir2/
        /// </summary>
        /// <returns>E.G. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/dir1/dir2/</returns>
        public static string GetPagePath(string url)
        {
            Uri address = new Uri(url);
            string result = address.Scheme + "://" + address.Authority;

            // Don't add last segment
            for (int i = 0; i < address.Segments.Length - 1; i++)
            {
                // adding segments
                result += address.Segments[i];
            }

            if (address.Segments.Length > 0)
            {
                // add if only the last segment is a directory!!!!!
                string last = address.Segments[address.Segments.Length - 1];

                // is a directory?
                if (last[last.Length - 1] == '/')
                    result += last;
            }

            // the end slash ensure
            // unnessacery, but needed for maintenance
            if (result[result.Length - 1] != '/')
                result += '/';

            return result;
        }

        /// <summary>
        /// Gets url page full path without parameter queries
        /// </summary>
        public static string GetPageAbsolutePath(string url)
        {
            Uri address = new Uri(url);
            return address.Scheme + "://" + address.Authority + address.AbsolutePath;
        }


        /// <summary>
        /// Gets URL site base path. e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/
        /// </summary>
        /// <returns>e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/</returns>
        public static string GetRootPath(string url)
        {
            Uri uri = new Uri(url);
            return uri.Scheme + "://" + uri.Authority + uri.Segments[0];
        }

        /// <summary>
        /// Gets URL site base path. e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/
        /// </summary>
        /// <returns>e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/</returns>
        public static string GetRootPath(Uri uri)
        {
            return uri.Scheme + "://" + uri.Authority + uri.Segments[0];
        }

        /// <summary>
        /// Decode url address from unknown to know mode
        /// </summary>
        /// <param name="url">An encoded url address</param>
        public static string DecodeUrl(string url)
        {
            string result = url;
            int pos = url.LastIndexOf(Consts.Query.Base64Unknowner);
            if (pos != -1)
            {
                // After Base64Unknowner nothing is needed, so we can remove them
                // This can fix lots of javascript generated URLs bad behaviour
                // Since v5.0
                result = result.Substring(0, pos);
            }

            // BUGFIX: Base64 hash algorithm encodes (~) chracter to (+) and browsers indicates that this is a space!!
            // So, I have to replace spaces with (+) character
            // Since v5.0
            result = result.Replace(' ', '+');

            result = UrlEncoders.DecodeFromASProxyBase64(result);

            //byte[] buff = Convert.FromBase64String(result);
            //result = Encoding.UTF8.GetString(buff);
            return result;
        }

        /// <summary>
        /// Encode url address to make it unknown
        /// </summary>
        /// <param name="url">A url address</param>
        public static string EncodeUrl(string url)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(url);
            //string result = Convert.ToBase64String(bytes);
            string result = UrlEncoders.EncodeToASProxyBase64(url);

            result += Consts.Query.Base64Unknowner;
            return result;
        }


        public static string SwitchToDirSep(string str)
        {
            return str.Replace('/', '\\');
        }
        public static string SwitchToUrlSep(string str)
        {
            return str.Replace('\\', '/');
        }


        /// <summary>
        /// Use this method for file addresses ,only. This method may be use in debug mode and is useless in the web!
        /// </summary>
        /// <returns>Exam: file://h:\dir1\dir2/dir3/file.zip --> file://h:\dir1\dir2\dir3\file.zip</returns>
        public static string CorrectFileUrl(string fileUrl)
        {
            string result = fileUrl;
            if (fileUrl.ToLower().IndexOf("file://") != -1)
            {
                result = UrlProvider.SwitchToDirSep(result);
                result = result.Remove(0, @"file:\\".Length);
                result = "file://" + result;
            }
            return result;
        }

        public static string RemoveUrlBookmark(string url, out string bookmark)
        {
            bookmark = "";
            int markpos = url.IndexOf('#');
            if (markpos == -1)
                return url;
            bookmark = url.Substring(markpos, url.Length - markpos);
            return url.Substring(0, markpos);
        }


        public static bool GetRequestQuery(NameValueCollection collection, string key, out string result)
        {
            string r = collection[key];
            if (!string.IsNullOrEmpty(r))
            {
                result = r;
                return true;
            }
            else
            {
                result = "";
                return false;
            }
        }


    }
}
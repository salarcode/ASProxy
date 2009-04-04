using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy
{
    public class UrlProvider
    {
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
            string str = collection[Consts.qUrlAddress];
            if (string.IsNullOrEmpty(str))
                return false;
            else
                return true;
        }

        public static string AddArgumantsToUrl(string url, bool encode)
        {
            return url + "?" + Consts.qDecode + "=" + Convert.ToInt32(encode) + "&" + Consts.qUrlAddress + "={0}";
        }

        public static string AddArgumantsToUrl(string page, string url, bool encodeurl)
        {
            if (encodeurl)
                url = EncodeUrl(url);
            return page + "?" + Consts.qDecode + "=" + Convert.ToSByte(encodeurl) + "&" + Consts.qUrlAddress + "=" + url;
        }

        public static void GetUrlArguments(NameValueCollection collection, out bool decode, out string url)
        {
            decode = true;
            url = "";
            try
            {
                string str = collection[Consts.qUrlAddress];
                if (!string.IsNullOrEmpty(str))
                {
                    url = str;
                    object obj = collection[Consts.qDecode];
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

        public static bool IsClientSitdeUrl(string path)
        {
            string[] stdurl = new string[] { "mailto:", "file://", "javascript:", "vbscript:", "jscript:", "vbs:", "ymsgr:", "data:" };
            path = path.ToLower();
            for (int i = 0; i < stdurl.Length; i++)
            {
                if (path.StartsWith(stdurl[i]))
                    return true;
            }
            return false;
        }

        public static bool IsVirtualUrl(string path)
        {
            string[] stdurl = new string[] { "http://", "https://", "mailto:", "ftp://", "file://", "telnet://", "news://", "nntp://", "ldap://", "ymsgr:", "javascript:", "vbscript:", "jscript:", "vbs:", "data:" };
            path = path.ToLower();
            for (int i = 0; i < stdurl.Length; i++)
            {
                if (path.StartsWith(stdurl[i]))
                    return false;
            }
            return true;
        }

        public static string IgnoreInvalidUrlCharctersInHtml(string htmlurl)
        {
            htmlurl = htmlurl.Replace("\n", "");
            htmlurl = htmlurl.Replace("\r", "");
            htmlurl = htmlurl.Replace("\t", "");
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

            string tmpPage = query.ToLower();

            // If the query contains only parameter
            if (query[0] == '?')
            {
                return UrlBuilder.AppendAntoherQueries(pageUrlWithoutParameters, query);// Safe but slow
                //return pageUrlWithoutParameters + query; //
            }

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

                // if "page" is a virual url like this: SalarSoft/default.aspx
                return pageBasePath + query;
            }
        }

        public static string GetPagePathWithoutParameters(string url)
        {
            Uri address = new Uri(url);
            return address.Scheme + "://" + address.Authority + address.LocalPath;
        }

        /// <summary>
        /// Gets URL site base path. e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/dir1/dir2/
        /// </summary>
        /// <returns>e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/dir1/dir2/</returns>
        public static string GetUrlPagePath(string url)
        {
            Uri address = new Uri(url);
            string result = address.Scheme + "://" + address.Authority;

            // Don't add last segment
            for (int i = 0; i < address.Segments.Length - 1; i++)
            {
                result += address.Segments[i];
            }
            if (address.Segments.Length > 0)
            {
                string last = address.Segments[address.Segments.Length - 1];
                if (last[last.Length - 1] == '/')
                    result += last;
            }
            return result;
        }

        /// <summary>
        /// Gets URL site base path. e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/
        /// </summary>
        /// <returns>e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/</returns>
        public static string GetUrlSiteBasePath(string url)
        {
            Uri uri = new Uri(url);
            return uri.Scheme + "://" + uri.Authority + "" + uri.Segments[0];
        }

        /// <summary>
        /// Decode url address from unknow to know mode
        /// </summary>
        /// <param name="url">An encoded url address</param>
        public static string DecodeUrl(string url)
        {
            string result = url;
            int pos = url.LastIndexOf(Consts.Base64Unknowner);
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

            byte[] buff = Convert.FromBase64String(result);
            result = UTF8Encoding.UTF8.GetString(buff);
            return result;
        }

        /// <summary>
        /// Encode url address to make it unknown
        /// </summary>
        /// <param name="url">A url address</param>
        public static string EncodeUrl(string url)
        {
            string result;
            byte[] bytes = UTF8Encoding.UTF8.GetBytes(url);
            result = Convert.ToBase64String(bytes);
            //result=HttpUtility.UrlEncode(result);
            result += Consts.Base64Unknowner;
            return result;
        }


        public static string SwitchToDirSep(string addr)
        {
            return addr.Replace('/', '\\');
        }
        public static string SwitchToUrlSep(string addr)
        {
            return addr.Replace('\\', '/');
        }


        /// <summary>
        /// Use this method for file addresses ,only. This method may be use in debug mode and is useless in the web!
        /// </summary>
        /// <returns>Exam: file://h:\dir1\dir2/dir3/file.zip --> file://h:\dir1\dir2\dir3\file.zip</returns>
        public static string CorrectFileUrl(string fileurl)
        {
            string result = fileurl;
            if (fileurl.ToLower().IndexOf("file://") != -1)
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
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy
{
    public class Common
    {
        #region public static

        public static string GenerateUserAgent(HttpContext context)
        {
            string userAgent;
            if (context != null)
                userAgent = context.Request.UserAgent;
            else
                return GlobalConsts.ASProxyUserAgent;
            if (string.IsNullOrEmpty(userAgent))
                return GlobalConsts.ASProxyUserAgent;
            else
                return userAgent.Trim() + " " + GlobalConsts.ASProxyAgentVersion;
        }


        /// <summary>
        /// Get string of a query collection
        /// </summary>
        public static string NameValueCollectionToString(NameValueCollection queryCollection)
        {
            string result = "";
            for (int i = 0; i < queryCollection.Count; i++)
            {
                result += queryCollection.GetKey(i) + "=" + queryCollection.Get(i);
                if (i < queryCollection.Count - 1)
                    result += "&";
            }
            return result;
        }

        public static byte[] AspDotNetViewStateResetToDef(Stream src)
        {
            byte[] result;
            try
            {
                result = new byte[src.Length];
                src.Read(result, 0, result.Length);
            }
            catch (NotSupportedException)
            {
                // If reading directly nor supported
                ArrayList arrayBuff = new ArrayList();
                byte[] buff = new byte[256];
                int readed = 0;
                do
                {
                    readed = src.Read(buff, 0, buff.Length);
                    for (int i = 0; i < readed; i++)
                    {
                        arrayBuff.Add(buff[i]);
                    }
                }
                while (readed > 0);
                result = (byte[])arrayBuff.ToArray(typeof(byte));
                arrayBuff.Clear();
            }

            result = ApplyViewStateResetRename(result);
            return result;
        }

        public static byte[] ApplyViewStateResetRename(byte[] buff)
        {
            string str = Encoding.ASCII.GetString(buff);
            int pos = str.IndexOf(Consts.ASPDotNETRenamedViewState);
            if (pos != -1)
            {
                // Add frist part before ViewState 
                byte[] result = new byte[buff.Length + (Consts.ASPDotNETViewState.Length - Consts.ASPDotNETRenamedViewState.Length)];
                for (int i = 0; i < pos; i++)
                    result[i] = buff[i];

                // Add new ViewState 
                byte[] aspDotNET = Encoding.ASCII.GetBytes(Consts.ASPDotNETViewState);

                for (int i = 0; i < aspDotNET.Length; i++)
                    result[i + pos] = aspDotNET[i];


                // Add third part after ViewState
                int diff = Consts.ASPDotNETRenamedViewState.Length - Consts.ASPDotNETViewState.Length;
                for (int i = pos + aspDotNET.Length; i < result.Length; i++)
                    result[i] = buff[i + diff];

                //string s = Encoding.ASCII.GetString(result);
                return result;

            }
            else
                return buff;
        }


        public static string AspDotNetViewStateResetToDef(string query)
        {
            return query.Replace(Consts.ASPDotNETRenamedViewState, Consts.ASPDotNETViewState);
        }

        /// <summary>
        /// Clears response header then recovers compression header
        /// </summary>
        public static void ClearASProxyRespnseHeader(HttpResponse response)
        {
            HttpCookieCollection cool = response.Cookies;

            HttpCookie cookie = response.Cookies[GlobalConsts.HttpCompressorCookieMasterName];
            string encode = cookie["CompressEncoding"];
            response.ClearHeaders();

            if (string.IsNullOrEmpty(encode))
                return;

            response.AddHeader("Content-Encoding", encode);
        }

        /// <summary>
        /// Is this url a FTP url
        /// </summary>
        public static bool IsFTPUrl(string url)
        {
            return url.Trim().StartsWith("ftp://", StringComparison.CurrentCultureIgnoreCase);
        }

        public static string ContentTypeToString(MimeContentType type)
        {
            if (type == MimeContentType.application)
                return "application/octet-stream";
            return type.ToString().Replace('_', '/');
        }

        public static MimeContentType StringToContentType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return MimeContentType.application;

            type = type.ToLower();

            string[] parts = type.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                type = parts[0];

            switch (type)
            {
                case "text/html":
                    return MimeContentType.text_html;
                case "text/plain":
                    return MimeContentType.text_plain;
                case "text/css":
                    return MimeContentType.text_css;
                case "text/javascript":
                case "text/jscript":
                    return MimeContentType.text_javascript;
                case "image/jpeg":
                    return MimeContentType.image_jpeg;
                case "image/gif":
                    return MimeContentType.image_gif;
                case "application/octet-stream":
                    return MimeContentType.application;
                default:
                    {

                        if (type.IndexOf("javascript") != -1)
                            return MimeContentType.text_javascript;
                        if (type.IndexOf("text") != -1)
                            return MimeContentType.text_html;
                        if (type.IndexOf("image") != -1)
                            return MimeContentType.image_jpeg;
                        if (type.IndexOf("application") != -1)
                            return MimeContentType.application;
                        return MimeContentType.application;
                    }
            }
        }

        public static string FromBase64(string str)
        {
            string result;

            byte[] buff = Convert.FromBase64String(str);
            result = UTF8Encoding.UTF8.GetString(buff);
            return result;
        }

        public static string ToBase64(string B64String)
        {
            string result;
            byte[] bytes = UTF8Encoding.UTF8.GetBytes(B64String);
            result = Convert.ToBase64String(bytes);
            return result;
        }
        #endregion
    }
}
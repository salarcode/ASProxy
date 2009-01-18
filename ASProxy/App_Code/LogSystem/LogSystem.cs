using System;
using System.IO;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy
{
    public class LogSystem
    {
		const string _strEntityFormat = "\n<Entry type=\"{0}\">\n{1}</Entry>\n";
        const string _strUrlFormat = "<RequestedUrl>{0}</RequestedUrl>\n";
        const string _strIPFormat = "<IP>{0}</IP>\n";
        const string _strDataFormat = "<Param>{0}</Param>\n";

        private static string _LogFileNameFormat;
        public static bool Enabled
        {
            get { return ASProxyConfig.LogSystemEnabled; }
        }

        static LogSystem()
        {
            if (ASProxyConfig.LogSystemEnabled == false)
                return;

            _LogFileNameFormat = ASProxyConfig.LogFilePath;
            string path = HttpContext.Current.Request.PhysicalApplicationPath;
            try
            {
                _LogFileNameFormat = Path.Combine(path, _LogFileNameFormat);
                path = Path.GetDirectoryName(_LogFileNameFormat);

                System.IO.Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                ASProxyConfig.LogSystemEnabled = false;
                return;
            }
        }

        public static void Log(LogEntity entity, params object[] optionalData)
        {
            if (ASProxyConfig.LogSystemEnabled == false)
                return;
            if (entity == LogEntity.ImageRequested && ASProxyConfig.LogImagesEnabled == false)
                return;


            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(_strIPFormat, HttpContext.Current.Request.UserHostAddress);

            for (int i = 0; i < optionalData.Length; i++)
            {
                builder.AppendFormat(_strDataFormat, optionalData[i]);
            }

            DateTime dt = DateTime.Now;
            string fileDate = dt.ToString("yyy-MM-dd");
            builder.AppendFormat(_strDataFormat, dt.ToString());


            string result = string.Format(_strEntityFormat, entity, builder.ToString());
            SaveToLogFile(ref result, fileDate);
        }

        public static void Log(LogEntity entity,HttpRequest request,string requestedUrl, params object[] optionalData)
        {
            if (ASProxyConfig.LogSystemEnabled == false)
                return;
            if (entity == LogEntity.ImageRequested && ASProxyConfig.LogImagesEnabled == false)
                return;

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(_strUrlFormat, requestedUrl);
            builder.AppendFormat(_strIPFormat, request.UserHostAddress);

            for (int i = 0; i < optionalData.Length; i++)
            {
                builder.AppendFormat(_strDataFormat, optionalData[i]);
            }

            DateTime dt = DateTime.Now;
            string fileDate = dt.ToString("yyy-MM-dd");
            builder.AppendFormat(_strDataFormat, dt.ToString());


            string result = string.Format(_strEntityFormat, entity, builder.ToString());
            SaveToLogFile(ref result, fileDate);
        }

        private static void SaveToLogFile(ref string data, string fileDate)
        {
            try
            {
                string filePath=string.Format(_LogFileNameFormat,fileDate);
                File.AppendAllText(filePath, data);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// Log entities type
    /// </summary>
    public enum LogEntity
    {
        UrlRequested,

        ImageRequested,

        DownloadRequested,

        Error,

        ASProxyLoginPassed,

        AuthorizationRequired
    }
}
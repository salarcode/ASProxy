using System.IO;
using System.Text;

namespace SalarSoft.ASProxy
{
    public class Processors
    {
        private const string _DefaultEncodingInt = "windows-1256";
        private static Encoding _DefaultEncoding = Encoding.GetEncoding(_DefaultEncodingInt);

        #region Public static
        public static string GetDirectString(WebDataCore urldata)
        {
            byte[] bytes = urldata.ResponseData.GetBuffer();
            return Encoding.ASCII.GetString(bytes, 0, bytes.Length);
        }

        public static string ConvertEncoding(string srcStr, Encoding srcEnc, Encoding destEnc)
        {
            byte[] bytes = srcEnc.GetBytes(srcStr);
            bytes = Encoding.Convert(srcEnc, destEnc, bytes);
            return destEnc.GetString(bytes);
        }

        public static string GetString(WebDataCore urldata, bool IgnorePageEncoding, out Encoding DetectedEncode)
        {
            string result = "";
            StreamReader reader;
            Encoding ResultEncoding, StreamEncoding;

            // Seek to beginning of text
            urldata.ResponseData.Seek(0, SeekOrigin.Begin);

            // Read the string
            if (IgnorePageEncoding)
                reader = new StreamReader(urldata.ResponseData, true);
            else
                reader = new StreamReader(urldata.ResponseData, _DefaultEncoding, true);

            result = reader.ReadToEnd();
            StreamEncoding = reader.CurrentEncoding;

            DetectedEncode = StreamEncoding;
            // End of string reading

            try
            {
                // Get content type from html meta tag content
                string HtmlContentType = HtmlTags.GetContentType(ref result, "");

                // If html content type is present
                if (HtmlContentType != "")
                {
                    ResultEncoding = Encoding.GetEncoding(HtmlContentType);
                    if (ResultEncoding != StreamEncoding)
                    {
                        // Seek to fisrt
                        urldata.ResponseData.Seek(0, SeekOrigin.Begin);

                        // Read the string
                        reader = new StreamReader(urldata.ResponseData, ResultEncoding, true);
                        result = reader.ReadToEnd();
                        // End of string reading

                        ResultEncoding = reader.CurrentEncoding;
                    }
                    else
                        ResultEncoding = StreamEncoding;
                }
                else
                    ResultEncoding = StreamEncoding;


                // Ignore encoding
                if (IgnorePageEncoding)
                {
                    // If encoding isn't UTF-8, we should convert it to Utf-8
                    if (ResultEncoding != Encoding.UTF8)
                        result = ConvertEncoding(result, ResultEncoding, Encoding.UTF8);

                    // Now the result is UTF-8
                    ResultEncoding = Encoding.UTF8;
                }

                // Return the detected result
                DetectedEncode = ResultEncoding;
            }
            catch
            {
            }
            return result;
        }
        #endregion

    }
}
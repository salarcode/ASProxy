using System.Globalization;
using System.Threading;
using System.Text;
using System.IO;

namespace SalarSoft.ASProxy
{

    /// <summary>
    /// A utility to work with strings throught streams
    /// </summary>
    public class StringStream
    {
        private const string _DefaultEncodingInt = "windows-1256";
        private static Encoding _DefaultEncoding = Encoding.GetEncoding(_DefaultEncodingInt);

        #region Public static
        public static string ConvertEncoding(string srcStr, Encoding srcEnc, Encoding destEnc)
        {
            byte[] bytes = srcEnc.GetBytes(srcStr);
            bytes = Encoding.Convert(srcEnc, destEnc, bytes);

            return destEnc.GetString(bytes);
        }

        public static string GetString(Stream stream, bool ignorePageEncoding, out Encoding DetectedEncode)
        {
            string result = "";
            StreamReader reader;
            Encoding ResultEncoding;

            // Seek to beginning of the stream
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            // Read the string
            if (ignorePageEncoding)
                reader = new StreamReader(stream, true);
            else
                reader = new StreamReader(stream, _DefaultEncoding, true);

            // Reading data from stream to the string
            result = reader.ReadToEnd();

            // Getting detected encoding
            Encoding StreamEncoding = reader.CurrentEncoding;

            DetectedEncode = StreamEncoding;
            // End of string reading

            try
            {
                // Get content type from html meta tag content
                string HtmlContentType = HtmlTags.GetContentType(ref result, "");

                // If html content type is present
                if (!string.IsNullOrEmpty(HtmlContentType))
                {
                    // Getting encoding of content type
                    ResultEncoding = Encoding.GetEncoding(HtmlContentType);

                    if (ResultEncoding != StreamEncoding)
                    {
                        // Seek to fisrt
                        stream.Seek(0, SeekOrigin.Begin);

                        // Read the string
                        reader = new StreamReader(stream, ResultEncoding, true);
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
                if (ignorePageEncoding)
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
                // Ingoring any error
                // Just go away
            }
            return result;
        }
        #endregion
    }
}
using System.Globalization;
using System.Threading;
using System.Text;
using System.IO;
using System;

namespace SalarSoft.ASProxy
{

	/// <summary>
	/// A utility to work with strings throught streams
	/// </summary>
	public class StringStream
	{
		//private const string _DefaultEncodingInt = "windows-1256";
		private static Encoding _DefaultEncoding;//= Encoding.GetEncoding(_DefaultEncodingInt);

		static StringStream()
		{
			_DefaultEncoding = Encoding.GetEncoding(Configurations.WebData.PreferredLocalEncoding);
		}

		#region Public static
		public static string ConvertEncoding(string srcStr, Encoding srcEnc, Encoding destEnc)
		{
			byte[] bytes = srcEnc.GetBytes(srcStr);
			bytes = Encoding.Convert(srcEnc, destEnc, bytes);

			return destEnc.GetString(bytes);
		}

		private static Encoding GetContentTypeEncoding(string contentType)
		{
			try
			{
				if (string.IsNullOrEmpty(contentType))
				{
					return null;
				}
				string[] strArray = contentType.ToLower(CultureInfo.InvariantCulture).Split(new char[] { ';', '=', ' ' });

				bool charset = false;
				foreach (string part in strArray)
				{
					if (part == "charset")
					{
						charset = true;
					}
					else if (charset)
					{
						return Encoding.GetEncoding(part);
					}
				}

				// nothing found
				return null;
			}
			catch
			{
				return null;
			}
		}

		public static string GetString(Stream stream, string contentType, bool ignorePageEncoding, bool detectHtmlContentType, out Encoding DetectedEncode)
		{
			bool detectEncoding = true;
			string result = string.Empty;
			StreamReader reader;
			Encoding resultEncoding = null;

			if (string.IsNullOrEmpty(contentType) == false)
			{
				resultEncoding = GetContentTypeEncoding(contentType);
				if (resultEncoding != null)
				{
					// encoding found by Response Content Type
					detectEncoding = false;
				}
			}


			// Seek to beginning of the stream
			if (stream.CanSeek)
				stream.Seek(0, SeekOrigin.Begin);

			if (detectEncoding)
			{
				// Read the string
				if (ignorePageEncoding)
					reader = new StreamReader(stream, true);
				else
					reader = new StreamReader(stream, _DefaultEncoding, true);

				// Reading data from stream to the string
				result = reader.ReadToEnd();

				// get the detected encoding
				resultEncoding = reader.CurrentEncoding;
			}
			else
			{
				// open the reader with found result
				// do not detect encoding
				reader = new StreamReader(stream, resultEncoding, false);

				// Reading data from stream to the string
				result = reader.ReadToEnd();
			}


			try
			{
				if (detectHtmlContentType)
				{
					// Get content type from html meta tag content
					string htmlContentType = HtmlTags.GetContentType(ref result, null);

					// If html content type is present
					if (!string.IsNullOrEmpty(htmlContentType))
					{
						// Getting encoding of content type
						Encoding htmlEncoding = Encoding.GetEncoding(htmlContentType);

						// if the encoding is different
						if (resultEncoding != htmlEncoding)
						{
							// Seek to fisrt
							if (stream.CanSeek)
								stream.Seek(0, SeekOrigin.Begin);

							// Read the string
							reader = new StreamReader(stream, htmlEncoding, true);
							result = reader.ReadToEnd();
							// End of string reading

							// getting the detected encoding
							resultEncoding = reader.CurrentEncoding;
						}
					}
				}

				// Ignore encoding
				if (ignorePageEncoding)
				{
					// If encoding isn't UTF-8, we should convert it to Utf-8
					if (resultEncoding != Encoding.UTF8)
						result = ConvertEncoding(result, resultEncoding, Encoding.UTF8);

					// Now the result is UTF-8
					resultEncoding = Encoding.UTF8;
				}
			}
			catch (Exception ex)
			{
				// Ingoring any error
				// Just go away

				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, "GetString error");
			}

			// Return the detected result
			DetectedEncode = resultEncoding;

			return result;
		}

		public static string GetString(Stream stream, bool ignorePageEncoding, bool detectHtmlContentType, out Encoding DetectedEncode)
		{
			return GetString(stream, null, ignorePageEncoding, detectHtmlContentType, out DetectedEncode);
		}

		public static string GetString(Stream stream)
		{
			if (stream == null)
				return "";
			string result = string.Empty;
			StreamReader reader;

			// Seek to beginning of the stream
			if (stream.CanSeek)
				stream.Seek(0, SeekOrigin.Begin);

			// do not detect encoding
			reader = new StreamReader(stream, true);

			// Reading data from stream to the string
			result = reader.ReadToEnd();
			result.Clone();

			return result;
		}

		private static string GetString_OLD(Stream stream, bool ignorePageEncoding, out Encoding DetectedEncode)
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


		/// <summary>
		/// For test
		/// </summary>
        private static string GetString__HARD(Stream stream, bool ignorePageEncoding, out Encoding DetectedEncode)
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
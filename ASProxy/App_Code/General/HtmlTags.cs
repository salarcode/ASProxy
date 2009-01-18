using System;

namespace SalarSoft.ASProxy
{

	public class HtmlTags
	{
		internal const string ImgStartTag = "<img";
		internal const string ImgSourceTag = "src";
		internal const string HtmlEndTag = ">";

		internal const string AnchorStartTag = "<a";
		internal const string AnchorSourceTag = "href";

		internal const string IFrameStartTag = "<iframe";
		internal const string IFrameSourceTag = "src";

		internal const string FrameSetStartTag = "<frame ";
		internal const string FrameSetSourceTag = "src";

		internal const string ScriptStartTag = "<script";
		internal const string ScriptSourceTag = "src";

		internal const string TableStartTag = "<table";
		internal const string TableBackImgSourceTag = "background";

		internal const string THeaderStartTag = "<th";
		internal const string THeaderBackImgSourceTag = "background";

		internal const string TCellStartTag = "<td";
		internal const string TCellBackImgSourceTag = "background";

		internal const string BodyStartTag = "<body";
		internal const string BodyBackImgSourceTag = "background";

		internal const string FormStartTag = "<form";
		internal const string FormSourceTag = "action";
		internal const string FormMethodTag = "method";

		internal const string EmbedStartTag = "<embed";
		internal const string EmbedSourceTag = "src";


		#region Generic functions

		/// <summary>
		/// Specifies that a string with specified position index, is enclosed by some tags
		/// </summary>
		public static bool IsEnclosedBy(ref string htmlCode, int index, string startTag, string endTag)
		{
			int start = Performance.LastIndexOfIgnoreCase(ref htmlCode, startTag, index);
			if (start == -1)
				return false;

			int end = Performance.IndexOfIgnoreCase(ref htmlCode, endTag, start);
			if (end == -1)
				return true;

			if (end > index)
				return true;

			return false;
		}

		/// <summary>
		/// Correct start and end position for value position by ignoring some characters
		/// </summary>
		public static TextRange CorrectValueIfQuoteExists(ref string htmlCode, TextRange ranges)
		{
			char chr;
			if ((ranges.End - ranges.Start) == 1)
			{
				if (ranges.Start > -1)
				{
					chr = htmlCode[ranges.Start];
					if (chr == '\'' || chr == '\"')
						ranges.Start++;
				}
				return ranges;
			}

			if (ranges.Start > -1)
			{
				chr = htmlCode[ranges.Start];
				if (chr == '\'' || chr == '\"')
					ranges.Start++;
			}
			if (ranges.End - 1 > -1)
			{
				chr = htmlCode[ranges.End - 1];
				if (chr == '\'' || chr == '\"')
					ranges.End--;
			}
			return ranges;
		}



		/// <summary>
		/// Remove quote from begin or end of value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string RemoveQuotesFromTagAttributeValue(string value)
		{
			value = value.Trim();
			if (value.Length > 0)
			{
				if (value[0] == '\'' || value[0] == '\"')
					value = value.Remove(0, 1);
			}
			if (value.Length > 0)
			{
				if (value[value.Length - 1] == '\'' || value[value.Length - 1] == '\"')
					value = value.Remove(value.Length - 1, 1);
			}
			return value;
		}

		/// <summary>
		/// Removes URL attribute in css urls like this: url(other.css) will change to "other.css"
		/// </summary>
		public static bool RemoveUrlAttribCssLocation(string text, out string result)
		{
			result = text;
			const string urlAttrib = "url(";
			const string urlEnd = ")";

			// start attrib
			int start = Performance.IndexOfIgnoreCase(ref text, urlAttrib);
			if (start == -1)
				return false;
			text = text.Remove(start, urlAttrib.Length);

			// end attrib
			start = Performance.IndexOfMatchCase(ref text, urlEnd);
			if (start == -1)
				return false;
			text = text.Remove(start, urlEnd.Length);

			result = text;
			return true;
		}

		public static string RemoveQuotesFromStartEnd(string text)
		{
			text = text.Trim();
			if (text.Length > 0)
			{
				if (text[0] == '"' || text[0] == '\'')
					text = text.Remove(0, 1);
				if (text[text.Length - 1] == '"' || text[text.Length - 1] == '\'')
					text = text.Remove(text.Length - 1, 1);
			}
			return text;
		}

		public static string RemoveBraketsFromStartEnd(string text)
		{
			text = text.Trim();
			if (text.Length > 0)
			{
				if (text[0] == '{')
					text = text.Remove(0, 1);
				if (text[text.Length - 1] == '}')
					text = text.Remove(text.Length - 1, 1);
			}
			return text;
		}



		/// <summary>
		/// Removes escaped characters from start and end of phrase
		/// </summary>
		public static string RemoveEscapeQuotesFromTagAttributeValue(string value)
		{
			value = value.Trim();
			if (value.Length > 0)
			{
				// if the text started with escaped characters.
				if (value.StartsWith("\\'") || value.StartsWith(@"\"""))
				{
					// remove
					value = value.Remove(0, 2);

					// End of text will be checked only if begging of it oncludes escaped characters
					if (value.Length > 0)
					{
						// if the text ends with ( \ ) character only,
						// it seems there is a problem so we will prevent it by adding another ( \ ) at the end of text.
						if (value.EndsWith(@"\"))
							value = value.Remove(value.Length - 1, 1) + " \\";
						else if (value.EndsWith("\\'") || value.EndsWith(@"\"""))
							value = value.Remove(value.Length - 1, 1);
					}
				}
			}
			return value;
		}

		public static string GetContentType(ref string html)
		{
			return GetContentType(ref html, "");
		}

		public static string GetContentType(ref string html, string defaultvalue)
		{
			string result = defaultvalue;
			int end, tmp, start = Performance.IndexOfIgnoreCase(ref html, "<meta");
			if (start == -1) return result;

			tmp = Performance.IndexOfIgnoreCase(ref html, "http-equiv=\"content-type\"", start);
			if (tmp == -1)
			{
				tmp = Performance.IndexOfIgnoreCase(ref html, "http-equiv='content-type'", start);
				if (tmp == -1)
				{
					tmp = Performance.IndexOfIgnoreCase(ref html, "http-equiv=content-type", start);
					if (tmp == -1)
					{
						tmp = Performance.IndexOfIgnoreCase(ref html, "http-equiv= content-type", start);
						if (tmp == -1) return result;
					}
				}
			}
			start = tmp;

			start = Performance.IndexOfIgnoreCase(ref html, "charset", start);
			if (start == -1) return result;
			start = Performance.IndexOfIgnoreCase(ref html, "=", start);
			if (start == -1) return result;
			start++;

			end = Performance.IndexOfIgnoreCase(ref html, "\"", start);
			if (end == -1) return result;
			tmp = Performance.IndexOfIgnoreCase(ref html, "\'", start);
			if (tmp > start && end > tmp)
				end = tmp;

			result = html.Substring(start, end - start);
			result = result.Trim();
			return result;
		}

		/// <summary>
		/// Finds and returns DOCTYPE definition.
		/// </summary>
		public static string GetDocType(ref string html)
		{
			const string startTag = "<!DOCTYPE";
			const string endTag = ">";
			int start, end;
			string result = "";

			start = Performance.IndexOfIgnoreCase(ref html, startTag);
			if (start == -1)
				return result;

			end = Performance.IndexOfIgnoreCase(ref html, endTag, start);
			if (end == -1)
				return result;

			// add a character length to position
			end++;

			// Test start tag
			if (start > 0)
			{
				// DocType should be first tag in html document
				string temp = html.Substring(0, start).Trim();
				if (temp.Length > 0 && temp.IndexOf('<') != -1)
					return result;
			}

			// substring
			result = html.Substring(start, end - start);

			return result;
		}

		#endregion



		#region Generate html tags
		public static string JavascriptTag(string content, string src)
		{
			string result = "<script language='javascript' ";
			if (!string.IsNullOrEmpty(src))
				result += "src='" + src + "'";
			result += "type='text/javascript'>" + content + "</script>";
			return result;
		}
		public static string IFrameTag(string src, string width, string height)
		{
			return "<iframe border='2' src='" + src + "' width='" + width + "' height='" + height + "' ></iframe>";
		}

		public static bool IsFramesetHtml(ref string html)
		{
			return (Performance.IndexOfIgnoreCase(ref  html, "<frameset", 0) != -1);
		}

		public static string ImgTag(string img)
		{
			return "<img border='0' src=\"" + img + "\" />";
		}
		#endregion

	}
}
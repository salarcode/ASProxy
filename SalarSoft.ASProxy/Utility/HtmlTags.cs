using System;
using System.Web;

namespace SalarSoft.ASProxy
{

	/// <summary>
	/// Html tag codes generator/analyzer
	/// </summary>
	public class HtmlTags
	{
		#region Generic functions

		/// <summary>
		/// Specifies that a string with specified position index, is enclosed by some tags
		/// </summary>
		public static bool IsEnclosedBy(ref string htmlCode, int index, string startTag, string endTag)
		{
			int start = StringCompare.LastIndexOfIgnoreCase(ref htmlCode, startTag, index);
			if (start == -1)
				return false;

			int end = StringCompare.IndexOfIgnoreCase(ref htmlCode, endTag, start);
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
			int start = StringCompare.IndexOfIgnoreCase(ref text, urlAttrib);
			if (start == -1)
				return false;
			text = text.Remove(start, urlAttrib.Length);

			// end attrib
			start = StringCompare.IndexOfMatchCase(ref text, urlEnd);
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
				//if (value.StartsWith("\\'") || value.StartsWith(@"\"""))
				if (StringCompare.StartsWithMatchCase(ref value, "\\'") ||
					StringCompare.StartsWithMatchCase(ref value, @"\"""))
				{
					// remove
					value = value.Remove(0, 2);

					// End of text will be checked only if begging of it oncludes escaped characters
					if (value.Length > 0)
					{
						// if the text ends with ( \ ) character only,
						// it seems there is a problem so we will prevent it by adding another ( \ ) at the end of text.
						if (StringCompare.EndsWithMatchCase(ref value, "\\"))
						{
							value = value.Remove(value.Length - 1, 1) + " \\";
						}
						else if (StringCompare.EndsWithMatchCase(ref value, "\\'") ||
							StringCompare.EndsWithMatchCase(ref value, @"\"""))
						{
							value = value.Remove(value.Length - 1, 1);
						}
						//if (value.EndsWith("\\"))
						//    value = value.Remove(value.Length - 1, 1) + " \\";
						//else if (value.EndsWith("\\'") || value.EndsWith(@"\"""))
						//    value = value.Remove(value.Length - 1, 1);
					}
				}
			}
			return value;
		}

		public static string GetContentType(ref string html)
		{
			return GetContentType(ref html, string.Empty);
		}

		public static string GetContentType(ref string html, string defaultValue)
		{
			string result = defaultValue;
			int end, tmp, start = StringCompare.IndexOfIgnoreCase(ref html, "<meta");
			if (start == -1) return result;

			tmp = StringCompare.IndexOfIgnoreCase(ref html, "http-equiv=\"content-type\"", start);
			if (tmp == -1)
			{
				tmp = StringCompare.IndexOfIgnoreCase(ref html, "http-equiv='content-type'", start);
				if (tmp == -1)
				{
					tmp = StringCompare.IndexOfIgnoreCase(ref html, "http-equiv=content-type", start);
					if (tmp == -1)
					{
						tmp = StringCompare.IndexOfIgnoreCase(ref html, "http-equiv= content-type", start);
						if (tmp == -1) return result;
					}
				}
			}
			start = tmp;

			start = StringCompare.IndexOfIgnoreCase(ref html, "charset", start);
			if (start == -1) return result;
			start = StringCompare.IndexOfIgnoreCase(ref html, "=", start);
			if (start == -1) return result;
			start++;

			end = StringCompare.IndexOfIgnoreCase(ref html, "\"", start);
			if (end == -1) return result;
			tmp = StringCompare.IndexOfIgnoreCase(ref html, "\'", start);
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

			start = StringCompare.IndexOfIgnoreCase(ref html, startTag);
			if (start == -1)
				return result;

			end = StringCompare.IndexOfIgnoreCase(ref html, endTag, start);
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

		/// <summary>
		/// Encodes quote for javascript string
		/// </summary>
		public static string EncodeJavascriptString(string pageUrl, bool quote)
		{
			if (string.IsNullOrEmpty(pageUrl))
				return pageUrl;
			char lookfor = quote ? '\"' : '\'';
			
			int index = 0;
			while ((index = pageUrl.IndexOf(lookfor, index)) != -1)
			{
				if (index == 0)
				{
					pageUrl = pageUrl.Insert(index, "\\");
					index += 2;
				}
				else if (pageUrl[index - 1] != '\\')
				{
					pageUrl = pageUrl.Insert(index, "\\");
					index += 2;
				}
				else
					index++;
			}
			return pageUrl;
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
			return "<iframe border='1' src='" + src + "' width='" + width + "' height='" + height + "' ></iframe>";
		}

		public static bool IsFramesetHtml(ref string html)
		{
			return (StringCompare.IndexOfIgnoreCase(ref  html, "<frameset", 0) != -1);
		}

		/// <summary>
		/// Hyperlink tag
		/// </summary>
		public static string LinkTag(string text, string url)
		{
			url = HttpUtility.HtmlEncode(url);
			return "<a href='" + url + "'>" + text + "</a>";
		}

		/// <summary>
		/// Hyperlink tag
		/// </summary>
		public static string LinkTag(string text, string url, string title)
		{
			url = HttpUtility.HtmlEncode(url);
			return "<a href='" + url + "' title='" + title + "'>" + text + "</a>";
		}

		public static string ImgTag(string img)
		{
			return "<img border='0' src=\"" + img + "\" />";
		}
		#endregion


	}
}
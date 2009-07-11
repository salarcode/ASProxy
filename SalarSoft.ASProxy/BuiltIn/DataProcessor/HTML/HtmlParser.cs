using System;

namespace SalarSoft.ASProxy.BuiltIn
{
	/// <summary>
	/// Html tags parser
	/// </summary>
	public class HtmlParser
	{
		private enum ValueStartType { None, Quote, DblQuote };

		/// <summary>
		/// Locate and remove a tag from html codes
		/// </summary>
		/// <param name="pageHtml">Html codes</param>
		/// <param name="tagName">Name of tags</param>
		/// <param name="removeTag">Remove the tag also</param>
		public static void RemoveTagContent(ref string pageHtml, string tagName, bool removeTag)
		{
			string tagEnd = "</" + tagName + ">";
			string tagStart = "<" + tagName;
			string tagStartEnd = ">";

			int tagStartPos = 0;
			int tagStartEndPos = 0;
			int tagEndPos = 0;
			if (removeTag)
			{
				do
				{
					tagStartPos = StringCompare.IndexOfIgnoreCase(ref pageHtml, tagStart, tagStartPos);
					if (tagStartPos == -1)
						return;
					tagStartEndPos = StringCompare.IndexOfMatchCase(ref pageHtml, tagStartEnd, tagStartPos);

					if (tagStartEndPos == -1)
						return;
					tagStartEndPos += tagStartEnd.Length;

					tagEndPos = StringCompare.IndexOfMatchCase(ref pageHtml, tagEnd, tagStartEndPos);

					if (tagEndPos == -1)
						return;
					tagEndPos += tagEnd.Length;

					pageHtml = pageHtml.Remove(tagStartPos, tagEndPos - tagStartPos);
				} while (tagEndPos != -1);

			}
			else
			{
				do
				{
					tagStartPos = StringCompare.IndexOfIgnoreCase(ref pageHtml, tagStart, tagStartPos);
					if (tagStartPos == -1)
						return;
					tagStartEndPos = StringCompare.IndexOfMatchCase(ref pageHtml, tagStartEnd, tagStartPos);

					if (tagStartEndPos == -1)
						return;
					tagStartEndPos += tagStartEnd.Length;

					tagEndPos = StringCompare.IndexOfMatchCase(ref pageHtml, tagEnd, tagStartEndPos);

					if (tagEndPos == -1)
						return;

					pageHtml = pageHtml.Remove(tagStartEndPos, tagEndPos - tagStartEndPos);
				} while (tagEndPos != -1);
			}

		}

		/// <summary>
		/// Removes specified tag
		/// </summary>
		/// <param name="pageHtml"></param>
		/// <param name="tagName"></param>
		public static void RemoveTagOnly(ref string pageHtml, string tagName)
		{
			string tagEnd = "</" + tagName + ">";
			string tagStart = "<" + tagName;
			string tagStartEnd = ">";

			int tagStartPos = 0;
			int tagStartEndPos = 0;
			int tagEndPos = 0;

			do
			{
				tagStartPos = StringCompare.IndexOfIgnoreCase(ref pageHtml, tagStart, tagStartPos);
				if (tagStartPos == -1)
					return;
				tagStartEndPos = StringCompare.IndexOfMatchCase(ref pageHtml, tagStartEnd, tagStartPos);

				if (tagStartEndPos == -1)
					return;
				tagStartEndPos += tagStartEnd.Length;

				// removes tag start
				pageHtml = pageHtml.Remove(tagStartPos, tagStartEndPos - tagStartPos);


				// find tag end pos
				tagEndPos = StringCompare.IndexOfMatchCase(ref pageHtml, tagEnd, tagStartPos);

				if (tagEndPos == -1)
					return;

				pageHtml = pageHtml.Remove(tagEndPos, tagEnd.Length);
			} while (tagEndPos != -1);

		}
		/// <summary>
		/// Locate value of attribute position for specified html tag
		/// </summary>
		public static TextRange GetTagAttributeValuePos(ref string pageHtml, string tagName, string attributeName, int start)
		{
			int tagStart, tagEnd;
			int attrStart;
			int valueStart;

			// Allocate result with default values
			TextRange result = new TextRange(-1, -1);

			// Incorrect input
			if (start >= pageHtml.Length)
				return result;


			// Correct tag name
			if (tagName[0] != '<')
				tagName = '<' + tagName;

			//=============================================================//

			// Find tag first position 
			tagStart = StringCompare.IndexOfIgnoreCase(ref pageHtml, tagName, start);
			if (tagStart == -1)
				return result;

			// Locate end of tag
			tagEnd = StringCompare.IndexOfMatchCase(ref pageHtml, ">", tagStart);

			// Wrong html code
			if (tagEnd == -1)
				return result;

			// Find the Attribute position
			attrStart = StringCompare.IndexOfIgnoreCase(ref pageHtml, attributeName, tagStart);


			// There is no attribute, so go away!
			if (attrStart == -1 || attrStart >= tagEnd)
			{
				// Set found start position to result
				result.Start = tagStart;
				result.End = -1;
				return result;
			}

			// Find value start position
			valueStart = StringCompare.IndexOfMatchCase(ref pageHtml, '=', attrStart);

			// There is no value, so go away!
			if (valueStart == -1 || valueStart >= tagEnd)
			{
				// Set found start position to result
				result.Start = attrStart;
				result.End = -1;
				return result;
			}

			// Locate value of attribute position
			result = FindAttributeValuePosition(ref pageHtml, tagEnd, valueStart);

			return result;
		}

		/// <summary>
		/// Locate value of attribute position using specified start index
		/// </summary>
		internal static TextRange FindAttributeValuePosition(ref string htmlCode, int tagEnd, int valueStart)
		{

			// Allocate result with default values
			TextRange result;// = new TextRange(-1, -1);

			// Increase start position
			valueStart++;

			int valueStartPos = -1, valueEndPos = -1;
			int index = valueStart;
			char current;
			bool valueStartFound = false;
			bool continueDo = true;
			ValueStartType startType = ValueStartType.None;

			// Set default start position
			valueStartPos = valueStart;

			do
			{
				if (index >= tagEnd)
				{
					valueEndPos = index;
					break;
				}
				current = htmlCode[index];

				if (valueStartFound == false && current != ' ' && current != '\r' && current != '\t' && current != '\n')
				{
					valueStartPos = index;
					valueStartFound = true;

					if (current == '\'')
						startType = ValueStartType.Quote;
					else if (current == '\"')
						startType = ValueStartType.DblQuote;
					else
						startType = ValueStartType.None;

					index++;
					continue;
				}

				if (valueStartFound && (startType == ValueStartType.None) && (current == '\r' || current == '\n' || current == ' ' || current == '\t'))
				{
					valueEndPos = index;
					break;
				}

				if (valueStartFound && (startType != ValueStartType.None))
				{
					if (startType == ValueStartType.Quote && current == '\'')
					{
						valueEndPos = index;
						break;
					}
					else if ((startType == ValueStartType.DblQuote) && current == '\"')
					{
						valueEndPos = index;
						break;
					}
				}

				index++;
			}
			while (continueDo);
			result.End = valueEndPos;
			result.Start = valueStartPos;

			// Remove needless characters
			result = HtmlTags.CorrectValueIfQuoteExists(ref htmlCode, result);
			return result;
		}

		/// <summary>
		/// Returns the content of specified tag.
		/// </summary>
		/// <param name="pageHtml">html code</param>
		/// <param name="justTagName">The tag name like TABLE</param>
		public static string GetTagContent(ref string pageHtml, string justTagName)
		{
			string startTag = '<' + justTagName;
			string endTag = justTagName + '>';
			int start, end;

			start = StringCompare.IndexOfIgnoreCase(ref pageHtml, startTag);
			if (start == -1)
				return "";
			start = StringCompare.IndexOfMatchCase(ref pageHtml, '>', start);
			if (start == -1)
				return "";
			start++;

			end = StringCompare.IndexOfIgnoreCase(ref pageHtml, endTag, start);
			if (end == -1)
				return "";
			end = StringCompare.LastIndexOfMatchCase(ref pageHtml, '<', end);
			if (end == -1 || start > end)
				return "";

			return pageHtml.Substring(start, end - start).Trim();
		}


	}
}
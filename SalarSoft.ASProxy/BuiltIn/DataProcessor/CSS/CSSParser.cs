
namespace SalarSoft.ASProxy.BuiltIn
{

	public class CSSParser
	{
		/// <summary>
		/// Locates @import rule value
		/// </summary>
		/// <param name="cssCodes"></param>
		/// <param name="startindex"></param>
		/// <param name="onlyWithoutUrlOption">Specifies that operator should not catch rule with URL attribute.</param>
		/// <param name="captureExactInUrl">Specifies that operator should strip the URL attribute if there is any</param>
		/// <returns></returns>
		public static TextRange FindImportRuleUrlPosition(ref string cssCodes, int startindex, bool onlyWithoutUrlOption, bool captureExactlyInUrl)
		{
			int valueStart, valueEnd;
			TextRange result = new TextRange(-1, -1);
			const string strCSSImportRule = "@import";
			const string strCSSUrlValue = "url(";

			//==============================
			if (startindex >= cssCodes.Length)
				return result;


			// Find first position
			valueStart = StringCompare.IndexOfIgnoreCase(ref cssCodes, strCSSImportRule, startindex);
			if (valueStart == -1)
				return result;

			valueStart += strCSSImportRule.Length;

			// 
			if (cssCodes.Substring(valueStart, 1).Trim().Length > 0)
				return result;
			else
				valueStart++;

			valueEnd = StringCompare.IndexOfMatchCase(ref cssCodes, ";", valueStart);

			if (valueEnd == -1)
				return result;
			else
			{
				int urlPos = StringCompare.IndexOfIgnoreCase(ref cssCodes, strCSSUrlValue, valueStart);
				if (urlPos != -1 && urlPos < valueEnd)
				{
					if (onlyWithoutUrlOption)
					{
						result.Start = valueEnd;
						result.End = -1;
						return result;
					}

					if (captureExactlyInUrl)
					{
						valueStart = urlPos + strCSSUrlValue.Length;

						urlPos = StringCompare.IndexOfMatchCase(ref cssCodes, ")", valueStart);
						if (urlPos != -1 && urlPos < valueEnd)
						{
							valueEnd = urlPos;
						}
					}
				}
			}

			result.Start = valueStart;
			result.End = valueEnd;
			return result;

		}

		public static TextRange FindCSSClassStyleUrlValuePosition(ref string pageHtml, int startindex)
		{
			int valueStart, valueEnd;
			const string strCSSUrlValue = "url(";

			TextRange result;// = new TextRange(-1, -1);
			result.End = -1;
			result.Start = -1;


			//==============================
			if (startindex >= pageHtml.Length)
				return result;


			// Find first position
			valueStart = StringCompare.IndexOfIgnoreCase(ref pageHtml, strCSSUrlValue, startindex);
			if (valueStart == -1)
				return result;

			valueStart += strCSSUrlValue.Length;
			valueEnd = StringCompare.IndexOfMatchCase(ref pageHtml, ")", valueStart);

			if (valueEnd == -1)
				return result;

			if (valueEnd > StringCompare.IndexOfMatchCase(ref pageHtml, ";", valueStart))
				return result;

			result.Start = valueStart;
			result.End = valueEnd;
			return result;
		}

	}
}
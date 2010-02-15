using System.Web;

namespace SalarSoft.ASProxy.BuiltIn
{

	public class CSSReplacer
	{

		/// <summary>
		/// Finds and override url addresses within "style" tag.
		/// </summary>
		public static void ReplaceStyleTagStyleUrl(
            ref string htmlCode, 
            string currentUrlWithoutParameters, 
            string importRuleUrl, 
            string othersNewUrl, 
            string replacmentBasePath, 
            string siteAddress, 
            bool encodeUrl)
		{
			const string styleStart = "<style";
			const string styleEnd = "</style>";
			const string htmlTagEnd = ">";
			TextRange position;
			int index = 0;
			//string bookmarkPart = "";

			do
			{
				string styleContent = "";

				// Find style tag position start
				position.Start = StringCompare.IndexOfIgnoreCase(ref htmlCode, styleStart, index);
				if (position.Start == -1)
					break;

				// locate tag end
				position.Start = StringCompare.IndexOfMatchCase(ref htmlCode, htmlTagEnd, position.Start);
				if (position.Start == -1)
					break;
				position.Start++;

				// Locate end tag
				position.End = StringCompare.IndexOfIgnoreCase(ref htmlCode, styleEnd, position.Start);
				if (position.End == -1)
					break;

				// Set next searching start position
				index = position.End;

				// Get the content
				styleContent = htmlCode.Substring(position.Start, position.End - position.Start);

				// If there is nothing, go to next tag
				if (styleContent.Trim().Length == 0)
					continue;

				// Process the style content for "Import Rule"
				// We don't need "Import Rule" process with "url" property any more. It will done by bext command. Since v4.0
				ReplaceCSSClassStyleUrl(ref styleContent, currentUrlWithoutParameters, importRuleUrl, replacmentBasePath, siteAddress, encodeUrl, true);

				// Process the style content for backgrounds.
				// So, this breach the backgound option role. Since v4.0
				ReplaceCSSClassStyleUrl(ref styleContent, currentUrlWithoutParameters, othersNewUrl, replacmentBasePath, siteAddress, encodeUrl, false);


				// Replace the new style
				htmlCode = htmlCode.Remove(position.Start, position.End - position.Start);
				htmlCode = htmlCode.Insert(position.Start, styleContent);

			} while (index != -1);

		}

		/// <summary>
		/// Process the styles and replace them 
		/// </summary>
		public static void ReplaceCSSClassStyleUrl(ref string htmlCode, string currentUrlWithoutParameters, string newUrl, string replacmentBasePath, string siteAddress, bool encodeUrl, bool forImportRule)
		{
			int index = 0;
			TextRange position;
			string oldValue, newValue;
			string bookmarkPart = "";

			do
			{
				if (forImportRule)
				{
					// do not find "Import Rule"s with url option, it will done by other codes. Since v4.0
					position = CSSParser.FindImportRuleUrlPosition(ref htmlCode, index, false, false);
				}
				else
					position = CSSParser.FindCSSClassStyleUrlValuePosition(ref htmlCode, index);

				if (position.Start == -1)
					break;

				if (position.Start != -1 && position.End != -1)
				{
					bool shouldAddQuote = false;
					//======OK. go to end of tag=============
					index = position.End;

					//========================================================//
					//====================Replace new address=================//
					//========================================================//

					//====== Correct value position according to quotes existence=======
					position = HtmlTags.CorrectValueIfQuoteExists(ref htmlCode, position);


					//====== Get the attribute value ======
					oldValue = htmlCode.Substring(position.Start, position.End - position.Start);

					// Trim!
					oldValue = oldValue.Trim();

					// Removes URL attribute if there is any
					if (HtmlTags.RemoveUrlAttribCssLocation(oldValue, out oldValue))
						shouldAddQuote = true;

					oldValue = HtmlTags.RemoveQuotesFromTagAttributeValue(oldValue);
					//===== If link is a bookmark don't change it=====
					if (oldValue.StartsWith("#"))
						continue;

					//====== Convert virtual url to absolute ======
					oldValue = UrlProvider.JoinUrl(oldValue, currentUrlWithoutParameters, replacmentBasePath, siteAddress);

					//====== Delete invalid character such as tab and line feed ======
					oldValue = UrlProvider.IgnoreInvalidUrlCharctersInHtml(oldValue);

					//===== If another site url, has bookmark=====
					if (StringCompare.IndexOfMatchCase(ref oldValue, '#') != -1)
						oldValue = UrlBuilder.RemoveUrlBookmark(oldValue, out bookmarkPart);

					//==== Make it clear=========
					oldValue = HttpUtility.HtmlDecode(oldValue);

					//====== Encode url to make it unknown ======
					if (encodeUrl)
						oldValue = UrlProvider.EncodeUrl(oldValue);
					else
						// just url safe
						oldValue = UrlProvider.EscapeUrlQuery(oldValue);


					//====== Add it to our url ======
					newValue = string.Format(newUrl, oldValue);

					//===== Add bookmark to last url =====
					if (bookmarkPart.Length > 0)
					{
						newValue += bookmarkPart;
						bookmarkPart = "";
					}

					// Make it safe
					//newValue = HttpUtility.HtmlEncode(newValue);

					if (shouldAddQuote)
						newValue = "\"" + newValue + "\"";

					//====== Replace it with old url ======
					htmlCode = htmlCode.Remove(position.Start, position.End - position.Start);
					htmlCode = htmlCode.Insert(position.Start, newValue);

					//========================================================//
					//==============End of Replace new address================//
					//========================================================//
				}
				else
				{
					if (position.Start != -1)
						index = position.Start;
					index = StringCompare.IndexOfMatchCase(ref htmlCode, ' ', index);
				}

			}
			while ((index != -1));
		}
	}
}
using System;
using System.Web;
using System.Text.RegularExpressions;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy.BuiltIn
{

	public class HtmlReplacer
	{
		#region <General> Replace tag attributes value BASE functions
		private static void ReplaceTagSrcAttribute(ref string htmlCodes,
			string pageUrlNoQuery,
			string tagName,
			string attributeName,
			string pagePath,
			string newPageFormat,
			string siteRootUrl,
			bool encodeUrl,
			string extraAttributeFormat,
			bool canEncloseWithTags)
		{
			int index = 0;//====== In first run, index must be Zero ======
			TextRange position;
			string oldValue, newValue;
			string orgValue = "";
			string bookmarkPart = "";
			string newAttribute = "";
			bool addNewAttribute = false;
			bool hasNewAttribute = false;

			if (!string.IsNullOrEmpty(extraAttributeFormat))
			{
				addNewAttribute = true;
				hasNewAttribute = true;
			}

			do
			{
				addNewAttribute = hasNewAttribute;

				position = HtmlParser.GetTagAttributeValuePos(ref htmlCodes, tagName, attributeName, index);

				if (position.Start == -1)
					break;

				// If requested, test statement that shouldn't enclose with specified tags
				bool continueLoop = true;

				// this causes heavy pressure
				//if (canEncloseWithTags && position.Start != -1 && position.End != -1)
				//    continueLoop = !HtmlTags.IsEnclosedBy(ref htmlCodes, position.Start, "<script", "</script>");

				if (continueLoop &&
					position.Start != -1 &&
					position.End != -1 &&
					position.End > position.Start)
				{
					//======OK. go to end of tag=============
					index = StringCompare.IndexOfMatchCase(ref htmlCodes, '>', position.Start);

					// Replace new address

					//====== Correct value position according to quotes existence=======
					//position = ASProxyFunctions.CorrectValueIfQuoteExists(ref pageHtml, position);

					//====== Get the attribute value ======
					oldValue = htmlCodes.Substring(position.Start, position.End - position.Start);

					oldValue = HtmlTags.RemoveEscapeQuotesFromTagAttributeValue(oldValue);

					oldValue = HtmlTags.RemoveQuotesFromTagAttributeValue(oldValue);

					//===== If link is a bookmark don't change it=====
					if (oldValue.StartsWith("#"))
						continue;

					if (UrlProvider.IsClientSitdeUrl(oldValue) == false)
					{
						//====== Convert virtual url to absolute ======
						oldValue = UrlProvider.JoinUrl(oldValue, pageUrlNoQuery, pagePath, siteRootUrl);

						//====== Delete invalid character such as tab and line feed ======
						oldValue = UrlProvider.IgnoreInvalidUrlCharctersInHtml(oldValue);

						// Save orginal value
						orgValue = oldValue;

						//===== If another site url, has bookmark
						if (oldValue.IndexOf('#') != -1)
							oldValue = UrlBuilder.RemoveUrlBookmark(oldValue, out bookmarkPart);


						//====== Get desigred url addrress
						oldValue = HttpUtility.HtmlDecode(oldValue);

						//====== Encode url to make it unknown ======
						if (encodeUrl)
							oldValue = UrlProvider.EncodeUrl(oldValue);
						else
							// just url safe
							oldValue = UrlProvider.EscapeUrlQuery(oldValue);

						//====== Add it to our url ======
						newValue = string.Format(newPageFormat, oldValue);

						//===== Add bookmark to last url =====
						if (bookmarkPart.Length > 0)
						{
							newValue += bookmarkPart;
							bookmarkPart = "";
						}
					}
					else
					{
						newValue = oldValue;
						addNewAttribute = false;
					}

					//====== Make it safe
					newValue = HttpUtility.HtmlEncode(newValue);

					//====== Replace it with old url
					htmlCodes = htmlCodes.Remove(position.Start, position.End - position.Start);
					htmlCodes = htmlCodes.Insert(position.Start, newValue);


					if (addNewAttribute)
					{
						// Apply original value and encoded value to format
						// BUG: Problem with format string that contain (') or (") characters
						// Bug Fixed since version 4.7
						newAttribute = string.Format(extraAttributeFormat, orgValue, newValue);

						// Locate end of tag
						index = StringCompare.IndexOfMatchCase(ref htmlCodes, '>', position.Start);
						if (htmlCodes[index - 1] == '/')
							index--;

						// Insert to tag
						htmlCodes = htmlCodes.Insert(index, newAttribute);
					}

					//===============End of Replace new address =========
				}
				else
				{
					if (position.Start != -1)
						index = position.Start;
					index = StringCompare.IndexOfMatchCase(ref htmlCodes, '>', index);
				}

			}
			while ((index != -1));
		}

		public static void ReplaceTwoAttributeTagsValue(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string siteRootUrl,
			bool encodeUrl,
			string tagName,
			string attr1,
			string attr1Value,
			string attr2)
		{
			TextRange attr1Result = new TextRange(-1, -1);
			TextRange attr2Result = new TextRange(-1, -1);
			int cursorPos = 0;
			string tmp, actionSrc = "";

			do
			{
				attr1Result = HtmlParser.GetTagAttributeValuePos(ref htmlCodes, tagName, attr1, cursorPos);
				if (attr1Result.Start > -1 && attr1Result.End > -1)
				{
					string tmpRelType = htmlCodes.Substring(attr1Result.Start, attr1Result.End - attr1Result.Start);
					if (tmpRelType.Trim().ToLower() != attr1Value.Trim().ToLower())
					{
						if (attr1Result.Start != -1)
							cursorPos = attr1Result.Start;
						continue;
					}
				}
				else
				{
					break;
				}

				attr2Result = HtmlParser.GetTagAttributeValuePos(ref htmlCodes, tagName, attr2, cursorPos);

				if (attr2Result.Start == -1) break;

				if (attr2Result.Start > -1 && attr2Result.End > -1)
				{
					cursorPos = attr2Result.Start;

					//====== Correct value position according to quotes existence=======
					attr2Result = HtmlTags.CorrectValueIfQuoteExists(ref htmlCodes, attr2Result);

					// Get the value
					actionSrc = htmlCodes.Substring(attr2Result.Start, attr2Result.End - attr2Result.Start);


					//====== Convert virtual url to absolute ======
					actionSrc = UrlProvider.JoinUrl(actionSrc, pageUrlNoQuery, pagePath, siteRootUrl);

					//====== Delete invalid character such as tab and line feed ======
					actionSrc = UrlProvider.IgnoreInvalidUrlCharctersInHtml(actionSrc);

					//===== If another site url, has bookmark=====
					if (actionSrc.IndexOf('#') != -1)
						actionSrc = UrlBuilder.RemoveUrlBookmark(actionSrc, out tmp);

					// Get clear url
					actionSrc = HttpUtility.HtmlDecode(actionSrc);

					//====== Encode url to make it unknown ======
					if (encodeUrl)
						actionSrc = UrlProvider.EncodeUrl(actionSrc);
					else
						// just url safe
						actionSrc = UrlProvider.EscapeUrlQuery(actionSrc);

					//====== Add it to our url ======
					actionSrc = string.Format(newPageFormat, actionSrc);

					// Make it safe
					actionSrc = HttpUtility.HtmlEncode(actionSrc);

					//====== Replace it with old url ======
					htmlCodes = htmlCodes.Remove(attr2Result.Start, attr2Result.End - attr2Result.Start);
					htmlCodes = htmlCodes.Insert(attr2Result.Start, actionSrc);

				}
				else
				{
					if (attr2Result.Start != -1)
						cursorPos = attr2Result.Start;
					cursorPos = StringCompare.IndexOfMatchCase(ref htmlCodes, ">", cursorPos);
				}
			}
			while (attr2Result.Start != -1);
		}

		public static void ReplaceTwoAttributeTagsValue(ref string htmlCodes,
			string newValueFormat,
			bool encodeUrl,
			string tagName,
			string attr1,
			string attr1Value,
			string attr2)
		{
			TextRange attr1Result = new TextRange(-1, -1);
			TextRange attr2Result = new TextRange(-1, -1);
			int cursorPos = 0;
			string actionSrc = "";

			do
			{
				attr1Result = HtmlParser.GetTagAttributeValuePos(ref htmlCodes, tagName, attr1, cursorPos);
				if (attr1Result.Start > -1 && attr1Result.End > -1)
				{
					string tmpRelType = htmlCodes.Substring(attr1Result.Start, attr1Result.End - attr1Result.Start);
					if (tmpRelType.Trim().ToLower() != attr1Value.Trim().ToLower())
					{
						if (attr1Result.Start != -1)
							cursorPos = attr1Result.Start;
						continue;
					}
				}
				else
				{
					break;
				}

				attr2Result = HtmlParser.GetTagAttributeValuePos(ref htmlCodes, tagName, attr2, cursorPos);

				if (attr2Result.Start == -1) break;

				if (attr2Result.Start > -1 && attr2Result.End > -1)
				{
					cursorPos = attr2Result.Start;

					//====== Correct value position according to quotes existence=======
					attr2Result = HtmlTags.CorrectValueIfQuoteExists(ref htmlCodes, attr2Result);

					// Get the value
					actionSrc = htmlCodes.Substring(attr2Result.Start, attr2Result.End - attr2Result.Start);

					// Get clear url
					actionSrc = HttpUtility.HtmlDecode(actionSrc);

					//====== Encode url to make it unknown ======
					if (encodeUrl)
						actionSrc = UrlProvider.EncodeUrl(actionSrc);
					else
						// just url safe
						actionSrc = UrlProvider.EscapeUrlQuery(actionSrc);

					//====== Add it to our url ======
					actionSrc = string.Format(newValueFormat, actionSrc);

					// Make it safe
					actionSrc = HttpUtility.HtmlEncode(actionSrc);

					//====== Replace it with old url ======
					htmlCodes = htmlCodes.Remove(attr2Result.Start, attr2Result.End - attr2Result.Start);
					htmlCodes = htmlCodes.Insert(attr2Result.Start, actionSrc);

				}
				else
				{
					if (attr2Result.Start != -1)
						cursorPos = attr2Result.Start;
					cursorPos = StringCompare.IndexOfMatchCase(ref htmlCodes, ">", cursorPos);
				}
			}
			while (attr2Result.Start != -1);
		}


		/// <summary>
		/// Find "BASE" tag and return "HREF" attrribute value, then remove the tag "HREF" attribute.
		/// </summary>
		/// <param name="hrefPath">href value</param>
		/// <returns>return if Base tag found or not</returns>
		public static bool ReplaceBaseSources(ref string pageHtml,
			bool removeHREFAttribute,
			out string hrefPath)
		{
			string tagName = "<base";
			string attributeName = "href";
			TextRange position;
			string oldValue;
			hrefPath = "";


			// Find position of BASE tag
			position = HtmlParser.GetTagAttributeValuePos(ref pageHtml, tagName, attributeName, 0);

			// If any Base tag exists
			if (position.Start != -1 && position.End != -1)
			{

				//====== Get the attribute value ======
				oldValue = pageHtml.Substring(position.Start, position.End - position.Start);

				// Remove unwanted characters
				oldValue = HtmlTags.RemoveEscapeQuotesFromTagAttributeValue(oldValue);
				oldValue = HtmlTags.RemoveQuotesFromTagAttributeValue(oldValue);

				//===== If link is a bookmark don't change it =====
				if (oldValue.Length == 0 || oldValue.StartsWith("#"))
					return false;

				// Browsers law!! The base url should end with slash(/)
				if (oldValue[oldValue.Length - 1] != '/')
				{
					// If the entered url isn't a directory specified with (/)
					// try to find the end a base directory
					int lastI = oldValue.LastIndexOf('/');
					if (lastI == -1)
						return false;
					lastI++; // character lenght
					oldValue = oldValue.Substring(0, lastI);
				}


				// Set href value
				hrefPath = oldValue;

				//====== Replace it with old url ======
				if (removeHREFAttribute)
					pageHtml = pageHtml.Remove(position.Start, position.End - position.Start);

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Remove all the scripts from html code
		/// </summary>
		public static void RemoveScripts(ref string html)
		{
			HtmlParser.RemoveTagContent(ref html, "script", true);
			HtmlParser.RemoveTagOnly(ref html, "noscript");
		}
		#endregion

		#region <Specified> Raplace special tags

		public static void ReplaceTagsEvents(ref string codes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string pageUrl,
			string siteRootUrl,
			bool encodeUrl)
		{

			//const string regexString = @"(?><[A-Z][A-Z0-9]{0,15})(?>\s+[^>\s]+)*?\s*(?>on\w{1,15}\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
			const string regexString = @"\b(on(?<!\.on)[a-z]{2,20})\s*=\s*([\'""])?(?<URL>(?(2)(?(?<="")[^""]+|[^\']+)|[^\s""\'>]+))(?(2)\2|)";

			// any event name in tags which starts with "on" for example "onclick"
			Regex regex = new Regex(regexString,
				RegexOptions.Compiled | RegexOptions.IgnoreCase);


			// find matches
			MatchCollection mColl = regex.Matches(codes);

			// match collection
			for (int i = mColl.Count - 1; i >= 0; i--)
			{
				Match match = mColl[i];
				Group group = match.Groups["URL"];

				if (group != null)
				{
					// Get clear url
					string eventCode = group.Value;
					string eventCodeOrg = eventCode;

					// appy changes
					Processors.JSProcessor.Execute(ref eventCode,
						pageUrl,
						pageUrlNoQuery,
						pagePath,
						siteRootUrl);

					if (eventCodeOrg.Length != eventCode.Length)
					{
						// Apply the changes!
						codes = codes.Remove(group.Index, group.Length);
						codes = codes.Insert(group.Index, eventCode);
					}
				}
			}
		}

		public static void ReplaceFormsSources(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string siteRootUrl,
			bool encodeUrl,
			bool changeMethod,
			string extraAttributeFormat)
		{
			TextRange methodResult;// = new TextRange();
			TextRange actionResult;// = new TextRange();
			int cursorPos = 0;
			string newAttribute = "";
			string formMethod = "";
			string tmp, actionSrc = "";
			string orgValue = "";

			bool addNewAttribute = false;
			bool hasNewAttribute = false;

			if (!string.IsNullOrEmpty(extraAttributeFormat))
			{
				addNewAttribute = true;
				hasNewAttribute = true;
			}


			do
			{
				addNewAttribute = hasNewAttribute;

				if (changeMethod)
				{
					methodResult = HtmlParser.GetTagAttributeValuePos(ref htmlCodes, "<form", "method", cursorPos);
					if (methodResult.Start > -1 && methodResult.End > -1)
					{
						// get the method
						formMethod = htmlCodes.Substring(methodResult.Start, 2);

						// validate the method
						formMethod = WebMethods.DetectMethod(formMethod, WebMethods.DefaultMethods.GET);

						htmlCodes = htmlCodes.Remove(methodResult.Start, methodResult.End - methodResult.Start);
						htmlCodes = htmlCodes.Insert(methodResult.Start, "POST");
					}
					else
					{
						int formPos = StringCompare.IndexOfIgnoreCase(ref htmlCodes, "<form", cursorPos);
						int tagEnd;
						if (formPos != -1)
						{
							tagEnd = StringCompare.IndexOfMatchCase(ref htmlCodes, '>', formPos);
							if (tagEnd != -1)
							{
								htmlCodes = htmlCodes.Insert(tagEnd, " method=POST ");
							}
						}

						formMethod = WebMethods.GET;
					}
				}

				actionResult = HtmlParser.GetTagAttributeValuePos(ref htmlCodes, "<form", "action", cursorPos);

				if (actionResult.Start == -1) break;

				if (actionResult.Start > -1 && actionResult.End > -1)
				{
					cursorPos = actionResult.Start;


					//====== Correct value position according to quotes existence=======
					// actionResult = ASProxyFunctions.CorrectValueIfQuoteExists(ref pageHtml, actionResult);

					// Get the value
					actionSrc = htmlCodes.Substring(actionResult.Start, actionResult.End - actionResult.Start);

					// BUG fixed in v5 beta 2
					// now supports forms with javascript
					if (UrlProvider.IsClientSitdeUrl(actionSrc) == false)
					{

						//====== Convert virtual url to absolute ======
						actionSrc = UrlProvider.JoinUrl(actionSrc, pageUrlNoQuery, pagePath, siteRootUrl);

						//====== Delete invalid character such as tab and line feed ======
						actionSrc = UrlProvider.IgnoreInvalidUrlCharctersInHtml(actionSrc);

						orgValue = actionSrc;

						//===== If another site url, has bookmark=====
						if (actionSrc.IndexOf('#') != -1)
							actionSrc = UrlBuilder.RemoveUrlBookmark(actionSrc, out tmp);

						//=====Get desired address=======
						actionSrc = HttpUtility.HtmlDecode(actionSrc);

						//====== Encode url to make unknown it ======
						if (encodeUrl)
							actionSrc = UrlProvider.EncodeUrl(actionSrc);
						else
							// just url safe
							actionSrc = UrlProvider.EscapeUrlQuery(actionSrc);

						//====== Add it to our url ======
						actionSrc = string.Format(newPageFormat, actionSrc);

						if (changeMethod)
							//actionSrc = UrlBuilder.AddUrlQuery(actionSrc, Consts.qIsPostForm, ((int)method).ToString());
							actionSrc = UrlBuilder.AddUrlQueryToEnd(actionSrc, Consts.Query.WebMethod, formMethod);


						// Make it html safe
						actionSrc = HttpUtility.HtmlEncode(actionSrc);

						//====== Replace it with old url ======
						htmlCodes = htmlCodes.Remove(actionResult.Start, actionResult.End - actionResult.Start);
						htmlCodes = htmlCodes.Insert(actionResult.Start, actionSrc);
					}
					else
					{
						// this is client side url
						addNewAttribute = false;
					}


					if (addNewAttribute)
					{
						// Apply orginal value and encoded value to format
						newAttribute = string.Format(extraAttributeFormat, orgValue, actionSrc, "POST");

						// Locate end of tag
						cursorPos = StringCompare.IndexOfMatchCase(ref htmlCodes, '>', actionResult.Start);
						if (htmlCodes[cursorPos - 1] == '/')
							cursorPos--;

						// Insert to it
						htmlCodes = htmlCodes.Insert(cursorPos, newAttribute);
					}

				}
				else
				{
					if (actionResult.Start != -1)
						cursorPos = actionResult.Start;
					cursorPos = StringCompare.IndexOfMatchCase(ref htmlCodes, ">", cursorPos);
				}
			}
			while (actionResult.Start != -1);
		}


		/// <summary>
		/// Replaces head http refresh codes
		/// </summary>
		public static void ReplaceHttpRefresh(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string rootUrl,
			bool encodeUrl)
		{
			const string tag = "<meta";
			const string attr1 = "http-equiv";
			const string attr1Value = "refresh";
			const string attr2 = "content";
			const string contentUrlTag = "url=";

			TextRange attr1Result = new TextRange(-1, -1);
			TextRange attr2Result = new TextRange(-1, -1);
			int cursorPos = 0;
			string tmp, actionSrc = "";

			do
			{
				// Find position of Meta tag and HTTP-EQUIV attribute
				attr1Result = HtmlParser.GetTagAttributeValuePos(ref htmlCodes, tag, attr1, cursorPos);
				if (attr1Result.Start > -1 && attr1Result.End > -1)
				{
					string tmpRelType = htmlCodes.Substring(attr1Result.Start, attr1Result.End - attr1Result.Start);
					if (tmpRelType.Trim().ToLower() != attr1Value.Trim().ToLower())
					{
						if (attr1Result.Start != -1)
							cursorPos = attr1Result.Start;
						continue;
					}
				}
				else
				{
					break;
				}

				// Find position of CONTENT attribute
				attr2Result = HtmlParser.GetTagAttributeValuePos(ref htmlCodes, tag, attr2, cursorPos);

				if (attr2Result.Start == -1) break;

				if (attr2Result.Start > -1 && attr2Result.End > -1)
				{
					cursorPos = attr2Result.Start;

					// Get CONTENT attribute
					actionSrc = htmlCodes.Substring(attr2Result.Start, attr2Result.End - attr2Result.Start);

					// Some time CONTENT contains url address and refresh time, take them apart
					string[] contentParts = actionSrc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					// If the CONTENT does not contain any url address, no changes needed!
					if (contentParts.Length < 2)
					{
						cursorPos = attr2Result.Start;
						// next one!!
						continue;
					}

					// remove Url= from CONTENT
					actionSrc = contentParts[1].Trim();
					if (actionSrc.ToLower().StartsWith(contentUrlTag))
						actionSrc = actionSrc.Substring(contentUrlTag.Length, actionSrc.Length - contentUrlTag.Length);

					// Convert virtual url to absolute 
					actionSrc = UrlProvider.JoinUrl(actionSrc, pageUrlNoQuery, pagePath, rootUrl);

					// Delete invalid character such as tab and line feed ======
					actionSrc = UrlProvider.IgnoreInvalidUrlCharctersInHtml(actionSrc);

					// If there is a bookmark
					if (actionSrc.IndexOf('#') != -1)
						actionSrc = UrlBuilder.RemoveUrlBookmark(actionSrc, out tmp);

					// Get clear url
					actionSrc = HttpUtility.HtmlDecode(actionSrc);

					// Encode url to make it unknown
					if (encodeUrl)
						actionSrc = UrlProvider.EncodeUrl(actionSrc);
					else
						// just url safe
						actionSrc = UrlProvider.EscapeUrlQuery(actionSrc);

					// Add it to our url
					actionSrc = string.Format(newPageFormat, actionSrc);

					// Make CONTENT attribute state
					actionSrc = contentParts[0] + ';' + contentUrlTag + actionSrc;

					// Make it safe
					actionSrc = HttpUtility.HtmlEncode(actionSrc);

					// Replace it with old url
					htmlCodes = htmlCodes.Remove(attr2Result.Start, attr2Result.End - attr2Result.Start);
					htmlCodes = htmlCodes.Insert(attr2Result.Start, actionSrc);

				}
				else
				{
					if (attr2Result.Start != -1)
						cursorPos = attr2Result.Start;
					cursorPos = StringCompare.IndexOfMatchCase(ref htmlCodes, ">", cursorPos);
				}
			}
			while (attr2Result.Start != -1);
		}
		#endregion

		#region Html direct processing functions

		/// <summary>
		/// processes css style sheet links
		/// </summary>
		public static void ReplaceCssLinks(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string rootUrl,
			bool encodeUrl)
		{
			ReplaceTwoAttributeTagsValue(ref htmlCodes,
				pageUrlNoQuery,
				newPageFormat,
				pagePath,
				rootUrl,
				encodeUrl,
				"<link",
				"rel",
				"stylesheet",
				"href");
		}

		/// <summary>
		/// Processes links 
		/// </summary>
		public static void ReplaceAnchors(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string rootUrl,
			bool encodeUrl,
			string extraAttributeFormat)
		{
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<a",
				"href",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				extraAttributeFormat,
				true);
		}

		/// <summary>
		/// Processes iframes
		/// </summary>
		public static void ReplaceIFrames(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string rootUrl,
			bool encodeUrl,
			string extraAttributeFormat)
		{
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<iframe",
				"src",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				extraAttributeFormat,
				true);
		}

		/// <summary>
		/// Processes framesets
		/// </summary>
		public static void ReplaceFrameSets(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string rootUrl,
			bool encodeUrl)
		{
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<frame ",
				"src",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				"",
				true);
		}

		/// <summary>
		/// Processes image tags
		/// </summary>
		public static void ReplaceImages(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string rootUrl,
			bool encodeUrl,
			string extraAttributeFormat)
		{
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<img",
				"src",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				extraAttributeFormat,
				true);
		}

		/// <summary>
		/// Processes script tags sources "src"
		/// </summary>
		public static void ReplaceScripts(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string rootUrl,
			bool encodeUrl)
		{
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<script",
				"src",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				"",
				false);
		}

		/// <summary>
		/// Processes background images
		/// </summary>
		public static void ReplaceBackgrounds(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string rootUrl,
			bool encodeUrl)
		{
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<table",
				"background",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				"",
				true);
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<th",
				"background",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				"",
				true);
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<td",
				"background",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				"",
				true);
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<body",
				"background",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				"",
				true);
		}

		/// <summary>
		/// Processes embeded medias
		/// </summary>
		public static void ReplaceEmbeds(ref string htmlCodes,
			string pageUrlNoQuery,
			string newPageFormat,
			string pagePath,
			string rootUrl,
			bool encodeUrl)
		{
			ReplaceTagSrcAttribute(ref htmlCodes,
				pageUrlNoQuery,
				"<embed",
				"src",
				pagePath,
				newPageFormat,
				rootUrl,
				encodeUrl,
				"",
				true);
		}
		#endregion

		#region Replace tag attributes
		public static void ReplaceAspDotNETViewState(ref string htmlCodes)
		{
			ReplaceTwoAttributeTagsValue(ref htmlCodes,
				Consts.DataProccessing.ASPDotNETRenamedViewState,
				false,
				"<input",
				"name",
				Consts.DataProccessing.ASPDotNETViewState,
				"name");
		}

		#endregion

	}
}
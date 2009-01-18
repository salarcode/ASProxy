using System;
using System.Web;

namespace SalarSoft.ASProxy
{

	public class HtmlReplacer
	{
		#region Replace tag attributes value BASE functions
		private static void ReplaceTagSrcAttribute(ref string pageHtml, string currentUrlWithoutParameters, string tagName, string attributeName, string newPagePath, string newPageFormat, string siteBasePath, bool encodeUrl, string newAttributeValueFormat, bool canEncloseWithTags)
		{
			int index = 0;//====== In first run, index must be Zero ======
			TextRange position;
			string oldValue, newValue;
			string orgValue = "";
			string bookmarkPart = "";
			string newAttribute = "";
			bool addNewAttribute = false;
			bool hasNewAttribute = false;

			if (!string.IsNullOrEmpty(newAttributeValueFormat))
			{
				addNewAttribute = true;
				hasNewAttribute = true;
			}

			do
			{
				addNewAttribute = hasNewAttribute;

				position = HtmlParser.GetTagAttributeValuePos(ref pageHtml, tagName, attributeName, index);

				if (position.Start == -1)
					break;

				// If requested, test statement that shouldn't enclose with specified tags
				bool continueLoop = true;
				if (canEncloseWithTags && position.Start != -1 && position.End != -1)
					continueLoop = !HtmlTags.IsEnclosedBy(ref pageHtml, position.Start, "<script", "</script>");

				if (continueLoop && position.Start != -1 && position.End != -1 && position.End > position.Start)
				{
					//======OK. go to end of tag=============
					index = Performance.IndexOfMatchCase(ref pageHtml, '>', position.Start);

					// Replace new address

					//====== Correct value position according to quotes existence=======
					//position = ASProxyFunctions.CorrectValueIfQuoteExists(ref pageHtml, position);

					//====== Get the attribute value ======
					oldValue = pageHtml.Substring(position.Start, position.End - position.Start);

					oldValue = HtmlTags.RemoveEscapeQuotesFromTagAttributeValue(oldValue);

					oldValue = HtmlTags.RemoveQuotesFromTagAttributeValue(oldValue);

					//===== If link is a bookmark don't change it=====
					if (oldValue.StartsWith("#"))
						continue;

					if (UrlProvider.IsClientSitdeUrl(oldValue) == false)
					{
						//====== Convert virtual url to absolute ======
						oldValue = UrlProvider.JoinUrl(oldValue, currentUrlWithoutParameters, newPagePath, siteBasePath);

						//====== Delete invalid character such as tab and line feed ======
						oldValue = UrlProvider.IgnoreInvalidUrlCharctersInHtml(oldValue);

						// Save orginal value
						orgValue = oldValue;

						//===== If another site url, has bookmark
						if (oldValue.IndexOf('#') != -1)
							oldValue = UrlProvider.RemoveUrlBookmark(oldValue, out bookmarkPart);


						//====== Get desigred url addrress
						oldValue = HttpUtility.HtmlDecode(oldValue);

						//====== Encode url to make it unknown ======
						if (encodeUrl)
						{
							oldValue = UrlProvider.EncodeUrl(oldValue);
						}

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
					pageHtml = pageHtml.Remove(position.Start, position.End - position.Start);
					pageHtml = pageHtml.Insert(position.Start, newValue);


					if (addNewAttribute)
					{
						// Apply orginal value and encoded value to format
						// BUG: Problem with format string that contain (') or (") characters
						// Bug Fixed since version 4.7
						newAttribute = string.Format(newAttributeValueFormat, orgValue, newValue);

						// Locate end of tag
						index = Performance.IndexOfMatchCase(ref pageHtml, '>', position.Start);
						if (pageHtml[index - 1] == '/')
							index--;

						// Insert to tag
						pageHtml = pageHtml.Insert(index, newAttribute);
					}

					//===============End of Replace new address =========
				}
				else
				{
					if (position.Start != -1)
						index = position.Start;
					index = Performance.IndexOfMatchCase(ref pageHtml, '>', index);
				}

			}
			while ((index != -1));
		}
		public static void ReplaceTwoAttributeTagsValue(ref string pageHtml, string currentUrlWithoutParameters, string newPageFormat, string newPagePath, string siteBasePath, bool encodeUrl, string tag, string attr1, string attr1Value, string attr2)
		{
			TextRange attr1Result = new TextRange();
			TextRange attr2Result = new TextRange();
			int cursorPos = 0;
			string tmp, actionSrc = "";

			do
			{
				attr1Result = HtmlParser.GetTagAttributeValuePos(ref pageHtml, tag, attr1, cursorPos);
				if (attr1Result.Start > -1 && attr1Result.End > -1)
				{
					string tmpRelType = pageHtml.Substring(attr1Result.Start, attr1Result.End - attr1Result.Start);
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

				attr2Result = HtmlParser.GetTagAttributeValuePos(ref pageHtml, tag, attr2, cursorPos);

				if (attr2Result.Start == -1) break;

				if (attr2Result.Start > -1 && attr2Result.End > -1)
				{
					cursorPos = attr2Result.Start;

					//====== Correct value position according to quotes existence=======
					attr2Result = HtmlTags.CorrectValueIfQuoteExists(ref pageHtml, attr2Result);

					// Get the value
					actionSrc = pageHtml.Substring(attr2Result.Start, attr2Result.End - attr2Result.Start);


					//====== Convert virtual url to absolute ======
					actionSrc = UrlProvider.JoinUrl(actionSrc, currentUrlWithoutParameters, newPagePath, siteBasePath);

					//====== Delete invalid character such as tab and line feed ======
					actionSrc = UrlProvider.IgnoreInvalidUrlCharctersInHtml(actionSrc);

					//===== If another site url, has bookmark=====
					if (actionSrc.IndexOf('#') != -1)
						actionSrc = UrlProvider.RemoveUrlBookmark(actionSrc, out tmp);

					// Get clear url
					actionSrc = HttpUtility.HtmlDecode(actionSrc);

					//====== Encode url to make it unknown ======
					if (encodeUrl)
						actionSrc = UrlProvider.EncodeUrl(actionSrc);

					//====== Add it to our url ======
					actionSrc = string.Format(newPageFormat, actionSrc);

					// Make it safe
					actionSrc = HttpUtility.HtmlEncode(actionSrc);

					//====== Replace it with old url ======
					pageHtml = pageHtml.Remove(attr2Result.Start, attr2Result.End - attr2Result.Start);
					pageHtml = pageHtml.Insert(attr2Result.Start, actionSrc);

				}
				else
				{
					if (attr2Result.Start != -1)
						cursorPos = attr2Result.Start;
					cursorPos = Performance.IndexOfMatchCase(ref pageHtml, ">", cursorPos);
				}
			}
			while (attr2Result.Start != -1);
		}

		public static void ReplaceTwoAttributeTagsValue(ref string pageHtml, string newValueFormat, bool encodeUrl, string tag, string attr1, string attr1Value, string attr2)
		{
			TextRange attr1Result = new TextRange();
			TextRange attr2Result = new TextRange();
			int cursorPos = 0;
			string actionSrc = "";

			do
			{
				attr1Result = HtmlParser.GetTagAttributeValuePos(ref pageHtml, tag, attr1, cursorPos);
				if (attr1Result.Start > -1 && attr1Result.End > -1)
				{
					string tmpRelType = pageHtml.Substring(attr1Result.Start, attr1Result.End - attr1Result.Start);
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

				attr2Result = HtmlParser.GetTagAttributeValuePos(ref pageHtml, tag, attr2, cursorPos);

				if (attr2Result.Start == -1) break;

				if (attr2Result.Start > -1 && attr2Result.End > -1)
				{
					cursorPos = attr2Result.Start;

					//====== Correct value position according to quotes existence=======
					attr2Result = HtmlTags.CorrectValueIfQuoteExists(ref pageHtml, attr2Result);

					// Get the value
					actionSrc = pageHtml.Substring(attr2Result.Start, attr2Result.End - attr2Result.Start);

					// Get clear url
					actionSrc = HttpUtility.HtmlDecode(actionSrc);

					//====== Encode url to make it unknown ======
					if (encodeUrl)
						actionSrc = UrlProvider.EncodeUrl(actionSrc);

					//====== Add it to our url ======
					actionSrc = string.Format(newValueFormat, actionSrc);

					// Make it safe
					actionSrc = HttpUtility.HtmlEncode(actionSrc);

					//====== Replace it with old url ======
					pageHtml = pageHtml.Remove(attr2Result.Start, attr2Result.End - attr2Result.Start);
					pageHtml = pageHtml.Insert(attr2Result.Start, actionSrc);

				}
				else
				{
					if (attr2Result.Start != -1)
						cursorPos = attr2Result.Start;
					cursorPos = Performance.IndexOfMatchCase(ref pageHtml, ">", cursorPos);
				}
			}
			while (attr2Result.Start != -1);
		}


		/// <summary>
		/// Find "BASE" tag and return "HREF" attrribute value, then remove the tag "HREF" attribute.
		/// </summary>
		/// <param name="hrefPath">href value</param>
		/// <returns>return if Base tag found or not</returns>
		public static bool ReplaceBaseSources(ref string pageHtml, bool removeHREFAttribute, out string hrefPath)
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

				//===== If link is a bookmark don't change it=====
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
			HtmlParser.RemoveTag(ref html, "script", true);
		}
		#endregion




		#region Replace tag attributes
		public static void ReplaceAspDotNETViewState(ref string htmlbody)
		{
			ReplaceTwoAttributeTagsValue(ref htmlbody, Consts.ASPDotNETRenamedViewState, false, "<input", "name", Consts.ASPDotNETViewState, "name");
		}


		public static void ReplaceScriptSources(ref string htmlbody, string currentUrlWithoutParameters, string ScriptSource, string oldABasePath, string siteBasePath, bool encodeurl)
		{
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.ScriptStartTag, HtmlTags.ScriptSourceTag, oldABasePath, ScriptSource, siteBasePath, encodeurl, "", false);
		}
		public static void ReplaceAnchorSources(ref string htmlbody, string currentUrlWithoutParameters, string AnchorSource, string oldABasePath, string siteBasePath, bool encodeurl, string newAttributeValueFormat)
		{
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.AnchorStartTag, HtmlTags.AnchorSourceTag, oldABasePath, AnchorSource, siteBasePath, encodeurl, newAttributeValueFormat, true);
		}
		public static void ReplaceIFrameSources(ref string htmlbody, string currentUrlWithoutParameters, string FrameSource, string oldABasePath, string siteBasePath, bool encodeurl, string newAttributeValueFormat)
		{
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.IFrameStartTag, HtmlTags.IFrameSourceTag, oldABasePath, FrameSource, siteBasePath, encodeurl, newAttributeValueFormat, true);
		}
		public static void ReplaceFrameSetSources(ref string htmlbody, string currentUrlWithoutParameters, string FrameSetSource, string oldABasePath, string siteBasePath, bool encodeurl)
		{
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.FrameSetStartTag, HtmlTags.FrameSetSourceTag, oldABasePath, FrameSetSource, siteBasePath, encodeurl, "", true);
		}
		public static void ReplaceImageSources(ref string htmlbody, string currentUrlWithoutParameters, string imgsource, string oldImgBasePath, string siteBasePath, bool encodeurl, string newAttributeValueFormat)
		{
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.ImgStartTag, HtmlTags.ImgSourceTag, oldImgBasePath, imgsource, siteBasePath, encodeurl, newAttributeValueFormat, true);
		}
		public static void ReplaceBackgroundSources(ref string htmlbody, string currentUrlWithoutParameters, string backsource, string oldBackBasePath, string siteBasePath, bool encodeurl)
		{
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.TableStartTag, HtmlTags.TableBackImgSourceTag, oldBackBasePath, backsource, siteBasePath, encodeurl, "", true);
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.THeaderStartTag, HtmlTags.THeaderBackImgSourceTag, oldBackBasePath, backsource, siteBasePath, encodeurl, "", true);
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.TCellStartTag, HtmlTags.TCellBackImgSourceTag, oldBackBasePath, backsource, siteBasePath, encodeurl, "", true);
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.BodyStartTag, HtmlTags.BodyBackImgSourceTag, oldBackBasePath, backsource, siteBasePath, encodeurl, "", true);
		}
		public static void ReplaceEmbedSources(ref string htmlbody, string currentUrlWithoutParameters, string embedsource, string oldEmbednBasePath, string siteBasePath, bool encodeurl)
		{
			ReplaceTagSrcAttribute(ref htmlbody, currentUrlWithoutParameters, HtmlTags.EmbedStartTag, HtmlTags.EmbedSourceTag, oldEmbednBasePath, embedsource, siteBasePath, encodeurl, "", true);
		}

		public static void ReplaceCssLinkSources(ref string pageHtml, string currentUrlWithoutParameters, string newPageFormat, string newPagePath, string siteBasePath, bool encodeUrl)
		{
			ReplaceTwoAttributeTagsValue(ref pageHtml, currentUrlWithoutParameters, newPageFormat, newPagePath, siteBasePath, encodeUrl, "<link", "rel", "stylesheet", "href");
		}
		public static void ReplaceFormsSources(ref string pageHtml, string currentUrlWithoutParameters, string newPageFormat, string newPagePath, string siteBasePath, bool encodeUrl, bool changeMethod, string newAttributeValueFormat)
		{
			TextRange methodResult = new TextRange();
			TextRange actionResult = new TextRange();
			int cursorPos = 0;
			string newAttribute = "";
			string formMethod = "";
			string tmp, actionSrc = "";
			string orgValue = "";

			bool addNewAttribute = false;
			bool hasNewAttribute = false;

			if (!string.IsNullOrEmpty(newAttributeValueFormat))
			{
				addNewAttribute = true;
				hasNewAttribute = true;
			}


			do
			{
				addNewAttribute = hasNewAttribute;

				if (changeMethod)
				{
					methodResult = HtmlParser.GetTagAttributeValuePos(ref pageHtml, "<form", "method", cursorPos);
					if (methodResult.Start > -1 && methodResult.End > -1)
					{
						// get the method
						formMethod = pageHtml.Substring(methodResult.Start, 2);

						// validate the method
						formMethod = WebMethods.DetectMethod(formMethod, WebMethods.DefaultMethods.GET);

						pageHtml = pageHtml.Remove(methodResult.Start, methodResult.End - methodResult.Start);
						pageHtml = pageHtml.Insert(methodResult.Start, "POST");
					}
					else
					{
						int formPos = Performance.IndexOfIgnoreCase(ref pageHtml, "<form", cursorPos);
						int tagEnd;
						if (formPos != -1)
						{
							tagEnd = Performance.IndexOfMatchCase(ref pageHtml, '>', formPos);
							if (tagEnd != -1)
							{
								pageHtml = pageHtml.Insert(tagEnd, " method=POST ");
							}
						}

						formMethod = WebMethods.GET;
					}
				}

				actionResult = HtmlParser.GetTagAttributeValuePos(ref pageHtml, "<form", "action", cursorPos);

				if (actionResult.Start == -1) break;

				if (actionResult.Start > -1 && actionResult.End > -1)
				{
					cursorPos = actionResult.Start;


					//====== Correct value position according to quotes existence=======
					// actionResult = ASProxyFunctions.CorrectValueIfQuoteExists(ref pageHtml, actionResult);

					// Get the value
					actionSrc = pageHtml.Substring(actionResult.Start, actionResult.End - actionResult.Start);

					// BUG fixed in v5 beta 2
					// now supports forms with javascript
					if (UrlProvider.IsClientSitdeUrl(actionSrc) == false)
					{

						//====== Convert virtual url to absolute ======
						actionSrc = UrlProvider.JoinUrl(actionSrc, currentUrlWithoutParameters, newPagePath, siteBasePath);

						//====== Delete invalid character such as tab and line feed ======
						actionSrc = UrlProvider.IgnoreInvalidUrlCharctersInHtml(actionSrc);

						orgValue = actionSrc;

						//===== If another site url, has bookmark=====
						if (actionSrc.IndexOf('#') != -1)
							actionSrc = UrlProvider.RemoveUrlBookmark(actionSrc, out tmp);

						//=====Get desired address=======
						actionSrc = HttpUtility.HtmlDecode(actionSrc);

						//====== Encode url to make unknown it ======
						if (encodeUrl)
							actionSrc = UrlProvider.EncodeUrl(actionSrc);

						//====== Add it to our url ======
						actionSrc = string.Format(newPageFormat, actionSrc);

						if (changeMethod)
							//actionSrc = UrlBuilder.AddUrlQuery(actionSrc, Consts.qIsPostForm, ((int)method).ToString());
							actionSrc = UrlBuilder.AddUrlQueryToEnd(actionSrc, Consts.qWebMethod, formMethod);


						// Make it html safe
						actionSrc = HttpUtility.HtmlEncode(actionSrc);

						//====== Replace it with old url ======
						pageHtml = pageHtml.Remove(actionResult.Start, actionResult.End - actionResult.Start);
						pageHtml = pageHtml.Insert(actionResult.Start, actionSrc);
					}
					else
					{
						// this is client side url
						addNewAttribute = false;
					}


					if (addNewAttribute)
					{
						// Apply orginal value and encoded value to format
						newAttribute = string.Format(newAttributeValueFormat, orgValue, actionSrc, "POST");

						// Locate end of tag
						cursorPos = Performance.IndexOfMatchCase(ref pageHtml, '>', actionResult.Start);
						if (pageHtml[cursorPos - 1] == '/')
							cursorPos--;

						// Insert to it
						pageHtml = pageHtml.Insert(cursorPos, newAttribute);
					}

				}
				else
				{
					if (actionResult.Start != -1)
						cursorPos = actionResult.Start;
					cursorPos = Performance.IndexOfMatchCase(ref pageHtml, ">", cursorPos);
				}
			}
			while (actionResult.Start != -1);
		}
		public static void ReplaceHttpRefreshSources(ref string pageHtml, string currentUrlWithoutParameters, string newPageFormat, string newPagePath, string siteBasePath, bool encodeUrl)
		{
			const string tag = "<meta";
			const string attr1 = "http-equiv";
			const string attr1Value = "refresh";
			const string attr2 = "content";
			const string contentUrlTag = "url=";

			TextRange attr1Result = new TextRange();
			TextRange attr2Result = new TextRange();
			int cursorPos = 0;
			string tmp, actionSrc = "";

			do
			{
				// Find position of Meta tag and HTTP-EQUIV attribute
				attr1Result = HtmlParser.GetTagAttributeValuePos(ref pageHtml, tag, attr1, cursorPos);
				if (attr1Result.Start > -1 && attr1Result.End > -1)
				{
					string tmpRelType = pageHtml.Substring(attr1Result.Start, attr1Result.End - attr1Result.Start);
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
				attr2Result = HtmlParser.GetTagAttributeValuePos(ref pageHtml, tag, attr2, cursorPos);

				if (attr2Result.Start == -1) break;

				if (attr2Result.Start > -1 && attr2Result.End > -1)
				{
					cursorPos = attr2Result.Start;

					// Get CONTENT attribute
					actionSrc = pageHtml.Substring(attr2Result.Start, attr2Result.End - attr2Result.Start);

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
					actionSrc = UrlProvider.JoinUrl(actionSrc, currentUrlWithoutParameters, newPagePath, siteBasePath);

					// Delete invalid character such as tab and line feed ======
					actionSrc = UrlProvider.IgnoreInvalidUrlCharctersInHtml(actionSrc);

					// If there is a bookmark
					if (actionSrc.IndexOf('#') != -1)
						actionSrc = UrlProvider.RemoveUrlBookmark(actionSrc, out tmp);

					// Get clear url
					actionSrc = HttpUtility.HtmlDecode(actionSrc);

					// Encode url to make it unknown
					if (encodeUrl)
						actionSrc = UrlProvider.EncodeUrl(actionSrc);

					// Add it to our url
					actionSrc = string.Format(newPageFormat, actionSrc);

					// Make CONTENT attribute state
					actionSrc = contentParts[0] + ';' + contentUrlTag + actionSrc;

					// Make it safe
					actionSrc = HttpUtility.HtmlEncode(actionSrc);

					// Replace it with old url
					pageHtml = pageHtml.Remove(attr2Result.Start, attr2Result.End - attr2Result.Start);
					pageHtml = pageHtml.Insert(attr2Result.Start, actionSrc);

				}
				else
				{
					if (attr2Result.Start != -1)
						cursorPos = attr2Result.Start;
					cursorPos = Performance.IndexOfMatchCase(ref pageHtml, ">", cursorPos);
				}
			}
			while (attr2Result.Start != -1);
		}
		#endregion




	}
}
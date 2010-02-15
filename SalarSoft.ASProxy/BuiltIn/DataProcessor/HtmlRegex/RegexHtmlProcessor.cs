using System;
using System.Text;
using SalarSoft.ASProxy.Exposed;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Specialized;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class RegexHtmlProcessor : ExHtmlProcessor
	{
		private UserOptions _UserOptions;

		public RegexHtmlProcessor()
		{
			_UserOptions = CurrentContext.UserOptions;
		}

		public override string Execute()
		{
			Encoding _encoding;

			string resultHtml = StringStream.GetString(
				WebData.ResponseData,
				WebData.ResponseInfo.ContentType,
				_UserOptions.ForceEncoding,
				true,
				out _encoding);

			ContentEncoding = _encoding;

			if (_UserOptions.Frames)
				IsFrameSet = HtmlTags.IsFramesetHtml(ref resultHtml);

			if (_UserOptions.PageTitle)
				PageTitle = HtmlParser.GetTagContent(ref resultHtml, "title");

			if (_UserOptions.DocType)
				DocType = HtmlTags.GetDocType(ref resultHtml);


			// Page url. E.G. http://Site.com/users/profile.aspx?uid=90
			string pageUrl = WebData.ResponseInfo.ResponseUrl;

			// this is page path, used in processing relative paths in source html
			// for example the pageRootUrl for "http://Site.com/users/profile.aspx" will be "http://Site.com/users/"
			// gets page root Url
			string pagePath = UrlProvider.GetPagePath(pageUrl);

			// the page Url without any query parameter, used in processing relative query parameters
			// the pageUrlNoQuery for "http://Site.com/profile.aspx?uid=90" will be "http://Site.com/profile.aspx"
			// Gets page Url without any query parameter
			string pageUrlNoQuery = UrlProvider.GetPageAbsolutePath(pageUrl);

			// Execute the result
			Execute(ref resultHtml,
				pageUrl,
				pageUrlNoQuery,
				pagePath,
				WebData.ResponseInfo.ResponseRootUrl);

			// the result
			return resultHtml;
		}

		public override void Execute(ref string codes, string pageUrl, string pageUrlNoQuery, string pagePath, string rootUrl)
		{
			try
			{
				// 1- executing plugins
				if (Plugins.IsPluginAvailable(PluginHosts.IPluginHtmlProcessor))
					Plugins.CallPluginMethod(PluginHosts.IPluginHtmlProcessor,
						PluginMethods.IPluginHtmlProcessor.BeforeExecute,
						this, (object)codes, pageUrl, pageUrlNoQuery, pagePath, rootUrl);

				// ASProxy pages url formats generator
				ASProxyPagesFormat pages = new ASProxyPagesFormat(_UserOptions.EncodeUrl);

				// Original urls addistional codes option
				bool orginalUrlRequired = false;
				string regexPattarn;


				// Renames ASPDotNET standard ViewState name to a temporary name
				// This name will reset to default when the page posted back
				HtmlReplacer.ReplaceAspDotNETViewState(ref codes);


				// If remove scripts chosen, remove all the scripts.
				if (_UserOptions.RemoveScripts)
					HtmlReplacer.RemoveScripts(ref codes);

				// If remove embeded objects is requested
				if (_UserOptions.RemoveObjects)
				{
					// Removing <object> tag
					HtmlParser.RemoveTagContent(ref codes, "object", true);

					// Removing <embed> tag
					HtmlParser.RemoveTagContent(ref codes, "embed", true);
				}


				// Applying the BASE tag to the URLs.
				string baseHref;
				if (GetBaseTagSource(ref codes, out baseHref))
				{
					// changing page base path to specified Base in the document
					pagePath = UrlProvider.AddSlashToEnd(baseHref);

					// site root url should change also
					rootUrl = UrlProvider.GetRootPath(pagePath);
				}

				// splitting the code by scripts
				regexPattarn = @"<script\b[^>]*>(?:.*?)</script>";
				MatchCollection matchColl = Regex.Matches(codes,
												regexPattarn,
												RegexOptions.Compiled |
												RegexOptions.Singleline |
												RegexOptions.IgnoreCase);
				// check if is any script found
				if (matchColl.Count > 0)
				{
					// end of capture is end of html
					int endOfCapture = codes.Length;
					string codePart;
					int scriptEnd;
					bool processScripts = !_UserOptions.RemoveScripts;

					for (int i = matchColl.Count - 1; i >= 0; i--)
					{
						Match match = matchColl[i];
						scriptEnd = match.Index + match.Length;

						// grab the part
						codePart = codes.Substring(scriptEnd, endOfCapture - scriptEnd);

						// empty test
						if (codePart.Length > 0)
						{
							// Proccess the selected html part
							ProcessHtml(ref codePart, pageUrl, pageUrlNoQuery, pagePath, rootUrl, ref orginalUrlRequired, pages);

							// apply processed part
							codes = codes.Remove(scriptEnd, endOfCapture - scriptEnd);
							codes = codes.Insert(scriptEnd, codePart);
						}


						// removes scripts
						if (processScripts && match.Length > 0)
						{
							// grab the script part
							codePart = codes.Substring(match.Index, match.Length);

							// processes embeded scripts
							ReplaceEmbededScript(ref codePart,
									pageUrl,
									pageUrlNoQuery,
									pagePath,
									rootUrl);

							// Replaces scripts source 
							regexPattarn = @"(?><script)(?>\s+[^>\s]+)*?\s*(?>src\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
							ApplyToUrlSpecifed(ref codePart,
								regexPattarn,
								pageUrl,
								pageUrlNoQuery,
								pagePath,
								rootUrl,
								pages.PageJS,
								_UserOptions.EncodeUrl,
								null);

							// apply processed part
							codes = codes.Remove(match.Index, match.Length);
							codes = codes.Insert(match.Index, codePart);

						}


						// grab until this point
						endOfCapture = match.Index;
					}

					// and here we should process the reset of contents
					scriptEnd = 0;

					// grab the part
					codePart = codes.Substring(scriptEnd, endOfCapture - scriptEnd);

					// Proccess the selected html part
					ProcessHtml(ref codePart, pageUrl, pageUrlNoQuery, pagePath, rootUrl, ref orginalUrlRequired, pages);

					// apply processed part
					codes = codes.Remove(scriptEnd, endOfCapture - scriptEnd);
					codes = codes.Insert(scriptEnd, codePart);

					// releasing memory
					codePart = null;

				}
				else
				{
					// Proccess the html
					ProcessHtml(ref codes, pageUrl, pageUrlNoQuery, pagePath, rootUrl, ref orginalUrlRequired, pages);
				}

				// Adding dynamic encoding javascript codes
				string jsEncoderCodes = GenerateJsEncoderCodes(pageUrl,
							pageUrlNoQuery,
							pagePath,
							rootUrl);

				// Add jsEncoder codes to page
				ExtraCodesForPage = jsEncoderCodes + ExtraCodesForPage;

				// OrginalUrl additional injection html codes
				if (orginalUrlRequired)
				{
					// Inject to html, should run after body tag
					ExtraCodesForBody = Resources.STR_OrginalUrl_Functions + ExtraCodesForBody;

					// TODO: Check necessary
					ExtraCodesForPage = Resources.STR_OrginalUrl_FloatBar + ExtraCodesForPage;
				}

				// 2- executing plugins
				if (Plugins.IsPluginAvailable(PluginHosts.IPluginHtmlProcessor))
					Plugins.CallPluginMethod(PluginHosts.IPluginHtmlProcessor,
						PluginMethods.IPluginHtmlProcessor.AfterExecute,
						this, (object)codes, pageUrl, pageUrlNoQuery, pagePath, rootUrl);

			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pageUrl);

				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "ASProxy has some errors! The delivered content may not work properly.";
			}
		}

		private void ProcessHtml(ref string codes, string pageUrl, string pageUrlNoQuery, string pagePath, string rootUrl, ref bool orginalUrlRequired, ASProxyPagesFormat pages)
		{
			string regexPattarn;

			// HttpRefresh
			// replaces <meta http-equiv="refresh" content=1;url=HttpRefresh.htm>
			// BUGFIX v5.5b2: Mono has some issue with "(?(UQ)\2|)(?(Q)\1|)", it is a backtrack to detect (") and (') after the URL, specially when we have something like: content="0;url='refresh.html'"
			// here is the original one, regexPattarn = @"<meta(?>\s+[^>\s]+)*?\s*content\s*=\s*(?<Q>[""'])?[0-9]+\s*;\s*url=(?<UQ>['""]|['])?(?<URL>(?(?<="")[^""]+|(?(?<=')[^']+|[^'"" >]+)))(?(UQ)\2|)(?(Q)\1|)";
			regexPattarn = @"<meta(?>\s+[^>\s]+)*?\s*content\s*=\s*(?<Q>[""'])?[0-9]+\s*;\s*url=(?<UQ>['""]|['])?(?<URL>(?(?<="")[^""]+|(?(?<=')[^']+|[^'"" >]+)))";
			ApplyToUrlSpecifed(ref codes,
				regexPattarn,
				pageUrl,
				pageUrlNoQuery,
				pagePath,
				rootUrl,
				pages.PageDefault,
				_UserOptions.EncodeUrl,
				null);


			if (_UserOptions.Links)
			{
				string extraAttib;

				// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
				extraAttib = Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

				// Add displaying orginal url address code
				if (_UserOptions.OrginalUrl)
				{
					orginalUrlRequired = true;
					extraAttib += Resources.STR_OrginalUrl_TagAttributeFormat;
				}

				// Anchors
				// Special replace, anchors should go to main page
				// the "EXT" in regex is required to inject extra attributes
				regexPattarn = @"(?><a)(?>\s+[^>\s]+)*?\s*(?>href\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)(?<EXT>)";
				ApplyToUrlSpecifed(ref codes,
					regexPattarn,
					pageUrl,
					pageUrlNoQuery,
					pagePath,
					rootUrl,
					pages.PageDefault,
					_UserOptions.EncodeUrl,
					extraAttib);
			}
			else
			{
				string extraAttib;

				// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
				extraAttib = Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

				// Anchors
				// Special replace, anchors should go to main page
				// the "EXT" in regex is required to inject extra attributes
				regexPattarn = @"(?><a)(?>\s+[^>\s]+)*?\s*(?>href\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)(?<EXT>)";
				ApplyToUrlSpecifed(ref codes,
					regexPattarn,
					pageUrl,
					pageUrlNoQuery,
					pagePath,
					rootUrl,
					"{0}",
					_UserOptions.EncodeUrl,
					extraAttib);
			}

			if (_UserOptions.Images)
			{
				// Background attribute
				regexPattarn = @"(?><[A-Z][A-Z0-9]+)(?>\s+[^>\s]+)*?\s*(?>background\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
				ApplyToUrlSpecifed(ref codes,
					regexPattarn,
					pageUrl,
					pageUrlNoQuery,
					pagePath,
					rootUrl,
					pages.PageAnyType,
					_UserOptions.EncodeUrl,
					null);

				// Replace embedded styles
				ReplaceEmbededStyle(ref codes,
						pageUrl,
						pageUrlNoQuery,
						pagePath,
						rootUrl);

				// Replace inline styles
				ReplaceInlineStyle(ref codes,
						pageUrl,
						pageUrlNoQuery,
						pagePath,
						rootUrl);

				// Replace with IMG tag
				// Replaces Link|Embed|script  for  href|src|background
				// FOR NEXT ApplyToUrlSpecifed
				regexPattarn = @"(?><(?<T>img|link|embed|script))(?>\s+[^>\s]+)*?\s*(?>(?<A>href|src)\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
			}
			else
				// Replace without IMG tag
				// Replaces link|embed|script  for  href|src|background
				// FOR NEXT ApplyToUrlSpecifed
				regexPattarn = @"(?><(?<T>link|embed|script))(?>\s+[^>\s]+)*?\s*(?>(?<A>href|src)\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
			// Replaces link|embed|script  for  href|src|background
			ApplyToUrlSpecifiedAutoDetect(ref codes,
					regexPattarn,
					pageUrl,
					pageUrlNoQuery,
					pagePath,
					rootUrl,
					pages,
					_UserOptions.EncodeUrl,
					null);

			// Tags events
			// Replaces tags events using RegEx
			ReplaceTagsEvents(ref codes,
					pageUrl,
					pageUrlNoQuery,
					pagePath,
					rootUrl);

			if (_UserOptions.Frames)
			{
				string extraAttib;

				// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
				extraAttib = Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

				// Adds onload event handler to detect the frame changes
				extraAttib += Resources.STR_IFrame_ExtraAttribute;

				// Iframe / Frameset
				// the "EXT" in regex is required to inject extra attributes
				regexPattarn = @"(?><(?<T>iframe|frame))(?>\s+[^>\s]+)*?\s*(?>src\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)(?<EXT>)";
				ApplyToUrlSpecifed(ref codes,
					regexPattarn,
					pageUrl,
					pageUrlNoQuery,
					pagePath,
					rootUrl,
					pages.PageHtml,
					_UserOptions.EncodeUrl,
					extraAttib);
			}

			// Object tags 
			// <param name="movie" value="before.swf">
			ReplaceObjectsMovie(ref codes,
				pageUrl,
				pageUrlNoQuery,
				pagePath,
				rootUrl,
				pages.PageAnyType,
				_UserOptions.EncodeUrl,
				null);


			// Encode <form> tags
			if (_UserOptions.SubmitForms)
			{
				string extraAttrib;

				// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
				extraAttrib =
					Consts.ClientContent.attrAlreadyEncodedAttributeWithValue
					+ Resources.STR_SubmitForms_ExtraAttribute;

				HtmlReplacer.ReplaceFormsSources(ref codes,
					pageUrlNoQuery,
					pages.PageDefault,
					pagePath,
					rootUrl,
					_UserOptions.EncodeUrl,
					_UserOptions.SubmitForms,
					extraAttrib);
			}
			else
			{
				string extraAttib;

				// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
				extraAttib =
					Consts.ClientContent.attrAlreadyEncodedAttributeWithValue
					+ Resources.STR_SubmitForms_ExtraAttribute;

				HtmlReplacer.ReplaceFormsSources(ref codes,
					pageUrlNoQuery,
					"{0}",
					pagePath,
					pagePath,
					false,
					_UserOptions.SubmitForms,
					extraAttib);
			}
		}

		private bool GetBaseTagSource(ref string codes, out string baseHref)
		{
			baseHref = string.Empty;
			try
			{
				const string pattern = @"(?><base)(?>\s+[^>\s]+)*?\s*(?>href\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
				//const string pattern = @"<base.*\n*href\s*=\s*(?<Q>['""])?(?<URL>(?(Q)(?(?<="")[^""]+|[^']+)|[^\s""'>]+))(?(Q)\k<Q>|)[^>]*>";
				Regex regex = new Regex(pattern,
					RegexOptions.Compiled |
					RegexOptions.IgnoreCase);

				// find matches
				MatchCollection mColl = regex.Matches(codes);
				bool result = false;

				// match collection
				for (int i = mColl.Count - 1; i >= 0; i--)
				{
					Match match = mColl[i];
					Group group = match.Groups["URL"];

					if (group != null)
					{
						// only last base should be used
						if (result == false)
						{
							baseHref = group.Value;
							if (baseHref.Length > 0)
								result = true;
						}

						// Remove the base tag!!
						codes = codes.Remove(group.Index, group.Length);
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "Failed to proccess 'base' tag!";
				return false;
			}
		}

		protected virtual void ApplyToUrlSpecifiedAutoDetect(
			ref string codes,
			string pattern,
			string pageUrl,
			string pageUrlNoQuery,
			string pagePath,
			string rootUrl,
			ASProxyPagesFormat pages,
			bool encodeUrl,
			string extraAttributeFormat)
		{
			try
			{

				bool addNewAttribute = false;
				bool hasNewAttribute = false;
				string newPageFormat;

				if (!string.IsNullOrEmpty(extraAttributeFormat))
				{
					addNewAttribute = true;
					hasNewAttribute = true;
				}

				MatchCollection matchColl = Regex.Matches(codes,
												pattern,
												RegexOptions.IgnoreCase |
												RegexOptions.Compiled);
				for (int i = matchColl.Count - 1; i >= 0; i--)
				{
					addNewAttribute = hasNewAttribute;

					Match match = matchColl[i];
					Group group = match.Groups["URL"];

					Group tagGroup = match.Groups["T"];

					// asproxy page, default is anytype
					newPageFormat = pages.PageAnyType;

					// if tag found
					if (tagGroup != null)
					{
						switch (tagGroup.Value.ToLower())
						{
							case "img":
								newPageFormat = pages.PageImage;
								break;
							case "script":
								newPageFormat = pages.PageJS;
								break;
							case "embed":
								newPageFormat = pages.PageAnyType;
								break;
						}
					}

					if (group != null)
					{
						string matchValue = group.Value;
						string orgValue;


						//===== If link is a bookmark don't change it=====
						if (matchValue.StartsWith("#"))
							continue;

						// Decode html code
						// some codes are in hex e.g. &#39; represents (')
						matchValue = HttpUtility.HtmlDecode(matchValue);

						// removes anu quotes from beginning and ending
						matchValue = HtmlTags.RemoveQuotesFromTagAttributeValue(matchValue);

						// if it is client side script
						if (UrlProvider.IsClientSitdeUrl(matchValue) == false)
						{
							string bookmarkPart = string.Empty;

							// Convert virtual url to absolute
							matchValue = UrlProvider.JoinUrl(matchValue, pageUrlNoQuery, pagePath, rootUrl);

							// Delete invalid character such as tab and line feed
							matchValue = UrlProvider.IgnoreInvalidUrlCharctersInHtml(matchValue);

							// Save orginal value
							orgValue = matchValue;

							//===== If another site url, has bookmark
							if (matchValue.IndexOf('#') != -1)
								matchValue = UrlBuilder.RemoveUrlBookmark(matchValue, out bookmarkPart);

							//====== Encode url to make it unknown ======
							if (encodeUrl)
								matchValue = UrlProvider.EncodeUrl(matchValue);
							else
								// just url safe
								matchValue = UrlProvider.EscapeUrlQuery(matchValue);

							//====== Add it to our url ======
							matchValue = string.Format(newPageFormat, matchValue);

							//===== Add bookmark to last url =====
							if (bookmarkPart.Length > 0)
							{
								matchValue += bookmarkPart;
								bookmarkPart = "";
							}
						}
						else
						{
							// parse the javascript url
							if (UrlProvider.IsJavascriptUrl(matchValue))
							{
								// execute
								Processors.JSProcessor.Execute(ref matchValue,
									pageUrl,
									pageUrlNoQuery,
									pagePath,
									rootUrl);
							}


							orgValue = matchValue;
							addNewAttribute = false;
						}

						//====== Make it safe
						matchValue = HttpUtility.HtmlEncode(matchValue);


						if (addNewAttribute)
						{
							// Apply original value and encoded value to format
							// BUG: Problem with format string that contain (') or (") characters
							// Bug Fixed since version 4.7
							string newAttribute = string.Format(extraAttributeFormat, orgValue, matchValue);

							Group extGroup = match.Groups["EXT"];
							if (extGroup != null)
							{
								codes = codes.Insert(extGroup.Index, newAttribute);
							}
						}

						// Replace the tag
						codes = codes.Remove(group.Index, group.Length);
						codes = codes.Insert(group.Index, matchValue);
					}
				}
			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pagePath);

				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "Failed to process some contents.";
			}
		}


		protected virtual void ApplyToUrlSpecifed(
			ref string codes,
			string pattern,
			string pageUrl,
			string pageUrlNoQuery,
			string pagePath,
			string rootUrl,
			string newPageFormat,
			bool encodeUrl,
			string extraAttributeFormat)
		{
			try
			{

				bool addNewAttribute = false;
				bool hasNewAttribute = false;

				if (!string.IsNullOrEmpty(extraAttributeFormat))
				{
					addNewAttribute = true;
					hasNewAttribute = true;
				}

				MatchCollection matchColl = Regex.Matches(codes,
												pattern,
												RegexOptions.IgnoreCase |
												RegexOptions.Compiled);
				for (int i = matchColl.Count - 1; i >= 0; i--)
				{

					// ISSUE: ***********************
					// The bottleneck starts here. 
					// sometime this "for" block runs about 2500 time per a request and it takes about 30 seconds.
					// There should be a way to rid of this issue
					// ISSUE: ***********************

					addNewAttribute = hasNewAttribute;

					Match match = matchColl[i];
					Group group = match.Groups["URL"];

					if (group != null)
					{
						string matchValue = group.Value;
						string orgValue;


						//===== If link is a bookmark don't change it=====
						if (matchValue.StartsWith("#"))
							continue;

						// Decode html code
						// some codes are in hex e.g. &#39; represents (')
						matchValue = HttpUtility.HtmlDecode(matchValue);

						// removes anu quotes from beginning and ending
						matchValue = HtmlTags.RemoveQuotesFromTagAttributeValue(matchValue);

						// if it is client side script
						if (UrlProvider.IsClientSitdeUrl(matchValue) == false)
						{
							string bookmarkPart = string.Empty;

							// Convert virtual url to absolute
							matchValue = UrlProvider.JoinUrl(matchValue, pageUrlNoQuery, pagePath, rootUrl);

							// Delete invalid character such as tab and line feed
							matchValue = UrlProvider.IgnoreInvalidUrlCharctersInHtml(matchValue);

							// Save orginal value
							orgValue = matchValue;

							//===== If another site url, has bookmark
							if (matchValue.IndexOf('#') != -1)
								matchValue = UrlBuilder.RemoveUrlBookmark(matchValue, out bookmarkPart);


							//====== Encode url to make it unknown ======
							if (encodeUrl)
								matchValue = UrlProvider.EncodeUrl(matchValue);
							else
								// just url safe
								matchValue = UrlProvider.EscapeUrlQuery(matchValue);


							//====== Add it to our url ======
							matchValue = string.Format(newPageFormat, matchValue);

							//===== Add bookmark to last url =====
							if (bookmarkPart.Length > 0)
							{
								matchValue += bookmarkPart;
								bookmarkPart = "";
							}
						}
						else
						{
							if (UrlProvider.IsJavascriptUrl(matchValue))
							{
								// execute
								Processors.JSProcessor.Execute(ref matchValue,
									pageUrl,
									pageUrlNoQuery,
									pagePath,
									rootUrl);
							}


							orgValue = matchValue;
							addNewAttribute = false;
						}

						//====== Make it safe
						matchValue = HttpUtility.HtmlEncode(matchValue);


						if (addNewAttribute)
						{
							// Apply original value and encoded value to format
							// BUG: Problem with format string that contain (') or (") characters
							// Bug Fixed since version 4.7
							string newAttribute = string.Format(extraAttributeFormat, orgValue, matchValue);

							Group extGroup = match.Groups["EXT"];
							if (extGroup != null)
							{
								codes = codes.Insert(extGroup.Index, newAttribute);
							}
						}

						// Replace the tag
						codes = codes.Remove(group.Index, group.Length);
						codes = codes.Insert(group.Index, matchValue);
					}
				}
			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pagePath);

				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "Failed to process some contents.";
			}
		}

		public void ReplaceEmbededScript(ref string codes,
			string pageUrl,
			string pageUrlNoQuery,
			string pagePath,
			string rootUrl)
		{
			try
			{
				const string pattern = @"<script\b[^>]*>(?<text>.*?)</script>";
				Regex regex = new Regex(pattern,
					RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

				// find matches
				MatchCollection mColl = regex.Matches(codes);

				// match collection
				for (int i = mColl.Count - 1; i >= 0; i--)
				{
					Match match = mColl[i];
					Group group = match.Groups["text"];

					if (group != null)
					{
						// Get clear url
						string eventCode = group.Value;
						int eventCodeOrgLen = eventCode.Length;

						// appy changes
						Processors.JSProcessor.Execute(ref eventCode,
							pageUrl,
							pageUrlNoQuery,
							pagePath,
							rootUrl);

						// if something changed!
						if (eventCodeOrgLen != eventCode.Length)
						{
							// Repace the change!
							codes = codes.Remove(group.Index, group.Length);
							codes = codes.Insert(group.Index, eventCode);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pagePath);

				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "Failed to process some scripts.";
			}
		}

		public void ReplaceInlineStyle(ref string codes,
			string pageUrl,
			string pageUrlNoQuery,
			string pagePath,
			string rootUrl)
		{
			try
			{

				Regex regex = new Regex(@"(?><[A-Z][A-Z0-9]+)(?>\s+[^>\s]+)*?\s*(?>style\s*=(?!\\)\s*)(?>(['""])?)(?<STYLE>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)",
				  RegexOptions.Compiled | RegexOptions.IgnoreCase);

				// find matches
				MatchCollection mColl = regex.Matches(codes);

				// match collection
				for (int i = mColl.Count - 1; i >= 0; i--)
				{
					Match match = mColl[i];
					Group group = match.Groups["STYLE"];

					if (group != null)
					{
						// Get clear url
						string eventCode = group.Value;
						string eventCodeOrg = eventCode;

						// appy changes
						Processors.CssProcessor.Execute(ref eventCode,
							pageUrl,
							pageUrlNoQuery,
							pagePath,
							rootUrl);

						if (eventCodeOrg != eventCode)
						{
							// Repace the change!
							codes = codes.Remove(group.Index, group.Length);
							codes = codes.Insert(group.Index, eventCode);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pagePath);

				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "Failed to process some styles.";
			}
		}

		public void ReplaceEmbededStyle(ref string codes,
			string pageUrl,
			string pageUrlNoQuery,
			string pagePath,
			string rootUrl)
		{
			try
			{

				Regex regex = new Regex(@"<style\b[^>]*>(?<text>.*?)</style>",
				  RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

				// find matches
				MatchCollection mColl = regex.Matches(codes);

				// match collection
				for (int i = mColl.Count - 1; i >= 0; i--)
				{
					Match match = mColl[i];
					Group group = match.Groups["text"];

					if (group != null)
					{
						// Get clear url
						string eventCode = group.Value;
						string eventCodeOrg = eventCode;

						// appy changes
						Processors.CssProcessor.Execute(ref eventCode,
							pageUrl,
							pageUrlNoQuery,
							pagePath,
							rootUrl);

						if (eventCodeOrg != eventCode)
						{
							// Repace the changes!
							codes = codes.Remove(group.Index, group.Length);
							codes = codes.Insert(group.Index, eventCode);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pagePath);

				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "Failed to process some styles.";
			}
		}

		public void ReplaceTagsEvents(ref string codes,
			string pageUrl,
			string pageUrlNoQuery,
			string pagePath,
			string rootUrl)
		{
			try
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

						// ignore empty events
						if (eventCode.Length == 0)
							continue;

						// Just save original code length, Memory saved
						//string eventCodeOrg = eventCode;
						int eventCodeOrgLen = eventCode.Length;

						// appy changes
						Processors.JSProcessor.Execute(ref eventCode,
							pageUrl,
							pageUrlNoQuery,
							pagePath,
							rootUrl);

						// instead of heavy but accurate (eventCodeOrg != eventCode)
						// we use a fast and dirty check
						if (eventCodeOrgLen != eventCode.Length)
						{
							// Repace the change!
							codes = codes.Remove(group.Index, group.Length);
							codes = codes.Insert(group.Index, eventCode);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pagePath);

				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "Failed to process some events in scripts.";
			}
		}

		private void ReplaceObjectsMovie(ref string codes,
			string pageUrl,
			string pageUrlNoQuery,
			string pagePath,
			string rootUrl,
			string newPageFormat,
			bool encodeUrl,
			string extraAttributeFormat)
		{
			try
			{
				// the "[^>]*>" is additional
				const string pattern = @"(?><param)(?>\s+[^>\s]+)*?\s*(?>value\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)[^>]*>";
				Regex regex = new Regex(pattern,
					RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

				// find matches
				MatchCollection mColl = regex.Matches(codes);

				bool addNewAttribute = false;
				bool hasNewAttribute = false;

				// match collection
				for (int i = mColl.Count - 1; i >= 0; i--)
				{
					addNewAttribute = hasNewAttribute;

					Match match = mColl[i];

					// match value
					string matchVal = match.Value;

					// test if match contains (name="movie")
					if (StringCompare.IndexOfIgnoreCase(ref matchVal, "movie") != -1)
					{
						// get the value group
						Group group = match.Groups["URL"];

						if (group != null)
						{
							string matchValue = group.Value;
							string orgValue;


							//===== If link is a bookmark don't change it=====
							if (matchValue.StartsWith("#"))
								continue;

							// Decode html code
							// some codes are in hex e.g. &#39; represents (')
							matchValue = HttpUtility.HtmlDecode(matchValue);

							// removes anu quotes from beginning and ending
							matchValue = HtmlTags.RemoveQuotesFromTagAttributeValue(matchValue);

							// if it is client side script
							if (UrlProvider.IsClientSitdeUrl(matchValue) == false)
							{
								string bookmarkPart = string.Empty;

								// Convert virtual url to absolute
								matchValue = UrlProvider.JoinUrl(matchValue, pageUrlNoQuery, pagePath, rootUrl);

								// Delete invalid character such as tab and line feed
								matchValue = UrlProvider.IgnoreInvalidUrlCharctersInHtml(matchValue);

								// Save orginal value
								orgValue = matchValue;

								//===== If another site url, has bookmark
								if (matchValue.IndexOf('#') != -1)
									matchValue = UrlBuilder.RemoveUrlBookmark(matchValue, out bookmarkPart);

								//====== Encode url to make it unknown ======
								if (encodeUrl)
									matchValue = UrlProvider.EncodeUrl(matchValue);
								else
									// just url safe
									matchValue = UrlProvider.EscapeUrlQuery(matchValue);

								//====== Add it to our url ======
								matchValue = string.Format(newPageFormat, matchValue);

								//===== Add bookmark to last url =====
								if (bookmarkPart.Length > 0)
								{
									matchValue += bookmarkPart;
									bookmarkPart = "";
								}
							}
							else
							{
								if (UrlProvider.IsJavascriptUrl(matchValue))
								{
									// execute
									Processors.JSProcessor.Execute(ref matchValue,
										pageUrl,
										pageUrlNoQuery,
										pagePath,
										rootUrl);
								}


								orgValue = matchValue;
								addNewAttribute = false;
							}

							//====== Make it safe
							matchValue = HttpUtility.HtmlEncode(matchValue);


							if (addNewAttribute)
							{
								// Apply original value and encoded value to format
								// BUG: Problem with format string that contain (') or (") characters
								// Bug Fixed since version 4.7
								string newAttribute = string.Format(extraAttributeFormat, orgValue, matchValue);

								Group extGroup = match.Groups["EXT"];
								if (extGroup != null)
								{
									codes = codes.Insert(extGroup.Index, newAttribute);
								}
							}

							// Replace the tag
							codes = codes.Remove(group.Index, group.Length);
							codes = codes.Insert(group.Index, matchValue);
						}

					}

				}
			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pagePath);

				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "Failed to process some embedded objects.";
			}
		}


		/// <summary>
		/// Generate dynamic encoding javascript codes
		/// </summary>
		/// <param name="pageBasePath">base path of current request page</param>
		/// <returns>Javascript codes</returns>
		string GenerateJsEncoderCodes(
			string pageUrl,
			string pageUrlNoQuery,
			string pagePath,
			string rootUrl)
		{

			string userConfig;
			string reqInfo;
			string cookieNames;
			string locationObject;

			userConfig = string.Format(Consts.ClientContent.JSEncoder_UserConfig,
				_UserOptions.EncodeUrl.ToString().ToLower(),
				_UserOptions.OrginalUrl.ToString().ToLower(),
				_UserOptions.Links.ToString().ToLower(),
				_UserOptions.Images.ToString().ToLower(),
				_UserOptions.SubmitForms.ToString().ToLower(),
				_UserOptions.Frames.ToString().ToLower(),
				_UserOptions.Cookies.ToString().ToLower(),
				_UserOptions.RemoveScripts.ToString().ToLower(),
				_UserOptions.RemoveObjects.ToString().ToLower(),
				_UserOptions.TempCookies.ToString().ToLower()
			);

			reqInfo = string.Format(Consts.ClientContent.JSEncoder_RequestInfo,
				// V5.5b4 BUGFIX: page url should be encoded, it may contain unsecure chars.
				HtmlTags.EncodeJavascriptString(pageUrl, true),
				HtmlTags.EncodeJavascriptString(pageUrlNoQuery, true),
				HtmlTags.EncodeJavascriptString(pagePath, true),
				HtmlTags.EncodeJavascriptString(rootUrl, true),
				Systems.CookieManager.GetCookieName(pageUrl),
				Systems.CookieManager.GetCookieNameExt,
				UrlProvider.JoinUrl(UrlProvider.GetAppAbsolutePath(), Consts.FilesConsts.PageDefault_Dynamic),
				UrlProvider.GetAppAbsolutePath(),
				UrlProvider.GetAppAbsoluteBasePath(),
				Consts.FilesConsts.PageDefault_Dynamic,
				Consts.Query.Base64Unknowner
			);

			// Cookie names
			StringCollection strColl = Systems.CookieManager.GetAppliedCookieNamesList(pageUrl);
			string cookieNamesTemp = "";
			for (int i = 0; i < strColl.Count; i++)
			{
				string name = strColl[i];
				cookieNamesTemp += "'" + name + "'";
				if (i != strColl.Count - 1)
					cookieNamesTemp += ',';
			}
			cookieNames = string.Format(Consts.ClientContent.JSEncoder_AppliedCookieNames,
				cookieNamesTemp);

			// Page uri
			Uri pageUri = new Uri(pageUrl);
			locationObject = string.Format(Consts.ClientContent.JSEncoder_RequestLocation,
				pageUri.Fragment,	// Hash
				pageUri.Authority,	// Host
				pageUri.Host,		// Hostname
				pageUri.AbsolutePath,// Pathname
				pageUri.Query,		// Search
				pageUri.Port,		// Port
				pageUri.Scheme		// Protocol
			);

			StringBuilder result = new StringBuilder();
			// ASProxy encoder variables
			result.Append(Resources.ASProxyJavaScriptTag(userConfig + reqInfo + locationObject + cookieNames, ""));

			// Base64 encoder 
			result.Append(Resources.ASProxyJavaScriptTag("", Consts.FilesConsts.JSBase64));

			// ASProxy encoder 
			result.Append(Resources.ASProxyJavaScriptTag("", Consts.FilesConsts.JSASProxyEncoder));

			// AJAX wrapper core, Usless since v5.5b4
			//result.Append(Resources.ASProxyJavaScriptTag("", Consts.FilesConsts.JSAJAXWrapperCore));

			return result.ToString();
		}

		/// <summary>
		/// removes tag name from html codes
		/// </summary>
		void RemoveTagOnly(ref string codes, string tagName)
		{
			codes = Regex.Replace(codes,
						@"<[/]?(" + tagName + ")[^>]*?>",
						string.Empty,
						RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}

		class Resources
		{
			public const string STR_SubmitForms_ExtraAttribute = " encodedurl=\"{1}\" methodorginal={2} ";
			public const string STR_IFrame_ExtraAttribute = " onload=_ASProxy.Enc.EncodeFrames() ";
			public const string STR_OrginalUrl_TagAttributeFormat = " onmouseout=ORG_OUT_() onmouseover=ORG_IN_(this) originalurl=\"{0}\" ";

			public const string STR_OrginalUrl_FloatBar = "<div id='__ASProxyOriginalURL' dir='ltr' style='display:block;font-family:tahoma;color:black;font-size:12px;padding:2px 5px 2px 5px;margin:0;position:absolute;left:0px;top:0px;width:98%;background:whitesmoke none;border:solid 2px black;overflow: visible;z-index:999999999;visibility:hidden;text-align:left;line-height:100%;'></div>";

			// float bar scripts should appear after ASProxyEncoder scripts
			public const string STR_OrginalUrl_Functions =
					"<script language='javascript' type='text/javascript'>" +
					"var _wparent=window.top ? window.top : window.parent;" +
					"_wparent=_wparent ? _wparent : window;" +
					"var _document=_wparent.document;" +
					"var _XFloatBar=_document.getElementById('__ASProxyOriginalURL');" +
					"_XFloatBar.Freeze=false; _XFloatBar.CurrentUrl=''; var ASProxyUnvisibleHide;" +
					"function ORG_Legible(str){if(typeof(_Base64_utf8_decode)=='undefined')return str;return _Base64_utf8_decode(unescape(str));}" +
					"function ORG_Position_(){if(typeof(_XFloatBar)=='undefined')return;var topValue='0';topValue=_document.body.scrollTop+'';" +
					"if(topValue=='0' || topValue=='undefined')topValue=_wparent.scrollY+'';" +
					"if(topValue=='0' || topValue=='undefined')topValue=_document.documentElement.scrollTop+'';" +
					"if(topValue!='undefined')_XFloatBar.style.top=topValue+'px';}" +
					"function ORG_IN_(obj){if(!_XFloatBar||_XFloatBar.Freeze)return;ORG_Position_();var attrib=obj.attributes['originalurl'];if(attrib!=null)attrib=attrib.value; else attrib=null;if(attrib!='undefined' && attrib!='' && attrib!=null){_wparent.clearTimeout(ASProxyUnvisibleHide);_XFloatBar.CurrentUrl=''+attrib;_XFloatBar.innerHTML='URL: <span style=\"color:maroon;\">'+ORG_Legible(attrib)+'</span>';_XFloatBar.style.visibility='visible';}}" +
					"function ORG_MSG_(msg){if(!_XFloatBar||_XFloatBar.Freeze)return;ORG_Position_();_wparent.clearTimeout(ASProxyUnvisibleHide);_XFloatBar.CurrentUrl='';_XFloatBar.innerHTML=msg;_XFloatBar.style.visibility='visible';}" +
					"function ORG_OUT_(){if(!_XFloatBar||_XFloatBar.Freeze)return;_XFloatBar.innerHTML='URL: ';_XFloatBar.CurrentUrl='';_wparent.clearTimeout(ASProxyUnvisibleHide);ASProxyUnvisibleHide=_wparent.setTimeout(ORG_HIDE_IT,500);}" +
					"function ORG_HIDE_IT(){if(_XFloatBar.Freeze)return;_XFloatBar.style.visibility='hidden';_XFloatBar.innerHTML='';}" +
					"_wparent.onscroll=ORG_Position_;" +
					"if(typeof(_ASProxy)!='undefined')_ASProxy.AttachEvent(document,'keydown',function(aEvent){var ev = window.event ? window.event : aEvent; ORG_Position_();" +
					"if(ev.ctrlKey && ev.shiftKey && ev.keyCode==88){if(typeof(_XFloatBar)=='undefined')return;" +
					"if(_XFloatBar.Freeze){_XFloatBar.Freeze=false;ORG_HIDE_IT();}" +
					"else if(_XFloatBar.CurrentUrl!=''){_XFloatBar.Freeze=true;" +
					"_XFloatBar.innerHTML=_XFloatBar.innerHTML+\"<br /><span style='color:navy;'>Press Ctrl+Shift+X again to unfreeze this bar.<span/>\";" +
					"}}});" +
					"</script>";

			/// <summary>
			/// adds an additional asproxydone='2'
			/// </summary>
			public static string ASProxyJavaScriptTag(string content, string src)
			{
				string result = "<script " + Consts.ClientContent.attrAlreadyEncodedAttributeIgnore;
				if (!string.IsNullOrEmpty(src))
					result += " src='" + src + "' ";
				result += " type='text/javascript'>" + content + "</script>";
				return result;
			}
		}
	}
}

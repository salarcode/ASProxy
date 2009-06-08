using System;
using System.Collections.Generic;
using System.Text;
using SalarSoft.ASProxy.Exposed;
using System.Text.RegularExpressions;
using System.Web;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class RegexHtmlProcessor : HtmlProcessor
	{

		/// Templates:
		/// 
		/// Grabbing HTML Tags:
		/// ([A-Z][A-Z0-9]*)\b[^>]*>(.*?)</\1>
		/// 
		/// Trimming Whitespace:
		/// ^[ \t]+|[ \t]+$
		/// 
		/// Text between tag:
		/// <script\b[^>]*>(?<text>.*?)</script>
		///
		/// 
		/// Tags{ a|link|embed|script|style  with href|src|background
		///     style content
		///     script content
		/// }
		/// </sinceNow>

		public override void Execute(ref string codes, string pageUrl, string rootUrl)
		{
			try
			{
				// ASProxy pages url formats generator
				ASProxyPagesFormat pages = new ASProxyPagesFormat(UserOptions);

				// Original urls addistional codes option
				bool orginalUrlRequired = false;

				// this is page path, used in processing relative paths in source html
				// for example the pageRootUrl for "http://Site.com/users/profile.aspx" will be "http://Site.com/users/"
				string pagePath;

				// the page Url without any query parameter, used in processing relative query parameters
				// the pageUrlNoQuery for "http://Site.com/profile.aspx?uid=90" will be "http://Site.com/profile.aspx"
				string pageUrlNoQuery;


				// Gets page Url without any query parameter
				pageUrlNoQuery = UrlProvider.GetPageAbsolutePath(pageUrl);

				// gets page root Url
				pagePath = UrlProvider.GetPagePath(pageUrl);

				// Renames ASPDotNET standard ViewState name to a temporary name
				// This name will reset to default when the page posted back
				HtmlReplacer.ReplaceAspDotNETViewState(ref codes);


				// If remove scripts chosen, remove all the scripts.
				if (UserOptions.RemoveScripts)
					HtmlReplacer.RemoveScripts(ref codes);


				string regexPattarn;

				// Applying the BASE tag to the URLs.
				string baseHref;
				if (GetBaseTagSource(ref codes, out baseHref))
				{
					// changing page base path to specified Base in the document
					pagePath = UrlProvider.AddSlashToEnd(baseHref);

					// site root url should change also
					rootUrl = UrlProvider.GetRootPath(pagePath);
				}

				// HttpRefresh
				// replaces <meta http-equiv="refresh" content=1;url=HttpRefresh.htm>
				regexPattarn = @"<meta(?>\s+[^>\s]+)*?\s*content\s*=\s*(?<Q>[""'])?[0-9]+\s*;\s*url=(?<UQ>['""]|['])?(?<URL>(?(?<="")[^""]+|(?(?<=')[^']+|[^'"" >]+)))(?(UQ)\2|)(?(Q)\1|)";
				ApplyToUrlSpecifed(ref codes,
					regexPattarn,
					pagePath,
					pageUrlNoQuery,
					rootUrl,
					pages.PageDefault,
					UserOptions.EncodeUrl,
					null);


				if (UserOptions.Links)
				{
					string extraAttib;

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					extraAttib = Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

					// Add displaying orginal url address code
					if (UserOptions.OrginalUrl)
					{
						orginalUrlRequired = true;
						extraAttib += Resources.STR_OrginalUrl_TagAttributeFormat;
					}

					// Anchors
					// Special replace, anchors should go to main page
					// PRE-OK regexPattarn = @"<a.*\n*href\s*=\s*(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^\s""'>]+))(?(1)\1|)[^>]*>";
					//regexPattarn = @"(?><a)(?>\s+[^>\s]+)*?\s*(?>href\s*=(?!\\)\s*)(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
					regexPattarn = @"(?><a)(?>\s+[^>\s]+)*?\s*(?>href\s*=(?!\\)\s*)(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)(?<EXT>)";
					ApplyToUrlSpecifed(ref codes,
						regexPattarn,
						pagePath,
						pageUrlNoQuery,
						rootUrl,
						pages.PageDefault,
						UserOptions.EncodeUrl,
						extraAttib);
				}
				else
				{
					string extraAttib;

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					extraAttib = Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

					// Anchors
					// Special replace, anchors should go to main page
					// PRE-OK regexPattarn = @"<a.*\n*href\s*=\s*(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^\s""'>]+))(?(1)\1|)[^>]*>";
					//regexPattarn = @"(?><a)(?>\s+[^>\s]+)*?\s*(?>href\s*=(?!\\)\s*)(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
					regexPattarn = @"(?><a)(?>\s+[^>\s]+)*?\s*(?>href\s*=(?!\\)\s*)(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)(?<EXT>)";
					ApplyToUrlSpecifed(ref codes,
						regexPattarn,
						pagePath,
						pageUrlNoQuery,
						rootUrl,
						"{0}",
						UserOptions.EncodeUrl,
						extraAttib);
				}

				// Replaces link|embed|script  for  href|src|background
				// PRE-OK regexPattarn = @"<(img|link|embed|script).*[\n]*(href|src|background)\s*=\s*(?<Q>['""])?(?<URL>(?(Q)(?(?<="")[^""]+|[^']+)|[^\s""'>]+))(?(Q)\k<Q>|)[^>]*>";
				// M-OK regexPattarn = @"(?><[A-Z][A-Z0-9]{0,15})(?>\s+[^>\s]+)*?\s*(?>(href|src|background)\s*=(?!\\)\s*)(?>(['""])?)(?<URL>(?(2)(?(?<="")[^""]{1,1000}|[^']{1,1000})|[^ >]{1,1000}))(?(2)\2|)";
				regexPattarn = @"(?><(?<T>img|link|embed|script))(?>\s+[^>\s]+)*?\s*(?>(?<A>href|src|background)\s*=(?!\\)\s*)(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
				ApplyToUrlSpecifed(ref codes,
					regexPattarn,
					pagePath,
					pageUrlNoQuery,
					rootUrl,
					pages.PageAnyType,
					UserOptions.EncodeUrl,
					null);

				// Background attribute
				regexPattarn = @"(?><[A-Z][A-Z0-9]+)(?>\s+[^>\s]+)*?\s*(?>background\s*=(?!\\)\s*)(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
				ApplyToUrlSpecifed(ref codes,
					regexPattarn,
					pagePath,
					pageUrlNoQuery,
					rootUrl,
					pages.PageAnyType,
					UserOptions.EncodeUrl,
					null);

				// Tags events
				// Replaces tags events using RegEx
				ReplaceTagsEvents(ref codes,
						pagePath,
						rootUrl);

				// Replace embedded styles
				ReplaceEmbededStyle(ref codes,
						pagePath,
						rootUrl);

				// Replace inline styles
				ReplaceInlineStyle(ref codes,
						pagePath,
						rootUrl);

				if (UserOptions.RemoveScripts == false)
				{
					// processes embeded scripts
					ReplaceEmbededScript(ref codes,
							pagePath,
							rootUrl);
				}

				if (UserOptions.FrameSet)
				{
					// Iframe / Frameset
					// regexPattarn = @"<(iframe|frame).*[\n]*(src)\s*=\s*(?<Q>['""])?(?<URL>(?(Q)(?(?<="")[^""]+|[^']+)|[^\s""'>]+))(?(Q)\k<Q>|)[^>]*>";
					regexPattarn = @"(?><(?<T>iframe|frame))(?>\s+[^>\s]+)*?\s*(?>src\s*=(?!\\)\s*)(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
					ApplyToUrlSpecifed(ref codes,
						regexPattarn,
						pagePath,
						pageUrlNoQuery,
						rootUrl,
						pages.PageHtml,
						UserOptions.EncodeUrl,
						null);
				}

				if (UserOptions.EmbedObjects)
				{
					// Encode <embed> tags
					// <param name="movie" value="before.swf"> not done!
					//regexPattarn = @"<(embed).*[\n]*(src)\s*=\s*(?<Q>['""])?(?<URL>(?(Q)(?(?<="")[^""]+|[^']+)|[^\s""'>]+))(?(Q)\k<Q>|)[^>]*>";
					regexPattarn = @"(?><embed)(?>\s+[^>\s]+)*?\s*(?>src\s*=(?!\\)\s*)(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)";
					ApplyToUrlSpecifed(ref codes,
						regexPattarn,
						pagePath,
						pageUrlNoQuery,
						rootUrl,
						pages.PageHtml,
						UserOptions.EncodeUrl,
						null);
				}

				// Encode <form> tags
				if (UserOptions.SubmitForms)
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
						UserOptions.EncodeUrl,
						UserOptions.SubmitForms,
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
						UserOptions.SubmitForms,
						extraAttib);
				}

				// OrginalUrl additional injection html codes
				if (orginalUrlRequired)
				{
					// TODO: Check necessary
					PageInitializerCodes = Resources.STR_OrginalUrl_FloatBar;

					// inject to html
					codes = Resources.STR_OrginalUrl_Functions
						+ codes;
				}

				// Add dynamic encoding javascript codes
				string jsEncoderCodes = GenerateJsEncoderCodes(pageUrl,
							pageUrlNoQuery,
							pagePath,
							rootUrl);


				// Add jsEncoder codes to page
				codes = jsEncoderCodes + codes;

			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pageUrl);

				LastStatus = LastStatus.ContinueWithError;
				LastErrorMessage = "ASProxy has some errors! The delivered content may not work properly.";
			}
		}

		private bool GetBaseTagSource(ref string codes, out string baseHref)
		{
			//const string pattern = @"<script\b[^>]*>(?<text>.*?)</script>";
			baseHref = string.Empty;
			try
			{
				const string pattern = @"<base.*\n*href\s*=\s*(?<Q>['""])?(?<URL>(?(Q)(?(?<="")[^""]+|[^']+)|[^\s""'>]+))(?(Q)\k<Q>|)[^>]*>";
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
			catch
			{
				LastStatus = LastStatus.ContinueWithError;
				LastErrorMessage = "Failed to proccess <base> tag!";
				return false;
			}
		}


		protected virtual void ApplyToUrlSpecifed(
			ref string codes,
			string pattern,
			string pageUrl,
			string pageUrlNoQuery,
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
					extraAttributeFormat = " " + extraAttributeFormat + " ";
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
					if (group != null)
					{
						string matchValue = group.Value;
						string orgValue;


						//===== If link is a bookmark don't change it=====
						if (matchValue.StartsWith("#"))
							continue;

						if (UrlProvider.IsClientSitdeUrl(matchValue) == false)
						{
							string bookmarkPart = string.Empty;

							// Convert virtual url to absolute
							matchValue = UrlProvider.JoinUrl(matchValue, pageUrlNoQuery, pageUrl, rootUrl);

							// Delete invalid character such as tab and line feed
							matchValue = UrlProvider.IgnoreInvalidUrlCharctersInHtml(matchValue);

							// Save orginal value
							orgValue = matchValue;

							//===== If another site url, has bookmark
							if (matchValue.IndexOf('#') != -1)
								matchValue = UrlProvider.RemoveUrlBookmark(matchValue, out bookmarkPart);


							//====== Get desigred url addrress
							matchValue = HttpUtility.HtmlDecode(matchValue);

							//====== Encode url to make it unknown ======
							if (encodeUrl)
							{
								matchValue = UrlProvider.EncodeUrl(matchValue);
							}

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
								IJSProcessor js = (IJSProcessor)Provider.CreateProviderInstance(ProviderType.IJSProcessor);
								js.UserOptions = UserOptions.ReadFromRequest();

								// execute
								js.Execute(ref matchValue, pageUrl, rootUrl);
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

							// Insert to tag
							//codes = codes.Insert(match.Index + match.Length + (matchValue.Length - group.Length), newAttribute);
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
					Systems.LogSystem.LogError(ex, pageUrl);

				LastStatus = LastStatus.ContinueWithError;
				LastErrorMessage = "Failed to process some contents.";
			}
		}

		public void ReplaceEmbededScript(ref string codes,
			string pageUrl,
			string rootUrl)
		{
			try
			{
				const string pattern = @"<script\b[^>]*>(?<text>.*?)</script>";
				Regex regex = new Regex(pattern,
					RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

				// find matches
				MatchCollection mColl = regex.Matches(codes);

				IJSProcessor processor = null;
				if (mColl.Count > 0)
				{
					processor = (IJSProcessor)Provider.CreateProviderInstance(ProviderType.IJSProcessor);
					processor.UserOptions = UserOptions.ReadFromRequest();
				}

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
						processor.Execute(ref eventCode, pageUrl, rootUrl);

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
					Systems.LogSystem.LogError(ex, pageUrl);

				LastStatus = LastStatus.ContinueWithError;
				LastErrorMessage = "Failed to process some scripts.";
			}
		}

		public void ReplaceInlineStyle(ref string codes,
			string pageUrl,
			string rootUrl)
		{
			try
			{

				Regex regex = new Regex(@"(?><[A-Z][A-Z0-9]+)(?>\s+[^>\s]+)*?\s*(?>style\s*=(?!\\)\s*)(['""])?(?<STYLE>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)",
				  RegexOptions.Compiled | RegexOptions.IgnoreCase);

				// find matches
				MatchCollection mColl = regex.Matches(codes);

				ICssProcessor processor = null;
				if (mColl.Count > 0)
				{
					processor = (ICssProcessor)Provider.CreateProviderInstance(ProviderType.ICssProcessor);
					processor.UserOptions = UserOptions.ReadFromRequest();
				}

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
						processor.Execute(ref eventCode, pageUrl, rootUrl);

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
					Systems.LogSystem.LogError(ex, pageUrl);

				LastStatus = LastStatus.ContinueWithError;
				LastErrorMessage = "Failed to process some styles.";
			}
		}

		public void ReplaceEmbededStyle(ref string codes,
			string pageUrl,
			string rootUrl)
		{
			try
			{

				Regex regex = new Regex(@"<style\b[^>]*>(?<text>.*?)</style>",
				  RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

				// find matches
				MatchCollection mColl = regex.Matches(codes);

				ICssProcessor processor = null;
				if (mColl.Count > 0)
				{
					processor = (ICssProcessor)Provider.CreateProviderInstance(ProviderType.ICssProcessor);
					processor.UserOptions = UserOptions.ReadFromRequest();
				}

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
						processor.Execute(ref eventCode, pageUrl, rootUrl);

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
					Systems.LogSystem.LogError(ex, pageUrl);

				LastStatus = LastStatus.ContinueWithError;
				LastErrorMessage = "Failed to process some styles.";
			}
		}

		public void ReplaceTagsEvents(ref string codes,
			string pageUrl,
			string rootUrl)
		{
			try
			{
				// any event name in tags which starts with "on" for example "onclick"
				Regex regex = new Regex(@"(?><[A-Z][A-Z0-9]{0,15})(?>\s+[^>\s]+)*?\s*(?>on\w{1,15}\s*=(?!\\)\s*)(['""])?(?<URL>(?(1)(?(?<="")[^""]+|[^']+)|[^ >]+))(?(1)\1|)",
					RegexOptions.Compiled | RegexOptions.IgnoreCase);

				// find matches
				MatchCollection mColl = regex.Matches(codes);

				IJSProcessor processor = null;
				if (mColl.Count > 0)
				{
					processor = (IJSProcessor)Provider.CreateProviderInstance(ProviderType.IJSProcessor);
					processor.UserOptions = UserOptions.ReadFromRequest();
				}

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
						processor.Execute(ref eventCode, pageUrl, rootUrl);

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
					Systems.LogSystem.LogError(ex, pageUrl);

				LastStatus = LastStatus.ContinueWithError;
				LastErrorMessage = "Failed to process some scripts.";
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
			StringBuilder strCodes = new StringBuilder();

			// Variables for ASProxy encoder
			strCodes.Append(Consts.ClientContent.JSEncoder_ASProxyEncodeUrl + "=" + Convert.ToInt32(UserOptions.EncodeUrl).ToString() + ";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ASproxyImagesEnabled + "=" + Convert.ToInt32(UserOptions.OrginalUrl).ToString() + ";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ASproxyOriginalUrlEnabled + "=" + Convert.ToInt32(UserOptions.Images).ToString() + ";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ASproxyLinksEnabled + "=" + Convert.ToInt32(UserOptions.Links).ToString() + ";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ASProxyFormsEnabled + "=" + Convert.ToInt32(UserOptions.SubmitForms).ToString() + ";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ASProxyFramesEnabled + "=" + Convert.ToInt32(UserOptions.IFrame).ToString() + ";");

			strCodes.Append(Consts.ClientContent.JSEncoder_RequestUrlFull + "=\"" + pageUrl + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_RequestUrlNoParam + "=\"" + pageUrlNoQuery + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_RequestUrlDir + "=\"" + pagePath + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_RequestUrlBaseDir + "=\"" + rootUrl + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ASProxyDefaultPage + "=\"" + Consts.FilesConsts.DefaultPage + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ASProxySiteBaseDir + "=\"" + UrlProvider.GetAppAbsolutePath() + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ASProxySiteHostBaseDir + "=\"" + UrlProvider.GetAppAbsoluteBasePath() + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ASProxyDefaultPagePath + "=\"" + UrlProvider.JoinUrl(UrlProvider.GetAppAbsolutePath(), Consts.FilesConsts.DefaultPage) + "\";");

			strCodes.Append(Consts.ClientContent.JSEncoder_Base64Unknowner + "=\"" + Consts.Query.Base64Unknowner + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_RequestCookieName + "=\"" + Systems.CookieManager.GetCookieName(pageUrl) + "\";");

			Uri pageUri = new Uri(pageUrl);

			// Window location variables
			strCodes.Append(Consts.ClientContent.JSEncoder_ReqLocBookmark + "=\"" + pageUri.Fragment + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ReqLocHost + "=\"" + pageUri.Authority + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ReqLocHostname + "=\"" + pageUri.Host + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ReqLocPathname + "=\"" + pageUri.AbsolutePath + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ReqLocPort + "=\"" + pageUri.Port.ToString() + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ReqLocProtocol + "=\"" + pageUri.Scheme + ':' + "\";");
			strCodes.Append(Consts.ClientContent.JSEncoder_ReqLocQueries + "=\"" + pageUri.Query + "\";");

			StringBuilder result = new StringBuilder();
			// AJAX wrapper core
			result.Append(HtmlTags.JavascriptTag("", Consts.FilesConsts.JSAJAXWrapperCore));

			// ASProxy encoder variables
			result.Append(HtmlTags.JavascriptTag(strCodes.ToString(), ""));

			// Base64 encoder 
			result.Append(HtmlTags.JavascriptTag("", Consts.FilesConsts.JSBase64));

			// ASProxy encoder 
			result.Append(HtmlTags.JavascriptTag("", Consts.FilesConsts.JSASProxyEncoder));

			return result.ToString();
		}

		class Resources
		{

			public const string STR_SubmitForms_ExtraAttribute = " encodedurl=\"{1}\" methodorginal={2} ";
			public const string STR_IFrame_ExtraAttribute = " onload=ASProxyEncodeFrames() ";
			public const string STR_OrginalUrl_TagAttributeFormat = " onmouseout=ORG_OUT_() onmouseover=ORG_IN_(this) originalurl=\"{0}\" ";

			public const string STR_OrginalUrl_FloatBar = "<div id='__ASProxyOriginalURL' dir='ltr' style='display:block;font-family:verdana;color:black;font-size:11px;padding:2px 5px 2px 5px;margin:0;position:absolute;left:0px;top:0px;width:98%;background:whitesmoke none;border:solid 2px black;overflow: visible;z-index:999999999;visibility:hidden;text-align:left;'></div>";
			public const string STR_OrginalUrl_Functions =
					"<script language='javascript' type='text/javascript'>" +
					"var _wparent=window.top ? window.top : window.parent;" +
					"_wparent=_wparent ? _wparent : window;" +
					"var _document=_wparent.document;" +
					"var ASProxyOriginalURL=_document.getElementById('__ASProxyOriginalURL');" +
				//"if(ASProxyOriginalURL==null){_document=_wparent.document; ASProxyOriginalURL=_document.getElementById('__ASProxyOriginalURL');}" +
					"var ASProxyUnvisibleHide;" +
					"function ORG_Position_(){if(!ASProxyOriginalURL)return;var topValue='0';topValue=_document.body.scrollTop+'';" +
					"if(topValue=='0' || topValue=='undefined')topValue=_wparent.scrollY+'';" +
					"if(topValue=='0' || topValue=='undefined')topValue=_document.documentElement.scrollTop+'';" +
					"if(topValue!='undefined')ASProxyOriginalURL.style.top=topValue+'px';}" +
					"function ORG_IN_(obj){if(!ASProxyOriginalURL)return;ORG_Position_();var attrib=obj.attributes['originalurl'];if(attrib!=null)attrib=attrib.value; else attrib=null;if(attrib!='undefined' && attrib!='' && attrib!=null){_wparent.clearTimeout(ASProxyUnvisibleHide);ASProxyOriginalURL.CurrentUrl=''+attrib;ASProxyOriginalURL.innerHTML='URL: <span style=\"color:maroon;\">'+attrib+'</span>';ASProxyOriginalURL.style.visibility='visible';}}" +
					"function ORG_OUT_(){if(!ASProxyOriginalURL)return;ASProxyOriginalURL.innerHTML='URL: ';ASProxyOriginalURL.CurrentUrl='';_wparent.clearTimeout(ASProxyUnvisibleHide);ASProxyUnvisibleHide=_wparent.setTimeout(ORG_HIDE_IT,500);}" +
					"function ORG_HIDE_IT(){ASProxyOriginalURL.style.visibility='hidden';ASProxyOriginalURL.innerHTML='';}" +
					"_wparent.onscroll=ORG_Position_;" +
					"</script>";
		}

	}
}

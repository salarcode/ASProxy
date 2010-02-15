using System;
using SalarSoft.ASProxy.Exposed;
using System.Text;
using System.Collections.Specialized;
using System.Web;

namespace SalarSoft.ASProxy.BuiltIn
{

	/// <summary>
	/// Summary description for ExHtmlProcessor
	/// </summary>
	[Obsolete("Use RegexHtmlProcessor instead.")]
	public class HtmlProcessor : ExHtmlProcessor
	{
		private UserOptions _UserOptions;


		public HtmlProcessor()
		{
			_UserOptions = CurrentContext.UserOptions;
		}


		#region public methods
		/// <summary>
		/// Processes the html codes
		/// </summary>
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

		/// <summary>
		/// Processes the html codes
		/// </summary>
		/// <param name="codes">Html codes</param>
		/// <param name="pageUrl">Page url. E.G. http://Site.com/users/profile.aspx?uid=90</param>
		/// <param name="rootUrl">Root path. E.G. http://Site.com/</param>
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
				if (HtmlReplacer.ReplaceBaseSources(ref codes, true, out baseHref))
				{
					// changing page base path to specified Base in the document
					pagePath = UrlProvider.AddSlashToEnd(baseHref);

					// BUGFIX v4.6.1:: site root url should change also
					rootUrl = UrlProvider.GetRootPath(rootUrl);
				}

				// processing style sheet links
				if (_UserOptions.Images)
				{
					HtmlReplacer.ReplaceCssLinks(ref codes,
						pageUrlNoQuery,
						pages.PageAnyType,
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl);

					// TODO: CSSReplacer
					// This function replaces "Import" rule and background images!
					// So, this breaches to background option role. Since v4.0
					CSSReplacer.ReplaceStyleTagStyleUrl(ref codes,
						pageUrlNoQuery,
						pages.PageAnyType,
						pages.PageAnyType,
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl);
				}

				// It seems with javascript encoding these methods are useless!!
				// The javascript may be disabled in browser so we have to this anyway
				if (_UserOptions.Links)
				{
					string extraAttib = "";

					// Add displaying orginal url address code
					if (_UserOptions.OrginalUrl)
					{
						orginalUrlRequired = true;
						extraAttib = Resources.STR_OrginalUrl_TagAttributeFormat;
					}

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					extraAttib += Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

					HtmlReplacer.ReplaceAnchors(ref codes,
						pageUrlNoQuery,
						pages.PageDefault,
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl,
						extraAttib);

					HtmlReplacer.ReplaceHttpRefresh(ref codes,
						pageUrlNoQuery,
						pages.PageDefault,
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl);
				}
				else
				{
					string extraAttib;

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					extraAttib = Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

					HtmlReplacer.ReplaceAnchors(ref codes,
						pageUrlNoQuery,
						"{0}",
						pagePath,
						rootUrl,
						false,
						extraAttib);

					HtmlReplacer.ReplaceHttpRefresh(ref codes,
						pageUrlNoQuery,
						"{0}",
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl);
				}

				if (_UserOptions.Frames)
				{
					string extraAttrib = Resources.STR_IFrame_ExtraAttribute;

					// Encode <iframe> tags
					HtmlReplacer.ReplaceIFrames(ref codes,
						pageUrlNoQuery,
						pages.PageHtml,
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl,
						extraAttrib);

					// Encode <framset> tags
					HtmlReplacer.ReplaceFrameSets(ref codes,
						pageUrlNoQuery,
						pages.PageHtml,
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl);
				}

				// Encode <img> tags
				if (_UserOptions.Images)
				{
					string extraAttrib = "";

					// Add code to display orginal url address
					if (_UserOptions.OrginalUrl)
					{
						orginalUrlRequired = true;
						extraAttrib = Resources.STR_OrginalUrl_TagAttributeFormat;
					}

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					extraAttrib += Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

					HtmlReplacer.ReplaceImages(ref codes,
						pageUrlNoQuery,
						pages.PageAnyType,
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl,
						extraAttrib);

					// Encode background image of <body> , <table> and <td> tags
					HtmlReplacer.ReplaceBackgrounds(ref codes,
						pageUrlNoQuery,
						pages.PageAnyType,
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl);
				}
				else
				{
					string extraAttrib;

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					extraAttrib = Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

					HtmlReplacer.ReplaceImages(ref codes,
						pageUrlNoQuery,
						"{0}",
						pagePath,
						rootUrl,
						false,
						extraAttrib);
				}

				// Encodes script tags if RemoveScripts option is disabled
				if (_UserOptions.RemoveScripts == false)
				{
					HtmlReplacer.ReplaceScripts(ref codes,
						pageUrlNoQuery,
						pages.PageAnyType,
						pagePath,
						rootUrl,
						_UserOptions.EncodeUrl);

					// TODO: JSReplacer
					JSReplacer.ReplaceScriptTagCodes(ref codes);

					// V5.2: Replaces tags events using RegEx
					HtmlReplacer.ReplaceTagsEvents(ref codes,
					  pageUrlNoQuery,
					  pages.PageAnyType,
					  pagePath,
					  pageUrl,
					  rootUrl,
					  _UserOptions.EncodeUrl);
				}

				// Encode <embed> tags
				HtmlReplacer.ReplaceEmbeds(ref codes,
					pageUrlNoQuery,
					pages.PageAnyType,
					pagePath,
					rootUrl,
					_UserOptions.EncodeUrl);

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

				// Add dynamic encoding javascript codes
				string jsEncoderCodes = GenerateJsEncoderCodes(pageUrl,
							pageUrlNoQuery,
							pagePath,
							rootUrl);


				// Add jsEncoder codes to page
				ExtraCodesForPage = jsEncoderCodes + ExtraCodesForPage;

				// OrginalUrl additional injection html codes
				if (orginalUrlRequired)
				{
					// TODO: Check necessary
					ExtraCodesForPage = Resources.STR_OrginalUrl_FloatBar + ExtraCodesForPage;

					// Inject to html, right after the body element
					ExtraCodesForBody = Resources.STR_OrginalUrl_Functions + ExtraCodesForBody;
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

				codes = "<center><b>ASProxy has some errors! The delivered content may not work properly.</b></center>" + ex.Message + "<br />"
					+ codes;
			}
		}

		#endregion


		#region private methods

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
			// result.Append(Resources.ASProxyJavaScriptTag("", Consts.FilesConsts.JSAJAXWrapperCore));

			return result.ToString();
		}

		#endregion

		class Resources
		{
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

			public const string STR_SubmitForms_ExtraAttribute = " encodedurl=\"{1}\" methodorginal={2} ";
			public const string STR_IFrame_ExtraAttribute = " onload=ASProxyEncodeFrames() ";
			public const string STR_OrginalUrl_TagAttributeFormat = " onmouseout=ORG_OUT_() onmouseover=ORG_IN_(this) originalurl=\"{0}\" ";

			public const string STR_OrginalUrl_FloatBar = "<div id='__ASProxyOriginalURL' dir='ltr' style='display:block;font-family:tahoma;color:black;font-size:12px;padding:2px 5px 2px 5px;margin:0;position:absolute;left:0px;top:0px;width:98%;background:whitesmoke none;border:solid 2px black;overflow: visible;z-index:999999999;visibility:hidden;text-align:left;line-height:100%;'></div>";
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
					"function ORG_OUT_(){if(!_XFloatBar || _XFloatBar.Freeze)return;_XFloatBar.innerHTML='URL: ';_XFloatBar.CurrentUrl='';_wparent.clearTimeout(ASProxyUnvisibleHide);ASProxyUnvisibleHide=_wparent.setTimeout(ORG_HIDE_IT,500);}" +
					"function ORG_HIDE_IT(){if(_XFloatBar.Freeze)return;_XFloatBar.style.visibility='hidden';_XFloatBar.innerHTML='';}" +
					"_wparent.onscroll=ORG_Position_;" +
					"if(typeof(_ASProxy)!='undefined')_ASProxy.AttachEvent(document,'keydown',function(aEvent){var ev = window.event ? window.event : aEvent; ORG_Position_();" +
					"if(ev.ctrlKey && ev.shiftKey && ev.keyCode==88){if(typeof(_XFloatBar)=='undefined')return;" +
					"if(_XFloatBar.Freeze){_XFloatBar.Freeze=false;ORG_HIDE_IT();}" +
					"else if(_XFloatBar.CurrentUrl!=''){_XFloatBar.Freeze=true;" +
					"_XFloatBar.innerHTML=_XFloatBar.innerHTML+\"<br /><span style='color:navy;'>Press Ctrl+Shift+X again to unfreeze this bar.<span/>\";" +
					"}}});" +
					"</script>";
		}

	}
}
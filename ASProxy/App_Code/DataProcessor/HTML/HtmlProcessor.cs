using System;
using System.Text;

namespace SalarSoft.ASProxy
{
	public class HtmlProcessor
	{
		public OptionsType Options = OptionsType.GetDefault(true);
		private WebDataCore fUrlData;
		private string fDefaultPage = FilesConsts.DefaultPage;
		private string fImagesPage = FilesConsts.ImagesPage;
		private string fDirectHtmlPage = FilesConsts.DirectHtmlPage;
		private string fDirectCSSPage = FilesConsts.DirectCSSPage;
		private string fDirectJSPage = FilesConsts.DirectJSPage;
		private string fDirectDataPage = FilesConsts.DirectDataPage;
		private Encoding fPageEncoding = Encoding.UTF8;
		private bool fHtmlIsFrameSet = false;
		private string fPageTitle = "";
		private string fDocType = "";
		private string fPageInitializerCodes = "";

		private const string javaScriptStatusMainCode = "<div id='__ASProxyOriginalURL' dir='ltr' style='display:block;font-family:verdana;color:black;font-size:11px;padding:2px 5px 2px 5px;margin:0;position:absolute;left:0px;top:0px;width:98%;background:whitesmoke none;border:solid 2px black;overflow: visible;z-index:999999999;visibility:hidden;text-align:left;'></div>";
		private const string javaScriptStatusCode =
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
				"function ORG_IN_(obj){if(!ASProxyOriginalURL)return;ORG_Position_();var attrib=obj.attributes['originalurl'];if(attrib!=null)attrib=attrib.value; else attrib=null;if(attrib!='undefined' && attrib!='' && attrib!=null){_wparent.clearTimeout(ASProxyUnvisibleHide);ASProxyOriginalURL.innerHTML='URL: <span style=\"color:maroon;\">'+attrib+'</span>';ASProxyOriginalURL.style.visibility='visible';}}" +
				"function ORG_OUT_(){if(!ASProxyOriginalURL)return;ASProxyOriginalURL.innerHTML='URL: ';_wparent.clearTimeout(ASProxyUnvisibleHide);ASProxyUnvisibleHide=_wparent.setTimeout('ORG_HIDE_IT()',500);}" +
				"function ORG_HIDE_IT(){ASProxyOriginalURL.style.visibility='hidden';ASProxyOriginalURL.innerHTML='';}" +
				"_wparent.onscroll=ORG_Position_;" +
				"</script>";


		public HtmlProcessor(WebDataCore urlData)
		{
			fUrlData = urlData;
		}

		public string Execute()
		{
			string html = Processors.GetString(fUrlData, Options.IgnorePageEncoding, out fPageEncoding);

			if (Options.IFrame)
				fHtmlIsFrameSet = HtmlTags.IsFramesetHtml(ref html);
			if (Options.DisplayPageTitle)
				fPageTitle = HtmlParser.GetTagContent(ref html, "title");
			if (Options.DocType)
				fDocType = HtmlTags.GetDocType(ref html);

			return Execute(html, fUrlData.ResponseInfo.ResponseUrl, fUrlData.ResponseInfo.SiteBasePath);
		}

		private string Execute(string html, string pageUrl, string siteBasePath)
		{
			try
			{
				bool addAdditionalHtmlCodes = false;
				const string _DisplayOrginalUrlFormat = " onmouseout=ORG_OUT_() onmouseover=ORG_IN_(this) originalurl=\"{0}\" ";

				string pageUrlWithoutParameters = UrlProvider.GetPagePathWithoutParameters(pageUrl);
				string requestedUrlPageBase = "";
				try
				{
					requestedUrlPageBase = UrlProvider.AddSlashToEnd(UrlProvider.GetUrlPagePath(pageUrl));
				}
				catch
				{
					requestedUrlPageBase = UrlProvider.AddSlashToEnd(pageUrl);
				}


				// Renames ASPDotNET standard ViewState name to a temporary name
				// This name will reset to default when the page posted back
				HtmlReplacer.ReplaceAspDotNETViewState(ref html);


				// If remove scripts chosen, remove all the scripts.
				if (Options.RemoveScripts)
				{
					HtmlReplacer.RemoveScripts(ref html);
				}

				// If links chosen, apply the BASE tag to the urls.
				if (Options.Links)
				{
					string baseHref;
					if (HtmlReplacer.ReplaceBaseSources(ref html, true, out baseHref))
					{
						requestedUrlPageBase = UrlProvider.AddSlashToEnd(baseHref);

						// BUGFIX v4.6.1 :: UrlProvider.GetUrlSiteBasePath is added.
						siteBasePath = UrlProvider.GetUrlSiteBasePath(requestedUrlPageBase);
					}
				}

				if (Options.CssLink)
				{
					HtmlReplacer.ReplaceCssLinkSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fDirectCSSPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl);

					// This function replaces "Import" rule and background images!
					// So, this breaches to backgound option role. Since v4.0
					CSSReplacer.ReplaceStyleTagStyleUrl(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fDirectCSSPage), UrlProvider.AddArgumantsToUrl(fDirectDataPage, Options.EncodeUrl), requestedUrlPageBase, siteBasePath, Options.EncodeUrl);
				}


				// It seems with javascript encoding this methods are useless!!
				// I'm not sure about javascript working, so still I use this functions
				if (Options.Links)
				{
					string format = "";

					// Add displaying orginal url address code
					if (Options.DisplayOrginalUrl == true)
					{
						addAdditionalHtmlCodes = true;
						format = _DisplayOrginalUrlFormat;
					}

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					format += Consts.attrAlreadyEncodedAttributeWithValue;

					HtmlReplacer.ReplaceAnchorSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fDefaultPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl, format);
					HtmlReplacer.ReplaceHttpRefreshSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fDefaultPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl);
				}
				else
				{
					string format = "";
					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					format += Consts.attrAlreadyEncodedAttributeWithValue;

					HtmlReplacer.ReplaceAnchorSources(ref html, pageUrlWithoutParameters, "{0}", requestedUrlPageBase, siteBasePath, false, format);
					HtmlReplacer.ReplaceHttpRefreshSources(ref html, pageUrlWithoutParameters, "{0}", requestedUrlPageBase, siteBasePath, Options.EncodeUrl);
				}

				// Encode <iframe> tags
				if (Options.IFrame)
				{
					const string format = " onload=ASProxyEncodeFrames() ";
					HtmlReplacer.ReplaceIFrameSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fDirectHtmlPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl, format);
				}

				// Encode <framset> tags
				if (Options.FrameSet)
					HtmlReplacer.ReplaceFrameSetSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fDirectHtmlPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl);

				// Encode <img> tags
				if (Options.Images)
				{
					string format = "";

					// Add code to display orginal url address
					if (Options.DisplayOrginalUrl == true)
					{
						addAdditionalHtmlCodes = true;
						format = _DisplayOrginalUrlFormat;
					}

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					format += Consts.attrAlreadyEncodedAttributeWithValue;

					HtmlReplacer.ReplaceImageSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fImagesPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl, format);
				}
				else
				{
					string format;

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					format = Consts.attrAlreadyEncodedAttributeWithValue;
					HtmlReplacer.ReplaceImageSources(ref html, pageUrlWithoutParameters, "{0}", requestedUrlPageBase, siteBasePath, false, format);
				}

				// Encode <script> tags if RemoveScripts option disabled
				if (Options.Scripts && Options.RemoveScripts == false)
				{
					HtmlReplacer.ReplaceScriptSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fDirectJSPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl);
					JSReplacer.ReplaceScriptTagCodes(ref html);
				}

				// Encode background image of <body> , <table> and <td> tags
				if (Options.BackImages)
					HtmlReplacer.ReplaceBackgroundSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fImagesPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl);

				// Encode <embed> tags
				if (Options.EmbedObjects)
					HtmlReplacer.ReplaceEmbedSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fDirectDataPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl);

				// Encode <form> tags
				if (Options.SubmitForms)
				{
					string format;

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					format = Consts.attrAlreadyEncodedAttributeWithValue
						+ " encodedurl=\"{1}\" methodorginal={2} ";
					HtmlReplacer.ReplaceFormsSources(ref html, pageUrlWithoutParameters, AddArgumantsToUrl(fDefaultPage), requestedUrlPageBase, siteBasePath, Options.EncodeUrl, Options.SubmitForms, format);
				}
				else
				{
					string format;

					// Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
					format = Consts.attrAlreadyEncodedAttributeWithValue
						+ " encodedurl=\"{1}\" methodorginal={2} ";
					HtmlReplacer.ReplaceFormsSources(ref html, pageUrlWithoutParameters, "{0}", requestedUrlPageBase, siteBasePath, false, Options.SubmitForms, format);
				}

				//
				if (addAdditionalHtmlCodes)
				{
					fPageInitializerCodes = javaScriptStatusMainCode + javaScriptStatusCode;

					html = javaScriptStatusCode + html;
				}

				// Add dynamic encoding javascript codes
				html = GenerateJavascriptDynamicEncoderCode(pageUrl, pageUrlWithoutParameters, requestedUrlPageBase, siteBasePath)
					+ html;
				return html;
			}
			catch (Exception err)
			{
				ASProxyExceptions.LogException(err);
				return "<center><b>ASProxy has some errors!</b></center>" + err.Message + "<br />"
					+ html;
			}
		}


		/// <summary>
		/// Generate dynamic encoding javascript codes
		/// </summary>
		/// <param name="pageBasePath">base path of current request page</param>
		/// <returns>Javascript codes</returns>
		public string GenerateJavascriptDynamicEncoderCode(string pageUrl, string pageUrlWithoutParameters, string requestUrlDir, string requestUrlBaseDir)
		{
			StringBuilder strCodes = new StringBuilder();

			// Variables for ASProxy encoder
			strCodes.Append(Consts.JSEncoder_ASProxyEncodeUrl + "=" + Convert.ToInt32(Options.EncodeUrl).ToString() + ";");
			strCodes.Append(Consts.JSEncoder_ASproxyImagesEnabled + "=" + Convert.ToInt32(Options.DisplayOrginalUrl).ToString() + ";");
			strCodes.Append(Consts.JSEncoder_ASproxyOriginalUrlEnabled + "=" + Convert.ToInt32(Options.Images).ToString() + ";");
			strCodes.Append(Consts.JSEncoder_ASproxyLinksEnabled + "=" + Convert.ToInt32(Options.Links).ToString() + ";");
			strCodes.Append(Consts.JSEncoder_ASProxyFormsEnabled + "=" + Convert.ToInt32(Options.SubmitForms).ToString() + ";");
			strCodes.Append(Consts.JSEncoder_ASProxyFramesEnabled + "=" + Convert.ToInt32(Options.IFrame).ToString() + ";");

			strCodes.Append(Consts.JSEncoder_RequestUrlFull + "=\"" + pageUrl + "\";");
			strCodes.Append(Consts.JSEncoder_RequestUrlNoParam + "=\"" + pageUrlWithoutParameters + "\";");
			strCodes.Append(Consts.JSEncoder_RequestUrlDir + "=\"" + requestUrlDir + "\";");
			strCodes.Append(Consts.JSEncoder_RequestUrlBaseDir + "=\"" + requestUrlBaseDir + "\";");
			strCodes.Append(Consts.JSEncoder_ASProxyDefaultPage + "=\"" + FilesConsts.DefaultPage + "\";");
			strCodes.Append(Consts.JSEncoder_ASProxySiteBaseDir + "=\"" + UrlProvider.GetAppAbsolutePath() + "\";");
			strCodes.Append(Consts.JSEncoder_ASProxySiteHostBaseDir + "=\"" + UrlProvider.GetAppAbsoluteBasePath() + "\";");
			strCodes.Append(Consts.JSEncoder_ASProxyDefaultPagePath + "=\"" + UrlProvider.JoinUrl(UrlProvider.GetAppAbsolutePath(), FilesConsts.DefaultPage) + "\";");

			strCodes.Append(Consts.JSEncoder_Base64Unknowner + "=\"" + Consts.Base64Unknowner + "\";");
			strCodes.Append(Consts.JSEncoder_RequestCookieName + "=\"" + CookieManager.GetCookieNameRequest(pageUrl) + "\";");

			Uri pageUri = new Uri(pageUrl);

			// Window location variables
			strCodes.Append(Consts.JSEncoder_ReqLocBookmark + "=\"" + pageUri.Fragment + "\";");
			strCodes.Append(Consts.JSEncoder_ReqLocHost + "=\"" + pageUri.Authority + "\";");
			strCodes.Append(Consts.JSEncoder_ReqLocHostname + "=\"" + pageUri.Host + "\";");
			strCodes.Append(Consts.JSEncoder_ReqLocPathname + "=\"" + pageUri.AbsolutePath + "\";");
			strCodes.Append(Consts.JSEncoder_ReqLocPort + "=\"" + pageUri.Port.ToString() + "\";");
			strCodes.Append(Consts.JSEncoder_ReqLocProtocol + "=\"" + pageUri.Scheme + ':' + "\";");
			strCodes.Append(Consts.JSEncoder_ReqLocQueries + "=\"" + pageUri.Query + "\";");

			StringBuilder result = new StringBuilder();
			// AJAX wrapper core
			result.Append(HtmlTags.JavascriptTag("", FilesConsts.JSAJAXWrapperCore));

			// ASProxy encoder variables
			result.Append(HtmlTags.JavascriptTag(strCodes.ToString(), ""));

			// Base64 encoder 
			result.Append(HtmlTags.JavascriptTag("", FilesConsts.JSBase64));

			// ASProxy encoder 
			result.Append(HtmlTags.JavascriptTag("", FilesConsts.JSASProxyEncoder));

			return result.ToString();
		}

		public string AddArgumantsToUrl(string url)
		{
			return UrlProvider.AddArgumantsToUrl(url, Options.EncodeUrl);
		}
		#region Properites
		public Encoding PageEncoding
		{
			get { return fPageEncoding; }
		}
		public string PageTitle
		{
			get { return fPageTitle; }
		}
		public bool HtmlIsFrameSet
		{
			get { return fHtmlIsFrameSet; }
		}
		public string DocType
		{
			get { return fDocType; }
		}
		public string PageInitializerCodes
		{
			get { return fPageInitializerCodes; }
		}

		public string ImagesPage
		{
			get { return fImagesPage; }
			set { fImagesPage = value; }
		}
		public string DirectHtmlPage
		{
			get { return fDirectHtmlPage; }
			set { fDirectHtmlPage = value; }
		}
		public string DirectDataPage
		{
			get { return fDirectDataPage; }
			set { fDirectDataPage = value; }
		}

		#endregion
	}
}
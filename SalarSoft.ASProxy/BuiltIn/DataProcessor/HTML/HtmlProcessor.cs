using System;
using SalarSoft.ASProxy.Exposed;
using System.Text;

namespace SalarSoft.ASProxy.BuiltIn
{

    /// <summary>
    /// Summary description for ExHtmlProcessor
    /// </summary>
    public class HtmlProcessor : ExHtmlProcessor
    {
        #region variables
        #endregion

        #region properties
        #endregion

        #region public methods
        /// <summary>
        /// Processes the html codes
        /// </summary>
        public override string Execute()
        {
            Encoding _encoding;
            string resultHtml = StringStream.GetString(WebData.ResponseData,
                                    UserOptions.ForceEncoding,
                                    out _encoding);

            ContentEncoding = _encoding;

            if (UserOptions.IFrame)
                IsFrameSet = HtmlTags.IsFramesetHtml(ref resultHtml);

            if (UserOptions.PageTitle)
                PageTitle = HtmlParser.GetTagContent(ref resultHtml, "title");

            if (UserOptions.DocType)
                DocType = HtmlTags.GetDocType(ref resultHtml);

            IWebData webData = (IWebData)WebData;

            // Execute the result
            Execute(ref resultHtml,
                webData.ResponseInfo.ResponseUrl,
                webData.ResponseInfo.ResponseRootUrl);

            // the result
            return resultHtml;
        }

        /// <summary>
        /// Processes the html codes
        /// </summary>
        /// <param name="codes">Html codes</param>
        /// <param name="pageUrl">Page url. E.G. http://Site.com/users/profile.aspx?uid=90</param>
        /// <param name="rootUrl">Root path. E.G. http://Site.com/</param>
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
                if (UserOptions.CssLink)
                {
                    HtmlReplacer.ReplaceCssLinks(ref codes,
                        pageUrlNoQuery,
                        pages.PageCss,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl);

                    // TODO: CSSReplacer
                    // This function replaces "Import" rule and background images!
                    // So, this breaches to background option role. Since v4.0
                    CSSReplacer.ReplaceStyleTagStyleUrl(ref codes,
                        pageUrlNoQuery, 
                        pages.PageCss, 
                        pages.PageAnyType,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl);
                }

                // It seems with javascript encoding these methods are useless!!
                // The javascript may be disabled in browser so we have to this anyway
                if (UserOptions.Links)
                {
                    string extraAttib = "";

                    // Add displaying orginal url address code
                    if (UserOptions.OrginalUrl)
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
                        UserOptions.EncodeUrl,
                        extraAttib);

                    HtmlReplacer.ReplaceHttpRefresh(ref codes,
                        pageUrlNoQuery,
                        pages.PageDefault,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl);
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
                        UserOptions.EncodeUrl);
                }

                // Encode <iframe> tags
                if (UserOptions.IFrame)
                {
                    string extraAttrib = Resources.STR_IFrame_ExtraAttribute;

                    HtmlReplacer.ReplaceIFrames(ref codes,
                        pageUrlNoQuery,
                        pages.PageHtml,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl,
                        extraAttrib);
                }

                // Encode <framset> tags
                if (UserOptions.FrameSet)
                {
                    HtmlReplacer.ReplaceFrameSets(ref codes,
                        pageUrlNoQuery,
                        pages.PageHtml,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl);
                }


                // Encode <img> tags
                if (UserOptions.Images)
                {
                    string extraAttrib = "";

                    // Add code to display orginal url address
                    if (UserOptions.OrginalUrl)
                    {
                        orginalUrlRequired = true;
                        extraAttrib = Resources.STR_OrginalUrl_TagAttributeFormat;
                    }

                    // Add an attribute to determine that this tag already encoded and javascript encoder shouldn't encode it again.
                    extraAttrib += Consts.ClientContent.attrAlreadyEncodedAttributeWithValue;

                    HtmlReplacer.ReplaceImages(ref codes,
                        pageUrlNoQuery,
                        pages.PageImage,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl,
                        extraAttrib);

                    // Encode background image of <body> , <table> and <td> tags
                    HtmlReplacer.ReplaceBackgrounds(ref codes,
                        pageUrlNoQuery,
                        pages.PageImage,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl);
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
                if (UserOptions.Scripts && UserOptions.RemoveScripts == false)
                {
                    HtmlReplacer.ReplaceScripts(ref codes,
                        pageUrlNoQuery,
                        pages.PageJS,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl);

                    // TODO: JSReplacer
                    JSReplacer.ReplaceScriptTagCodes(ref codes);

                    // V5.2: Replaces tags events using RegEx
                    HtmlReplacer.ReplaceTagsEvents(ref codes,
                      pageUrlNoQuery,
                      pages.PageJS,
                      pagePath,
                      rootUrl,
                      UserOptions.EncodeUrl);
                }

                // Encode <embed> tags
                if (UserOptions.EmbedObjects)
                {
                    HtmlReplacer.ReplaceEmbeds(ref codes,
                        pageUrlNoQuery,
                        pages.PageAnyType,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl);
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

                codes= "<center><b>ASProxy has some errors! The delivered content may not work properly.</b></center>" + ex.Message + "<br />"
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
        private string GenerateJsEncoderCodes(
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

        #endregion

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
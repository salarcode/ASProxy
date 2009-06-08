using System;
using System.Collections.Generic;
using System.Text;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy.BuiltIn
{
    public class CSSProcessor : ExCssProcessor 
    {
        #region variables
        #endregion

        #region properties
        #endregion

        #region public methods
        public override string Execute()
        {
            Encoding _encoding;
            string resultCodes = StringStream.GetString(WebData.ResponseData,
                                    UserOptions.ForceEncoding,
                                    out _encoding);
            ContentEncoding = _encoding;

            IWebData webData = (IWebData)WebData;

            // Execute the result
            Execute(ref resultCodes,
                webData.ResponseInfo.ResponseUrl,
                webData.ResponseInfo.ResponseRootUrl);

            // the result
            return resultCodes;
        }

        public override void Execute(ref string codes, string pageUrl, string rootUrl)
        {
            try
            {
                //string urlPagePath = "";
                //urlPagePath = UrlProvider.AddSlashToEnd(UrlProvider.GetUrlPagePath(responsePageUrl));
                //string pageUrlWithoutParameters = UrlProvider.GetPagePathWithoutParameters(responsePageUrl);

                // ASProxy pages url formats generator
                ASProxyPagesFormat pages = new ASProxyPagesFormat(UserOptions);

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

                if (UserOptions.Images)
                {
                    // For @Import rule
                    CSSReplacer.ReplaceCSSClassStyleUrl(ref codes,
                        pageUrlNoQuery,
                        pages.PageCss,
                        pagePath,
                        rootUrl,
                        UserOptions.EncodeUrl, 
                        true);

                    // For backgrounds
                    CSSReplacer.ReplaceCSSClassStyleUrl(ref codes,
                        pageUrlNoQuery,
                        pages.PageAnyType,
                        pagePath,
                        rootUrl, 
                        UserOptions.EncodeUrl, 
                        false);
                }

            }
            catch (Exception ex)
            {
                // error logs
                if (Systems.LogSystem.ErrorLogEnabled)
                    Systems.LogSystem.LogError(ex, pageUrl);

                codes = "/* ASProxy has some errors! \n"
                    + ex.Message + " */"
                    + codes;
            }
        }
        #endregion

    }
}

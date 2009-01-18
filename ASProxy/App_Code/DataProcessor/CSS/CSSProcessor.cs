using System;
using System.Text;

namespace SalarSoft.ASProxy
{
    public class CSSProcessor
    {
        private string fDirectDataPage =FilesConsts.DirectDataPage;
        private string fDirectCSSPage = FilesConsts.DirectCSSPage;
        private Encoding fResponsePageEncoding = Encoding.UTF8;
  		public OptionsType Options = OptionsType.GetDefault(true);
        private WebDataCore fData;


        #region Properites
        public Encoding ResponsePageEncoding
        {
            get { return fResponsePageEncoding; }
        }
        #endregion

        public CSSProcessor(WebDataCore urlData)
		{
            fData = urlData;
		}

        public string Execute()
        {
            string html = Processors.GetString(fData, Options.IgnorePageEncoding, out fResponsePageEncoding);
            return Execute(html, fData.ResponseInfo.ResponseUrl, fData.ResponseInfo.SiteBasePath);
        }

        //public string Execute(string html, UrlData urldata)//Useless 3.6 --> 2007-5-27
        //{
        //    return Execute(html, fData.ResponsePageUrl, fData.ResponseSiteBasePath);
        //}

        private string Execute(string html, string responsePageUrl, string responseSiteBasePath)
        {
            try
            {
                string urlPagePath = "";
				urlPagePath = UrlProvider.AddSlashToEnd(UrlProvider.GetUrlPagePath(responsePageUrl));
				string pageUrlWithoutParameters = UrlProvider.GetPagePathWithoutParameters(responsePageUrl);

                if (Options.BackImages)
                {
                    // For @Import rule
                    CSSReplacer.ReplaceCSSClassStyleUrl(ref html, pageUrlWithoutParameters, UrlProvider.AddArgumantsToUrl(fDirectCSSPage, Options.EncodeUrl), urlPagePath, responseSiteBasePath, Options.EncodeUrl, true);

                    // For backgrounds
                    CSSReplacer.ReplaceCSSClassStyleUrl(ref html, pageUrlWithoutParameters, UrlProvider.AddArgumantsToUrl(fDirectDataPage, Options.EncodeUrl), urlPagePath, responseSiteBasePath, Options.EncodeUrl, false);
                }

                return html;
            }
            catch (Exception err)
            {
				ASProxyExceptions.LogException(err);

                return "/* ASProxy has some errors! \n" + err.Message + " */"
                    + html;
            }
        }
    }
}
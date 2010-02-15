using System;

namespace SalarSoft.ASProxy
{

    /// <summary>
    /// ASProxy all consts
    /// </summary>
    public class Consts
    {
        /// <summary>
        /// General consts
        /// </summary>
        public class General
        {
            public const string ASProxyName = "ASProxy";
            public const string ASProxyVersion = "5.5b4";
            public const string ASProxyVersionFull = "5.5.0.4";

			public const string ContextUserOptionsKey = "ContextUserOptions";
        }

        /// <summary>
        /// Back-End conenction consts
        /// </summary>
        public class BackEndConenction
        {
            public const string ASProxyAgentVersion = "ASProxy/" + General.ASProxyVersion;
            public const string ASProxyProjectUrl = "http://asproxy.sourceforge.net";
            public const string ASProxyUserAgent = "ASProxy (compatible; Mozilla/5.0; " + ASProxyAgentVersion + "; " + ASProxyProjectUrl + ";) " + ASProxyAgentVersion;

            public const int RequestTimeOut = 60000; //1 min
            public const int RequestFormReadWriteTimeOut = 70000; //110 seconds

            /// <summary>
            /// The wrapped ajax headers will send with this header to asproxy
            /// </summary>
            public const string AJAX_Headers = "X-ASProxy-AJAX-Headers";
            public const string AJAX_ReferrerHeaders = "X-ASProxy-AJAX-Referrer";
        }

        /// <summary>
        /// Front-End presentation consts
        /// </summary>
        public class FrontEndPresentation
        {
            public const string UserOptionsCookieName = "ASProxyCFG";
			public const string HttpCompressEncoding = "Encoding";
			public const string HttpCompressor = "Compression";

        }


        /// <summary>
        /// Queries sent from url
        /// </summary>
        public class Query
        {
            public const string WebMethod = "method";
            public const string Redirect = "ref";
            public const string Decode = "dec";
            public const string UrlAddress = "url";
            public const string AjaxUrlAddress = "ajaxurl";

            /// <summary>
            /// Used to make base64 coding unknown for filtering machines
            /// </summary>
            public const string Base64Unknowner = "B6X!";
        }

        /// <summary>
        /// Data proccessors consts
        /// </summary>
        public class DataProccessing
        {
            public const string ASPDotNETViewState = "__VIEWSTATE";
            public const string ASPDotNETRenamedViewState = "ASPROXY_FOR_VIEWSTATE";
        }

        /// <summary>
        /// Client content generators consts
        /// </summary>
        public class ClientContent
        {
			public const string JSEncoder_UserConfig = "_userConfig={{EncodeUrl:{0}, OrginalUrl:{1}, Links:{2}, Images:{3}, Forms:{4}, Frames:{5}, Cookies:{6}, RemScripts:{7}, RemObjects:{8}, TempCookies:{9} }}; ";
			public const string JSEncoder_RequestInfo = @"_reqInfo={{pageUrl:""{0}"", pageUrlNoQuery:""{1}"", pagePath:""{2}"", rootUrl:""{3}"", cookieName:'{4}', cookieNameExt:'{5}', ASProxyUrl:""{6}"", ASProxyPath:""{7}"", ASProxyRoot:""{8}"", ASProxyPageName:'{9}', UrlUnknowner:'{10}'}}; ";
			public const string JSEncoder_RequestLocation = @"_reqInfo.location={{ Hash:'{0}', Host:""{1}"", Hostname:""{2}"", Pathname:""{3}"", Search:""{4}"", Port:'{5}', Protocol:'{6}' }}; ";
			public const string JSEncoder_AppliedCookieNames = @"_reqInfo.appliedCookiesList=[{0}]; ";

			public const string JSEncoder_ASProxyEncoderMethodName = "__UrlEncoder";
			public const string JSEncoder_ASProxySetCookieMethodName = "__CookieSet";
			public const string JSEncoder_ASProxyGetCookieMethodName = "__CookieGet";
			public const string JSEncoder_ASProxyWindowLocOverrider = "_WindowLocation";
			public const string JSEncoder_ASProxyWindowLocHostName = "_WindowLocation.host";
			public const string JSEncoder_ASProxyLocationXDomain = "document.XDomain";

            public const string attrAlreadyEncodedAttribute = "asproxydone";
			public const string attrAlreadyEncodedValue = "1";
			public const string attrAlreadyEncodedIgnore = "2";
			public const string attrAlreadyEncodedAttributeWithValue = " " + attrAlreadyEncodedAttribute + "=" + attrAlreadyEncodedValue + " ";
			public const string attrAlreadyEncodedAttributeIgnore = " " + attrAlreadyEncodedAttribute + "='" + attrAlreadyEncodedIgnore + "' ";
		}

        /// <summary>
        /// Files consts
        /// </summary>
        public class FilesConsts
        {
            /// <summary>
            /// The default page variable is static and can be changed during application execution
            /// </summary>
			public static string PageDefault_Dynamic = "surf.aspx";

            public const string Dir_Scripts = "scripts";
			public const string Dir_AppData = "App_Data";
			public const string Dir_UpdateSources = "UpdateSources";
			public const string Dir_Plugins = Dir_AppData + "\\" + "Plugins";
			public const string Dir_Providers = Dir_AppData + "\\" + "Providers";
			public const string Dir_Updater = Dir_AppData + "\\" + "Updater";
			public const string File_PluginInfoExt = "*.xml";
			public const string File_ProviderInfoExt = "*.xml";

            public const string AJAXHandler = "ajax.ashx";
            public const string PageAnyType = "getany.ashx";
            public const string PageDownload = "download.ashx";
            public const string PageAuthorization = "authorization.aspx";
            public const string PageDirectHtml = "gethtml.ashx";
            public const string PageDirectJS = "getjs.ashx";
            public const string PageImages = "images.ashx";

			public const string JSASProxyEncoder = Dir_Scripts + "/" + "asproxyencoder.js";
			public const string JSBase64 = Dir_Scripts + "/" + "base64encoder.js";

			// Usless since v5.5b4
			//public const string JSAJAXWrapperCore = Dir_Scripts + "/" + "ajaxwrapper.js";

            public const string PageCustomErrors = "error_page.htm";
        }
    }
}
using System;

namespace SalarSoft.ASProxy
{

	public class GlobalConsts
	{
		public const int RequestTimeOut = 60000;//1 min
		public const int RequestFormReadWriteTimeOut = 70000;//110 seconds
		public const string ASProxyName = "ASProxy";
		public const string ASProxyVersion = "5.1";
		public const string ASProxyAgentVersion = "ASProxy/" + ASProxyVersion + "";

		public const string ASProxyProjectUrl = "http://asproxy.sourceforge.net";
		public const string ASProxyUserAgent = "ASProxy (compatible; Mozilla/5.0; " + ASProxyAgentVersion + "; " + ASProxyProjectUrl + ";) " + ASProxyAgentVersion;

		public const string CookieMasterName = "SalarSoft.ASProxy";
		public const string HttpCompressorCookieMasterName = "SalarSoft.ASProxy.HttpCompressor";
	}

	public class FilesConsts
	{
		public const string Dir_Scripts = "scripts";
		public const string Dir_Bin = "bin";
		public const string Dir_UpdateSources = "UpdateSources";

		/// <summary>
		/// The default page variable is static and can change during application execution
		/// </summary>
		public static string DefaultPage = "default.aspx";
		public const string AJAXHandler = "ajax.ashx";
		public const string ImagesPage = "images.aspx";
		public const string DownloadPage = "download.aspx";
		public const string AuthorizationPage = "authorization.aspx";
		public const string DirectHtmlPage = "gethtml.aspx";
		public const string DirectCSSPage = "getcss.aspx";
		public const string DirectJSPage = "getjs.aspx";
		public const string DirectDataPage = "getany.aspx";

		public const string JSASProxyEncoder = Dir_Scripts + "/" + "asproxyencoder.js";
		public const string JSBase64 = Dir_Scripts + "/" + "base64encoder.js";

		public const string JSAJAXWrapperCore = Dir_Scripts + "/" + "ajaxwrapper.js";

		public const string CustomErrorsPage = "error_page.htm";
	}
	public class Consts
	{
		/// <summary>
		/// The wrapped ajax headers will send with this header to asproxy
		/// </summary>
		public const string AJAX_Headers = "X-ASProxy-AJAX-Headers";
		public const string AJAX_ReferrerHeaders = "X-ASProxy-AJAX-Referrer";

		public const string ASPDotNETViewState = "__VIEWSTATE";
		public const string ASPDotNETRenamedViewState = "WELLKNOWN_ASPROXY_BAD_ASP_VIEWSTATE_SHELTER";

		//public const string qIsPostForm = "posted";
		public const string qWebMethod = "method";
		public const string qRedirect = "ref";
		public const string qDecode = "decode";
		public const string qUrlAddress = "url";
		public const string qAjaxUrl = "ajaxurl";

		/// <summary>
		/// Used to make base64 coding unknown for filtering machines
		/// </summary>
		public const string Base64Unknowner = "B64Coded!";

		public const string JSEncoder_ASproxyOriginalUrlEnabled = "__ASProxyOriginalUrlEnabled";
		public const string JSEncoder_ASproxyLinksEnabled = "__ASProxyLinksEnabled";
		public const string JSEncoder_ASproxyImagesEnabled = "__ASProxyImagesEnabled";
		public const string JSEncoder_ASProxyFormsEnabled = "__ASProxyFormsEnabled";
		public const string JSEncoder_ASProxyFramesEnabled = "__ASProxyFramesEnabled";
		public const string JSEncoder_ASProxyEncodeUrl = "__ASProxyEncodeUrl";
		public const string JSEncoder_Base64Unknowner = "__B64Unknowner";
		public const string JSEncoder_ASProxyDefaultPage = "__ASProxyDefaultPage";

		public const string JSEncoder_RequestCookieName = "__ReqCookieName";
		public const string JSEncoder_RequestUrlBaseDir = "__ReqUrlBaseDir";
		public const string JSEncoder_RequestUrlDir = "__ReqUrlDir";
		public const string JSEncoder_RequestUrlNoParam = "__ReqUrlNoParam";
		public const string JSEncoder_RequestUrlFull = "__ReqUrlFull";

		public const string JSEncoder_ReqLocBookmark = "__WLocHash";
		public const string JSEncoder_ReqLocHost = "__WLocHost";
		public const string JSEncoder_ReqLocHostname = "__WLocHostname";
		public const string JSEncoder_ReqLocPathname = "__WLocPathname";
		public const string JSEncoder_ReqLocQueries = "__WLocSearch";
		public const string JSEncoder_ReqLocPort = "__WLocPort";
		public const string JSEncoder_ReqLocProtocol = "__WLocProtocol";

		public const string JSEncoder_ASProxySiteBaseDir = "__ASProxySiteBaseDir";
		public const string JSEncoder_ASProxyDefaultPagePath = "__ASProxyDefaultPagePath";
		public const string JSEncoder_ASProxySiteHostBaseDir = "__ASProxySiteHostBaseDir";


		public const string JSEncoder_ASProxyEncoderMethodName = "ASProxyEncoder";
		public const string JSEncoder_ASProxySetCookieMethodName = "ASProxySetCookie";
		public const string JSEncoder_ASProxyGetCookieMethodName = "ASProxyGetCookie";
		public const string JSEncoder_ASProxyWindowLocOverrider = "_WindowLocation";
		

		public const string attrAlreadyEncodedAttribute = "asproxydone";
		public const string attrAlreadyEncodedValue = "1";
		public const string attrAlreadyEncodedAttributeWithValue = " " + attrAlreadyEncodedAttribute + "=" + attrAlreadyEncodedValue;
	}
}
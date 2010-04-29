//**************************************************************************
// Product Name: ASProxy
// Description:  SalarSoft ASProxy is a free and open-source web proxy
// which allows the user to surf the net anonymously.
//
// MPL 1.1 License agreement.
// The contents of this file are subject to the Mozilla Public License
// Version 1.1 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS"
// basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
// License for the specific language governing rights and limitations
// under the License.
// 
// The Original Code is ASProxy (an ASP.NET Proxy).
// 
// The Initial Developer of the Original Code is Salar.Kh (SalarSoft).
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.Kh < salarsoftwares [@] gmail[.]com > (original author)
//
//**************************************************************************

namespace SalarSoft.ASProxy
{
    /// <summary>
    /// Provider types
    /// </summary>
    public enum ProviderType
    {
        IEngine,
        IWebData,
        ICookieManager,
		ILogSystem,
		IUAC,
		ICredentialCache,
        IHtmlProcessor,
        IJSProcessor,
        ICssProcessor
    }

	/// <summary>
	/// Represents what king of page is requesting.
	/// </summary>
	public enum RequesterType
	{
		Normal,
		Download,
		Image
	}

    ///<summary>
    /// Data type to process
    ///</summary>
    public enum DataTypeToProcess
    {
        /// <summary>
        /// Do not process data
        /// </summary>
        None,

        /// <summary>
        /// Detect data type automatically
        /// </summary>
        AutoDetect,
        Html,
        JavaScript,
        Css,

        /// <summary>
        /// Not implemented, Reserved
        /// </summary>
        AdobeFlash
    }

    ///<summary>
    /// Content MIME type
    ///</summary>
    public enum MimeContentType
    {
        text_html,
        text_plain,
        text_css,
        text_javascript,
        image_jpeg,
        image_gif,
        application
    }

    /// <summary>
    /// Last activity status
    /// </summary>
    public enum LastStatus
    {
        Normal,
        Error,
        ContinueWithError
    }

    /// <summary>
    /// Type of referrer used in ASProxy
    /// </summary>
    public enum ReferrerType
    {
        None,
        ASProxySite,
        Referrer,
        RequesterAsReferrer
    }


    public enum AutoRedirectType
    {
        /// <summary>
        /// Redirects to another page of current site
        /// </summary>
        RequestInternal,

        /// <summary>
        /// Redirects to out of currect requested site
        /// </summary>
        External,

        /// <summary>
        /// Redirects to ASProxy's another page
        /// </summary>
        ASProxyPages
    }

    /// <summary>
    /// Determines internet protocols which asproxy supports
    /// </summary>
    public enum InternetProtocols
    {
        HTTP,
        FTP,
        File,
        Other
    }


    /// <summary>
    /// Log entities type
    /// </summary>
    public enum LogEntity
    {
        UrlRequested,

        ImageRequested,

        DownloadRequested,

		Error,

		DebugInfo,

        ASProxyLoginPassed,

        AuthorizationRequired
    }


}
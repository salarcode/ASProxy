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
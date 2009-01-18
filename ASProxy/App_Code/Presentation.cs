using System;
using System.Web;

namespace SalarSoft.ASProxy
{

	/// <summary>
	/// Ready to use class for
	/// </summary>
	public class Presentation
	{
		#region variables
		private bool fGenerateForStandAlonePage = false;
		private bool fRedirectToOtherPages = false;
		private MimeContentType fContentType;
		private ProcessTypeForData fDataType = ProcessTypeForData.None;
		private OptionsType fOptions;
		private string fResponsePageTitle;
		private string fResponseUrl = "";
		#endregion

		#region Properties
		/// <summary>
		/// Gets the result url of request
		/// </summary>
		public string ResponseUrl
		{
			get { return fResponseUrl; }
		}
		public string ResponsePageTitle
		{
			get { return fResponsePageTitle; }
		}
		public OptionsType Options
		{
			get { return fOptions; }
			set { fOptions = value; }
		}
		public ProcessTypeForData DataType
		{
			get { return fDataType; }
			set { fDataType = value; }
		}
		public MimeContentType ContentType
		{
			get { return fContentType; }
			set { fContentType = value; }
		}
		/// <summary>
		/// Gets and sets applying the results. If this option is enable the result will apply to response 
		/// automatically. The response object will close after execution.
		/// </summary>
		public bool GenerateForStandAlonePage
		{
			get { return fGenerateForStandAlonePage; }
			set { fGenerateForStandAlonePage = value; }
		}
		public bool RedirectToOtherPages
		{
			get { return fRedirectToOtherPages; }
			set { fRedirectToOtherPages = value; }
		}
		#endregion

		public Presentation(ProcessTypeForData dataType, MimeContentType ContentType, bool loadOptions)
		{
			fContentType = ContentType;
			fDataType = dataType;
			if (loadOptions)
			{
				fOptions = ASProxyConfig.GetCookieOptions();
			}
		}

		public string Execute(HttpRequest request)
		{
			ASProxyEngine engine = new ASProxyEngine(fDataType, false);
			engine.RequestInfo.ContentType = fContentType;
			engine.Options = fOptions;
			engine.Initialize(request);
			return InternalExecute(engine);
		}

		public string Execute(string requestUrl)
		{
			ASProxyEngine engine = new ASProxyEngine(fDataType, false);
			engine.RequestInfo.ContentType = fContentType;
			engine.Options = fOptions;
			engine.Initialize(requestUrl);
			return InternalExecute(engine);
		}

		private string InternalExecute(ASProxyEngine engine)
		{
			HttpResponse response = HttpContext.Current.Response;
			if (fGenerateForStandAlonePage)
			{
				// Stand alone page process
				try
				{
					// Execute the request
					engine.Execute(response);

					fResponseUrl = engine.ResponseInfo.ResponseUrl;

					// Check the response state
					if (engine.LastStatus == LastActivityStatus.Error)
					{
						response.Clear();
						Common.ClearASProxyRespnseHeader(response);
						response.ContentType = "text/html";
						response.Write("//" + engine.LastErrorMessage);
						response.End();
					}
				}
				catch (System.Threading.ThreadAbortException)
				{
				}
				catch (Exception)
				{
				}
				return "";
			}
			else
			{
				// PreExecute the request
				engine.PreExecution();

				MimeContentType responseContentType;
				if (!Common.IsFTPUrl(engine.RequestInfo.RequestUrl))
				{
					// Check the response state
					if (engine.LastStatus == LastActivityStatus.Error)
					{
						string errorMessage = engine.LastErrorMessage;
						if (string.IsNullOrEmpty(errorMessage))
							errorMessage = "Unknown error on requesting data";
						throw new Exception(errorMessage);
					}
					responseContentType = Common.StringToContentType(engine.ResponseInfo.ContentType);
				}
				else
				{
					// this is FTP request so it should process in download page
					responseContentType = MimeContentType.application;
				}

				// If the request is not html or text do apply it for other pages
				switch (responseContentType)
				{
					case MimeContentType.application:
						response.Redirect(GetDownloadPageRedirectUrl(engine), true);
						return "";
					case MimeContentType.image_gif:
					case MimeContentType.image_jpeg:
						return GetImagePageUrl(engine);
				}

				// Set the response url value
				fResponseUrl = engine.ResponseInfo.ResponseUrl;

				// Declare result variable
				string result = "";

				// Execute and get the request
				engine.Execute(out result);

				// Check the response state
				if (engine.LastStatus == LastActivityStatus.Error)
				{
					string errorMessage = engine.LastErrorMessage;
					if (string.IsNullOrEmpty(errorMessage))
						errorMessage = "Unknown error on requesting data";
					throw new Exception(errorMessage);
				}

				// If the page include any FrameSet Tag apply it for other pages
				if (engine.ResponseInfo.IsFrameSet)
				{
					return GetFramesetTagForRequest(engine);
				}

				// If content is plain text format
				if (responseContentType == MimeContentType.text_javascript || responseContentType == MimeContentType.text_css || responseContentType == MimeContentType.text_plain)
					result = "<pre>" + result + "</pre>";
				//else
				//	result += responseContent;

				// Content encoding
				response.ContentEncoding = engine.ResponseInfo.ContentEncoding;
				response.Charset = engine.ResponseInfo.ContentCharset;

				if (engine.Options.DisplayPageTitle)
				{
					fResponsePageTitle = GlobalConsts.ASProxyName + ":: " + engine.ResponseInfo.ContentPageTitle;
				}
				else fResponsePageTitle = "";

				return result;
			}
		}

		/// <summary>
		/// Generate an IFrame tag that includes current request frameset page for displaying
		/// </summary>
		private string GetFramesetTagForRequest(ASProxyEngine engine)
		{
			if (engine.ResponseInfo.IsFrameSet)
			{
				string result = "";
				if (WebMethods.IsMethod(engine.RequestInfo.RequestMethod, WebMethods.DefaultMethods.GET))
					result = UrlProvider.AddArgumantsToUrl(FilesConsts.DirectHtmlPage, engine.RequestInfo.RequestUrl, true);
				else
					result = FilesConsts.DirectHtmlPage + "?" + engine.RequestInfo.RequestedQueries;
				return HtmlTags.IFrameTag(result, "100%", "600%");
			}
			return "";
		}

		/// <summary>
		/// Gets the image page url for this request image
		/// </summary>
		private string GetImagePageUrl(ASProxyEngine engine)
		{
			return HtmlTags.ImgTag(UrlProvider.AddArgumantsToUrl(FilesConsts.ImagesPage, engine.RequestInfo.RequestUrl, true));
		}

		/// <summary>
		/// Gets the download page url for this request file
		/// </summary>
		private string GetDownloadPageRedirectUrl(ASProxyEngine engine)
		{
			if (WebMethods.IsMethod(engine.RequestInfo.RequestMethod, WebMethods.DefaultMethods.GET))
				return UrlProvider.AddArgumantsToUrl(FilesConsts.DownloadPage, engine.RequestInfo.RequestUrl, true);
			else
				return FilesConsts.DownloadPage + "?" + engine.RequestInfo.RequestedQueries;
		}

	}
}
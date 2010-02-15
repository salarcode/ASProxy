using System;
using System.Web;
using SalarSoft.ASProxy.Exposed;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.IO.Compression;

namespace SalarSoft.ASProxy.BuiltIn
{
	/// <summary>
	/// This is ASProxy engine. Every user request should be handled by this class.
	/// </summary>
	public class ASProxyEngine : ExEngine
	{
		#region local variables
		private bool _isPluginAvailable;
		#endregion

		#region public methods
		public ASProxyEngine()
		{
			RequestInfo = new EngineRequestInfo();
			ResponseInfo = new EngineResponseInfo();

			// getting plugin availablity state
			_isPluginAvailable = Plugins.IsPluginAvailable(PluginHosts.IPluginEngine);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Initialize(HttpRequest httpRequest)
		{
			IntializeRequestInfo(httpRequest);
			IntializeRequestInfoPostData(httpRequest);

			// 0- executing plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
					PluginMethods.IPluginEngine.AfterInitialize,
					this);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Initialize(string requestUrl)
		{
			RequestInfo.RequestUrl = requestUrl;
			RequestInfo.RequestMethod = WebMethods.GET;
			IntializeRequestInfoPostData(null);

			// 0- executing plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
					PluginMethods.IPluginEngine.AfterInitialize,
					this);
		}

		/// <summary>
		/// Communicates with back-end and gets response information
		/// </summary>
		public override void ExecuteHandshake()
		{
			// Checks if handshake is alreadu done
			// Hashshakhe can only run one time
			if (_webData != null)
				return;

			// 1- executing the plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
					PluginMethods.IPluginEngine.BeforeHandshake,
					this, _webData);

			// If this is an image request, we should provide orginal link as referer
			if (RequestInfo.ContentTypeMime == MimeContentType.image_gif ||
				RequestInfo.ContentTypeMime == MimeContentType.image_jpeg)
			{
				// We ignore what setting was. It should send referrer for images.
				RequestInfo.RequestUrlAsReferrer = true;
			}

			do
			{

				// If this is auto recection request send request again to new location
				if (_webData != null && _webData.ResponseInfo.AutoRedirect)
				{
					// Handles the redirect request
					HandleResponseAutoRedirect();

					// Exit the method
					return;
				}
				else
				{
					// Generated user agent
					// Since v5.1
					string userAgent = Common.GenerateUserAgent(HttpContext.Current);

					if (_webData != null)
						_webData.Dispose();

					// initializing new DataCore
					_webData = (IWebData)Providers.GetProvider(ProviderType.IWebData);
					_webData.RequestInfo.RequestUrl = RequestInfo.RequestUrl;
					_webData.RequestInfo.UserAgent = userAgent;
				}

				// WebData request info
				ApplyWebDataRequestInfo(_webData);

				// Execute the request
				_webData.Execute();

				// If execution failed
				if (_webData.LastStatus == LastStatus.Error)
				{
					LastStatus = LastStatus.Error;
					LastErrorMessage = _webData.LastErrorMessage;
					LastException = _webData.LastException;
					return;
				}
				else if (_webData.LastStatus == LastStatus.ContinueWithError)
				{
					LastStatus = LastStatus.ContinueWithError;
					LastErrorMessage = _webData.LastErrorMessage;
					LastException = _webData.LastException;
				}

				// Read response info
				FinilizeResponseInfo(_webData);

			} while (_webData.ResponseInfo.AutoRedirect);

			// 2- executing the plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
					PluginMethods.IPluginEngine.AfterHandshake,
					this, _webData);

		}

		/// <summary>
		/// 
		/// </summary>
		public override string ExecuteToString()
		{
			// hanshake should be ran before and there should not be error
			if (_webData == null || LastStatus == LastStatus.Error)
				return String.Empty;

			// process type
			DataTypeToProcess processType = DataTypeToProcess;

			// gets mime type of content type
			MimeContentType contentMimeType = _webData.ResponseInfo.ContentTypeMime;

			// detecting processing method by response content type
			if (processType == DataTypeToProcess.AutoDetect)
			{
				// gets its process type
				processType = Common.MimeTypeToToProcessType(contentMimeType);
			}

			if (processType == DataTypeToProcess.Html && Systems.LogSystem.ActivityLogEnabled)
				Systems.LogSystem.Log(LogEntity.UrlRequested, ResponseInfo.ResponseUrl);

			IDataProcessor dataProcessor = null;

			switch (processType)
			{
				case DataTypeToProcess.AutoDetect:
					break;
				case DataTypeToProcess.Html:
					dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.IHtmlProcessor);
					break;
				case DataTypeToProcess.JavaScript:
					dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.IJSProcessor);
					break;
				case DataTypeToProcess.Css:
					dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.ICssProcessor);
					break;
				case DataTypeToProcess.AdobeFlash:
				// still nothing
				default:
					break;
			}

			if (dataProcessor != null)
			{
				// 3- executing the plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
						PluginMethods.IPluginEngine.BeforeProcessor,
						this, dataProcessor);

				// Web data instance
				dataProcessor.WebData = _webData;

				// executes the process
				string response = dataProcessor.Execute();

				// If execution occurred
				if (dataProcessor.LastStatus == LastStatus.Error)
				{
					LastStatus = LastStatus.Error;
					LastErrorMessage = dataProcessor.LastErrorMessage;
					LastException = dataProcessor.LastException;
					return response;
				}
				else if (dataProcessor.LastStatus == LastStatus.ContinueWithError)
				{
					LastStatus = LastStatus.ContinueWithError;
					LastErrorMessage = dataProcessor.LastErrorMessage;
					LastException = dataProcessor.LastException;
				}

				// processed content encoding
				ResponseInfo.ContentEncoding = dataProcessor.ContentEncoding;
				ResponseInfo.ContentLength = response.Length;



				// Html specifies
				if (processType == DataTypeToProcess.Html && dataProcessor is IHtmlProcessor)
				{
					IHtmlProcessor htmlProcessor = (IHtmlProcessor)dataProcessor;

					ResponseInfo.HtmlPageTitle = htmlProcessor.PageTitle;
					ResponseInfo.HtmlIsFrameSet = htmlProcessor.IsFrameSet;
					ResponseInfo.ExtraCodesForPage = htmlProcessor.ExtraCodesForPage;
					ResponseInfo.ExtraCodesForBody = htmlProcessor.ExtraCodesForBody;
					ResponseInfo.HtmlDocType = htmlProcessor.DocType;
				}

				// 4- executing the plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
						PluginMethods.IPluginEngine.AfterProcessor,
						this);

				// the processed content
				return response;
			}
			else
			{
				Encoding contentEncoding;

				// Reads response stream to a string
				string response = StringStream.GetString(
					_webData.ResponseData,
					WebData.ResponseInfo.ContentType,
					UserOptions.ForceEncoding,
					false,
					out contentEncoding);

				ResponseInfo.ContentEncoding = contentEncoding;
				ResponseInfo.ContentLength = response.Length;

				return response;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void ExecuteToResponse(HttpResponse httpResponse)
		{
			// handshake should be ran before and there should not be error
			if (_webData == null || LastStatus == LastStatus.Error)
				return;

			// process type
			DataTypeToProcess processType = DataTypeToProcess;

			// gets mime type of content type
			MimeContentType contentMimeType = _webData.ResponseInfo.ContentTypeMime;

			// detecting processing method by response content type
			if (processType == DataTypeToProcess.AutoDetect)
			{
				// gets its process type
				processType = Common.MimeTypeToToProcessType(contentMimeType);
			}

			// BUGFIX: v5.5b2, HttpCompression increases images size
			VerifyHttpCompressionByMimeType(httpResponse, contentMimeType);

			IDataProcessor dataProcessor = null;

			switch (processType)
			{
				case DataTypeToProcess.AutoDetect:
					break;
				case DataTypeToProcess.Html:
					dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.IHtmlProcessor);
					break;
				case DataTypeToProcess.JavaScript:
					dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.IJSProcessor);
					break;
				case DataTypeToProcess.Css:
					dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.ICssProcessor);
					break;
				case DataTypeToProcess.AdobeFlash:
				// still nothing
				default:
					break;
			}

			if (dataProcessor != null)
			{
				// 3- executing the plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
						PluginMethods.IPluginEngine.BeforeProcessor,
						this, dataProcessor);

				// Web data instance
				dataProcessor.WebData = _webData;

				// executes the process
				string response = dataProcessor.Execute();

				// If execution occurred
				if (dataProcessor.LastStatus == LastStatus.Error)
				{
					LastStatus = LastStatus.Error;
					LastErrorMessage = dataProcessor.LastErrorMessage;
					LastException = dataProcessor.LastException;
					return;
				}
				else if (dataProcessor.LastStatus == LastStatus.ContinueWithError)
				{
					LastStatus = LastStatus.ContinueWithError;
					LastErrorMessage = dataProcessor.LastErrorMessage;
					LastException = dataProcessor.LastException;
				}

				// processed content encoding
				ResponseInfo.ContentEncoding = dataProcessor.ContentEncoding;
				ResponseInfo.ContentLength = response.Length;



				// Html specifies
				if (processType == DataTypeToProcess.Html && dataProcessor is IHtmlProcessor)
				{
					IHtmlProcessor htmlProcessor = (IHtmlProcessor)dataProcessor;

					ResponseInfo.HtmlPageTitle = htmlProcessor.PageTitle;
					ResponseInfo.HtmlIsFrameSet = htmlProcessor.IsFrameSet;
					ResponseInfo.ExtraCodesForPage = htmlProcessor.ExtraCodesForPage;
					ResponseInfo.ExtraCodesForBody = htmlProcessor.ExtraCodesForBody;
					ResponseInfo.HtmlDocType = htmlProcessor.DocType;
				}

				// apply response info to response
				ExecuteToResponse_ApplyResponseInfo(httpResponse);

				// 4- executing the plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
						PluginMethods.IPluginEngine.AfterProcessor,
						this);

				// the processed content
				httpResponse.Write(response);

				// write the extra content if available
				// the extra content should be written after the content
				// because they should be executed carefully
				if (!string.IsNullOrEmpty(ResponseInfo.ExtraCodesForBody))
					httpResponse.Write(ResponseInfo.ExtraCodesForBody);
			}
			else
			{
				// used to resolvent mime processing conflict
				bool ContinueNonMime = true;

				// if response is a image
				if ((UserOptions.ImageCompressor) &&
					(contentMimeType == MimeContentType.image_gif || contentMimeType == MimeContentType.image_jpeg))
				{
					// apply response info to response
					ExecuteToResponse_ApplyResponseInfo(httpResponse);

					using (MemoryStream imgMem = ImageCompressor.CompressImage(
						_webData.ResponseData))
					{
						// check if compression is decreased size of data
						if (imgMem.Length < _webData.ResponseData.Length)
						{
							ContinueNonMime = false;

							// write image to response
							imgMem.WriteTo(httpResponse.OutputStream);
						}
						else
						{
							// Oops! the original image is smaller
							ContinueNonMime = true;
						}
					}
				}

				// can process other types?
				if (ContinueNonMime)
				{
					if (processType == DataTypeToProcess.None
							&& _webData.ResponseData is MemoryStream)
					{

						// apply response info to response
						ExecuteToResponse_ApplyResponseInfo(httpResponse);

						MemoryStream mem = (MemoryStream)_webData.ResponseData;
						if (mem.Length > 0)
							mem.WriteTo(httpResponse.OutputStream);

					}
					else if (processType == DataTypeToProcess.None)
					{
						int readed = -1;
						const int blockSize = 1024 * 5;

						// apply response info to response
						ExecuteToResponse_ApplyResponseInfo(httpResponse);

						byte[] buffer = new byte[blockSize];
						while ((int)(readed = _webData.ResponseData.Read(buffer, 0, blockSize)) > 0)
						{
							httpResponse.OutputStream.Write(buffer, 0, readed);
						}
					}
					else
					{
						Encoding contentEncoding;
						// Reads response stream to a string
						string response = StringStream.GetString(
							_webData.ResponseData,
							WebData.ResponseInfo.ContentType,
							UserOptions.ForceEncoding,
							false,
							out contentEncoding);

						ResponseInfo.ContentEncoding = contentEncoding;
						ResponseInfo.ContentLength = response.Length;

						// apply response info to response
						ExecuteToResponse_ApplyResponseInfo(httpResponse);

						// the content
						httpResponse.Write(response);
					}

					// closes the respose
					// httpResponse.OutputStream.Close();
				}
			}

			// 5- executing the plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
					PluginMethods.IPluginEngine.AfterExecuteToResponse,
					this, httpResponse);

		}

		/// <summary>
		/// Disables http compression for non-text contents
		/// </summary>
		/// <reason>BUGFIX v5.5b2:  HttpCompression increases images size</reason>
		private void VerifyHttpCompressionByMimeType(HttpResponse httpResponse, MimeContentType responseMimeType)
		{
			switch (responseMimeType)
			{
				case MimeContentType.text_html:
				case MimeContentType.text_plain:
				case MimeContentType.text_css:
				case MimeContentType.text_javascript:
					// nothing
					// the compression remains if it is there
					break;
				case MimeContentType.image_jpeg:
				case MimeContentType.image_gif:
				case MimeContentType.application:
				default:
					// Disabling HttpCompression for this request
					//httpResponse.Headers.Remove("Content-Encoding");
					httpResponse.ClearHeaders();

					if (httpResponse.Filter is GZipStream)
					{
						using (GZipStream compress = (GZipStream)httpResponse.Filter)
						{
							// reassign the original filter
							httpResponse.Filter = compress.BaseStream;
						}
					}
					else if (httpResponse.Filter is DeflateStream)
					{
						using (DeflateStream compress = (DeflateStream)httpResponse.Filter)
						{
							// reassign the original filter
							httpResponse.Filter = compress.BaseStream;
						}
					}
					break;
			}

		}

		/// <summary>
		/// 
		/// </summary>
		public override void ExecuteToStream(Stream stream)
		{
			// handshake should be ran before and there should not be error
			if (_webData == null || LastStatus == LastStatus.Error)
				return;

			// process type
			DataTypeToProcess processType = DataTypeToProcess;

			// gets mime type of content type
			MimeContentType contentMimeType = _webData.ResponseInfo.ContentTypeMime;

			// detecting processing method by response content type
			if (processType == DataTypeToProcess.AutoDetect)
			{
				// gets its process type
				processType = Common.MimeTypeToToProcessType(contentMimeType);
			}

			IDataProcessor dataProcessor = null;

			switch (processType)
			{
				case DataTypeToProcess.AutoDetect:
					break;
				case DataTypeToProcess.Html:
					dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.IHtmlProcessor);
					break;
				case DataTypeToProcess.JavaScript:
					dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.IJSProcessor);
					break;
				case DataTypeToProcess.Css:
					dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.ICssProcessor);
					break;
				case DataTypeToProcess.AdobeFlash:
				// still nothing
				case DataTypeToProcess.None:
					break;
				default:
					break;
			}

			if (dataProcessor != null)
			{
				// 3- executing the plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
						PluginMethods.IPluginEngine.BeforeProcessor,
						this, dataProcessor);

				// Web data instance
				dataProcessor.WebData = _webData;

				// executes the process
				string response = dataProcessor.Execute();

				// If execution occurred
				if (dataProcessor.LastStatus == LastStatus.Error)
				{
					LastStatus = LastStatus.Error;
					LastErrorMessage = dataProcessor.LastErrorMessage;
					LastException = dataProcessor.LastException;
					return;
				}
				else if (dataProcessor.LastStatus == LastStatus.ContinueWithError)
				{
					LastStatus = LastStatus.ContinueWithError;
					LastErrorMessage = dataProcessor.LastErrorMessage;
					LastException = dataProcessor.LastException;
				}

				// processed content encoding
				ResponseInfo.ContentEncoding = dataProcessor.ContentEncoding;
				ResponseInfo.ContentLength = response.Length;



				// Html specifies
				if (processType == DataTypeToProcess.Html && dataProcessor is IHtmlProcessor)
				{
					IHtmlProcessor htmlProcessor = (IHtmlProcessor)dataProcessor;

					ResponseInfo.HtmlPageTitle = htmlProcessor.PageTitle;
					ResponseInfo.HtmlIsFrameSet = htmlProcessor.IsFrameSet;
					ResponseInfo.ExtraCodesForPage = htmlProcessor.ExtraCodesForPage;
					ResponseInfo.ExtraCodesForBody = htmlProcessor.ExtraCodesForBody;
					ResponseInfo.HtmlDocType = htmlProcessor.DocType;
				}

				// 4- executing the plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginEngine,
						PluginMethods.IPluginEngine.AfterProcessor,
						this);

				// the processed content
				byte[] streamBuff = ResponseInfo.ContentEncoding.GetBytes(response);
				stream.Write(streamBuff, 0, streamBuff.Length);
			}
			else
			{
				// used to resolvent mime processing conflict
				bool ContinueNonMime = true;

				// if response is a image
				if ((UserOptions.ImageCompressor) &&
					(contentMimeType == MimeContentType.image_gif || contentMimeType == MimeContentType.image_jpeg))
				{
					using (MemoryStream imgMem = ImageCompressor.CompressImage(
						_webData.ResponseData))
					{
						// check if compression is decreased size of data
						if (imgMem.Length < _webData.ResponseData.Length)
						{
							ContinueNonMime = false;

							// write the image to result
							imgMem.WriteTo(stream);
						}
						else
						{
							// Oops! the original image is smaller
							ContinueNonMime = true;
						}
					}
				}

				// can process other types?
				if (ContinueNonMime)
				{
					if (processType == DataTypeToProcess.None
						&& _webData.ResponseData is MemoryStream)
					{
						MemoryStream mem = (MemoryStream)_webData.ResponseData;
						if (mem.Length > 0)
							mem.WriteTo(stream);
					}
					else if (processType == DataTypeToProcess.None)
					{
						int readed = -1;
						const int blockSize = 1024 * 5;

						byte[] buffer = new byte[blockSize];
						while ((int)(readed = _webData.ResponseData.Read(buffer, 0, blockSize)) > 0)
						{
							stream.Write(buffer, 0, readed);
						}
					}
					else
					{
						Encoding contentEncoding;
						// Reads response stream to a string
						string response = StringStream.GetString(
							_webData.ResponseData,
							WebData.ResponseInfo.ContentType,
							UserOptions.ForceEncoding,
							false,
							out contentEncoding);

						ResponseInfo.ContentEncoding = contentEncoding;
						ResponseInfo.ContentLength = response.Length;

						byte[] streamBuff = ResponseInfo.ContentEncoding.GetBytes(response);
						stream.Write(streamBuff, 0, streamBuff.Length);
					}
				}
			}
		}
		#endregion

		#region private methods
		protected virtual void ApplyWebDataRequestInfo(IWebData webData)
		{
			// Setting the referrer
			if (string.IsNullOrEmpty(RequestInfo.RedirectedFrom) == false)
			{
				webData.RequestInfo.Referrer = RequestInfo.RedirectedFrom;
				webData.RequestInfo.ReferrerUsage = ReferrerType.Referrer;
			}
			else
			{
				if (RequestInfo.RequestUrlAsReferrer)
					webData.RequestInfo.ReferrerUsage = ReferrerType.RequesterAsReferrer;
				else
					webData.RequestInfo.ReferrerUsage = ReferrerType.None;
			}

			webData.RequestInfo.AcceptCookies = UserOptions.Cookies;
			webData.RequestInfo.TempCookies = UserOptions.TempCookies;
			webData.RequestInfo.IfModifiedSince = RequestInfo.IfModifiedSince;
			webData.RequestInfo.CustomHeaders = RequestInfo.CustomHeaders;
			webData.RequestInfo.RequestMethod = RequestInfo.RequestMethod;
			webData.RequestInfo.PostDataString = RequestInfo.PostDataString;
			webData.RequestInfo.ContentType = RequestInfo.ContentTypeString;
			webData.RequestInfo.InputStream = RequestInfo.InputStream;
			webData.RequestInfo.RequesterType = RequestInfo.RequesterType;
			webData.RequestInfo.PrrocessErrorPage = RequestInfo.PrrocessErrorPage;
			webData.RequestInfo.BufferResponse = RequestInfo.BufferResponse;

			webData.RequestInfo.RangeBegin = RequestInfo.RangeBegin;
			webData.RequestInfo.RangeEnd = RequestInfo.RangeEnd;
			webData.RequestInfo.RangeRequest = RequestInfo.RangeRequest;
		}

		protected virtual void ExecuteToResponse_ApplyResponseInfo(HttpResponse httpResponse)
		{
			// Clear the past content
			httpResponse.ClearContent();

			// set content type
			httpResponse.ContentType = ResponseInfo.ContentType;

			if (ApplyContentFileName)
				ApplyContentFileNameToResponse(httpResponse);

			// Set response additional headers
			ApplyHeadersToResponse(httpResponse);

			// Add content type header
			httpResponse.AddHeader("Content-Type", ResponseInfo.ContentType);

			// ASProxy signature in request header
			if (Configurations.WebData.SendSignature)
			{
				httpResponse.AddHeader("X-Powered-By", Consts.BackEndConenction.ASProxyAgentVersion);
				httpResponse.AddHeader("X-Working-With", Consts.BackEndConenction.ASProxyAgentVersion);
			}

			// Set response content type 
			if (ResponseInfo.ContentEncoding != null)
			{
				httpResponse.ContentEncoding = ResponseInfo.ContentEncoding;
				// BUG: This causes "illegal character" in browsers
				//httpResponse.Charset = ResponseInfo.ContentEncoding.BodyName;
			}

			// Write document text
			if (!string.IsNullOrEmpty(ResponseInfo.HtmlDocType))
				httpResponse.Write(ResponseInfo.HtmlDocType);

			// Write document initialize code
			if (!string.IsNullOrEmpty(ResponseInfo.ExtraCodesForPage))
				httpResponse.Write(ResponseInfo.ExtraCodesForPage);
		}

		protected virtual void ApplyContentFileNameToResponse(HttpResponse response)
		{
			if (!string.IsNullOrEmpty(ResponseInfo.ContentFilename))
			{
				const string HTTP_HEADER_Content_Disposition = "Content-Disposition";
				string Content_Disposition_File = "attachment; filename=" + VirtualPathUtility.GetFileName(ResponseInfo.ContentFilename);

				response.AddHeader(HTTP_HEADER_Content_Disposition, Content_Disposition_File);
			}
		}


		protected virtual void ApplyHeadersToResponse(HttpResponse response)
		{

			if (_webData != null && _webData.ResponseInfo.Headers != null)
			{
				WebHeaderCollection coll = _webData.ResponseInfo.Headers;
				for (int i = 0; i < coll.Count; i++)
				{
					try
					{
						response.AppendHeader(coll.GetKey(i), coll[i]);
					}
					catch { }
				}
			}
		}


		/// <summary>
		/// Generates redirect location by specified parameters
		/// </summary>
		protected virtual string GetRedirectedRequestUrl(string asproxyPage, string destination)
		{
			return GetRedirectedRequestUrl(
				asproxyPage,
				destination,
				null,
				null,
				UserOptions.EncodeUrl);
		}

		/// <summary>
		/// Generates redirect location by specified EngineRequestInfo
		/// </summary>
		protected virtual string GetRedirectedRequestUrl(EngineRequestInfo RequestInfo, EngineResponseInfo responseInfo, string destination)
		{
			// As a temporary solution
			// Needs a mechanism to detect current used page
			string currentPage = HttpContext.Current.Request.Url.ToString();

			return GetRedirectedRequestUrl(
				currentPage,
				destination,
				responseInfo.ResponseUrl,
				RequestInfo.RequestMethod,
				UserOptions.EncodeUrl);
		}

		/// <summary>
		/// Generates redirect location by specified parameters
		/// </summary>
		protected virtual string GetRedirectedRequestUrl(string asproxyPage, string destination, string referrer, string webMethod, bool encodeUrl)
		{
			// Encode redirect page if needed
			if (encodeUrl)
			{
				destination = UrlProvider.EncodeUrl(destination);

				if (!string.IsNullOrEmpty(referrer))
					referrer = UrlProvider.EncodeUrl(referrer);
			}
			else
			{
				// just make it url safe
				destination = UrlProvider.EscapeUrlQuery(destination);
				
				if (!string.IsNullOrEmpty(referrer))
					referrer = UrlProvider.EscapeUrlQuery(referrer);
			}

			// If address exists in current page address it will automatically replaced
			asproxyPage = UrlBuilder.AddUrlQuery(asproxyPage, Consts.Query.UrlAddress, destination);

			// Apply decode option
			asproxyPage = UrlBuilder.AddUrlQuery(asproxyPage,
							  Consts.Query.Decode,
							  Convert.ToByte(encodeUrl).ToString());

			// Apply current page as referrer url for redirect url
			if (!string.IsNullOrEmpty(referrer))
				asproxyPage = UrlBuilder.AddUrlQueryToEnd(asproxyPage, Consts.Query.Redirect, referrer);

			// If page is marked as posted back, remove the remark
			if (string.IsNullOrEmpty(webMethod))
			{
				// The default web method will be used
				asproxyPage = UrlBuilder.RemoveQuery(asproxyPage, Consts.Query.WebMethod);
			}
			else
				// changing the web method
				asproxyPage = UrlBuilder.ReplaceUrlQuery(asproxyPage, Consts.Query.WebMethod, webMethod);

			return asproxyPage;
		}

		/// <summary>
		/// Handles redirection to requested location
		/// </summary>
		protected virtual void HandleResponseAutoRedirect()
		{
			if (_webData.ResponseInfo.AutoRedirectionType == AutoRedirectType.RequestInternal ||
						_webData.ResponseInfo.AutoRedirectionType == AutoRedirectType.External)
			{
				// Requested redirect location
				string redir = _webData.ResponseInfo.AutoRedirectLocation;

				// Correcting request web method
				// Checking response redirect status
				if (_webData.ResponseInfo.HttpStatusCode == (int)HttpStatusCode.SeeOther)
				{
					// SeeOther always use GET method
					RequestInfo.RequestMethod = WebMethods.GET;
				}

				// Get redirected location
				redir = GetRedirectedRequestUrl(RequestInfo, ResponseInfo, redir);

				// Redirect to requested location
				Redirect(redir, HttpStatusCode.Moved, true);
			}
			else if (_webData.ResponseInfo.AutoRedirectionType == AutoRedirectType.ASProxyPages)
			{
				// Get redirected location
				string redir = GetRedirectedRequestUrl(
						_webData.ResponseInfo.AutoRedirectLocation,
						RequestInfo.RequestUrl);

				// Redirect to requested location
				Redirect(redir, HttpStatusCode.Moved, true);
			}
			else if (_webData.ResponseInfo.AutoRedirectionType == AutoRedirectType.External)
			{
				// External redirections not implemented yet
				// External is applied in top level
				// This may causes security issues (e.g. cross site cookie access, etc.)
			}

		}

		/// <summary>
		/// Initializes the request info post data.
		/// </summary>
		protected virtual void IntializeRequestInfoPostData(HttpRequest httpRequest)
		{
			if (httpRequest != null)
			{

				// TODO: test this case
				// When a post back event occurred "httpRequest.ContentType" contains ContentType of request
				// In other cases the "httpRequest.ContentType" is empty and we shouldn't use this property
				if (!string.IsNullOrEmpty(httpRequest.ContentType))
					RequestInfo.SetContentType(httpRequest.ContentType);

				RequestInfo.InputStream = httpRequest.InputStream;


				// Get posted form data string
				string httpRequestMethod = httpRequest.HttpMethod;

				if (WebMethods.IsMethod(httpRequestMethod, WebMethods.DefaultMethods.POST))
				{
					RequestInfo.PostDataString = httpRequest.Form.ToString();

					// Some web sites encodes the url, and we need to decode it.
					//RequestInfo.PostDataString = HttpUtility.HtmlDecode(RequestInfo.PostDataString);

					// Since V5.5
					// The text should be decoded before any use
					RequestInfo.PostDataString = UnicodeUrlDecoder.UrlDecode(RequestInfo.PostDataString);

					// BUG: not working with (#) and (&) characters
					//RequestInfo.PostDataString = HttpUtility.UrlDecode(RequestInfo.PostDataString);
				}
				else
					RequestInfo.PostDataString = "";
			}
			else
				RequestInfo.PostDataString = "";


			// If requested method is GET
			if (WebMethods.IsMethod(RequestInfo.RequestMethod, WebMethods.DefaultMethods.GET))
			{
				// Apply filter for ASP.NET pages

				// Change requested url by posted data
				RequestInfo.RequestUrl = UrlBuilder.AppendAntoherQueries(RequestInfo.RequestUrl, RequestInfo.PostDataString);
			}
		}

		/// <summary>
		/// Initializes the request info. This method should be called when 
		/// Initializing the class directly from the request.
		/// </summary>
		protected virtual void IntializeRequestInfo(HttpRequest httpRequest)
		{
			// If Modified Since
			// BUG: v5beta4 custom headers doesn't support at this moment
			// RequestInfo.CustomHeaders = GetHeadersFromRequest(httpRequest);
			RequestInfo.IfModifiedSince = GetIfModifiedSinceHeader(httpRequest);


			bool tmpBool = false;

			// Get requested url
			string url = httpRequest.QueryString[Consts.Query.UrlAddress];

			// if url is provided
			if (!string.IsNullOrEmpty(url))
			{
				string decode = httpRequest.QueryString[Consts.Query.Decode];
				if (!string.IsNullOrEmpty(decode))
				{
					try
					{
						tmpBool = Convert.ToBoolean(Convert.ToInt32(decode));
					}
					catch
					{
						tmpBool = false;
					}
				}

				// If url is encoded, decode it
				if (tmpBool)
					url = UrlProvider.DecodeUrl(url);

				RequestInfo.RequestUrl = url;
			}

			url = httpRequest.QueryString[Consts.Query.Redirect];

			if (!string.IsNullOrEmpty(url))
			{
				// If url is encoded, decode it
				if (tmpBool)
					url = UrlProvider.DecodeUrl(url);

				RequestInfo.RedirectedFrom = url;
			}

			// Get request post method state
			string reqMethod;
			if (UrlProvider.GetRequestQuery(httpRequest.QueryString, Consts.Query.WebMethod, out reqMethod))
			{
				//RequestInfo.RequestMethod = WebMethods.ValidateMethod(reqMethod, WebMethods.DefaultMethods.GET);
				RequestInfo.RequestMethod = WebMethods.OmitInvalidCharacters(reqMethod);
			}
			else
				RequestInfo.RequestMethod = WebMethods.GET;

		}

		/// <summary>
		/// Applies WebData response information to responseInfo
		/// </summary>
		/// <param name="webData"></param>
		protected virtual void FinilizeResponseInfo(IWebData webData)
		{
			ResponseInfo.ResponseUrl = webData.ResponseInfo.ResponseUrl;
			ResponseInfo.ContentFilename = webData.ResponseInfo.ContentFilename;
			ResponseInfo.ContentLength = webData.ResponseInfo.ContentLength;
			ResponseInfo.ContentType = webData.ResponseInfo.ContentType;
		}

		protected virtual DateTime GetIfModifiedSinceHeader(HttpRequest httpRequest)
		{
			string header = httpRequest.Headers["If-Modified-Since"];
			if (string.IsNullOrEmpty(header))
				return new DateTime();
			try
			{
				return Convert.ToDateTime(header);
			}
			catch (Exception)
			{
				return new DateTime();
			}
		}

		#endregion

	}
}
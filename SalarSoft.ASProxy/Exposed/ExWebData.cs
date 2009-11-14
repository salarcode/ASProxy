using System;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace SalarSoft.ASProxy.Exposed
{
	/// <summary>
	/// Summary description for ExWebData 
	/// </summary>
	public abstract class ExWebData : IWebData
	{
		#region variables
		private WebDataRequestInfo _requestInfo;
		private WebDataResponseInfo _responseInfo;
		private Stream _responseData;
		private LastStatus _lastStatus;
		private Exception _lastException;
		private string _lastErrorMessage;
		#endregion

		#region properties
		/// <summary>
		/// Request information
		/// </summary>
		public WebDataRequestInfo RequestInfo
		{
			get
			{
				return _requestInfo;
			}
			set
			{
				_requestInfo = value;
			}
		}

		/// <summary>
		/// Response information
		/// </summary>
		public WebDataResponseInfo ResponseInfo
		{
			get
			{
				return _responseInfo;
			}
			set
			{
				_responseInfo = value;
			}
		}
		public LastStatus LastStatus
		{
			get { return _lastStatus; }
			set { _lastStatus = value; }
		}

		public Exception LastException
		{
			get { return _lastException; }
			set { _lastException = value; }
		}
		public string LastErrorMessage
		{
			get { return _lastErrorMessage; }
			set { _lastErrorMessage = value; }
		}

		public Stream ResponseData
		{
			get { return _responseData; }
			set { _responseData = value; }
		}
		#endregion

		#region public methods
		/// <summary>
		/// Confirms the request and gets the data from the web.
		/// </summary>
		public abstract void Execute();

		public virtual void Dispose()
		{
		}
		#endregion

		#region static methods
		#endregion

		#region protected methods
		/// <summary>
		/// Reads response content to a stream
		/// </summary>
		/// <param name="webResponse"></param>
		protected Stream ReadResponseData(WebResponse webResponse)
		{
			Stream responseData = null;
			// If it is going to redirect, the content is ignored
			if (ResponseInfo.AutoRedirect == false)
			{
				// when buffering the response
				if (RequestInfo.BufferResponse)
				{
					responseData = new MemoryStream();

					// Declare variables
					int maxBlockReadFromWeb = 1024;
					int readed = -1;
					byte[] buffer = new byte[maxBlockReadFromWeb];

					// Get the response stream for reading
					using (Stream readStream = webResponse.GetResponseStream())
					{
						// Read the stream and write it into memory
						while ((int)(readed = readStream.Read(buffer, 0, maxBlockReadFromWeb)) > 0)
						{
							responseData.Write(buffer, 0, readed);
						}
					}

					// Sometimes the capacity is larger than length!!
					// So I correct it here
					if ((responseData is MemoryStream))
						((MemoryStream)responseData).Capacity = (int)(responseData.Length);
				}
				else
				{
					// destroty previous instance
					if (responseData != null)
						responseData.Dispose();

					// just get the data stream
					responseData = webResponse.GetResponseStream();

				}
			}
			return responseData;
		}

		/// <summary>
		/// Applies custom headers to the request
		/// </summary>
		protected virtual void ApplyCustomHeaders(WebRequest webRequest)
		{
			// This is default method to apply custom headers
			// Because some of the headers can't be set indirectly 
			// this method should be overrided to set them directly from their properties

			NameValueCollection headers = RequestInfo.CustomHeaders;
			if (headers != null && headers.Count > 0)
			{
				foreach (string key in headers)
				{
					try
					{
						webRequest.Headers.Add(key, headers[key].ToString());
					}
					catch (Exception) { }
				}
			}
		}

		/// <summary>
		/// Sends post data to back-end request
		/// </summary>
		protected virtual void ApplyPostDataToRequest(WebRequest webRequest)
		{
			// Only if this is post
			if (WebMethods.IsMethod(RequestInfo.RequestMethod, WebMethods.DefaultMethods.POST))
			{

				if (RequestInfo.InputStream != null)
				{
					Stream reqStream = webRequest.GetRequestStream();

					// Read stream then reset renamed ViewState name to default
					byte[] inputBytes = Common.AspDotNetViewStateResetToDef(RequestInfo.InputStream);

					// Write to destination stream
					reqStream.Write(inputBytes, 0, inputBytes.Length);
					reqStream.Close();

					RequestInfo.InputStream.Close();
				}
				else
				{
					string postData = Common.AspDotNetViewStateResetToDef(RequestInfo.PostDataString);

					byte[] bytes = Encoding.ASCII.GetBytes(postData);
					webRequest.ContentLength = bytes.LongLength;

					Stream reqStream = webRequest.GetRequestStream();
					reqStream.Write(bytes, 0, bytes.Length);
					reqStream.Close();
				}
			}
		}

		/// <summary>
		/// Applies data ranges to the request
		/// </summary>
		protected virtual void ApplyContentRanges(WebRequest webRequest)
		{
			if (!RequestInfo.RangeRequest)
				return;

			if (webRequest == null)
				return;

			if (webRequest is HttpWebRequest)
			{
				HttpWebRequest httpWebRequest = (webRequest as HttpWebRequest);

				//if (!string.IsNullOrEmpty(RequestInfo.RangeHeader))
				//{
				//    httpWebRequest.Headers[HttpRequestHeader.Range] = RequestInfo.RangeHeader;
				//}
				//else 
				if (RequestInfo.RangeEnd > RequestInfo.RangeBegin)
				{
					httpWebRequest.AddRange(RequestInfo.RangeBegin, (int)RequestInfo.RangeEnd);
				}
				else
				{
					httpWebRequest.AddRange(RequestInfo.RangeBegin);
				}
			}
			else if (webRequest is FtpWebRequest)
			{
				FtpWebRequest ftpWebRequest = (webRequest as FtpWebRequest);
				ftpWebRequest.ContentOffset = RequestInfo.RangeBegin;
			}

			// Note: FileWebRequest does not support range request
		}


		#endregion

		#region private methods
		#endregion

		#region Type defination

		#endregion

	}
}
using System;
using System.Diagnostics;
using System.Net;
using System.Web;

namespace SalarSoft.ASProxy.ProxyServer
{
    public enum LastProxyServerState { Listening, Error, Aborted, Stopped, Started, RequestProcessed }

	/// <summary>
	/// ASProxyServer implementation version 4.0
	/// </summary>
	/// <Update>2007-12-22 24:55</Update>
    public class ASProxyServer
    {

        #region Private variables
        private static HttpListener fListener = null;
        private static string fLastErrorMessage = "";
        private static LastProxyServerState fLastState = LastProxyServerState.Stopped;
        #endregion

        #region Properties
        public static string LastErrorMessage
        {
            get { return fLastErrorMessage; }
            set { fLastErrorMessage = value; }
        }
        public static LastProxyServerState LastState
        {
            get { return fLastState; }
            set { fLastState = value; }
        }
       
		//private static AuthenticationSchemes fAuthScheme = AuthenticationSchemes.Anonymous;
		//public static AuthenticationSchemes AuthScheme
		//{
		//    get { return fAuthScheme; }
		//    set { fAuthScheme = value; }
		//}
        #endregion

        #region Public static methods
		
		/// <summary>
		/// Start proxy server with one prefix
		/// </summary>
        public static void StartProxy(string prefix)
        {
            string[] p ={ prefix };
            StartProxy(p);
        }

		/// <summary>
		/// Start listening for several prefixes
		/// </summary>
        public static void StartProxy(string[] prefixes)
        {
            try
            {
                EnsureListener();

				// Add specified prefixes
                if (prefixes != null && prefixes.Length>0)
                {
                    fListener.Prefixes.Clear();
                    for (int i = 0; i < prefixes.Length; i++)
                        fListener.Prefixes.Add(prefixes[i]);
                }
                //fListener.AuthenticationSchemes = fAuthScheme;
                //fListener.UnsafeConnectionNtlmAuthentication = true;

				// Start listener service
                fListener.Start();
                fLastState = LastProxyServerState.Started;

				// Start wating
                WaitForContext();
            }
            catch (Exception ex)
            {
                fLastErrorMessage = ex.ToString();
                fLastState = LastProxyServerState.Error;
            }
        }

		/// <summary>
		/// Stops proxy listening 
		/// </summary>
        public static void StopProxy()
        {
            try
            {
                ReleaseProxyServer();
                fLastState = LastProxyServerState.Stopped;
            }
            catch (Exception ex)
            {
                fLastErrorMessage = ex.ToString();
                fLastState = LastProxyServerState.Error;
            }
        }

        #endregion

        #region Private methods
		/// <summary>
		/// Release the listener
		/// </summary>
		private static void ReleaseProxyServer()
        {
            try
            {
                if (fListener != null) fListener.Close();
            }
            catch (Exception) { }
            fLastErrorMessage = "";
            fLastState = LastProxyServerState.Aborted;
            fListener = null;
        }

        private static void ListenerCallback(IAsyncResult result)
        {
            HttpListenerResponse response = null;
            WebDataCore dataCore = null;
            byte[] sign = ASPRoxySignature();
			bool startWaitCalled = false;
			try
			{
				if (result == null)
					return;

				// Get the listener instance
				HttpListener lst = (HttpListener)result.AsyncState;

				// Get the request context
				HttpListenerContext context = lst.EndGetContext(result);

				// Start waiting again
				WaitForContext();
				startWaitCalled = true;

				// Get the request
				HttpListenerRequest request = context.Request;

				// Get the request
				response = context.Response;

				// Initilize ASProxy Data Core
				dataCore = InitializeWebDataCore(request);

				// Displays orginal error pages
				dataCore.DisplayErrorPageAsResult = true;

				// Set applied cookies
				dataCore.RequestInfo.Cookies = new CookieCollection();
				CookieManager.CopyCookiesCollection(request.Cookies, dataCore.RequestInfo.Cookies);

				if (dataCore.ResponseInfo.AutoRedirect)
				{
					string autoRedirectLocation = dataCore.ResponseInfo.AutoRedirectLocation;
					HttpContext.Current.Response.Redirect(autoRedirectLocation);
					return;
				}

				// Run the request
				dataCore.Execute();

				// Get returened cookies and send them to user request response
				CookieManager.CopyCookiesCollection(dataCore.ResponseInfo.Cookies, response.Cookies);

				if (dataCore.Status == LastActivityStatus.Normal)
				{

					// Check whether content is html or not 
					bool contentIsHtml = ContentIsHtml(dataCore.ResponseInfo.ContentType);

					// Apply web url response to user request response
					ApplyReponseInfoToListener(response, dataCore.ResponseInfo);

					// If content is html add ASProxy signature length
					if (contentIsHtml)
					{
						response.ContentLength64 = dataCore.ResponseInfo.ContentLength + (sign.Length);
					}

					// Write web url response content to user request response
					dataCore.ResponseData.WriteTo(response.OutputStream);

					// If content is html write ASProxy signature
					if (contentIsHtml)
					{
						response.OutputStream.Write(sign, 0, sign.Length);
					}

					//response.RedirectLocation = urlData.ResponseInfo.ResponseUrl;
					if (dataCore.ResponseInfo.AutoRedirect)
					{
						response.Redirect(dataCore.ResponseInfo.AutoRedirectLocation);
						dataCore.Dispose();
						return;
					}
				}
				else
				{
					response.StatusDescription = dataCore.ResponseInfo.HttpStatusDescription;
					response.StatusCode = dataCore.ResponseInfo.HttpStatusCode;

					byte[] message = System.Text.Encoding.ASCII.GetBytes(dataCore.ErrorMessage);
					response.ContentLength64 = message.Length;
					response.OutputStream.Write(message, 0, message.Length);
				}


				dataCore.Dispose();

				fLastState = LastProxyServerState.RequestProcessed;
			}
			catch
			{
			}
			finally
			{
				if (dataCore != null)
					dataCore.Dispose();
				try
				{
					if (response != null)
						response.Close();
				}
				catch { }

				if (startWaitCalled == false)
					WaitForContext();
			}
        }

		private static void ApplyReponseInfoToListener(HttpListenerResponse response, DataCoreResponseInformation responseInfo)
		{
			response.AddHeader("X-Powered-By", GlobalConsts.ASProxyAgentVersion);
			
			response.ContentEncoding = responseInfo.ContentEncoding;
			response.ContentLength64 = responseInfo.ContentLength;
			response.ContentType = responseInfo.ContentType;

			response.StatusDescription = responseInfo.HttpStatusDescription;
			response.StatusCode = responseInfo.HttpStatusCode;
		}

		/// <summary>
		/// Initialize a new instance of WebDataCore for HttpListenerRequest
		/// </summary>
		private static WebDataCore InitializeWebDataCore(HttpListenerRequest request)
		{
			WebDataCore dataCore = new WebDataCore(request.Url.ToString(), request.UserAgent);

			dataCore.RequestInfo.RequestMethod = request.HttpMethod;
			dataCore.RequestInfo.ContentType = request.ContentType;
			dataCore.RequestInfo.InputStream = request.InputStream;
			if (request.UrlReferrer != null)
				dataCore.RequestInfo.Referrer = request.UrlReferrer.ToString();

			return dataCore;
		}


		private static bool ContentIsHtml(string contentType)
		{
			contentType = contentType.ToLower();
			return contentType.IndexOf("html") != -1;
		}

		/// <summary>
		/// Start waiting for a request
		/// </summary>
        private static void WaitForContext()
        {
            if (fListener == null)
                return;
            try
            {
                IAsyncResult result = fListener.BeginGetContext(new AsyncCallback(ListenerCallback), fListener);
                fLastState = LastProxyServerState.Listening;
            }
            catch (Exception ex)
            {
                fLastErrorMessage = ex.ToString();
                fLastState = LastProxyServerState.Error;
            }
        }

		/// <summary>
		/// Check listener existence
		/// </summary>
        private static void EnsureListener()
        {
            if (fListener == null)
            {
                fListener = new HttpListener();
            }
            else
            {
                try
                {
                    int test = fListener.Prefixes.Count;
                }
                catch (ObjectDisposedException)
                {
					try
					{
						fListener.Close();
					}
					catch (Exception) { }
                    fListener = new HttpListener();
                }
                catch
                {
                }
            }
        }

		/// <summary>
		/// Returns ASProxy signature as a byte array
		/// </summary>
		/// <returns></returns>
        private static byte[] ASPRoxySignature()
        {
			return System.Text.Encoding.ASCII.GetBytes("<span style='font-size:xx-large;display:none'><b>ASProxy provided site</b></span>");
        }
        #endregion

    }


	public class ASProxyServerServiceProcess
	{
		/// <summary>
		/// Kills all ASP.NET application servers.
		/// </summary>
		public static void StopASPNetApplicationPool()
		{
			try
			{
				Process[] processes = Process.GetProcessesByName("w3wp");
				foreach (Process prc in processes)
				{
					prc.Kill();
				}
			}
			catch { }

			try
			{
				Process[] processes = Process.GetProcessesByName("aspnet_wp");
				foreach (Process prc in processes)
				{
					prc.Kill();
				}
			}
			catch { }

		}
	}
}

<%@ WebHandler Language="C#" Class="Download" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using System.Threading;
using SalarSoft.ResumableDownload;
using SalarSoft.ASProxy.Exposed;
using System.IO;

public class Download : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{

	public void ProcessRequest(HttpContext context)
	{
		try
		{
			if (Configurations.Authentication.Enabled)
			{
				if (!Configurations.Authentication.HasPermission(context.User.Identity.Name,
					Configurations.AuthenticationConfig.UserPermission.Images))
				{
					context.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
					context.Response.ContentType = "text/html";
					context.Response.Write("You do not have access to downloads. Ask site administrator to grant permission.");
					return;
				}
			}

			if (UrlProvider.IsASProxyAddressUrlIncluded(context.Request.QueryString))
			{
				DownloadUrl(context);
			}
			else
			{
				context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound; ;
				context.Response.ContentType = "text/html";
				context.Response.Write("No url is specified.");
			}
		}
		catch (ThreadAbortException) { }
		catch (Exception ex)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, ex.Message, context.Request.Url.ToString());

			context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(ex);
			context.Response.ContentType = "text/html";
			context.Response.Write(ex.Message);

			return;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <Added>Since V4.1</Added>
	private string GetRedirectEncodedUrlForASProxyPages(string asproxyPage, string currentRequest, bool encodeUrl)
	{
		// Encode redirect page if needed
		if (encodeUrl)
		{
			currentRequest = UrlProvider.EncodeUrl(currentRequest);
		}

		// Apply current page as referrer url for redirect url
		asproxyPage = UrlBuilder.AddUrlQuery(asproxyPage, Consts.Query.UrlAddress, currentRequest);

		// Apply decode option
		asproxyPage = UrlBuilder.AddUrlQuery(asproxyPage, Consts.Query.Decode, Convert.ToByte(encodeUrl).ToString());

		// If page is marked as posted back, remark it as no post back
		if (asproxyPage.IndexOf(Consts.Query.WebMethod) != -1)
			asproxyPage = UrlBuilder.RemoveQuery(asproxyPage, Consts.Query.WebMethod);

		return asproxyPage;
	}

	public void DownloadUrl(HttpContext context)
	{
		IEngine engine = null;
		MemoryStream responseData = null;
		ResumableDownload download = null;
		try
		{
			engine = (IEngine)Provider.GetProvider(ProviderType.IEngine);
			engine.UserOptions = UserOptions.ReadFromRequest();

			engine.DataTypeToProcess = DataTypeToProcess.None;
			engine.RequestInfo.SetContentType(MimeContentType.application);
	
			// we request for a download
			engine.RequestInfo.RequesterType = RequesterType.Download;
			
			// Initializing the engine
			engine.Initialize(context.Request);

			// communicate with back-end
			engine.ExecuteHandshake();

			if (Configurations.WebData.Downloader_ResumeSupport)
			{

				// the data stream
				responseData = new MemoryStream();

				// Execute the response
				engine.ExecuteToStream(responseData);

				if (engine.LastStatus == LastStatus.Error)
				{
					if (Systems.LogSystem.ErrorLogEnabled)
						Systems.LogSystem.LogError(engine.LastException, engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

					context.Response.Clear();
					SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);
					context.Response.ContentType = "text/html";
					context.Response.Write("//" + engine.LastErrorMessage);
					context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

					context.ApplicationInstance.CompleteRequest();
					return;
				}

				context.Response.ClearContent();
				Common.ClearHeadersButSaveEncoding(context.Response);

				download = new ResumableDownload();
				download.ClearResponseData();
				download.ContentType = "application/octet-stream";

				string filename;
				//====Get file name====
				if (string.IsNullOrEmpty(engine.ResponseInfo.ContentFilename) == false)
					filename = engine.ResponseInfo.ContentFilename;
				else
					filename = System.IO.Path.GetFileName(engine.RequestInfo.RequestUrl);


				// Log download status
				if (Systems.LogSystem.ActivityLogEnabled)
					Systems.LogSystem.Log(LogEntity.DownloadRequested, engine.RequestInfo.RequestUrl, filename, responseData.Length.ToString());


				//********************************************//
				//	Important note!
				//	MemoryStream.ToArray() is slower than MemoryStream.GetBuffer()
				//	Because the MemoryStream.ToArray function returns a copy of stream content,
				//  but MemoryStream.GetBuffer() returns a refrence of stream contents.
				//	So i used GetBuffer()!!
				//********************************************//
				download.ProcessDownload(responseData.GetBuffer(), engine.RequestInfo.RequestUrl, filename);
			}
			else
			{
				// resume support is not enabled

				// writes data directly to the response
				engine.ExecuteToResponse(context.Response);

				// check for errors
				if (engine.LastStatus == LastStatus.Error)
				{
					if (Systems.LogSystem.ErrorLogEnabled)
						Systems.LogSystem.LogError(engine.LastException, engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

					context.Response.Clear();
					SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);
					context.Response.ContentType = "text/html";
					context.Response.Write("//" + engine.LastErrorMessage);
					context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

					context.ApplicationInstance.CompleteRequest();
					return;
				}

			}

		}
		catch (System.Threading.ThreadAbortException) { }
		catch (Exception ex)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, ex.Message, engine.RequestInfo.RequestUrl);

			context.Response.Clear();
			SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);
			context.Response.ContentType = "text/html";
			context.Response.Write(ex.Message);
			context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

			context.ApplicationInstance.CompleteRequest();
		}
		finally
		{
			if (download != null)
				download.Dispose();
			if (engine != null)
				engine.Dispose();
		}

	}


	public bool IsReusable
	{
		get
		{
			return false;
		}
	}

}
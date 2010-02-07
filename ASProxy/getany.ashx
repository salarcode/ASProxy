<%@ WebHandler Language="C#" Class="GetAny" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using System.Threading;
using SalarSoft.ASProxy.Exposed;

public class GetAny : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{
	public void ProcessRequest(HttpContext context)
	{
		IEngine engine = null;
		try
		{
			if (UrlProvider.IsASProxyAddressUrlIncluded(context.Request.QueryString))
			{
				engine = (IEngine)Providers.GetProvider(ProviderType.IEngine);
				engine.UserOptions = UserOptions.ReadFromRequest();

				// should detect automatically
				engine.DataTypeToProcess = DataTypeToProcess.AutoDetect;
				engine.RequestInfo.SetContentType(MimeContentType.application);

				// Initializing the engine
				engine.Initialize(context.Request);
				engine.ExecuteHandshake();

				if (!string.IsNullOrEmpty(engine.ResponseInfo.ContentType))
				{
					// gets mime type of content type
					MimeContentType mimeContentType = engine.ResponseInfo.ContentTypeMime;

					if (Configurations.Authentication.Enabled &&
						(mimeContentType == MimeContentType.image_gif ||
						mimeContentType == MimeContentType.image_jpeg))
					{
						if (!Configurations.Authentication.HasPermission(context.User.Identity.Name,
							Configurations.AuthenticationConfig.UserPermission.Images))
						{
							context.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
							context.Response.ContentType = "text/html";
							context.Response.Write("You do not have access to see images. Ask site administrator to grant permission.");
							return;
						}
					}
				}

				// apply http compression
				SalarSoft.ASProxy.BuiltIn.HttpCompressor.ApplyCompression(engine.ResponseInfo.ContentTypeMime);

				// Execute the response
				engine.ExecuteToResponse(context.Response);

				if (engine.LastStatus == LastStatus.Error)
				{
					if (Systems.LogSystem.ErrorLogEnabled)
						Systems.LogSystem.LogError(engine.LastException, engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

					context.Response.Clear();
					SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);

					context.Response.StatusDescription = engine.LastErrorMessage;
					context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

					// END
					context.ApplicationInstance.CompleteRequest();
				}
			}
			else
			{
				context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound; ;
				context.Response.ContentType = "text/html";
				context.Response.StatusDescription = "No url is specified.";
				context.Response.Write("No url is specified.");
			}
		}
		catch (ThreadAbortException) { }
		catch (Exception ex)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, ex.Message, context.Request.Url.ToString());

			context.Response.Clear();
			Common.ClearHeadersButSaveEncoding(context.Response);

			context.Response.StatusDescription = ex.Message;
			context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

			// END
			context.ApplicationInstance.CompleteRequest();
		}
		finally
		{
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
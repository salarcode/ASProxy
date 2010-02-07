<%@ WebHandler Language="C#" Class="GetCss" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using System.Threading;
using SalarSoft.ASProxy.Exposed;

public class GetCss : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
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

				engine.DataTypeToProcess = DataTypeToProcess.Css;
				engine.RequestInfo.SetContentType(MimeContentType.text_css);

				// Initialize the request
				engine.Initialize(context.Request);
				engine.ExecuteHandshake();

				// apply compression
				SalarSoft.ASProxy.BuiltIn.HttpCompressor.ApplyCompression(engine.ResponseInfo.ContentTypeMime);

				// Execute the request and apply it response
				engine.ExecuteToResponse(context.Response);

				if (engine.LastStatus == LastStatus.Error)
				{
					if (Systems.LogSystem.ErrorLogEnabled)
						Systems.LogSystem.LogError(engine.LastException, engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

					context.Response.Clear();
					SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);
					context.Response.ContentType = "text/html";
					context.Response.Write("/* " + engine.LastErrorMessage + " */");
					context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

					context.ApplicationInstance.CompleteRequest();
					//Response.End();
				}
			}
			else
			{
				context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
				context.Response.ContentType = "text/html";
				context.Response.Write("No url is specified.");
			}
		}
		catch (System.Threading.ThreadAbortException)
		{
		}
		catch (Exception ex)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, ex.Message, context.Request.Url.ToString());

			context.Response.Clear();
			SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);
			context.Response.ContentType = "text/html";
			context.Response.Write("/* Error: " + ex.Message + " */");
			context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

			context.ApplicationInstance.CompleteRequest();
			//context.Response.End();
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
<%@ WebHandler Language="C#" Class="GetHtml" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using System.Threading;
using SalarSoft.ASProxy.Exposed;

public class GetHtml : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{
    public void ProcessRequest(HttpContext context)
    {
        IEngine engine = null;
        try
        {
			if (Configurations.Authentication.Enabled)
			{
				if (!Configurations.Authentication.HasPermission(context.User.Identity.Name,
					Configurations.AuthenticationConfig.UserPermission.Pages))
				{
					context.Response.ContentType = "text/html";
					context.Response.Write("You do not have access to browse pages. Ask site administrator to grant permission.");
					context.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
					return;
				}
			}
			if (UrlProvider.IsASProxyAddressUrlIncluded(context.Request.QueryString))
            {
                engine = (IEngine)Providers.GetProvider(ProviderType.IEngine);
                engine.UserOptions = UserOptions.ReadFromRequest();

                engine.DataTypeToProcess = DataTypeToProcess.Html;
                engine.RequestInfo.SetContentType(MimeContentType.text_html);

                // initialize the request
                engine.Initialize(context.Request);
                engine.ExecuteHandshake();

				// apply http compression
				SalarSoft.ASProxy.BuiltIn.HttpCompressor.ApplyCompression(engine.ResponseInfo.ContentTypeMime);
				
				// execute and apply it to response
                engine.ExecuteToResponse(context.Response);

                if (engine.LastStatus == LastStatus.Error)
                {
                    if (Systems.LogSystem.ErrorLogEnabled)
						Systems.LogSystem.LogError(engine.LastException, engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

                    context.Response.Clear();
                    SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);
                    context.Response.ContentType = "text/html";
                    context.Response.Write("//" + engine.LastErrorMessage);
                    context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

                    // END
                    context.ApplicationInstance.CompleteRequest();
                }
            }
            else
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                context.Response.ContentType = "text/html";
                context.Response.Write("No url is specified.");
            }

        }
        catch (ThreadAbortException) { }
		catch (Exception ex)
        {
            if (Systems.LogSystem.ErrorLogEnabled)
                Systems.LogSystem.LogError(ex, ex.Message, context.Request.Url.ToString());

            context.Response.Clear();
            SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);
            context.Response.ContentType = "text/html";
			context.Response.Write("<center><b><font color='red'>Error: " + ex.Message + "</font></b></center>");
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
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
                engine = (IEngine)Provider.CreateProviderInstance(ProviderType.IEngine);
                engine.UserOptions = UserOptions.ReadFromRequest();

                // should detect automatically
                engine.DataTypeToProcess = DataTypeToProcess.AutoDetect;
                engine.RequestInfo.SetContentType(MimeContentType.application);

                // Initializing the engine
                engine.Initialize(context.Request);
                engine.ExecuteHandshake();

                // Execute the response
                engine.ExecuteToResponse(context.Response);

                if (engine.LastStatus == LastStatus.Error)
                {
                    if (Systems.LogSystem.ErrorLogEnabled)
                        Systems.LogSystem.LogError(engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

                    context.Response.Clear();
                    SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(context.Response);

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
                Systems.LogSystem.LogError(context.Request.Url.ToString(), ex.Message);

            context.Response.Clear();
            Common.ClearASProxyRespnseHeader(context.Response);

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
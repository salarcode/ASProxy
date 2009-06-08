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
                engine = (IEngine)Provider.CreateProviderInstance(ProviderType.IEngine);
                engine.UserOptions = UserOptions.ReadFromRequest();

                engine.DataTypeToProcess = DataTypeToProcess.Css;
                engine.RequestInfo.SetContentType (MimeContentType.text_css);

                // Initialize the request
                engine.Initialize(context.Request);
                engine.ExecuteHandshake();

                // Execute the request and apply it response
                engine.ExecuteToResponse(context.Response);

                if (engine.LastStatus == LastStatus.Error)
                {
                    if (Systems.LogSystem.ErrorLogEnabled)
                        Systems.LogSystem.LogError(engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

                    context.Response.Clear();
                    SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(context.Response);
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
        catch (Exception err)
        {
            if (Systems.LogSystem.ErrorLogEnabled)
                Systems.LogSystem.LogError(context.Request.Url.ToString(), err.Message);

            context.Response.Clear();
            SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(context.Response);
            context.Response.ContentType = "text/html";
            context.Response.Write("/* Error: " + err.Message + " */");
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
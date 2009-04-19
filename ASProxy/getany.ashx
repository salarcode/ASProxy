<%@ WebHandler Language="C#" Class="GetAny" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using System.Threading;

public class GetAny : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        ASProxyEngine engine = null;
        try
        {
            if (UrlProvider.IsASProxyAddressUrlIncluded(context.Request.QueryString))
            {
                engine = new ASProxyEngine(ProcessTypeForData.None, false);
                engine.RequestInfo.ContentType = MimeContentType.application;
                engine.Initialize(context.Request);
                engine.Execute(context.Response);

                if (engine.LastStatus == LastActivityStatus.Error)
                {
                    if (LogSystem.Enabled)
                        LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

                    context.Response.Clear();
                    SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(context.Response);
                    context.Response.ContentType = "text/html";
                    context.Response.Write("//" + engine.LastErrorMessage);
                    context.Response.StatusCode = (int)engine.LastErrorStatusCode();

                    context.ApplicationInstance.CompleteRequest();
                    //context.Response.End();
                }
            }
            //else
            //    Response.Redirect(FilesConsts.DefaultPage, false);
        }
        catch (ThreadAbortException)
        {
        }
        catch (Exception err)
        {
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.Error, context.Request.Url.ToString(), err.Message);

            context.Response.Clear();
            Common.ClearASProxyRespnseHeader(context.Response);
            context.Response.ContentType = "text/html";
            context.Response.Write("<center><b><font color='red'>Error: " + err.Message + "</font></b></center>");
            context.Response.StatusCode = (int)engine.LastErrorStatusCode();

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
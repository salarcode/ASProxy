﻿<%@ WebHandler Language="C#" Class="Download" %>

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
                Systems.LogSystem.LogError(ex,context.Request.Url.ToString());

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
            engine = (IEngine)Provider.CreateProviderInstance(ProviderType.IEngine);
            engine.UserOptions = UserOptions.ReadFromRequest();
            
            engine.DataTypeToProcess = DataTypeToProcess.None;
            engine.RequestInfo.SetContentType ( MimeContentType.application);

            // Initializing the engine
            engine.Initialize(context.Request);
            
            // communicate with back-end
            engine.ExecuteHandshake();

            
            // the data stream
            responseData = new MemoryStream();
            
            // Execute the response
            engine.ExecuteToStream(responseData);

            if (engine.LastStatus == LastStatus.Error)
            {
                if (Systems.LogSystem.ErrorLogEnabled)
                    Systems.LogSystem.LogError(engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

                context.Response.Clear();
                SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(context.Response);
                context.Response.ContentType = "text/html";
                context.Response.Write("//" + engine.LastErrorMessage);
                context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

                context.ApplicationInstance.CompleteRequest();
                return;
            }

            context.Response.ClearContent();
            Common.ClearASProxyRespnseHeader(context.Response);

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
        catch (System.Threading.ThreadAbortException) { }
        catch (Exception ex)
        {
            if (Systems.LogSystem.ErrorLogEnabled)
                Systems.LogSystem.LogError(engine.RequestInfo.RequestUrl, ex.Message);

            context.Response.Clear();
            SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(context.Response);
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
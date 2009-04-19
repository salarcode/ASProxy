<%@ WebHandler Language="C#" Class="Download" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using System.Threading;
using SalarSoft.ResumableDownload;


public class Download : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        bool decode = false;
        string url = "";
        try
        {
            UrlProvider.GetUrlArguments(context.Request.QueryString, out decode, out url);
            if (!string.IsNullOrEmpty(url))
            {
                if (decode)
                    url = UrlProvider.DecodeUrl(url);
                DoDownload(context, url);
            }
        }
        catch (ThreadAbortException)
        {
        }
        catch (Exception err)
        {
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.Error, context.Request.Url.ToString(), err.Message);

            ASProxyExceptions.LogException(err, "Error while downloading: " + url);
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
        asproxyPage = UrlBuilder.AddUrlQuery(asproxyPage, Consts.qUrlAddress, currentRequest);

        // Apply decode option
        asproxyPage = UrlBuilder.AddUrlQuery(asproxyPage, Consts.qDecode, Convert.ToByte(encodeUrl).ToString());

        // If page is marked as posted back, remark it as no post back
        if (asproxyPage.IndexOf(Consts.qWebMethod) != -1)
            asproxyPage = UrlBuilder.RemoveQuery(asproxyPage, Consts.qWebMethod);

        return asproxyPage;
    }

    public void DoDownload(HttpContext context, string url)
    {
        WebDataCore data = null;
        ResumableDownload download = null;
        try
        {

            data = new WebDataCore(url, context.Request.UserAgent);
            data.Execute();
            if (data.Status == LastActivityStatus.Error)
            {
                if (LogSystem.Enabled)
                    LogSystem.Log(LogEntity.Error, url, data.ErrorMessage);

                context.Response.StatusCode = (int)Common.GetErrorCode(data.LastException);
                //lblErrorMsg.Text = data.ErrorMessage;
                //lblErrorMsg.Visible = true;
                return;
            }

            if (data.ResponseInfo.AutoRedirect)
            {
                if (data.ResponseInfo.AutoRedirectionType == AutoRedirectType.ASProxyPages)
                {
                    // Encoded url needed
                    string newLocation = GetRedirectEncodedUrlForASProxyPages(data.ResponseInfo.AutoRedirectLocation, url, true);
                    context.Response.Redirect(newLocation);

                    return;
                }
                else
                    throw new NotSupportedException("Auto redirection not supported in download page.");
            }

            context.Response.ClearContent();
            SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(context.Response);
            context.Response.ContentType = "application/octet-stream";

            download = new ResumableDownload();
            download.ClearResponseData();
            download.ContentType = "application/octet-stream";
            string filename;
            //====Get file name====
            if (string.IsNullOrEmpty(data.ResponseInfo.ContentFilename) == false)
                filename = data.ResponseInfo.ContentFilename;
            else
                filename = System.IO.Path.GetFileName(url);


            // Log download status
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.DownloadRequested, url, filename, data.ResponseData.GetBuffer().Length.ToString());

            //********************************************//
            //	Important note!
            //	MemoryStream.ToArray() is slower than MemoryStream.GetBuffer()
            //	Because the MemoryStream.ToArray function returns a copy of stream content,
            //  but MemoryStream.GetBuffer() returns a refrence of stream contents.
            //	So i used GetBuffer()!!
            //********************************************//
            download.ProcessDownload(data.ResponseData.GetBuffer(), url, filename);

            //ApplicationInstance.CompleteRequest();
            //context.ApplicationInstance.CompleteRequest();
            //context.Response.End();

        }
        catch (System.Threading.ThreadAbortException)
        {
        }
        catch (Exception err)
        {
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.Error, url, err.Message);

            context.Response.Clear();
            context.Response.ContentType = "text/html";
            context.Response.Write(err.Message);
            context.Response.StatusCode = (int)Common.GetErrorCode(err);

            context.ApplicationInstance.CompleteRequest();

            //lblErrorMsg.Text = err.Message;
            //lblErrorMsg.Visible = true;
        }
        finally
        {
            if (download != null)
                download.Dispose();
            if (data != null)
                data.Dispose();
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
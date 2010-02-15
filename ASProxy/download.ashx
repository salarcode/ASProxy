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

            if (context.Response.BufferOutput)
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
			currentRequest = UrlProvider.EncodeUrl(currentRequest);
		else
			currentRequest = UrlProvider.EscapeUrlQuery(currentRequest);

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
        try
        {
            bool reqRangeRequest;
            int reqRangeBegin = 0;
            long reqRangeEnd = -1;

            engine = (IEngine)Providers.GetProvider(ProviderType.IEngine);
            engine.UserOptions = UserOptions.ReadFromRequest();

            ResumableTransfers.ParseRequestHeaderRange(context.Request, out reqRangeBegin, out reqRangeEnd, out reqRangeRequest);
            if (Configurations.WebData.Downloader_ResumeSupport)
            {
                engine.RequestInfo.RangeBegin = reqRangeBegin;
                engine.RequestInfo.RangeEnd = reqRangeEnd;
                engine.RequestInfo.RangeRequest = reqRangeRequest;
            }

            engine.DataTypeToProcess = DataTypeToProcess.None;
            engine.RequestInfo.SetContentType(MimeContentType.application);

            // we request for a download
            engine.RequestInfo.RequesterType = RequesterType.Download;

            // disable buffering, use streaming
            engine.RequestInfo.BufferResponse = false;

            // Initializing the engine
            engine.Initialize(context.Request);

            // communicate with back-end
            engine.ExecuteHandshake();

            if (engine.LastStatus == LastStatus.Error)
            {
                if (Systems.LogSystem.ErrorLogEnabled)
                    Systems.LogSystem.LogError(engine.LastException, engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

                context.Response.Clear();
                Common.ClearHeadersButSaveEncoding(context.Response);
                context.Response.ContentType = "text/html";
                context.Response.Write(engine.LastErrorMessage);
                context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

                context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (Configurations.WebData.Downloader_ResumeSupport)
            {
                context.Response.ClearContent();
                Common.ClearHeadersButSaveEncoding(context.Response);

                string resFilename;
                if (!string.IsNullOrEmpty(engine.ResponseInfo.ContentFilename))
                    resFilename = engine.ResponseInfo.ContentFilename;
                else
                    resFilename = Path.GetFileName(engine.RequestInfo.RequestUrl);

                ResumableResponseData responseData = new ResumableResponseData(engine.WebData.ResponseData, resFilename);

                // beause the data is using streaming, the content dosnn't support seeking
                responseData.ApplyRangeToStream = false;
                responseData.DataLength = engine.WebData.ResponseInfo.ContentLength;

                string matchedETag;
                int responseStatusCode = context.Response.StatusCode;
                if (!ResumableTransfers.ValidatePartialRequest(context.Request, responseData, out matchedETag, ref responseStatusCode))
                {
                    // the request is invalid
                    context.Response.StatusCode = responseStatusCode;
                    if (!string.IsNullOrEmpty(matchedETag))
                        context.Response.AppendHeader("ETag", matchedETag);

                    // stop the preoccess
                    // but don't hassle with error messages
                    return;
                }

                // if back-end site supports resuming
                if (engine.WebData.ResponseInfo.RangeResponse)
                {
                    // no need to seek in the data stream
                    responseData.ApplyRangeToStream = false;

                    // the data itself is in the range, no need to 
                    responseData.RangeRequest = true;
                    responseData.RangeBegin = engine.WebData.ResponseInfo.RangeBegin;
                    responseData.RangeEnd = engine.WebData.ResponseInfo.RangeEnd;
                }
                else
                {
                    // forcing to enable resume support
                    if (reqRangeRequest && Configurations.WebData.Downloader_ForceResumeSupport)
                    {
                        // Forced resume-support feature
                        // here the back-end site doesn't support resume downloading
                        // try to buffer the response in the memory and send as resume supported
                        // Note: most of hosting services won't let app to use more than 10MB memory

                        Stream backEndStream = responseData.DataStream;

                        // this code also downloads data from back-end site
                        Stream bufferedStream = ResumableResponseData.ReadToBuffer(backEndStream);
                        backEndStream.Dispose();

                        // save the buffered data
                        responseData.DataStream = bufferedStream;

                        // the ranges will apply to the buffered stream
                        responseData.ApplyRangeToStream = true;
                        if (reqRangeRequest)
                        {
                            responseData.RangeBegin = reqRangeBegin;
                            responseData.RangeEnd = reqRangeEnd;
                            responseData.RangeRequest = reqRangeRequest;
                        }
                        else
                        {
                            responseData.RangeBegin = 0;
                            responseData.RangeEnd = (bufferedStream.Length - 1);
                        }


                        if (responseData.RangeBegin > responseData.RangeEnd)
                            responseData.RangeEnd = reqRangeBegin + (bufferedStream.Length - 1);

                        if (engine.WebData.ResponseInfo.ResponseProtocol == InternetProtocols.FTP)
                        {
                            // Ftp always support resume-support but content length is unknown!
                            // So the response is in range itself.
                            // No need to apply ranges again
                            responseData.ApplyRangeToStream = false;

                            // data lenght is unknown most of the time
                            if (responseData.DataLength == -1)
                            {
                                responseData.DataLength = reqRangeBegin + bufferedStream.Length;
                            }
                        }

                    }
                    else
                    {
                        // just disable anything
                        responseData.ApplyRangeToStream = false;
                        responseData.RangeRequest = false;
                        responseData.RangeBegin = 0;
                        responseData.RangeEnd = responseData.DataLength - 1;

                        if (Configurations.WebData.Downloader_ForceResumeSupport)
                        {
                            if (engine.WebData.ResponseInfo.ResponseProtocol == InternetProtocols.FTP)
                            {
                                if (responseData.DataLength == -1)
                                {
                                    Stream backEndStream = responseData.DataStream;

                                    // this code also downloads data from back-end site
                                    Stream bufferedStream = ResumableResponseData.ReadToBuffer(backEndStream);
                                    backEndStream.Dispose();

                                    // save the buffered data
                                    responseData.DataStream = bufferedStream;

                                    // data lenght is unknown most of the time
                                    responseData.DataLength = reqRangeBegin + bufferedStream.Length;
                                    responseData.RangeEnd = responseData.DataLength - 1;
                                }
                            }
                        }
                    }
                }

                // Log download status
                if (Systems.LogSystem.ActivityLogEnabled)
                    Systems.LogSystem.Log(LogEntity.DownloadRequested, engine.RequestInfo.RequestUrl, resFilename, responseData.DataLength.ToString());

                // downloader
                using (ResumableResponse downloader = new ResumableResponse(responseData))
                {
                    // start the resume supported download
                    downloader.ProcessDownload(context.Response);
                }
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
                    context.Response.Write(engine.LastErrorMessage);
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
            Common.ClearHeadersButSaveEncoding(context.Response);
            context.Response.ContentType = "text/html";
            context.Response.Write(ex.Message);
            if (context.Response.BufferOutput)
                context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

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
<%@ WebHandler Language="C#" Class="images" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using System.Threading;
using System.Drawing;

public class images : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        try
        {
            if (UrlProvider.IsASProxyAddressUrlIncluded(context.Request.QueryString))
            {
                ASProxyEngine engine = new ASProxyEngine(ProcessTypeForData.None, false);
                engine.RequestInfo.ContentType = MimeContentType.image_jpeg;
                engine.Initialize(context.Request);

                if (LogSystem.Enabled)
                    LogSystem.Log(LogEntity.ImageRequested, context.Request, engine.RequestInfo.RequestUrl);

                ShowImage(context, engine);
            }
            else
            {
                ShowError(context, -1);
            }
        }
        catch (ThreadAbortException)
        {
        }
        catch (Exception ex)
        {
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.Error, context.Request.Url, ex.Message);

            ShowError(context, Common.GetErrorDetailedCode(ex));

            context.ApplicationInstance.CompleteRequest();
            //Response.End();
            return;
        }
    }

    public void ShowImage(HttpContext context, ASProxyEngine results)
    {
        try
        {
            results.Execute(context.Response);
            if (results.LastStatus == LastActivityStatus.Error)
            {
                if (LogSystem.Enabled)
                    LogSystem.Log(LogEntity.Error, results.RequestInfo.RequestUrl, results.LastErrorMessage);


                ShowError(context, results.LastErrorStatusDetailedCode());

                context.Response.StatusDescription = results.LastErrorMessage;
                context.Response.StatusCode = (int)results.LastErrorStatusCode();

                context.ApplicationInstance.CompleteRequest();
                //Response.End();
                return;
            }
        }
        catch (ThreadAbortException) { }
        catch (Exception ex)
        {
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.Error, results.RequestInfo.RequestUrl, ex.Message);

            ShowError(context, (int)results.LastErrorStatusDetailedCode());

            context.ApplicationInstance.CompleteRequest();
            //Response.End();
            return;
        }
        finally
        {
            results.Dispose();
        }
    }

    Bitmap GetErrorImage(string errorCode)
    {
        const string text = "Error!";
        Font textfont = new Font("Tahoma", 10);
        int txtwith;
        txtwith = text.Length * text.Length;
        Bitmap bmp = new System.Drawing.Bitmap(txtwith, (textfont.Height) * 2, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);

        Graphics graphicImage = Graphics.FromImage(bmp);

        graphicImage.Clear(Color.White);
        graphicImage.DrawString(text, textfont, Brushes.Black, new Point(0, 0));
        graphicImage.DrawString(errorCode, textfont, Brushes.Red, new Point(0, textfont.Height));

        graphicImage.Dispose();

        return bmp;
    }

    public void ShowError(HttpContext context, int errorCode)
    {
        SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(context.Response);
        context.Response.ClearContent();
        context.Response.ContentType = "image/gif";
        Bitmap bmp = GetErrorImage(errorCode.ToString());

        bmp.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Gif);
        bmp.Dispose();
    }




    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}
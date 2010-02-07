<%@ WebHandler Language="C#" Class="Images" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using System.Threading;
using System.Drawing;
using SalarSoft.ASProxy.Exposed;

public class Images : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
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
					ShowError(context, "NoAccess");
					return;
				}
			}
            if (UrlProvider.IsASProxyAddressUrlIncluded(context.Request.QueryString))
            {
                IEngine engine = (IEngine)Providers.GetProvider(ProviderType.IEngine);
                engine.UserOptions = UserOptions.ReadFromRequest();
                
                engine.DataTypeToProcess = DataTypeToProcess.None;
                engine.RequestInfo.SetContentType(MimeContentType.image_jpeg);

                // initalize the request
                engine.Initialize(context.Request);
                engine.ExecuteHandshake();

                if (Systems.LogSystem.ActivityLogEnabled)
                    Systems.LogSystem.Log(LogEntity.ImageRequested, context.Request, engine.RequestInfo.RequestUrl);

                ShowImage(context, engine);
            }
            else
            {
                ShowError(context, -1);
            }
        }
        catch (ThreadAbortException) { }
        catch (Exception ex)
        {
            if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, ex.Message, context.Request.Url.ToString());

            ShowError(context, Common.GetExceptionHttpDetailedErrorCode(ex));

            context.ApplicationInstance.CompleteRequest();
            //Response.End();
            return;
        }
    }

    public void ShowImage(HttpContext context, IEngine engine)
    {
        try
        {
            // execute to response
            engine.ExecuteToResponse(context.Response);

            if (engine.LastStatus == LastStatus.Error)
            {
                if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(engine.LastException, engine.LastErrorMessage, engine.RequestInfo.RequestUrl);


                ShowError(context, Common.GetExceptionHttpDetailedErrorCode(engine.LastException));

                context.Response.StatusDescription = engine.LastErrorMessage;
                context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);

                context.ApplicationInstance.CompleteRequest();
                //Response.End();
                return;
            }
        }
        catch (ThreadAbortException) { }
        catch (Exception ex)
        {
            if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, ex.Message, engine.RequestInfo.RequestUrl);

            ShowError(context, Common.GetExceptionHttpDetailedErrorCode(engine.LastException));

            context.ApplicationInstance.CompleteRequest();
            //Response.End();
            return;
        }
        finally
        {
            engine.Dispose();
        }
    }

    Bitmap GetErrorImage(string errorCode)
    {
        const string text = " Error!";
        Font textfont = new Font("Tahoma", 8);
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
		SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);
		context.Response.ClearContent();
		context.Response.ContentType = "image/gif";
		Bitmap bmp = GetErrorImage(errorCode.ToString());

		bmp.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Gif);
		bmp.Dispose();
	}

	public void ShowError(HttpContext context, string errorMsg)
	{
		SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(context.Response);
		context.Response.ClearContent();
		context.Response.ContentType = "image/gif";
		Bitmap bmp = GetErrorImage(errorMsg);

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
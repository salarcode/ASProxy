<%@ Page Language="C#" %>

<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="System.Drawing" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	Bitmap GetErrorImage()
	{
		const string text = "Error!";
		Font textfont = new Font("Tahoma", 10);
		int txtwith;
		txtwith = text.Length * text.Length;
		Bitmap bmp = new System.Drawing.Bitmap(txtwith, textfont.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
		
		Graphics graphicImage = Graphics.FromImage(bmp);

		graphicImage.Clear(Color.White);
		graphicImage.DrawString(text, textfont, Brushes.Black, new Point(0, 0));

		graphicImage.Dispose();

		return bmp;
	}

    public void ShowError()
    {
        SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(Response);
        Response.ClearContent();
        Response.ContentType = "image/gif";
        Bitmap bmp = GetErrorImage();

        bmp.Save(Response.OutputStream, System.Drawing.Imaging.ImageFormat.Gif);
        bmp.Dispose();
    }

    public void ShowImage(ASProxyEngine results)
    {
		try
		{
			results.Execute(Response);
			if (results.LastStatus == LastActivityStatus.Error)
			{
				if (LogSystem.Enabled)
					LogSystem.Log(LogEntity.Error, results.RequestInfo.RequestUrl, results.LastErrorMessage);

				ShowError();

				Response.StatusDescription = results.LastErrorMessage;
				Response.StatusCode = (int)results.LastErrorStatusCode();
				
				Response.End();
				return;
			}
		}
		catch (ThreadAbortException) { }
		catch (Exception ex)
		{
			if (LogSystem.Enabled)
				LogSystem.Log(LogEntity.Error, results.RequestInfo.RequestUrl, ex.Message);

			ShowError();
			Response.End();
			return;
		}
        finally
        {
            results.Dispose();
        }
    }

    void SurfingOnLoading()
    {
        try
        {
            if (!Page.IsPostBack)
            {
                if (UrlProvider.IsASProxyAddressUrlIncluded(Request.QueryString))
                {
                    ASProxyEngine engine = new ASProxyEngine(ProcessTypeForData.None, false);
                    engine.RequestInfo.ContentType = MimeContentType.image_jpeg;
                    engine.Initialize(Request);

                    if (LogSystem.Enabled)
                        LogSystem.Log(LogEntity.ImageRequested, Request, engine.RequestInfo.RequestUrl);

                    ShowImage(engine);
                }
            }
        }
        catch (ThreadAbortException)
        {
        }
        catch (Exception ex)
        {
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.Error, Request.Url, ex.Message);

            ShowError();
			Response.End();
			return;
        }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        SurfingOnLoading();
    }
    protected void btnDisplay_Click(object sender, EventArgs e)
    {
        txtUrl.Text = UrlProvider.CorrectInputUrl(txtUrl.Text);
        imgImageHolder.ImageUrl = UrlProvider.AddArgumantsToUrl(FilesConsts.ImagesPage, txtUrl.Text, true);
        imgImageHolder.Visible = true;
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>[PageTitle]</title>
<style type="text/css">
#ASProxyMain{display:block;padding:1px;width: 100%;margin:0px;border:2px solid #000000;text-align: center;}
#ASProxyMainTable{padding:0px;margin:0px;border:1px solid #C0C0C0;background-color:#f8f8f8;background-image:none;font-weight:normal;font-style:normal;line-height:normal;visibility:visible;table-layout:auto;white-space:normal;word-spacing:normal;}
#ASProxyMainTable td{font-family: Tahoma, sans-serif;font-size: 8pt;text-align: center;}
#ASProxyMainTable .Button{background-color: #ECE9D8;border:2px outset;}
#ASProxyMainTable .Sides{width: 140px;}
#ASProxyMainTable a,#ASProxyMainTable a:hover,#ASProxyMainTable a:visited,#ASProxyMainTable a:active{color: #000099; text-decoration:underline;}
#ASProxyMainTable input{border: thin solid silver;background-color: #FFFFFF;margin:1px 2px 1px 2px;}
#ASProxyMainTable span input{border-width: 0px;}
</style>
</head>
<body>
<form id="frmImages" runat="server">
<div id="ASProxyMain" dir="[Direction]">
<table id="ASProxyMainTable" style="width: 100%">
<tr>
<td>
<table style="width: 100%">
    <tr>
        <td class="Sides">
            <a href="/" asproxydone="2">[VersionFamily]</a></td>
        <td style="font-size: small;">
            <strong>[PageHeader]</strong></td>
        <td class="Sides">
            powered by SalarSoft</td>
    </tr>
</table></td></tr><tr><td>
<a href="default.aspx">[ReturnHome]</a>
</td></tr><tr><td>
<!--This is ASProxy that powered by SalarSoft. -->
[EnterUrl]<br />
<asp:TextBox ID="txtUrl" runat="server" BackColor="White" Columns="60" dir="ltr"></asp:TextBox><asp:Button
    ID="btnDisplay" runat="server" OnClick="btnDisplay_Click" CssClass="Button" Text="[btnDisplay]" />
</td></tr></table></div>
<div style="width: 100%; border-width: 0px; text-align: center">
<asp:Image ID="imgImageHolder" runat="server" EnableViewState="False" Visible="False" /></div>
</form>
</body></html>
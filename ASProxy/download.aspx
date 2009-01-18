<%@ Page Language="C#" %>

<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="SalarSoft.ResumableDownload" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
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

    public void DoDownload(string url)
    {
        WebDataCore data = null;
        ResumableDownload download = null;
        try
        {

            data = new WebDataCore(url, Request.UserAgent);
            data.Execute();
            if (data.Status == LastActivityStatus.Error)
            {
                if (LogSystem.Enabled)
                    LogSystem.Log(LogEntity.Error, url, data.ErrorMessage);

                lblErrorMsg.Text = data.ErrorMessage;
                lblErrorMsg.Visible = true;
                return;
            }

            if (data.ResponseInfo.AutoRedirect)
            {
                if (data.ResponseInfo.AutoRedirectionType == AutoRedirectType.ASProxyPages)
                {
                    // Encoded url needed
                    string newLocation = GetRedirectEncodedUrlForASProxyPages(data.ResponseInfo.AutoRedirectLocation, url, true);
                    Response.Redirect(newLocation);
                    
                    return;
                }
                else
                    throw new NotSupportedException("Auto redirection not supported in download page.");
            }

            Response.ClearContent();
            SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(Response);
            Response.ContentType = "application/octet-stream";

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
            Response.End();
        }
        catch (System.Threading.ThreadAbortException)
        {
        }
        catch (Exception err)
        {
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.Error, url, err.Message);
            
            Response.ContentType = "text/html";
            Response.AddHeader("ContentType", "text/html");

            lblErrorMsg.Text = err.Message;
            lblErrorMsg.Visible = true;
        }
        finally
        {
            if (download != null)
                download.Dispose();
            if (data != null)
                data.Dispose();
        }

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        bool decode = false;
        string url = "";
        if (!Page.IsPostBack)
        {
            try
            {
                UrlProvider.GetUrlArguments(Page.Request.QueryString, out decode, out url);
				if (!string.IsNullOrEmpty(url))
				{
                    if (decode)
                        url = UrlProvider.DecodeUrl(url);
                    txtUrl.Text = url;
                    DoDownload(url);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {
                if (LogSystem.Enabled)
                    LogSystem.Log(LogEntity.Error, Request.Url.ToString(), err.Message);
                
                ASProxyExceptions.LogException(err, "Error while downloading: " + url);
                lblErrorMsg.Text = err.Message;
                lblErrorMsg.Visible = true;
                return;
            }
        }
    }


    protected void btnDisplay_Click(object sender, EventArgs e)
    {
        try
        {
            txtUrl.Text = UrlProvider.CorrectInputUrl(txtUrl.Text);

            string downurl = UrlProvider.AddArgumantsToUrl(FilesConsts.DownloadPage, txtUrl.Text, true);
            lnkDownload.NavigateUrl = downurl;
            lnkDownload.Visible = true;
            lblDownloadTip.Visible = true;

            lblAutoPrompt.Text = string.Format(lblAutoPrompt.Text, downurl);
            lblAutoPrompt.Visible = true;
        }
        catch (ThreadAbortException)
        {
        }
        catch (Exception err)
        {
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.Error, Request.Url.ToString(), err.Message);
            
            lblErrorMsg.Text = err.Message;
            lblErrorMsg.Visible = true;
        }
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
<form id="frmDownlod" runat="server">
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
    </table>
</td>
</tr>
<tr>
<td>
    <a href="default.aspx">[ReturnHome]</a>
</td>
</tr>
<tr>
<td>
    [EnterUrl]<br />
    <!--This is ASProxy that powered by SalarSoft. -->
    <asp:TextBox ID="txtUrl" runat="server" BackColor="White" Columns="60" dir="ltr"></asp:TextBox><asp:Button
        ID="btnDisplay" runat="server" OnClick="btnDisplay_Click" Text="[btnDisplay]"
        CssClass="Button" />
        <br />
        [AdditionalComment]
</td>
</tr>
<tr>
<td>
    <asp:Label ID="lblDownloadTip" runat="server" EnableViewState="False" Text="[DownloadLinkDescription]"
        Visible="false"></asp:Label>
    <asp:HyperLink ID="lnkDownload" runat="server" EnableViewState="False" NavigateUrl="download.aspx"
        Visible="False">[DownloadLink]</asp:HyperLink>
</td>
</tr>
</table>
<asp:Label ID="lblErrorMsg" runat="server" EnableTheming="False" EnableViewState="False"
Font-Bold="True" Font-Names="Tahoma" Font-Size="9pt" ForeColor="Red" Text="Error message"
ToolTip="Error message" Visible="False"></asp:Label>
<asp:Label ID="lblAutoPrompt" runat="server" Text='<script type="text/javascript">window.location="{0}";</script>'
EnableViewState="false" Visible="false"></asp:Label></div>
</form>
</body></html>
<%@ Page Title="Download your file with ASProxy" Inherits="SalarSoft.ASProxy.PageInMasterLocale" Language="C#" MasterPageFile="~/Theme.master" culture="auto" meta:resourcekey="Page" uiculture="auto" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="SalarSoft.ResumableDownload" %>
<script runat="server">
	protected void btnDisplay_Click(object sender, EventArgs e)
	{
		try
		{
			txtUrl.Text = UrlProvider.CorrectInputUrl(txtUrl.Text);

			string downurl = UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageDownload, txtUrl.Text, true);
			lnkDownload.NavigateUrl = downurl;
			lnkDownload.Visible = true;
			divDownloadLink.Visible = true;

			lblAutoPrompt.Text = string.Format(lblAutoPrompt.Text, downurl);
			lblAutoPrompt.Visible = true;

		}
		catch (ThreadAbortException)
		{
		}
		catch (Exception ex)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, ex.Message, Request.Url.ToString());

			lblErrorMsg.Text = ex.Message;
			lblErrorMsg.Visible = true;
		}
	}
</script>

<asp:Content ContentPlaceHolderID="plhHeadMeta" Runat="Server">
<title>Download your file with ASProxy</title>
</asp:Content>


<asp:Content ContentPlaceHolderID="plhMainBar" Runat="Server">
<script type="text/javascript">	document.getElementById('mnuDownload').className = 'first';</script>
<div class="urlBar"><asp:Literal ID="lblUrl" EnableViewState="False" runat="server" 
meta:resourcekey="lblUrl" Text="Enter URL:"></asp:Literal><br />
<asp:TextBox ID="txtUrl" CssClass="urlText" runat="server" Columns="70" dir="ltr"
Width="550px" meta:resourcekey="txtUrl"></asp:TextBox>
<asp:Button CssClass="button" ID="btnDisplay" runat="server"
OnClick="btnDisplay_Click" Text="Download it" meta:resourcekey="btnDisplay" />
<div class="urlBarLinkBar" id="divDownloadLink" enableviewstate="false" visible="false"
runat="server">
<asp:Literal ID="lblPrompt" runat="server" enableviewstate="False" 
meta:resourcekey="lblPrompt" 
Text="If your borwser does not prompt download dialog click the link below."></asp:Literal>
<asp:HyperLink ID="lnkDownload" runat="server" EnableViewState="False" 
NavigateUrl="download.aspx" meta:resourcekey="lnkDownload">Download link</asp:HyperLink>
</div>
<asp:Label Style="display: block" class="urlBarDesc" ID="lblErrorMsg" runat="server"
EnableViewState="False" Font-Bold="True" ForeColor="Red" Text="Error message"
ToolTip="Error message" Visible="False" meta:resourcekey="lblErrorMsg"></asp:Label>
</div>
</asp:Content>


<asp:Content ContentPlaceHolderID="plhMainTitle" Runat="Server">
<asp:Label ID="lblMainTitle" runat="server" EnableViewState="False" meta:resourcekey="lblMainTitle" Text="Download tool"></asp:Label>
</asp:Content>


<asp:Content ContentPlaceHolderID="plhContent" Runat="Server">
<asp:Label ID="ltrMainDesc" runat="server" EnableViewState="False"
meta:resourcekey="ltrMainDesc" Text="
Here you can everything you want. You can download files, html pages, images, ftp
files, and etc.&lt;br /&gt;
The download will be resume support and fresh."></asp:Label>

<asp:Label ID="lblAutoPrompt" runat="server" Text='<script type="text/javascript">window.onload=function(){{window.location="{0}";}}</script>'
EnableViewState="False" Visible="False" meta:resourcekey="lblAutoPrompt"></asp:Label>
</asp:Content>


<asp:Content ContentPlaceHolderID="plhOptionsTitle" Runat="Server">
</asp:Content>


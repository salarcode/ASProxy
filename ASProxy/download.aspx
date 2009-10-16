<%@ Page Language="C#" meta:resourcekey="Page"%>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="SalarSoft.ResumableDownload" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

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

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>Download your file with ASProxy</title>
<link href="theme/default/style-<%=Resources.Languages.TextDirection %>.css" rel="stylesheet" type="text/css" />
</head><body>
<form id="frmDownlod" runat="server">
<div class="header"><div id="logo"><h1>
<a asproxydone="2" href=".">ASProxy</a><span class="super"><%=Consts.General.ASProxyVersion %></span></h1>
<h2><asp:Literal id="lblLogo2" enableviewstate="False" runat="server" meta:resourcekey="lblLogo2" Text="Surf the web with"></asp:Literal></h2>
</div></div><div id="menu-wrap"><div id="menu"><ul>
<li><a asproxydone="2" href="." accesskey="1"><asp:Literal ID="mnuHome" EnableViewState="False" runat="server" meta:resourcekey="mnuHome"
Text="Home"></asp:Literal></a></li>
<li><a asproxydone="2" href="cookieman.aspx" accesskey="2"><asp:Literal ID="mnuCookie" EnableViewState="False" runat="server" meta:resourcekey="mnuCookie"
Text="Cookie Manager"></asp:Literal></a></li>
<li class="first"><a asproxydone="2" href="download.aspx" accesskey="3"><asp:Literal ID="mnuDownload" EnableViewState="False" runat="server" meta:resourcekey="mnuDownload"
Text="Download Tool"></asp:Literal></a></li>
<li><a asproxydone="2" href="surf.aspx?dec=1&amp;url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!"
target="_blank" accesskey="4"><asp:Literal ID="mnuASProxy" EnableViewState="False" runat="server" meta:resourcekey="mnuASProxy"
Text="ASProxy Page"></asp:Literal></a></li>
</ul></div></div><div id="content"><div class="content">

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


<div class="about"><h1 class="title">
<asp:Label ID="lblMainTitle" runat="server" EnableViewState="False"
meta:resourcekey="lblMainTitle" Text="Download tool"></asp:Label>
</h1><div class="entry"><asp:Label ID="ltrMainDesc" runat="server" EnableViewState="False"
meta:resourcekey="ltrMainDesc" Text="
Here you can everything you want. You can download files, html pages, images, ftp
files, and etc.&lt;br /&gt;
The download will be resume support and fresh."></asp:Label>
</div></div></div></div>
<div id="links"><p>
<a asproxydone="2" href="cookieman.aspx" target="_blank"><asp:Literal ID="mnuFootCookie" EnableViewState="False" runat="server" meta:resourcekey="mnuFootCookie" Text="Cookie Manager"></asp:Literal></a>
<a asproxydone="2" href="download.aspx" target="_blank"><asp:Literal ID="mnuFootDownload" EnableViewState="False" runat="server" meta:resourcekey="mnuFootDownload" Text="Download Tool"></asp:Literal></a>
<asp:Literal ID="lblFootSlogan1" EnableViewState="False" runat="server" meta:resourcekey="lblFootSlogan1" Text="Have your own"></asp:Literal>
<a asproxydone="2" href="surf.aspx?dec=1&url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank">ASProxy</a>.
<asp:Literal ID="lblFootSlogan2" EnableViewState="False" runat="server" meta:resourcekey="lblFootSlogan2" Text="It's free."></asp:Literal>
</p></div><div id="footer"><p>
@2009 ASProxy <%=Consts.General.ASProxyVersion %>: powered by SalarSoft</p>
</div>
<asp:Label ID="lblAutoPrompt" runat="server" Text='<script type="text/javascript">window.location="{0}";</script>'
EnableViewState="False" Visible="False" 
meta:resourcekey="lblAutoPrompt"></asp:Label>
</form></body></html>
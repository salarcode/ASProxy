<%@ Page Language="C#" %>

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
				Systems.LogSystem.LogError(ex, Request.Url.ToString(), ex.Message);

			lblErrorMsg.Text = ex.Message;
			lblErrorMsg.Visible = true;
		}
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>[PageTitle]</title>
	<link href="theme/default/style.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<form id="frmDownlod" runat="server">
	<div class="header">
		<div id="logo">
			<h1>
				<a asproxydone="2" href=".">ASProxy</a><span class="super"><%=Consts.General.ASProxyVersion %></span></h1>
			<h2>
				Surf the web with</h2>
		</div>
	</div>
	<div id="menu-wrap">
		<div id="menu">
			<ul>
				<li><a asproxydone="2" href="." accesskey="1">Home</a></li>
				<li><a asproxydone="2" href="cookieman.aspx" accesskey="2">Cookie Manager</a></li>
				<li class="first"><a asproxydone="2" href="download.aspx" accesskey="3">Download Tool</a></li>
				<li><a asproxydone="2" href="surf.aspx?dec=1&amp;url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!"
					target="_blank" accesskey="4">ASProxy Page</a></li>
			</ul>
		</div>
	</div>
	<div id="content">
		<div class="content">
			<div class="urlBar">
				Enter URL:<br />
				<asp:TextBox ID="txtUrl" CssClass="urlText" runat="server" Columns="70" dir="ltr"
					Width="550px"></asp:TextBox><asp:Button CssClass="button" ID="btnDisplay" runat="server"
						OnClick="btnDisplay_Click" Text="Download it" />
				<div class="urlBarLinkBar" id="divDownloadLink" enableviewstate="false" visible="false"
					runat="server">
					If your borwser does not prompt download dialog click the link below.
					<asp:HyperLink ID="lnkDownload" runat="server" EnableViewState="False" NavigateUrl="download.aspx"
						Visible="true">[DownloadLink]</asp:HyperLink>
				</div>
				<asp:Label Style="display: block" class="urlBarDesc" ID="lblErrorMsg" runat="server"
					EnableViewState="False" Font-Bold="True" ForeColor="Red" Text="Error message"
					ToolTip="Error message" Visible="false"></asp:Label>
			</div>
			<div class="about">
				<h1 class="title">
					Download tool</h1>
				<div class="entry">
					Here you can everything you want. You can download files, html pages, images, ftp
					files, and etc.
					<br />
					The download will be resume support and fresh.
				</div>
			</div>
		</div>
	</div>
	<div id="links">
		<p>
			<a asproxydone="2" href="cookieman.aspx" target="_blank">Cookie Manager</a> <a asproxydone="2"
				href="download.aspx" target="_blank">Download Tool</a> Have your own <a asproxydone="2"
					href="surf.aspx?dec=1&url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank">
					ASProxy</a>. It's free.
		</p>
	</div>
	<div id="footer">
		<p>
			@2009 ASProxy <%=Consts.General.ASProxyVersion %>: powered by SalarSoft</p>
	</div>
	<asp:Label ID="lblAutoPrompt" runat="server" Text='<script type="text/javascript">window.location="{0}";</script>'
		EnableViewState="false" Visible="false"></asp:Label>
	</form>
</body>
</html>

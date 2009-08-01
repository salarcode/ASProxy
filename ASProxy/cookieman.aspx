<%@ Page Language="C#" meta:resourcekey="Page"%>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	const string authenticationCookie = "ASProxyUser";
	const string sessionStateCookie = "ASProxySession";

	private HttpCookieCollection GetCookiesCollection()
	{
		HttpCookieCollection result = new HttpCookieCollection();
		for (int i = 0; i < Request.Cookies.Count; i++)
		{
			HttpCookie cookie = Request.Cookies[i];
			if (cookie.Name != sessionStateCookie &&
				cookie.Name != authenticationCookie &&
				cookie.Name != Consts.FrontEndPresentation.UserOptionsCookieName &&
				cookie.Name != Consts.FrontEndPresentation.HttpCompressorCookieName)
			{
				result.Add(cookie);
			}
		}
		return result;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		rptCookies.DataSource = GetCookiesCollection();

		rptCookies.DataBind();
	}

	protected void rptCookies_btnDelete(object source, EventArgs e)
	{
		Button btn = (Button)source;
		string cookieName = btn.CommandArgument;

		if (btn.CommandName == "DeleteCookie")
		{
			HttpCookie cookie = Response.Cookies[cookieName];
			cookie.Expires = DateTime.Now.AddYears(-10);

			Response.Cookies.Add(cookie);
			Response.Redirect(Request.Url.ToString(), false);
		}
	}

	protected void rptCookies_btnClearCookies(object source, EventArgs e)
	{
		Button btn = (Button)source;
		string cmdName = btn.CommandName;

		if (cmdName == "ClearCookies")
		{
			HttpCookieCollection reqCookies = GetCookiesCollection();
			for (int i = 0; i < reqCookies.Count; i++)
			{
				HttpCookie cookie = reqCookies[i];
				cookie.Expires = DateTime.Now.AddYears(-10);
				Response.Cookies.Add(cookie);
			}
			Response.Redirect(Request.Url.ToString(), false);
		}
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Manage stored cookies by ASProxy in your PC</title>
<link href="theme/default/style-<%=Resources.Languages.TextDirection %>.css" rel="stylesheet" type="text/css" />
	<style type="text/css">
		.about h1
		{
			margin-top: 0px;
			padding-top: 5px;
		}
		.desc
		{
			overflow: auto;
		}
		.cookieValue
		{
			min-height: 25px;
			max-height: 90px;
			overflow-y: auto;
			width: 650px;
			word-break: break-all;
			word-wrap: break-word;
		}
		.tblOptions
		{
			margin-top: 5px;
		}
		.option th
		{
			background-color: black;
			color: White;
			margin: 0 auto;
			padding: 10px 0 10px 0;
			text-align: center;
			-moz-border-radius: 10px 10px 0px 0px;
			-webkit-border-radius: 10px 10px 0px 0px;
			border-radius: 10px 10px 0px 0px;
		}
	</style>
</head>
<body>
<form id="frmCookieMan" runat="server" dir="<%=Resources.Languages.TextDirection%>">
<div class="header">
<div id="logo">
<h1><a asproxydone="2" href=".">ASProxy</a><span class="super"><%=Consts.General.ASProxyVersion %></span></h1>
<h2><asp:Literal id="lblLogo2" enableviewstate="False" runat="server" 
meta:resourcekey="lblLogo2" Text="Surf the web with"></asp:Literal></h2>
</div></div><div id="menu-wrap"><div id="menu"><ul><li><a asproxydone="2" href="." accesskey="1">
<asp:Literal ID="mnuHome" EnableViewState="False" runat="server" meta:resourcekey="mnuHome" Text="Home"></asp:Literal>
</a></li><li class="first"><a asproxydone="2" href="cookieman.aspx" accesskey="2"><asp:Literal ID="mnuCookie" EnableViewState="False" runat="server" meta:resourcekey="mnuCookie"
Text="Cookie Manager"></asp:Literal></a></li><li><a asproxydone="2" href="download.aspx" accesskey="3">
<asp:Literal ID="mnuDownload" EnableViewState="False" runat="server" meta:resourcekey="mnuDownload" Text="Download Tool"></asp:Literal>
</a></li><li><a asproxydone="2" href="surf.aspx?dec=1&amp;url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank" accesskey="4">
<asp:Literal ID="mnuASProxy" EnableViewState="False" runat="server" meta:resourcekey="mnuASProxy" Text="ASProxy Page"></asp:Literal>
</a></li></ul></div></div><div id="content"><div class="content"><div class="about"><h1 class="title"><asp:Literal ID="lblMainTitle" EnableViewState="False" runat="server" 
meta:resourcekey="lblMainTitle" Text="Cookie Manager"></asp:Literal></h1><asp:Literal ID="lblMainDesc" EnableViewState="False" runat="server" 
meta:resourcekey="lblMainDesc" Text="Warning, if the number of ASProxy cookies increases, ASProxy may not be able to load,&lt;br /&gt;In this case remove the cookies."></asp:Literal>
</div><asp:Repeater ID="rptCookies" runat="server"><HeaderTemplate><table cellpadding="0" cellspacing="0" class="tblOptions">
<tr class="option"><th style="width: 100px"><asp:Literal ID="lblHeaderOptions" EnableViewState="False" runat="server" 
meta:resourcekey="lblHeaderOptions" Text="Options"></asp:Literal></th>
<th colspan="4" dir="<%=Resources.Languages.TextDirection%>" style="text-align: center;">
<asp:Literal ID="lblHeaderDetails" EnableViewState="False" runat="server" 
meta:resourcekey="lblHeaderDetails" Text="Details"></asp:Literal>
</th></tr><tr class="option"><td>&nbsp;</td><td>
<asp:Literal ID="tdName" EnableViewState="False" runat="server" meta:resourcekey="tdName" Text="Name"></asp:Literal>
</td><td><asp:Literal ID="tdValue" EnableViewState="False" runat="server" meta:resourcekey="tdValue" Text="Value"></asp:Literal>
</td></tr></HeaderTemplate><FooterTemplate>
<tr class="option"><td colspan="4" align="left"><asp:Button CssClass="button" ID="btnClearCookies" runat="server" OnClick="rptCookies_btnClearCookies" CommandName="ClearCookies" Text="Delete All" meta:resourcekey="btnClearCookies" />
</td></tr></table></FooterTemplate>
<ItemTemplate><tr class="option">
<td><asp:Button CssClass="button" ID="btnDelete" runat="server" OnClick="rptCookies_btnDelete" CommandName="DeleteCookie" CommandArgument="<%# Container.DataItem.ToString() %>" Text="Delete" meta:resourcekey="btnDelete" />
</td>
<td class="name"><%#Request.Cookies[Container.DataItem.ToString()].Name%></td>
<td class="desc"></td></tr><tr class="option"><td></td><td class="desc2" colspan="2"><div class="cookieValue"><%#Request.Cookies[Container.DataItem.ToString()].Value%></div>
</td></tr></ItemTemplate>
</asp:Repeater>
</div><div class="cookies"></div></div><div id="links"><p>
<a asproxydone="2" href="cookieman.aspx" target="_blank"><asp:Literal ID="mnuFootCookie" EnableViewState="False" runat="server" meta:resourcekey="mnuFootCookie" Text="Cookie Manager"></asp:Literal></a>
<a asproxydone="2" href="download.aspx" target="_blank"><asp:Literal ID="mnuFootDownload" EnableViewState="False" runat="server" meta:resourcekey="mnuFootDownload" Text="Download Tool"></asp:Literal></a>
<asp:Literal ID="lblFootSlogan1" EnableViewState="False" runat="server" meta:resourcekey="lblFootSlogan1" Text="Have your own"></asp:Literal>
<a asproxydone="2" href="surf.aspx?dec=1&url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank">ASProxy</a>.
<asp:Literal ID="lblFootSlogan2" EnableViewState="False" runat="server" meta:resourcekey="lblFootSlogan2" Text="It's free."></asp:Literal>
</p></div><div id="footer"><p>@2009 ASProxy<%=Consts.General.ASProxyVersion %>: powered by SalarSoft</p></div>
</form></body></html>
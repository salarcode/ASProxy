<%@ Page Language="C#" %>

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

	protected void rptCookies_ItemDataBound__(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
		{
			Button btn = (Button)e.Item.FindControl("btnDelete");
			btn.CommandArgument = e.Item.DataItem.ToString();
		}
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
<head runat="server">
<title>[PageTitle]</title>
<link href="theme/default/style.css" rel="stylesheet" type="text/css" />
<style type="text/css">
.about h1{margin-top: 0px;padding-top: 5px;}
.desc{overflow:auto;}
</style>
</head>
<body>
<form id="frmCookieMan" runat="server" dir="[Direction]">
<div class="header">
<div id="logo">
<h1><a asproxydone="2" href=".">ASProxy</a><span class="super"><%=Consts.General.ASProxyVersion %></span></h1>
<h2>Surf the web with</h2></div>
</div><div id="menu-wrap"><div id="menu"><ul><li><a asproxydone="2" href="." accesskey="1">Home</a></li>
<li class="first"><a asproxydone="2" href="cookieman.aspx" accesskey="2">Cookie Manager</a></li>
<li><a asproxydone="2" href="download.aspx" accesskey="3">Download Tool</a></li>
<li><a asproxydone="2" href="surf.aspx?dec=1&amp;url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank" accesskey="4">ASProxy Page</a></li>
</ul></div></div><div id="content"><div class="content"><div class="about"><h1 class="title">Cookie Manager</h1>
<div class="entry">Warning, if the number of ASProxy cookies increases, ASProxy may not be able to load,<br />
In this case remove the cookies.</div></div></div>
<div class="cookies"><asp:Repeater ID="rptCookies" runat="server"><HeaderTemplate>
<table cellpadding="0" cellspacing="0" class="tblOptions" style="width: 100px;">
<tr class="option"><th style="width: 50px">[Options]</th><th colspan="4" dir="[Direction]" style="text-align: center;">[Details]</th></tr><tr class="option"><td>&nbsp;</td><td>Name</td><td>Value</td></tr>
</HeaderTemplate><FooterTemplate>
<tr class="option">
<th colspan="4" align="left">
<asp:Button CssClass="button" ID="btnClearCookies" runat="server" OnClick="rptCookies_btnClearCookies" CommandName="ClearCookies" Text="[DeleteAllCookies]" />
</th></tr></table></FooterTemplate><ItemTemplate>
<tr class="option"><td>
<asp:Button CssClass="button" ID="btnDelete" runat="server" OnClick="rptCookies_btnDelete" CommandName="DeleteCookie"
CommandArgument="<%#Container.DataItem.ToString() %>" Text="[Delete]" />
</td><td class="name" style="word-break: break-all;word-wrap:break-word;"><%#Request.Cookies[Container.DataItem.ToString()].Name%>
</td><td class="desc" style="word-break: break-all;word-wrap:break-word;"><%#Request.Cookies[Container.DataItem.ToString()].Value%>
</td></tr></ItemTemplate></asp:Repeater></div>
</div><div id="links"><p><a asproxydone="2" href="cookieman.aspx" target="_blank">Cookie Manager</a> <a asproxydone="2"
href="download.aspx" target="_blank">Download Tool</a> Have your own <a asproxydone="2"
href="surf.aspx?dec=1&url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank">ASProxy</a>. It's free.
</p></div><div id="footer"><p>
@2009 ASProxy <%=Consts.General.ASProxyVersion %>: powered by SalarSoft</p>
</div></form></body></html>
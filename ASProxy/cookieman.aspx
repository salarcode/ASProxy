<%@ Page Title="Manage stored cookies by ASProxy in your PC" Inherits="SalarSoft.ASProxy.PageInMasterLocale" Language="C#" MasterPageFile="~/Theme.master" culture="auto" meta:resourcekey="Page" uiculture="auto" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
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
				cookie.Name != Consts.FrontEndPresentation.HttpCompressor)
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

<asp:Content ContentPlaceHolderID="plhHeadMeta" Runat="Server">
<title>Manage stored cookies by ASProxy in your PC</title>
	<style type="text/css">
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
			margin-top: 10px;
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
</asp:Content>


<asp:Content ContentPlaceHolderID="plhMainBar" Runat="Server">
<script type="text/javascript">	document.getElementById('mnuCookie').className = 'first';</script>
</asp:Content>


<asp:Content ContentPlaceHolderID="plhMainTitle" Runat="Server">
<asp:Literal ID="lblMainTitle" EnableViewState="False" runat="server" meta:resourcekey="lblMainTitle"
Text="Cookie Manager"></asp:Literal></asp:Content>


<asp:Content ContentPlaceHolderID="plhContent" Runat="Server">
<asp:Literal ID="lblMainDesc" EnableViewState="False" runat="server" meta:resourcekey="lblMainDesc"
Text="Warning, if the number of ASProxy cookies increases, ASProxy may not be able to load,&lt;br /&gt;In this case remove the cookies."></asp:Literal>
<asp:Repeater ID="rptCookies" runat="server">
<HeaderTemplate>
<table cellpadding="0" cellspacing="0" class="tblOptions">
	<tr class="option">
		<th style="width: 100px">
			<asp:Literal ID="lblHeaderOptions" EnableViewState="False" runat="server" meta:resourcekey="lblHeaderOptions"
				Text="Options"></asp:Literal>
		</th>
		<th colspan="4" dir="" style="text-align: center;">
			<asp:Literal ID="lblHeaderDetails" EnableViewState="False" runat="server" meta:resourcekey="lblHeaderDetails"
				Text="Details"></asp:Literal>
		</th>
	</tr>
	<tr class="option">
		<td>
			&nbsp;
		</td>
		<td>
			<asp:Literal ID="tdName" EnableViewState="False" runat="server" meta:resourcekey="tdName"
				Text="Name"></asp:Literal>
		</td>
		<td>
			<asp:Literal ID="tdValue" EnableViewState="False" runat="server" meta:resourcekey="tdValue"
				Text="Value"></asp:Literal>
		</td>
	</tr>
</HeaderTemplate>
<FooterTemplate>
<tr class="option">
	<td colspan="4" align="left">
		<asp:Button CssClass="button" ID="btnClearCookies" runat="server" OnClick="rptCookies_btnClearCookies"
			CommandName="ClearCookies" Text="Delete All" meta:resourcekey="btnClearCookies" />
	</td>
</tr>
</table></FooterTemplate>
<ItemTemplate>
<tr class="option">
	<td>
		<asp:Button CssClass="button" ID="btnDelete" runat="server" OnClick="rptCookies_btnDelete"
			CommandName="DeleteCookie" CommandArgument="<%# Container.DataItem.ToString() %>"
			Text="Delete" meta:resourcekey="btnDelete" />
	</td>
	<td class="name">
		<%#Request.Cookies[Container.DataItem.ToString()].Name%>
	</td>
	<td class="desc">
	</td>
</tr>
<tr class="option">
	<td>
	</td>
	<td class="desc2" colspan="2">
		<div class="cookieValue"><%#HttpUtility.HtmlEncode(HttpUtility.UrlDecode(Request.Cookies[Container.DataItem.ToString()].Value)).Replace("&amp; Name=","&amp;<br /> Name=")%></div>
	</td>
</tr>
</ItemTemplate>
</asp:Repeater>
</asp:Content>


<asp:Content ContentPlaceHolderID="plhOptionsTitle" Runat="Server">
Stored cookies
</asp:Content>



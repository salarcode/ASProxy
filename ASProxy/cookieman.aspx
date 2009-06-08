<%@ Page Language="C#" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
	
	private HttpCookieCollection GetCookiesCollection()
	{
		string authenticationCookie = "ASProxyAuthentication";
		string sessionStateCookie = "ASProxySessionID";
        
		HttpCookieCollection result = new HttpCookieCollection();
		for (int i = 0; i < Request.Cookies.Count;i++)
		{
			HttpCookie cookie = Request.Cookies[i];
            if (cookie.Name != sessionStateCookie &&
                cookie.Name != authenticationCookie &&
                cookie.Name != Consts.FrontEndPresentation.CookieMasterName &&
                cookie.Name != Consts.FrontEndPresentation.HttpCompressorCookieMasterName)
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

	protected void rptCookies_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
		{
			Button btn =(Button) e.Item.FindControl("btnDelete");
			btn.CommandArgument = e.Item.DataItem.ToString();
		}
	}

	protected void rptCookies_btnDelete(object source, EventArgs e)
	{
		Button btn = (Button)source;
		string cookieName = btn.CommandArgument;
		
		HttpCookie cookie = Response.Cookies[cookieName];
		cookie.Expires = DateTime.Now.AddYears(-10);
		
		Response.Cookies.Add(cookie);
		Response.Redirect(Request.Url.ToString(), false);
	}
</script>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
<title>[PageTitle]</title>
<style type="text/css">
Body{ font-family:Tahoma; font-size:10pt;}
Input{ border:solid 1px gray;font-size:8pt;}
.Cookies_Table{ border:solid 2px black;}
.Cookies_Table td{ border:solid 1px silver;}
.Cookies_FirstCell{width:30px;text-align:center;}
.Cookies_HederRow{background-color:WhiteSmoke; font-weight:bold;}
</style>
</head>
<body>
<form id="frmCookieMan" runat="server" dir="[Direction]">
<table class="Cookies_Table" style="width: 100%; background-color: WhiteSmoke; ">
<tr>
	<td class="asproxy_td" style="width: 130px;">
		[VersionFamily]</td>
	<td style="text-align: center; font-weight: bold;">
		[PageHeader]</td>
	<td class="asproxy_td" style="width: 130px; font-size: 10pt;">
		powered by SalarSoft</td>
</tr>
</table>
<p style="text-align:center"><a href="default.aspx">[ReturnHome]</a></p>
<p style="text-align: center">[Description]</p>

<asp:Repeater ID="rptCookies" runat="server" OnItemDataBound="rptCookies_ItemDataBound">
<HeaderTemplate>
<table class="Cookies_Table" style="width: 100%;">
<tr class="Cookies_HederRow">
<th class="Cookies_FirstCell">[Row]</th>
<th style="width:50px">[Options]</th>
<th colspan="4" dir="[Direction]" style="text-align:center;">[Details]</th>
</tr>
<tr><td>&nbsp;</td><td>&nbsp;</td><td>Name</td><td>Path</td><td>Expires</td><td>Value</td></tr>
</HeaderTemplate>

<ItemTemplate>
<tr>
	<td class="Cookies_FirstCell"><%#Container.ItemIndex+1%></td>
	<td><asp:Button ID="btnDelete" runat="server" OnClick="rptCookies_btnDelete" CommandName="DeleteCookie" Text="[Delete]" /></td>
	<td style="word-break:break-all;"><%#Request.Cookies[Container.DataItem.ToString()].Name%></td>
	<td><%#Request.Cookies[Container.DataItem.ToString()].Path%></td>
	<td><%#Request.Cookies[Container.DataItem.ToString()].Expires%></td>
	<td style="word-break:break-all;"><%#Request.Cookies[Container.DataItem.ToString()].Value%></td>
</tr>
</ItemTemplate>
<FooterTemplate></table></FooterTemplate>
</asp:Repeater>
</form>
</body></html>
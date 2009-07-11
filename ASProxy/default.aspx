<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<%@ Page Language="C#" %>

<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>

<script runat="server">
UserOptions _userOptions;
void ReadFromUserOptions(UserOptions opt)
{
	chkRemoveScripts.Checked = opt.RemoveScripts;
	chkProcessLinks.Checked = opt.Links;
	chkDisplayImages.Checked = opt.Images;
	chkForms.Checked = opt.SubmitForms;
	chkCompression.Checked = opt.HttpCompression;
	chkCookies.Checked = opt.Cookies;
	chkOrginalUrl.Checked = opt.OrginalUrl;
	chkFrames.Checked = opt.Frames;
	chkPageTitle.Checked = opt.PageTitle;
	chkUTF8.Checked = opt.ForceEncoding;
	chkTempCookies.Checked = opt.TempCookies;
}
UserOptions ApplyToUserOptions(UserOptions defaultOptions)
{
	UserOptions opt;
	opt = defaultOptions;
	opt.RemoveScripts = chkRemoveScripts.Checked;
	opt.Links = chkProcessLinks.Checked;
	opt.Images = chkDisplayImages.Checked;
	opt.SubmitForms = chkForms.Checked;
	opt.HttpCompression = chkCompression.Checked;
	opt.Cookies = chkCookies.Checked;
	opt.OrginalUrl = chkOrginalUrl.Checked;
	opt.Frames = chkFrames.Checked;
	opt.PageTitle = chkPageTitle.Checked;
	opt.ForceEncoding = chkUTF8.Checked;
	opt.TempCookies = chkTempCookies.Checked;
	return opt;
}
protected void btnDisplay_Click(object sender, EventArgs e)
{
	txtUrl.Text = UrlProvider.CorrectInputUrl(txtUrl.Text);

	_userOptions = ApplyToUserOptions(_userOptions);
	_userOptions.SaveToResponse();

	// The default page to surf
	Consts.FilesConsts.PageDefault_Dynamic = "surf.aspx";

	string surfUrl = UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageDefault_Dynamic, txtUrl.Text, _userOptions.EncodeUrl);
	Response.Redirect(surfUrl, true);
}

protected void Page_PreRender(object sender, EventArgs e)
{
	ReadFromUserOptions(_userOptions);
}

protected void Page_Load(object sender, EventArgs e)
{
	_userOptions = UserOptions.ReadFromRequest();
	if (UrlProvider.IsASProxyAddressUrlIncluded(Request.QueryString))
	{
		string queries = Request.Url.Query;
		string surfUrl = UrlBuilder.AppendAntoherQueries(Consts.FilesConsts.PageDefault_Dynamic, queries);
		Response.Redirect(surfUrl, true);
	}
}
</script>

<html><head runat="server">
<title runat="server">Surf the web with ASProxy</title>
<meta content='Surf the web invisibly using ASProxy power. A Powerfull web proxy is in your hands.' name='description' />
<meta content='ASProxy,free,anonymous proxy,anonymous,proxy,asp.net,surfing,filter,antifilter,anti filter' name='keywords' />
<link href="theme/default/style.css" rel="stylesheet" type="text/css" />
<link rel='shortcut icon' href='theme/default/favicon.ico' />
</head><body>
<form id="frmASProxyDefault" runat="server" asproxydone="2">

<script language="javascript" type="text/javascript">
function toggleOpt(){var optBlock=document.getElementById('tblOptions');
if (optBlock.style.display=='none'){optBlock.style.display='';}else{optBlock.style.display='none';}
}</script><div class="header"><div id="logo">
<h1><a asproxydone="2" href=".">ASProxy</a><span class="super"><%=Consts.General.ASProxyVersion %></span></h1>
<h2>Surf the web with</h2></div></div><div id="menu-wrap"><div id="menu">
<ul><li class="first"><a asproxydone="2" href="." accesskey="1">Home</a></li>
<li><a asproxydone="2" href="cookieman.aspx" accesskey="2">Cookie Manager</a></li>
<li><a asproxydone="2" href="download.aspx" accesskey="3">Download Tool</a></li>
<li><a asproxydone="2" href="surf.aspx?dec=1&amp;url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank" accesskey="4">ASProxy Page</a></li>
</ul></div></div>
<div id="content"><div class="content"><div class="urlBar">Enter URL:<br />
<asp:TextBox ID="txtUrl" CssClass="urlText" onkeydown="_Page_HandleTextKey(event)" runat="server" Columns="70" dir="ltr" Width="550px"></asp:TextBox>
<asp:Button CssClass="button" ID="btnASProxyDisplayButton" runat="server" OnClick="btnDisplay_Click" Text="Display" />
</div><div class="about">
<h1 class="title">What is ASProxy?</h1>
<div class="entry">ASProxy is a service which allows the user to surf the net anonymously. When using
ASProxy, not only is your identity hidden but you will be able to escape filters
and firewalls from an internet connection.
<br />In most cases your job, school, or even your country may prevent you from accessing
your favorite websites. ASProxy will circumvent this.<br />
The purpose of ASProxy is spreading freedom on the net, but this proxy can be used
for any purposes. ASProxy is not responsible for your activities.
</div></div></div></div>
<div id="options"><div class="options"><h2><a href="javascript:void(0);" onclick="toggleOpt()">Options</a></h2>
<table id="tblOptions" class="tblOptions" cellpadding="0" cellspacing="0">
<tr class="option"><td class="name"><asp:CheckBox ID="chkDisplayImages" runat="server" Checked="True" Text="Images" />
</td><td class="desc">Displays images.</td></tr><tr class="option">
<td class="name"><asp:CheckBox ID="chkCompression" runat="server" Text="Compress response" Checked="False" />
</td><td class="desc">Compresses the responded page.<br />This is a recommended option but it is not compatible with all hosting servers.
</td></tr><tr class="option">
<td class="name"><asp:CheckBox ID="chkRemoveScripts" runat="server" Text="Remove Scripts" Checked="True" />
</td><td class="desc">Removes scripts from page. This option increases anonymity but may loose some functionalities.</td>
</tr><tr class="option"><td class="name">
<asp:CheckBox ID="chkOrginalUrl" runat="server" Checked="True" Text="Original URLs" />
</td><td class="desc">Displays original URL address in a float bar on the top of page.<br />Note that this option can increase page size.<br />
(Tip: To copy the address that the float bar shows press Ctrl+Shift+X keys)
</td></tr><tr class="option">
<td class="name"><asp:CheckBox ID="chkUTF8" runat="server" Checked="False" Text="Force UTF-8" />
</td><td class="desc">Uses UTF-8 encoding for pages.<br />Suitable for non-English sites that contains non-ASCII characters.
</td></tr><tr class="option"><td class="name"><asp:CheckBox ID="chkCookies" runat="server" Text="Cookies" Checked="True" />
</td><td class="desc">Enables cookie support.</td>
</tr><tr class="option"><td class="name"><asp:CheckBox ID="chkTempCookies" runat="server" Text="Save Cookies as Temp" />
</td><td class="desc">Saves cookies only for current session, and they will not be available after browser is closed.
</td></tr><tr class="option"><td class="name"><asp:CheckBox ID="chkProcessLinks" runat="server" Text="Links" Checked="True" />
</td><td class="desc">Processes links and encodes them.</td>
</tr><tr class="option"><td class="name"><asp:CheckBox ID="chkPageTitle" runat="server" Text="Display page title" Checked="True" />
</td><td class="desc">Displays page title in browser title.</td></tr>
<tr class="option"><td class="name"><asp:CheckBox ID="chkForms" runat="server" Text="Process forms" Checked="True" /></td>
<td class="desc">Processes submit forms.</td></tr>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkFrames" runat="server" Text="Frames" Checked="True" />
</td><td class="desc">Enables inline frames.</td></tr>
</table></div></div>

<script language="javascript" type="text/javascript">
_XPage={};
_XPage.UrlBox =document.getElementById('txtUrl');
function _Page_HandleTextKey(ev){
	var IE=false;
	if(window.event) {ev=window.event;IE=true;}
	if(ev.keyCode==13 || ev.keyCode==10){
		var loc=_XPage.UrlBox.value.toLowerCase();
		if(loc.lastIndexOf('.com')== -1 && loc.lastIndexOf('.net')== -1 && loc.lastIndexOf('.org')== -1){
		if(ev.ctrlKey && ev.shiftKey)
			_XPage.UrlBox.value+='.org';
		else if(ev.ctrlKey)
			_XPage.UrlBox.value+='.com';
		else if(ev.shiftKey)
			_XPage.UrlBox.value+='.net';
		}
	}
	return true;
}
</script>

<div id="links"><p><a asproxydone="2" href="cookieman.aspx" target="_blank">Cookie Manager</a>
<a asproxydone="2" href="download.aspx" target="_blank">Download Tool</a> Have your own <a asproxydone="2" href="surf.aspx?dec=1&url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank">ASProxy</a>. It's free.
</p></div><div id="footer"><p>@2009 ASProxy <%=Consts.General.ASProxyVersion %>: powered by SalarSoft</p></div>
</form>
</body></html>
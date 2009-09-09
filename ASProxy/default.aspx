<%@ Page Language="C#" meta:resourcekey="Page" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
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
<html><head>
<title>Surf the web with ASProxy</title>
<meta content='Surf the web invisibly using ASProxy power. A Powerfull web proxy is in your hands.' name='description' />
<meta content='ASProxy,free,anonymous proxy,anonymous,proxy,asp.net,surfing,filter,antifilter,anti filter' name='keywords' />
<link href="theme/default/style-<%=Resources.Languages.TextDirection %>.css" rel="stylesheet" type="text/css" />
<link rel='shortcut icon' href='theme/default/favicon.ico' />
</head><body>
<form id="frmASProxyDefault" runat="server" asproxydone="2" defaultbutton="btnASProxyDisplayButton">
<script language="javascript" type="text/javascript">
function toggleOpt(){var optBlock=document.getElementById('tblOptions');
if (optBlock.style.display=='none'){optBlock.style.display='';}else{optBlock.style.display='none';}
}</script>

<div class="header"><div id="logo"><h1>
<a asproxydone="2" href=".">ASProxy</a><span class="super"><%=Consts.General.ASProxyVersion %></span></h1>
<h2><asp:Literal id="lblLogo2" enableviewstate="False" runat="server" meta:resourcekey="lblLogo2" Text="Surf the web with"></asp:Literal></h2>
</div></div><div id="menu-wrap"><div id="menu"><ul>
<li class="first"><a asproxydone="2" href="." accesskey="1"><asp:Literal ID="mnuHome" EnableViewState="False" runat="server" meta:resourcekey="mnuHome" Text="Home"></asp:Literal></a></li>
<li><a asproxydone="2" href="cookieman.aspx" accesskey="2"><asp:Literal ID="mnuCookie" EnableViewState="False" runat="server" meta:resourcekey="mnuCookie" Text="Cookie Manager"></asp:Literal></a></li>
<li><a asproxydone="2" href="download.aspx" accesskey="3"><asp:Literal ID="mnuDownload" EnableViewState="False" runat="server" meta:resourcekey="mnuDownload" Text="Download Tool"></asp:Literal></a></li>
<li><a asproxydone="2" href="surf.aspx?dec=1&amp;url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank" accesskey="4"><asp:Literal ID="mnuASProxy" EnableViewState="False" runat="server" meta:resourcekey="mnuASProxy"
Text="ASProxy Page"></asp:Literal></a></li></ul></div>
</div><div id="content"><div class="content"><div class="urlBar">
<asp:Literal ID="lblUrl" EnableViewState="False" runat="server" meta:resourcekey="lblUrl" Text="Enter URL:"></asp:Literal>
<br /><asp:TextBox ID="txtUrl" CssClass="urlText" onkeydown="_Page_HandleTextKey(event)" runat="server" Columns="70" dir="ltr" Width="550px" meta:resourcekey="txtUrl"></asp:TextBox>
<asp:Button CssClass="button" ID="btnASProxyDisplayButton" runat="server" OnClick="btnDisplay_Click" Text="Display" meta:resourcekey="btnASProxyDisplayButton" />
</div><div class="about"><h1 class="title">
<asp:Label ID="lblMainTitle" runat="server" EnableViewState="False" meta:resourcekey="lblMainTitle">What is ASProxy?</asp:Label>
</h1><div class="entry">
<asp:Literal ID="ltrMainDesc" runat="server" meta:resourcekey="ltrMainDesc"
Text="ASProxy is an open-source service which allows the user to surf the net anonymously. When using
ASProxy, not only is your identity hidden but you will be able to escape filters
and firewalls from an internet connection.
&lt;br /&gt;In most cases your job, school, or even your country may prevent you from accessing
your favorite websites. ASProxy will circumvent this.&lt;br /&gt;
The purpose of ASProxy is spreading freedom on the net, but this proxy can be used
for any purposes. ASProxy is not responsible for your activities." EnableViewState="False"></asp:Literal>
</div></div></div></div>
<div id="options"><div class="options"><h2>
<a href="javascript:void(0);" onclick="toggleOpt()"><asp:Literal ID="lblOptions" EnableViewState="False" runat="server" meta:resourcekey="lblOptions" Text="Options"></asp:Literal>
</a></h2><table id="tblOptions" class="tblOptions" cellpadding="0" cellspacing="0">

<%if (Configurations.UserOptions.Images.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkDisplayImages" runat="server" Checked="True" Text="Images" meta:resourcekey="chkDisplayImages" />
</td><td class="desc"><asp:Literal ID="lblDisplayImages" runat="server" meta:resourcekey="lblDisplayImages" Text="Displays images."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.HttpCompression.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkCompression" runat="server" Text="Compress response" meta:resourcekey="chkCompression" />
</td><td class="desc"><asp:Literal ID="lblCompression" EnableViewState="False" runat="server" meta:resourcekey="lblCompression" Text="Compresses the responded page.&lt;br /&gt;This is a recommended option but it is not compatible with all hosting servers."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.RemoveScripts.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkRemoveScripts" runat="server" Text="Remove Scripts" Checked="True" meta:resourcekey="chkRemoveScripts" />
</td><td class="desc"><asp:Literal ID="lblRemoveScripts" EnableViewState="False" runat="server" meta:resourcekey="lblRemoveScripts" Text="Removes scripts from page. This option increases anonymity but may loose some functionalities."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.OrginalUrl.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkOrginalUrl" runat="server" Checked="True" Text="Original URLs" meta:resourcekey="chkOrginalUrl" /></td><td class="desc">
<asp:Literal ID="lblOrginalUrl" EnableViewState="False" runat="server" meta:resourcekey="lblOrginalUrl" Text="Displays original URL address in a float bar on the top of page.&lt;br /&gt;Note that this option can increase page size.&lt;br /&gt;(Tip: To copy the address that the float bar shows press Ctrl+Shift+X keys)"></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.EncodeUrl.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkEncodeUrl" runat="server" Checked="True" Text="Encode URLs" meta:resourcekey="chkEncodeUrl" /></td><td class="desc">
<asp:Literal ID="lblEncodeUrl" EnableViewState="False" runat="server" meta:resourcekey="lblEncodeUrl" Text="Encodes original site address and hides it."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.ForceEncoding.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkUTF8" runat="server" Text="Force UTF-8" meta:resourcekey="chkUTF8" /></td><td class="desc">
<asp:Literal ID="lblUTF8" EnableViewState="False" runat="server" meta:resourcekey="lblUTF8" Text="Uses UTF-8 encoding for pages.&lt;br /&gt;Suitable for non-English sites that contains non-ASCII characters."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.Cookies.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkCookies" runat="server" Text="Cookies" Checked="True" meta:resourcekey="chkCookies" /></td><td class="desc">
<asp:Literal ID="lblCookies" EnableViewState="False" runat="server" meta:resourcekey="lblCookies" Text="Enables cookie support."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.TempCookies.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkTempCookies" runat="server" Text="Save Cookies as Temp" meta:resourcekey="chkTempCookies" /></td><td class="desc">
<asp:Literal ID="lblTempCookies" EnableViewState="False" runat="server" meta:resourcekey="lblTempCookies" Text="Saves cookies only for current session, and they will not be available after browser is closed."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.Links.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkProcessLinks" runat="server" Text="Links" Checked="True" meta:resourcekey="chkProcessLinks" /></td><td class="desc">
<asp:Literal ID="lblProcessLinks" EnableViewState="False" runat="server" meta:resourcekey="lblProcessLinks" Text="Processes links and encodes them."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.PageTitle.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkPageTitle" runat="server" Text="Display page title" Checked="True" meta:resourcekey="chkPageTitle" />
</td><td class="desc"><asp:Literal ID="lblPageTitle" EnableViewState="False" runat="server" meta:resourcekey="lblPageTitle" Text="Displays page title in browser title."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.SubmitForms.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkForms" runat="server" Text="Process forms" Checked="True" meta:resourcekey="chkForms" />
</td><td class="desc"><asp:Literal ID="lblForms" EnableViewState="False" runat="server" meta:resourcekey="lblForms" Text="Processes submit forms."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.Frames.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkFrames" runat="server" Text="Frames" Checked="True" meta:resourcekey="chkFrames" />
</td><td class="desc"><asp:Literal ID="lblFrames" EnableViewState="False" runat="server" meta:resourcekey="lblFrames" Text="Enables inline frames."></asp:Literal>
</td></tr>
<%} %>
</table>
</div></div>

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
<div id="links"><p>
<a asproxydone="2" href="cookieman.aspx" target="_blank"><asp:Literal ID="mnuFootCookie" EnableViewState="False" runat="server" meta:resourcekey="mnuFootCookie" Text="Cookie Manager"></asp:Literal></a>
<a asproxydone="2" href="download.aspx" target="_blank"><asp:Literal ID="mnuFootDownload" EnableViewState="False" runat="server" meta:resourcekey="mnuFootDownload" Text="Download Tool"></asp:Literal></a>
<asp:Literal ID="lblFootSlogan1" EnableViewState="False" runat="server" meta:resourcekey="lblFootSlogan1" Text="Have your own"></asp:Literal>
<a asproxydone="2" href="surf.aspx?dec=1&url=aHR0cDovL2FzcHJveHkuc291cmNlZm9yZ2UubmV0B64Coded!" target="_blank">ASProxy</a>
<asp:Literal ID="lblFootSlogan2" EnableViewState="False" runat="server" meta:resourcekey="lblFootSlogan2" Text="It's free."></asp:Literal>
</p></div><div id="footer"><p>@2009 ASProxy <%=Consts.General.ASProxyVersion %>: powered by SalarSoft</p>
</div>
</form></body></html>
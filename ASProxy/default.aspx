<%@ Page Language="C#" MasterPageFile="~/Theme.master" Inherits="SalarSoft.ASProxy.PageInMasterLocale" culture="auto" meta:resourcekey="Page" uiculture="auto" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>

<script runat="server">
	UserOptions _userOptions;
	void ReadFromUserOptions(UserOptions opt)
	{
		chkRemoveScripts.Checked = opt.RemoveScripts;
		chkRemoveObjects.Checked = opt.RemoveObjects;
		chkProcessLinks.Checked = opt.Links;
		chkDisplayImages.Checked = opt.Images;
		chkForms.Checked = opt.SubmitForms;
		chkCompression.Checked = opt.HttpCompression;
		chkImgCompressor.Checked = opt.ImageCompressor;
		chkCookies.Checked = opt.Cookies;
		chkOrginalUrl.Checked = opt.OrginalUrl;
		chkFrames.Checked = opt.Frames;
		chkPageTitle.Checked = opt.PageTitle;
		chkUTF8.Checked = opt.ForceEncoding;
		chkTempCookies.Checked = opt.TempCookies;
	}
	/// <summary>
	/// If we don't set the visible of checkboxes, they will return false for unchangeable options anyway
	/// </summary>
	UserOptions ApplyToUserOptions(UserOptions defaultOptions)
	{
		UserOptions opt = defaultOptions;
		SalarSoft.ASProxy.Configurations.UserOptionsConfig userOptions=Configurations.UserOptions;
		if (userOptions.RemoveScripts.Changeable)
			opt.RemoveScripts = chkRemoveScripts.Checked;
		
		if (userOptions.RemoveObjects.Changeable)
			opt.RemoveObjects = chkRemoveObjects.Checked;

		if (userOptions.Links.Changeable)
			opt.Links = chkProcessLinks.Checked;

		if (userOptions.Images.Changeable)
			opt.Images = chkDisplayImages.Checked;
		
		if (userOptions.SubmitForms.Changeable)
			opt.SubmitForms = chkForms.Checked;

		if (userOptions.HttpCompression.Changeable)
			opt.HttpCompression = chkCompression.Checked;

		if (userOptions.ImageCompressor.Changeable && Configurations.ImageCompressor.Enabled)
			opt.ImageCompressor = chkImgCompressor.Checked;

		if (userOptions.Cookies.Changeable)
			opt.Cookies = chkCookies.Checked;
		
		if (userOptions.OrginalUrl.Changeable)
			opt.OrginalUrl = chkOrginalUrl.Checked;
		
		if (userOptions.Frames.Changeable)
			opt.Frames = chkFrames.Checked;
		
		if (userOptions.PageTitle.Changeable)
			opt.PageTitle = chkPageTitle.Checked;
		
		if (userOptions.ForceEncoding.Changeable)
			opt.ForceEncoding = chkUTF8.Checked;
		
		if (userOptions.TempCookies.Changeable)
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

	protected void Page_Init(object sender, EventArgs e)
	{
		_userOptions = UserOptions.ReadFromRequest();
		ReadFromUserOptions(_userOptions);
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		if (UrlProvider.IsASProxyAddressUrlIncluded(Request.QueryString))
		{
			string queries = Request.Url.Query;
			string surfUrl = UrlBuilder.AppendAntoherQueries(Consts.FilesConsts.PageDefault_Dynamic, queries);
			Response.Redirect(surfUrl, true);
		}
	}
</script>



<asp:Content ContentPlaceHolderID="plhHeadMeta" Runat="Server">
<title>Surf the web with ASProxy</title>
<meta content='Surf the web invisibly using ASProxy power. A Powerful web proxy is in your hands.' name='description' />
<meta content='ASProxy,free,anonymous proxy,anonymous,proxy,asp.net,surfing,filter,antifilter,anti filter' name='keywords' />
<script type="text/javascript">
function toggleOpt() {
	var optBlock = document.getElementById('tblOptions');
	if (optBlock.style.display == 'none') { optBlock.style.display = ''; } else { optBlock.style.display = 'none'; }
}</script>
</asp:Content>



<asp:Content ContentPlaceHolderID="plhMainBar" Runat="Server">
<script type="text/javascript">document.getElementById('mnuHome').className = 'first';</script>
<div class="urlBar">
<asp:Literal ID="lblUrl" EnableViewState="False" runat="server" meta:resourcekey="lblUrl" Text="Enter URL:"></asp:Literal>
<br /><asp:TextBox ID="txtUrl" CssClass="urlText" onkeydown="_Page_HandleTextKey(event)" runat="server" Columns="70" dir="ltr" Width="550px" meta:resourcekey="txtUrl"></asp:TextBox>
<asp:Button CssClass="button" ID="btnDisplay" runat="server" OnClick="btnDisplay_Click" Text="Display" meta:resourcekey="btnASProxyDisplayButton" />
</div>
<script language="javascript" type="text/javascript">
if (typeof(_XPage)=='undefined')
	_XPage={};
_XPage.UrlBox = document.getElementById('<%=txtUrl.ClientID%>');
function _Page_HandleTextKey(ev){
	var IE=false;
	if (window.event) { ev = window.event; IE = true; }
	if (ev.keyCode == 13 || ev.keyCode == 10) {
		var loc = _XPage.UrlBox.value.toLowerCase();
		if (loc.lastIndexOf('.com') == -1 && loc.lastIndexOf('.net') == -1 && loc.lastIndexOf('.org') == -1) {
			if (ev.ctrlKey && ev.shiftKey)
				_XPage.UrlBox.value += '.org';
			else if (ev.ctrlKey)
				_XPage.UrlBox.value += '.com';
			else if (ev.shiftKey)
				_XPage.UrlBox.value += '.net';
		}
	}
	return true;
}
</script>
</asp:Content>



<asp:Content ContentPlaceHolderID="plhMainTitle" Runat="Server">
<asp:Label ID="lblMainTitle" runat="server" EnableViewState="False" meta:resourcekey="lblMainTitle">What is ASProxy?</asp:Label>
</asp:Content>



<asp:Content ContentPlaceHolderID="plhContent" Runat="Server">
<asp:Literal ID="ltrMainDesc" runat="server" meta:resourcekey="ltrMainDesc"
Text="ASProxy is an open-source web proxy service which allows the user to surf the net anonymously. When using
ASProxy, not only is your identity hidden but you will be able to escape filters
and firewalls from an internet connection.
&lt;br /&gt;In most cases your job, school, or even your country may prevent you from accessing
your favorite websites. ASProxy will circumvent this.&lt;br /&gt;
The purpose of ASProxy is spreading freedom on the net, but this proxy can be used
for any purposes. ASProxy is not responsible for your activities." EnableViewState="False"></asp:Literal>
<%--<div id="ads">
<a href="#"><div class="ad_item"><h3>website.com</h3>Best web hosting ever</div></a>
<a href="#"><div class="ad_item"><h3>website.com</h3>Best web hosting ever</div></a>
</div>
--%>
</asp:Content>



<asp:Content ContentPlaceHolderID="plhOptionsTitle" Runat="Server">
<a href="javascript:void(0);" onclick="toggleOpt()"><asp:Literal ID="lblOptions" EnableViewState="False" runat="server" meta:resourcekey="lblOptions" Text="Options"></asp:Literal>
</a></asp:Content>



<asp:Content ContentPlaceHolderID="plhOptions" Runat="Server">
<table id="tblOptions" class="tblOptions" cellpadding="0" cellspacing="0">

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
<%if (Configurations.UserOptions.ImageCompressor.Changeable && Configurations.ImageCompressor.Enabled){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkImgCompressor" runat="server" Text="Compress images" meta:resourcekey="chkImgCompressor" />
</td><td class="desc"><asp:Literal ID="lblImgCompressor" EnableViewState="False" runat="server" meta:resourcekey="lblImgCompressor" Text="Compresses the images.&lt;br /&gt;This option decreases images size by decreasing their quality."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.RemoveScripts.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkRemoveScripts" runat="server" Text="Remove Scripts" Checked="True" meta:resourcekey="chkRemoveScripts" />
</td><td class="desc"><asp:Literal ID="lblRemoveScripts" EnableViewState="False" runat="server" meta:resourcekey="lblRemoveScripts" Text="Removes scripts from page. This option increases anonymity but may loose some functionalities."></asp:Literal>
</td></tr>
<%} %>
<%if (Configurations.UserOptions.RemoveObjects.Changeable){ %>
<tr class="option"><td class="name">
<asp:CheckBox ID="chkRemoveObjects" runat="server" Text="Remove Objects" Checked="True" meta:resourcekey="chkRemoveObjects" />
</td><td class="desc"><asp:Literal ID="lblRemoveObjects" EnableViewState="False" runat="server" meta:resourcekey="lblRemoveObjects" Text="Removes embedded objects from page. Use this option to get rid of flash and embedded media in pages and save bandwidth."></asp:Literal>
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
</asp:Content>


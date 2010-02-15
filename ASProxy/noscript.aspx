<%@ Page Language="C#" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="SalarSoft.ASProxy.Exposed" %>
<%@ Import Namespace="System.Threading" %>
<html>
<script runat="server">
UserOptions _userOptions;

void ReadFromUserOptions(UserOptions opt)
{
	chkRemoveScripts.Checked = opt.RemoveScripts;
	chkDisplayImages.Checked = opt.Images;
	chkCompression.Checked = opt.HttpCompression;
	chkImgCompressor.Checked = opt.ImageCompressor;
	chkCookies.Checked = opt.Cookies;
	chkUTF8.Checked = opt.ForceEncoding;
	chkOrginalUrl.Checked = opt.OrginalUrl;
}

/// <summary>
/// If we don't set the visible of checkboxes, they will return false for unchangeable options anyway
/// </summary>
UserOptions ApplyToUserOptions(UserOptions defaultOptions)
{
	UserOptions opt = defaultOptions;
	SalarSoft.ASProxy.Configurations.UserOptionsConfig userOptions = Configurations.UserOptions;
	if (userOptions.RemoveScripts.Changeable)
		opt.RemoveScripts = chkRemoveScripts.Checked;

	if (userOptions.Images.Changeable)
		opt.Images = chkDisplayImages.Checked;

	if (userOptions.HttpCompression.Changeable)
		opt.HttpCompression = chkCompression.Checked;

	if (userOptions.ImageCompressor.Changeable && Configurations.ImageCompressor.Enabled)
		opt.ImageCompressor = chkImgCompressor.Checked;

	if (userOptions.Cookies.Changeable)
		opt.Cookies = chkCookies.Checked;

	if (userOptions.OrginalUrl.Changeable)
		opt.OrginalUrl = chkOrginalUrl.Checked;

	if (userOptions.ForceEncoding.Changeable)
		opt.ForceEncoding = chkUTF8.Checked;
	return opt;
}


void ApplyFramesetResults(IEngine engine)
{
	if (engine.ResponseInfo.HtmlIsFrameSet)
	{
		string result = "";
		if (WebMethods.IsMethod(engine.RequestInfo.RequestMethod, WebMethods.DefaultMethods.GET))
			result = UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageDirectHtml, engine.RequestInfo.RequestUrl, engine.UserOptions.EncodeUrl);
		else
			result = UrlBuilder.AppendAntoherQueries(Consts.FilesConsts.PageDirectHtml, engine.RequestInfo.PostDataString);
		ltrHtmlBody.Text = HtmlTags.IFrameTag(result, "100%", "600%");
	}
}

void GetResults(IEngine engine)
{
	txtUrl.Text = engine.RequestInfo.RequestUrl;
	try
	{
		MimeContentType pageContentType;
		if (!UrlProvider.IsFTPUrl(engine.RequestInfo.RequestUrl))
		{
			engine.ExecuteHandshake();

			if (engine.LastStatus == LastStatus.Error)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

				lblErrorMsg.Text = engine.LastErrorMessage;
				if (string.IsNullOrEmpty(lblErrorMsg.Text))
					lblErrorMsg.Text = "Unknown error on requesting data";
				lblErrorMsg.Visible = true;
				ltrHtmlBody.Text = "";
				return;
			}
			pageContentType = engine.ResponseInfo.ContentTypeMime;
		}
		else
			pageContentType = MimeContentType.application;

		switch (pageContentType)
		{
			case MimeContentType.application:
				if (WebMethods.IsMethod(engine.RequestInfo.RequestMethod, WebMethods.DefaultMethods.GET))
					Response.Redirect(UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageDownload, engine.RequestInfo.RequestUrl, _userOptions.EncodeUrl), false);
				else
					Response.Redirect(UrlBuilder.AppendAntoherQueries(Consts.FilesConsts.PageDownload, engine.RequestInfo.PostDataString), false);
				return;
			case MimeContentType.image_gif:
			case MimeContentType.image_jpeg:
				ltrHtmlBody.Text = HtmlTags.ImgTag(UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageImages, engine.RequestInfo.RequestUrl, _userOptions.EncodeUrl));
				return;
		}

		ltrBeforeContent.Text = "</form></body></html>\n";
		ltrHtmlBody.Text = "";
		string responseContent;

		// Don't process for these type in default page
		if (pageContentType == MimeContentType.text_css || pageContentType == MimeContentType.text_plain || pageContentType == MimeContentType.text_javascript)
			engine.DataTypeToProcess = DataTypeToProcess.None;


		// apply http compression
		SalarSoft.ASProxy.BuiltIn.HttpCompressor.ApplyCompression(engine.ResponseInfo.ContentTypeMime);
		
		// Execute the request
		responseContent = engine.ExecuteToString();
		
		// Append extra content
		if (!string.IsNullOrEmpty(engine.ResponseInfo.ExtraCodesForBody))
			responseContent = engine.ResponseInfo.ExtraCodesForBody + responseContent;


		// If content is text format
		if (pageContentType == MimeContentType.text_css || pageContentType == MimeContentType.text_plain || pageContentType == MimeContentType.text_javascript)
			ltrHtmlBody.Text += "<pre>" + HttpUtility.HtmlEncode(responseContent) + "</pre>";
		else
			ltrHtmlBody.Text += responseContent;

		// Set returned query
		txtUrl.Text = engine.ResponseInfo.ResponseUrl;

		// Response charset
		Response.ContentEncoding = engine.ResponseInfo.ContentEncoding;
		Response.Charset = engine.ResponseInfo.ContentEncoding.BodyName;

		if (engine.LastStatus == LastStatus.Error)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

			lblErrorMsg.Text = engine.LastErrorMessage;
			lblErrorMsg.Visible = true;
			ltrHtmlBody.Text = "";
			return;
		}
		else if (engine.LastStatus == LastStatus.ContinueWithError)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

			// if has error but can continue
			lblErrorMsg.Text = engine.LastErrorMessage;
			lblErrorMsg.Visible = true;
		}

		if (engine.UserOptions.DocType && !engine.ResponseInfo.HtmlIsFrameSet)
		{
			Response.Write(engine.ResponseInfo.HtmlDocType);
		}
		Response.Write(engine.ResponseInfo.ExtraCodesForPage);

		if (engine.UserOptions.PageTitle)
		{
			Page.Title = engine.ResponseInfo.HtmlPageTitle + " - [" + Consts.General.ASProxyName + "]";
		}
		if (engine.ResponseInfo.HtmlIsFrameSet)
		{
			ApplyFramesetResults(engine);
			return;
		}
	}
	catch (ThreadAbortException) { }
	catch (Exception ex)
	{
		if (Systems.LogSystem.ErrorLogEnabled)
			Systems.LogSystem.LogError(ex, ex.Message, engine.RequestInfo.RequestUrl);

		lblErrorMsg.Text = ex.Message;
		lblErrorMsg.Visible = true;
	}
}

void ProccessRequest()
{
	// Don't apply operation only if this is post back from asproxy "Display" button.
	if (Request.Form[btnASProxyDisplayButton.ID] == null)
	{
		ReadFromUserOptions(_userOptions);

		if (UrlProvider.IsASProxyAddressUrlIncluded(Request.QueryString))
		{
			using (IEngine engine = (IEngine)Providers.GetProvider(ProviderType.IEngine))
			{

				engine.UserOptions = _userOptions;
				engine.DataTypeToProcess = DataTypeToProcess.Html;
				engine.RequestInfo.SetContentType(MimeContentType.text_html);

				engine.Initialize(Request);
				engine.RequestInfo.RequestUrl = UrlProvider.CorrectInputUrl(engine.RequestInfo.RequestUrl);

				GetResults(engine);
			}
		}
	}
}

protected void btnDisplay_Click(object sender, EventArgs e)
{
	if (Configurations.Authentication.Enabled)
	{
		if (!Configurations.Authentication.HasPermission(User.Identity.Name, Configurations.AuthenticationConfig.UserPermission.Pages))
		{
			string _ErrorMessage = "Access denied!";
			_ErrorMessage += "<br />You do not have access to browse pages. Ask site administrator to grant permission.";

			lblErrorMsg.Text = _ErrorMessage;
			lblErrorMsg.Visible = true;
			ltrHtmlBody.Text = "";
			return;
		}
	}
	txtUrl.Text = UrlProvider.CorrectInputUrl(txtUrl.Text);

	_userOptions = ApplyToUserOptions(_userOptions);
	_userOptions.SaveToResponse();
		
	using (IEngine engine = (IEngine)Providers.GetProvider(ProviderType.IEngine))
	{
		engine.RequestInfo.RequestUrl = txtUrl.Text;

		engine.UserOptions = _userOptions;
		engine.DataTypeToProcess = DataTypeToProcess.Html;
		engine.RequestInfo.SetContentType(MimeContentType.text_html);

		engine.RequestInfo.RequestUrl = UrlProvider.CorrectInputUrl(engine.RequestInfo.RequestUrl);
		engine.Initialize(engine.RequestInfo.RequestUrl);

		GetResults(engine);
	}
}
protected void Page_Init(object sender, EventArgs e)
{
	_userOptions = UserOptions.ReadFromRequest();
	ReadFromUserOptions(_userOptions);
}

protected void Page_Load(object sender, EventArgs e)
{
	Consts.FilesConsts.PageDefault_Dynamic = System.IO.Path.GetFileName(Request.Url.AbsolutePath).ToLower();

	if (Configurations.Authentication.Enabled)
	{
		if (!Configurations.Authentication.HasPermission(User.Identity.Name, Configurations.AuthenticationConfig.UserPermission.Pages))
		{
			string _ErrorMessage = "Access denied!";
			_ErrorMessage += "<br />You do not have access to browse pages. Ask site administrator to grant permission.";

			lblErrorMsg.Text = _ErrorMessage;
			lblErrorMsg.Visible = true;
			ltrHtmlBody.Text = "";
			return;
		}
	}

	ProccessRequest();
}
</script>
<head runat="server">
<title runat="server">Surf the web with ASProxy</title>
<!-- Surf the web invisibly using ASProxy power. A Powerfull web proxy is in your hands. -->
<style type="text/css" asproxydone="2">
.ASProxyBlock,.AddressBar,.FastOptions,#MoreOptions{background-color: #f8f8f8;height: auto !important;margin: 0px 2px;padding: 0px;float: inherit;display: inherit;color: black;font: normal normal normal 100% Tahoma;font-family: Tahoma, sans-serif;font-size: 10pt;}
.ASProxyBlock{background-color:white;width: 99.5%;display: block;padding: 1px;margin: 0px;border: 2px solid #000000;height: auto !important;float: none;}
.ASProxyForm{display:block;padding: 0px;margin: 0px;width: auto;height: auto;}
.ASProxyMain{color:black;padding: 2px;margin: 0px;border: 1px solid #C0C0C0;background-color: #f8f8f8;background-image: none;font-weight: normal;font-style: normal;line-height: normal;visibility: visible;table-layout: auto;white-space: normal;word-spacing: normal;float: none;}
.ASProxyMain a,.ASProxyMain a:link,.ASProxyMain a:hover,.ASProxyMain a:visited,.ASProxyMain a:active{font: normal normal normal 100% Tahoma;font-family: Tahoma, sans-serif;color: #000099;text-decoration: underline;}
.AddressBar{margin:3px 1px;}
.AddressBar input{width:auto;height:auto;border: solid 1px silver; font:inherit; color:Black;}
.AddressBar .Button{width:auto;height: 25px;color: black;float: none;width:auto !important;margin:0px;background-color: #ECE9D8;border: outset 2px;vertical-align: bottom;font-size: 10pt;padding: 2px 5px;}
.AddressBar .TextBox{width:auto !important;height: auto;color: black;background-color: #FFFFFF;margin:0px;float: none;font-size: 10pt;border: solid 1px silver;padding: 3px;}
.ASProxyOption{background-color:#f8f8f8;height: auto !important;margin: 0px 1px;padding: 0px;float:none;color: black;font: normal normal normal 100% Tahoma;font-family: Tahoma, sans-serif;font-size: 8pt;display: inline;border-width: 0px;text-align: center;}
.ASProxyOption input{width:auto;height:auto !important;margin:0px;background-color:#F8F8F8;display:inline;border-width: 0px;float: none;}
.ASProxyOption label{width:auto;height:auto !important;margin:0px 2px;padding:0px;vertical-align:baseline;float: none;color: Black;font: normal normal normal 100% Tahoma;display: inline;border-width: 0px;background-color: #F8F8F8;}
</style></head><body>
<script asproxydone="2" type="text/javascript">
var _ASProxyVersion="<%=Consts.General.ASProxyVersion %>";
</script>
<form id="frmSurfNoScript" runat="server" asproxydone="2" class="ASProxyForm">
<div id="ASProxyFormBlock" class="ASProxyBlock" dir="<%=Resources.Languages.TextDirection%>">
<div class="ASProxyMain" style="text-align:<%=Resources.Languages.TextAlign%>"><div class="AddressBar">
<a href="." asproxydone="2" style="font-weight: bold; text-decoration: none">ASProxy <%=Consts.General.ASProxyVersion %></a>
<!--This is ASProxy powered by SalarSoft. -->
<asp:TextBox runat="server" type="text" size="80" id="txtUrl" dir="ltr" style="width: 550px;" class="TextBox"/>
<asp:Button ID="btnASProxyDisplayButton" runat="server" CssClass="Button" OnClick="btnDisplay_Click" Text="Display" />
<a href="cookieman.aspx" target="_blank" asproxydone="2"><%=this.GetLocalResourceObject("CookieManager")%></a>
<a href="download.aspx" target="_blank" asproxydone="2"><%=this.GetLocalResourceObject("DownloadTool")%></a>
</div>
<div class="FastOptions">
<%if (Configurations.UserOptions.ForceEncoding.Changeable){ %><span class="ASProxyOption"><asp:CheckBox runat="server" id="chkUTF8"  /><label for="chkUTF8"><%=this.GetLocalResourceObject("chkUTF8")%></label></span> <%} %>
<%if (Configurations.UserOptions.OrginalUrl.Changeable){ %><span class="ASProxyOption"><asp:CheckBox runat="server" id="chkOrginalUrl" /><label for="chkOrginalUrl"><%=this.GetLocalResourceObject("chkOrginalUrl")%></label></span> <%} %>
<%if (Configurations.UserOptions.RemoveScripts.Changeable){ %><span class="ASProxyOption"><asp:CheckBox runat="server" id="chkRemoveScripts"  /><label for="chkRemoveScripts"><%=this.GetLocalResourceObject("chkRemoveScripts")%></label></span> <%} %>
<%if (Configurations.UserOptions.Images.Changeable){ %><span class="ASProxyOption"><asp:CheckBox runat="server" id="chkDisplayImages" /><label for="chkDisplayImages"><%=this.GetLocalResourceObject("chkDisplayImages")%></label></span> <%} %>
<%if (Configurations.UserOptions.Cookies.Changeable){ %><span class="ASProxyOption"><asp:CheckBox runat="server" id="chkCookies" /><label for="chkCookies"><%=this.GetLocalResourceObject("chkCookies")%></label></span> <%} %>
<%if (Configurations.UserOptions.HttpCompression.Changeable){ %><span class="ASProxyOption"><asp:CheckBox runat="server" id="chkCompression" /><label for="chkCompression"><%=this.GetLocalResourceObject("chkCompression")%></label></span> <%} %>
<%if (Configurations.UserOptions.ImageCompressor.Changeable&&Configurations.ImageCompressor.Enabled){ %><span class="ASProxyOption"><asp:CheckBox id="chkImgCompressor" runat="server"/><label for="chkImgCompressor"><%=this.GetLocalResourceObject("chkImgCompressor")%></label></span> <%} %>
</div></div>
<script type="text/javascript" asproxydone="2">
// iframes, hide the form
if(window.self != window.top)document.getElementById('ASProxyFormBlock').style.display='none';
</script>
<div title="Error message" style="text-align: center; color: Red; font-weight: bold;font-family: Tahoma; font-size: 10pt;">
<asp:Label ID="lblErrorMsg" runat="server" EnableTheming="False" EnableViewState="False" Visible="False"></asp:Label>
</div>
</div>

<asp:Literal ID="ltrBeforeContent" runat="server" EnableViewState="false"></asp:Literal>
<div style="position: relative; left: 0px; top: 5px; width: 100%; height: auto;">
<asp:Literal ID="ltrHtmlBody" runat="server" EnableViewState="false"></asp:Literal>
</div>

</form>
</body></html>
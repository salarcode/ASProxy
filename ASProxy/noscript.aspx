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
		if (!Common.IsFTPUrl(engine.RequestInfo.RequestUrl))
		{
			engine.ExecuteHandshake();

			if (engine.LastStatus == LastStatus.Error)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

				lblErrorMsg.Text = engine.LastErrorMessage;
				if (string.IsNullOrEmpty(lblErrorMsg.Text))
					lblErrorMsg.Text = "Unknown error on requesting data";
				lblErrorMsg.Visible = true;
				ltrHtmlBody.Text = "";
				return;
			}
			pageContentType = Common.StringToContentType(engine.ResponseInfo.ContentType);
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

		// Execute the request
		responseContent = engine.ExecuteToString();


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
				Systems.LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

			lblErrorMsg.Text = engine.LastErrorMessage;
			lblErrorMsg.Visible = true;
			ltrHtmlBody.Text = "";
			return;
		}
		else if (engine.LastStatus == LastStatus.ContinueWithError)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

			// if has error but can continue
			lblErrorMsg.Text = engine.LastErrorMessage;
			lblErrorMsg.Visible = true;
		}

		if (engine.UserOptions.DocType)
		{
			Response.Write(engine.ResponseInfo.HtmlDocType);
		}
		Response.Write(engine.ResponseInfo.HtmlInitilizerCodes);

		if (engine.UserOptions.PageTitle)
		{
			Page.Title = Consts.General.ASProxyName + ": " + engine.ResponseInfo.HtmlPageTitle;
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
			Systems.LogSystem.LogError(ex, engine.RequestInfo.RequestUrl, ex.Message);

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
			using (IEngine engine = (IEngine)Provider.CreateProviderInstance(ProviderType.IEngine))
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
	txtUrl.Text = UrlProvider.CorrectInputUrl(txtUrl.Text);

	_userOptions = ApplyToUserOptions(_userOptions);
	_userOptions.SaveToResponse();
		
	using (IEngine engine = (IEngine)Provider.CreateProviderInstance(ProviderType.IEngine))
	{
		engine.RequestInfo.RequestUrl = txtUrl.Text;
			
		engine.UserOptions = _userOptions;
		engine.DataTypeToProcess = DataTypeToProcess.Html;
		engine.RequestInfo.SetContentType(MimeContentType.text_html);

		engine.Initialize(Request);
		engine.RequestInfo.RequestUrl = UrlProvider.CorrectInputUrl(engine.RequestInfo.RequestUrl);

		GetResults(engine);
	}
}

protected void Page_Load(object sender, EventArgs e)
{
	Consts.FilesConsts.PageDefault_Dynamic = System.IO.Path.GetFileName(Request.Url.AbsolutePath).ToLower();
	_userOptions = UserOptions.ReadFromRequest();

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
<title runat="server">[PageTitle]</title>
<!-- Surf the web invisibly using ASProxy power. A Powerfull web proxy is in your hands. -->
<style type="text/css">
#ASProxyMain{width:99.5%;display:block;padding:1px;margin:0px;border:2px solid #000000;text-align: center;}
#ASProxyMain table{margin:0; padding:0px;}
#ASProxyMainTable{color:black;padding:0px;margin:0px;border:1px solid #C0C0C0;background-color:#f8f8f8;background-image:none;font-weight:normal;font-style:normal;line-height:normal;visibility:visible;table-layout:auto;white-space:normal;word-spacing:normal;}
#ASProxyMainTable td{margin:0; padding:0px; border-width:0px;color:black;font-family: Tahoma, sans-serif;font-size: 8pt;text-align: center;float:none;background-color:#f8f8f8;}
#ASProxyMainTable .Button{background-color: #ECE9D8;border:2px outset;float:none;}
#ASProxyMainTable .Sides{width: 140px;}
#ASProxyMainTable a,#ASProxyMainTable a:hover,#ASProxyMainTable a:visited,#ASProxyMainTable a:active{font:normal normal normal 100% Tahoma;font-family: Tahoma, sans-serif; color: #000099; text-decoration:underline;}
#ASProxyMainTable input{width:auto !important; font-size: 10pt;border:solid 1px silver;background-color: #FFFFFF;margin:1px 2px 1px 2px;}
#ASProxyMainTable label{color:Black;font:normal normal normal 100% Tahoma;}
#ASProxyMainTable span input{display:inline;border-width: 0px;float:none;height:auto !important;}
#ASProxyMainTable span label{display:inline;float:none;height:auto !important;}
.ASProxyCheckBox{display:inline;border-width:0px;background-color:#F8F8F8;padding:0px;margin:0px 0px 0px 0px;float:none;height:auto !important;}
</style></head><body>
<script language="javascript" type="text/javascript">
var _ASProxyVersion="<%=Consts.General.ASProxyVersion %>";
function toggleOpt(lnk){var trMoreOpt=document.getElementById('trMoreOpt'); if (trMoreOpt.style.display=='none'){trMoreOpt.style.display='';lnk.innerHTML='[lnkMoreOpt]...<small>&lt;</small>';
}else{trMoreOpt.style.display='none';lnk.innerHTML='[lnkMoreOpt]...<small>&gt;</small>';}}
</script>
<form id="frmASProxyDefault" runat="server" asproxydone="2" style="height:auto; margin-bottom:0px;">
<div id="ASProxyMain" dir="[Direction]">
<table id="ASProxyMainTable" style="width: 100%; ">
<tr><td style="padding:0px; margin:0px;"><table style="width: 100%;border-width:0px;" cellpadding="0" cellspacing="0">
<tr><td class="Sides"><a href="." asproxydone="2">ASProxy <%=Consts.General.ASProxyVersion %></a></td><td style="font-size:small;"><strong>[PageHeader]</strong></td><td class="Sides">powered by SalarSoft</td></tr>
</table></td></tr><tr><td><!--This is ASProxy powered by SalarSoft. --><asp:TextBox ID="txtUrl" runat="server" Columns="60" dir="ltr" Width="450px"></asp:TextBox><asp:Button ID="btnASProxyDisplayButton" runat="server" Style="height: 22px" CssClass="Button" OnClick="btnDisplay_Click" Text="[btnDisplay]" />&nbsp;<br />
<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkUTF8" runat="server" Checked="False" Text="[chkUTF8]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkOrginalUrl" runat="server" Checked="True" Text="[chkOrginalUrl]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkRemoveScripts" runat="server" Text="[chkRemoveScripts]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkDisplayImages" runat="server" Checked="True" Text="[chkDisplayImages]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkCookies" runat="server" Checked="True" Text="[chkCookies]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkCompression" runat="server" Checked="False" Text="[chkCompression]" />&nbsp;<a asproxydone="2" id="lnkMoreOpt" href="javascript:void(0);" onclick="toggleOpt(this);">[lnkMoreOpt]...<small>&lt;</small></a></td>
</tr><tr id="trMoreOpt" style=""><td id="tdMoreOpt"><asp:CheckBox CssClass="ASProxyCheckBox" ID="chkFrames" runat="server" Checked="True" Text="[chkFrames]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkPageTitle" runat="server" Text="[chkPageTitle]" Checked="True" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkForms" runat="server" Checked="True" Text="[chkForms]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkProcessLinks" runat="server" Checked="True" Text="[chkProcessLinks]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkTempCookies" runat="server" Checked="False" Text="[chkTempCookies]" /></td></tr>
<tr><td><a asproxydone="2" href="cookieman.aspx" target="_blank">[CookieManager]</a>&nbsp;&nbsp;<a asproxydone="2" href="download.aspx" target="_blank">[DownloadTool]</a>&nbsp;&nbsp;[GetFreeVersion]&nbsp;&nbsp;<span id="lblVersionNotifier"></span></td>
</tr></table><asp:Label ID="lblErrorMsg" runat="server" EnableTheming="False" EnableViewState="False" Font-Bold="True" Font-Names="Tahoma" Font-Size="10pt" ForeColor="Red" Text="Error message" ToolTip="Error message" Visible="False"></asp:Label>
</div>
<asp:Literal ID="ltrBeforeContent" runat="server" EnableViewState="false"></asp:Literal>
<div style="position: relative; left: 0px; top: 5px; width: 100%; height:auto;">
<asp:Literal ID="ltrHtmlBody" runat="server" EnableViewState="false"></asp:Literal></div>
</form>
<script type="text/javascript" src="scripts/versionchecker.js"></script>
</body></html>
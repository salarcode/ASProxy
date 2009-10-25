<%@ Page Language="C#" meta:resourcekey="Page" Inherits="SalarSoft.ASProxy.PageInMasterLocale"%>
<%@ Import Namespace="SalarSoft.ASProxy.Exposed" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>
<html>

<script runat="server">
	bool _HasError = true;
	string _ToDisplayUrl = "";
	string _ErrorMessage = "";
	string _ResponseContent = "";
	UserOptions _userOptions;

	protected void Page_Load(object sender, EventArgs e)
	{
		Consts.FilesConsts.PageDefault_Dynamic = System.IO.Path.GetFileName(Request.Url.AbsolutePath).ToLower();
		_userOptions = UserOptions.ReadFromRequest();

		if (Configurations.Authentication.Enabled)
		{
			if (!Configurations.Authentication.HasPermission(User.Identity.Name, Configurations.AuthenticationConfig.UserPermission.Pages))
			{
				_ErrorMessage = "Access denied!";
				_ErrorMessage += "<br />You do not have access to browse pages. Ask site administrator to grant permission.";
				_HasError = true;
				return;
			}
		}

		ProccessRequest();
	}

	void ProccessRequest()
	{
		if (UrlProvider.IsASProxyAddressUrlIncluded(Request.QueryString))
		{
			using (IEngine engine = (IEngine)Provider.GetProvider(ProviderType.IEngine))
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

	void GetResults(IEngine engine)
	{
		_ToDisplayUrl = engine.RequestInfo.RequestUrl;
		try
		{
			MimeContentType pageContentType;
			if (!Common.IsFTPUrl(engine.RequestInfo.RequestUrl))
			{
				engine.ExecuteHandshake();

				if (engine.LastStatus == LastStatus.Error)
				{
					if (Systems.LogSystem.ErrorLogEnabled)
						Systems.LogSystem.LogError(engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

					_ErrorMessage = engine.LastErrorMessage;
					if (string.IsNullOrEmpty(_ErrorMessage))
						_ErrorMessage = "Unknown error on requesting data";
					_HasError = true;
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
					_ResponseContent = HtmlTags.ImgTag(UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageImages, engine.RequestInfo.RequestUrl, _userOptions.EncodeUrl));
					return;
			}

			// Don't process for these type in default page
			if (pageContentType == MimeContentType.text_css || pageContentType == MimeContentType.text_plain || pageContentType == MimeContentType.text_javascript)
				engine.DataTypeToProcess = DataTypeToProcess.None;

			// Execute the request
			_ResponseContent = engine.ExecuteToString();


			// Set response query
			_ToDisplayUrl = engine.ResponseInfo.ResponseUrl;

			// If content is text format
			if (pageContentType == MimeContentType.text_css || pageContentType == MimeContentType.text_plain || pageContentType == MimeContentType.text_javascript)
				_ResponseContent = "<pre>" + HttpUtility.HtmlEncode(_ResponseContent) + "</pre>";

			// Response charset
			Response.ContentEncoding = engine.ResponseInfo.ContentEncoding;
			Response.Charset = engine.ResponseInfo.ContentEncoding.BodyName;

			if (engine.LastStatus == LastStatus.Error)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

				_ErrorMessage = engine.LastErrorMessage;
				_HasError = true;
				_ResponseContent = "";
				return;
			}
			else if (engine.LastStatus == LastStatus.ContinueWithError)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

				// if has error but can continue
				_ErrorMessage = engine.LastErrorMessage;
				_HasError = true;
			}


			if (engine.UserOptions.DocType || !engine.ResponseInfo.HtmlIsFrameSet)
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
				Systems.LogSystem.LogError(ex, ex.Message, engine.RequestInfo.RequestUrl);

			_ErrorMessage = ex.Message;
			_HasError = true;
		}
	}

	void ApplyFramesetResults(IEngine resutls)
	{
		if (resutls.ResponseInfo.HtmlIsFrameSet)
		{
			string result = "";
			if (WebMethods.IsMethod(resutls.RequestInfo.RequestMethod, WebMethods.DefaultMethods.GET))
				result = UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageDirectHtml, resutls.RequestInfo.RequestUrl, true);
			else
				result = UrlBuilder.AppendAntoherQueries(Consts.FilesConsts.PageDirectHtml, resutls.RequestInfo.PostDataString);
			_ResponseContent = HtmlTags.IFrameTag(result, "100%", "600%");
		}
		// quit
		HttpContext.Current.ApplicationInstance.CompleteRequest();
	}
</script>
<head runat="server"><title runat="server">Surf the web with ASProxy</title>
<script src="scripts/base64encoder.js" type="text/javascript"></script>
<!-- Surf the web invisibly using ASProxy power. A Powerfull web proxy is in your hands. -->
<style type="text/css">
.ASProxyBlock, .AddressBar, .FastOptions, #MoreOptions{background-color: #f8f8f8;height: auto !important;margin: 0px 2px;padding: 0px;float: inherit;display: inherit;color: black;font: normal normal normal 100% Tahoma;font-family: Tahoma, sans-serif;font-size: 10pt;}
.ASProxyBlock{background-color: white;width: 99.5%;display: block;padding: 1px;margin: 0px;border: 2px solid #000000;height: auto !important;float: none;}
.ASProxyForm{display: block;padding: 0px;margin: 0px;width: auto;height: auto;}
.ASProxyMain{color: black;padding: 2px;margin: 0px;border: 1px solid #C0C0C0;background-color: #f8f8f8;background-image: none;font-weight: normal;font-style: normal;line-height: normal;visibility: visible;table-layout: auto;white-space: normal;word-spacing: normal;float: none;}
.ASProxyMain a, .ASProxyMain a:hover, .ASProxyMain a:visited, .ASProxyMain a:active{font: normal normal normal 100% Tahoma;font-family: Tahoma, sans-serif;color: #000099;text-decoration: underline;}
.AddressBar{margin: 3px 1px;}
.AddressBar input{width:auto;height:auto;border: solid 1px silver; font:inherit;}
.AddressBar .Button{width:auto;height: 25px;color: black;float: none;width: auto !important;margin: 0px;background-color: #ECE9D8;border: outset 2px;vertical-align: bottom;font-size: 10pt;padding: 2px 5px;}
.AddressBar .TextBox{width: auto !important;height: auto;color: black;background-color: #FFFFFF;margin: 0px;float: none;font-size: 10pt;border: solid 1px silver;padding: 3px;}
.ASProxyOption{background-color: #f8f8f8;height: auto !important;margin: 0px 1px;padding: 0px;float: none;color: black;font: normal normal normal 100% Tahoma;font-family: Tahoma, sans-serif;font-size: 8pt;display: inline;border-width: 0px;text-align: center;}
.ASProxyOption input{width:auto;height: auto !important;margin: 0px;background-color: #F8F8F8;display: inline;border-width: 0px;float: none;}
.ASProxyOption label{width:auto;height: auto !important;margin: 0px 2px;padding: 0px;vertical-align: baseline;float: none;color: Black;font: normal normal normal 100% Tahoma;display: inline;border-width: 0px;background-color: #F8F8F8;}
</style>
</head>
<body>

<script asproxydone="2" type="text/javascript">
var _ASProxyVersion="<%=Consts.General.ASProxyVersion %>";
function toggleOpt(lnk){var trMoreOpt=document.getElementById('MoreOptions'); if (trMoreOpt.style.display=='none'){trMoreOpt.style.display='';lnk.innerHTML='<%=this.GetLocalResourceObject("lnkMoreOptions")%>...<small>&lt;</small>';
}else{trMoreOpt.style.display='none';lnk.innerHTML='<%=this.GetLocalResourceObject("lnkMoreOptions")%>...<small>&gt;</small>';}}
</script>
<script asproxydone="2" src="scripts/surfoptions.js" type="text/javascript"></script>
<script asproxydone="2" type="text/javascript">
var _Page_B64Unknowner = "<%=Consts.Query.Base64Unknowner%>";
var _Page_CookieName = "<%=Consts.FrontEndPresentation.UserOptionsCookieName%>";
var _Page_UrlIsEmpty = '<%=this.GetLocalResourceObject("UrlIsEmpty")%>';
var _Page_XNav = '<%=Consts.FilesConsts.PageDefault_Dynamic%>';
function _Page_SetOptions(){
_XPage.ProcessLinks.checked =<%=_userOptions.Links.ToString().ToLower() %>;
_XPage.DisplayImages.checked =<%=_userOptions.Images.ToString().ToLower() %>;
_XPage.Forms.checked =<%=_userOptions.SubmitForms.ToString().ToLower() %>;
_XPage.Compression.checked =<%=_userOptions.HttpCompression.ToString().ToLower() %>;
_XPage.Cookies.checked =<%=_userOptions.Cookies.ToString().ToLower() %>;
_XPage.TempCookies.checked =<%=_userOptions.TempCookies.ToString().ToLower() %>;
_XPage.OrginalUrl.checked =<%=_userOptions.OrginalUrl.ToString().ToLower() %>;
_XPage.Frames.checked =<%=_userOptions.Frames.ToString().ToLower() %>;
_XPage.PageTitle.checked =<%=_userOptions.PageTitle.ToString().ToLower() %>;
_XPage.UTF8.checked =<%=_userOptions.ForceEncoding.ToString().ToLower() %>;
_XPage.RemoveScripts.checked =<%=_userOptions.RemoveScripts.ToString().ToLower() %>;
_XPage.RemoveObjects.checked =<%=_userOptions.RemoveObjects.ToString().ToLower() %>;
_XPage.EncodeUrl.checked=<%=_userOptions.EncodeUrl.ToString().ToLower() %>;
}
</script>

<form asproxydone="2" onsubmit="return _PageOnSubmit();" method="post" class="ASProxyForm">
<div class="ASProxyBlock" dir="<%=Resources.Languages.TextDirection%>">
<div class="ASProxyMain" style="text-align:<%=Resources.Languages.TextAlign%>"><div class="AddressBar">
<a href="." asproxydone="2" style="font-weight: bold; text-decoration: none">ASProxy <%=Consts.General.ASProxyVersion %></a>
<!--This is ASProxy powered by SalarSoft. -->
<input name="url" type="text" size="80" id="txtUrl" dir="ltr" style="width: 550px;"
class="TextBox" onkeyup="_Page_HandleTextKey(event)" value="<%=_ToDisplayUrl%>" /><input
type="submit" value="<%=this.GetLocalResourceObject("btnDisplay")%>" id="btnASProxyDisplayButton" class="Button" />
<a href="cookieman.aspx" target="_blank" asproxydone="2"><%=this.GetLocalResourceObject("CookieManager")%></a>
<a href="download.aspx" target="_blank" asproxydone="2"><%=this.GetLocalResourceObject("DownloadTool")%></a>
</div>
<div class="FastOptions">
<%if (Configurations.UserOptions.ForceEncoding.Changeable){ %><span class="ASProxyOption"><input id="chkUTF8" type="checkbox" onclick="_Page_SaveOptions()" /><label for="chkUTF8"><%=this.GetLocalResourceObject("chkUTF8")%></label></span> <%} %>
<%if (Configurations.UserOptions.OrginalUrl.Changeable){ %><span class="ASProxyOption"><input id="chkOrginalUrl" type="checkbox" checked="checked" onclick="_Page_SaveOptions()" /><label for="chkOrginalUrl"><%=this.GetLocalResourceObject("chkOrginalUrl")%></label></span> <%} %>
<%if (Configurations.UserOptions.RemoveScripts.Changeable){ %><span class="ASProxyOption"><input id="chkRemoveScripts" type="checkbox" onclick="_Page_SaveOptions()" /><label for="chkRemoveScripts"><%=this.GetLocalResourceObject("chkRemoveScripts")%></label></span> <%} %>
<%if (Configurations.UserOptions.Images.Changeable){ %><span class="ASProxyOption"><input id="chkDisplayImages" type="checkbox" checked="checked" onclick="_Page_SaveOptions()" /><label for="chkDisplayImages"><%=this.GetLocalResourceObject("chkDisplayImages")%></label></span> <%} %>
<%if (Configurations.UserOptions.Cookies.Changeable){ %><span class="ASProxyOption"><input id="chkCookies" type="checkbox" checked="checked" onclick="_Page_SaveOptions()" /><label for="chkCookies"><%=this.GetLocalResourceObject("chkCookies")%></label></span> <%} %>
<%if (Configurations.UserOptions.HttpCompression.Changeable){ %><span class="ASProxyOption"><input id="chkCompression" type="checkbox" onclick="_Page_SaveOptions()" /><label for="chkCompression"><%=this.GetLocalResourceObject("chkCompression")%></label></span> <%} %>
<a asproxydone="2" id="lnkMoreOpt" href="javascript:void(0);" onclick="toggleOpt(this);"><%=this.GetLocalResourceObject("lnkMoreOptions")%>...<small>&gt;</small></a>
</div>
<div id="MoreOptions" style="display: none;">
<%if (Configurations.UserOptions.EncodeUrl.Changeable){ %><span class="ASProxyOption"><input id="chkEncodeUrl" type="checkbox" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkEncodeUrl"><%=this.GetLocalResourceObject("chkEncodeUrl")%></label></span> <%} %>
<%if (Configurations.UserOptions.Frames.Changeable){ %><span class="ASProxyOption"><input id="chkFrames" type="checkbox" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkFrames"><%=this.GetLocalResourceObject("chkFrames")%></label></span> <%} %>
<%if (Configurations.UserOptions.PageTitle.Changeable){ %><span class="ASProxyOption"><input id="chkPageTitle" type="checkbox" checked="checked" onclick="_Page_SaveOptions()" /><label for="chkPageTitle"><%=this.GetLocalResourceObject("chkPageTitle")%></label></span> <%} %>
<%if (Configurations.UserOptions.SubmitForms.Changeable){ %><span class="ASProxyOption"><input id="chkForms" type="checkbox" checked="checked" onclick="_Page_SaveOptions()" /><label for="chkForms"><%=this.GetLocalResourceObject("chkForms")%></label></span> <%} %>
<%if (Configurations.UserOptions.RemoveObjects.Changeable){ %><span class="ASProxyOption"><input id="chkRemoveObjects" type="checkbox" checked="checked" onclick="_Page_SaveOptions()" /><label for="chkRemoveObjects"><%=this.GetLocalResourceObject("chkRemoveObjects")%></label></span> <%} %>
<%if (Configurations.UserOptions.Links.Changeable){ %><span class="ASProxyOption"><input id="chkProcessLinks" type="checkbox" checked="checked" onclick="_Page_SaveOptions()" /><label for="chkProcessLinks"><%=this.GetLocalResourceObject("chkProcessLinks")%></label></span> <%} %>
<%if (Configurations.UserOptions.TempCookies.Changeable){ %><span class="ASProxyOption"><input id="chkTempCookies" type="checkbox" checked="checked" onclick="_Page_SaveOptions()" /><label for="chkTempCookies"><%=this.GetLocalResourceObject("chkTempCookies")%></label></span> <%} %>
</div></div>
<script type="text/javascript">
_Page_Initialize();
_Page_SetOptions();
</script>

<%if (_HasError){ %>
<div title="Error message" style="text-align: center; color: Red; font-weight: bold;
font-family: Tahoma; font-size: 10pt;">
<%=_ErrorMessage%></div>
<%} %>
<noscript style="color: Maroon; font-weight: bold; font-family: Tahoma; font-size: 11pt;"><%=this.GetLocalResourceObject("JsIsDisabled")%></noscript>
</div></form></body>
</html>

<div style="position: relative; left: 0px; top: 5px; width: 100%; height: auto;">
<%=_ResponseContent%>
</div>

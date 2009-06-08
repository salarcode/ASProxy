<%@ Page Language="C#" %>
<%@ Import Namespace="SalarSoft.ASProxy.Exposed"%>
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
	Consts.FilesConsts.DefaultPage = System.IO.Path.GetFileName(Request.Url.AbsolutePath).ToLower();
    _userOptions = UserOptions.ReadFromRequest();
	ProccessRequest();
}

void ProccessRequest()
{
	if (UrlProvider.IsASProxyAddressUrlIncluded(Request.QueryString))
	{
        using (IEngine engine = (IEngine)Provider.CreateProviderInstance(ProviderType.IEngine))
		{

            engine.UserOptions = _userOptions;
            engine.DataTypeToProcess = DataTypeToProcess.Html;
			engine.RequestInfo.SetContentType ( MimeContentType.text_html);
			
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

			if (engine.LastStatus== LastStatus.Error)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
                    Systems.LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

				_ErrorMessage = engine.LastErrorMessage;
				if(string.IsNullOrEmpty(_ErrorMessage))
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
                    Response.Redirect(UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageDownload, engine.RequestInfo.RequestUrl, true), false);
				else
                    Response.Redirect(Consts.FilesConsts.PageDownload + "?" + engine.RequestInfo.PostDataString, false);
				return;
			case MimeContentType.image_gif:
			case MimeContentType.image_jpeg:
                _ResponseContent = HtmlTags.ImgTag(UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageImages, engine.RequestInfo.RequestUrl, true));
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
		if (pageContentType == MimeContentType.text_css || pageContentType == MimeContentType.text_plain || pageContentType ==MimeContentType.text_javascript)
			_ResponseContent = "<pre>" + HttpUtility.HtmlEncode(_ResponseContent) + "</pre>";

		// Response charset
		Response.ContentEncoding = engine.ResponseInfo.ContentEncoding;
        Response.Charset = engine.ResponseInfo.ContentEncoding.BodyName;

		if (engine.LastStatus == LastStatus.Error)
		{
            if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

			_ErrorMessage = engine.LastErrorMessage;
			_HasError = true;
			_ResponseContent = "";
			return;
		} 
        else if (engine.LastStatus == LastStatus.ContinueWithError)
        {
            if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);
            
            // if has error but can continue
            _ErrorMessage = engine.LastErrorMessage;
            _HasError = true;
        }


		if (engine.UserOptions.DocType)
		{
			Response.Write(engine.ResponseInfo.HtmlDocType);
		}
		Response.Write(engine.ResponseInfo.HtmlInitilizerCodes);

        if (engine.UserOptions.PageTitle)
		{
			Page.Title = Consts.General.ASProxyName + ":: " + engine.ResponseInfo.HtmlPageTitle;
		}
		if (engine.ResponseInfo.HtmlIsFrameSet)
		{
			ApplyFramesetResults(engine);
			return;
		}
	}
	catch (ThreadAbortException){}
	catch (Exception err)
	{
		if (Systems.LogSystem.ErrorLogEnabled)
            Systems.LogSystem.LogError(engine.RequestInfo.RequestUrl, err.Message);

		_ErrorMessage = err.Message;
		_HasError = true;
	}
	finally{}
}

void ApplyFramesetResults(IEngine resutls)
{
	if (resutls.ResponseInfo.HtmlIsFrameSet)
	{
		string result = "";
		if (WebMethods.IsMethod(resutls.RequestInfo.RequestMethod, WebMethods.DefaultMethods.GET))
            result = UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageDirectHtml, resutls.RequestInfo.RequestUrl, true);
		else
            result = Consts.FilesConsts.PageDirectHtml + "?" + resutls.RequestInfo.PostDataString;
		_ResponseContent = HtmlTags.IFrameTag(result, "100%", "600%");
	}
	HttpContext.Current.ApplicationInstance.CompleteRequest();
}
</script>
<head runat="server">
<title runat="server">[PageTitle]</title>
<script src="scripts/base64encoder.js" type="text/javascript"></script>
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

<script type="text/javascript">
if(typeof _MainPage=='undefined')
	_MainPage=new function(){};
_MainPage.Options=new function(){};
_MainPage.Page=new function(){};
var _Page_B64Unknowner="<%=Consts.Query.Base64Unknowner%>";
var _Page_CookieName="<%=Consts.FrontEndPresentation.CookieMasterName%>";

function _Page_Initialize(){
	_MainPage.DefaultPage='<%=Consts.FilesConsts.DefaultPage%>';
	_MainPage.Page.UrlBox =document.getElementById('txtUrl');
	_MainPage.Page.ProcessScripts =document.getElementById('chkProcessScripts');
	_MainPage.Page.ProcessLinks =document.getElementById('chkProcessLinks');
	_MainPage.Page.DisplayImages =document.getElementById('chkDisplayImages');
	_MainPage.Page.BackImage =document.getElementById('chkBackImage');
	_MainPage.Page.Forms =document.getElementById('chkForms');
	_MainPage.Page.Compression =document.getElementById('chkCompression');
	_MainPage.Page.Cookies =document.getElementById('chkCookies');
	_MainPage.Page.OrginalUrl =document.getElementById('chkOrginalUrl');
	_MainPage.Page.IFrame =document.getElementById('chkIFrame');
	_MainPage.Page.PageTitle =document.getElementById('chkPageTitle');
	_MainPage.Page.UTF8 =document.getElementById('chkUTF8');
	_MainPage.Page.RemoveScripts =document.getElementById('chkRemoveScripts');
}

function _Page_SetOptions(){
	//_MainPage.Page.UrlBox.value = _MainPage.RequestUrl;
	_MainPage.Page.ProcessScripts.checked =<%=_userOptions.Scripts.ToString().ToLower() %>;
	_MainPage.Page.ProcessLinks.checked =<%=_userOptions.Links.ToString().ToLower() %>;
	_MainPage.Page.DisplayImages.checked =<%=_userOptions.Images.ToString().ToLower() %>;
	_MainPage.Page.BackImage.checked =<%=_userOptions.Images.ToString().ToLower() %>;
	_MainPage.Page.Forms.checked =<%=_userOptions.SubmitForms.ToString().ToLower() %>;
	_MainPage.Page.Compression.checked =<%=
	_userOptions.HttpCompression.ToString().ToLower()
	 %>;
	_MainPage.Page.Cookies.checked =<%=_userOptions.Cookies.ToString().ToLower() %>;
	_MainPage.Page.OrginalUrl.checked =<%=_userOptions.OrginalUrl.ToString().ToLower() %>;
	_MainPage.Page.IFrame.checked =<%=_userOptions.IFrame.ToString().ToLower() %>;
	_MainPage.Page.PageTitle.checked =<%=_userOptions.PageTitle.ToString().ToLower() %>;
	_MainPage.Page.UTF8.checked =<%=_userOptions.ForceEncoding.ToString().ToLower() %>;
	_MainPage.Page.RemoveScripts.checked =<%=_userOptions.RemoveScripts.ToString().ToLower() %>;
	_MainPage.Options.EncodeUrl=<%=_userOptions.EncodeUrl.ToString().ToLower() %>;
}
function _Page_SaveOptions(){
	var cookieOpt=_Page_CookieName+"=";
	cookieOpt+="Scripts="+_MainPage.Page.ProcessScripts.checked;
	cookieOpt+="&Links="+_MainPage.Page.ProcessLinks.checked;
	cookieOpt+="&Images="+_MainPage.Page.DisplayImages.checked;
	cookieOpt+="&CssLink="+_MainPage.Page.BackImage.checked;
	cookieOpt+="&SubmitForms="+_MainPage.Page.Forms.checked;
	cookieOpt+="&HttpCompression="+_MainPage.Page.Compression.checked;
	cookieOpt+="&Cookies="+_MainPage.Page.Cookies.checked;
	cookieOpt+="&OrginalUrl="+_MainPage.Page.OrginalUrl.checked;
	cookieOpt+="&IFrame="+_MainPage.Page.IFrame.checked;
	cookieOpt+="&PageTitle="+_MainPage.Page.PageTitle.checked;
	cookieOpt+="&ForceEncoding="+_MainPage.Page.UTF8.checked;
	cookieOpt+="&RemoveScripts="+_MainPage.Page.RemoveScripts.checked;
	//cookieOpt+="&EncodeUrl="+_MainPage.Page.EncodeUrl.checked;
	//cookieOpt+="&EmbedObjects="+_MainPage.Page.EmbedObjects.checked;
	
	var dt=new Date();
	dt.setYear(dt.getFullYear()+1);
	
	cookieOpt+=" ;Path=/ ;expires="+dt.toString();
	document.cookie=cookieOpt;
}
function _Page_HandleTextKey(ev){
	var IE=false;
	if(window.event) {ev=window.event;IE=true;}
	if(ev.keyCode==13){
		var loc=_MainPage.Page.UrlBox.value.toLowerCase();
		if(loc.lastIndexOf('.com')== -1 && loc.lastIndexOf('.net')== -1 && loc.lastIndexOf('.org')== -1){
		if(ev.ctrlKey && ev.shiftKey)
			_MainPage.Page.UrlBox.value+='.org';
		else if(ev.ctrlKey)
			_MainPage.Page.UrlBox.value+='.com';
		else if(ev.shiftKey)
			_MainPage.Page.UrlBox.value+='.net';
		}
		_Page_SubmitForm();		
	}
}
function _Page_SubmitForm(){
	_Page_SaveOptions();
	var url=_MainPage.Page.UrlBox.value;
	if(url!='') _Page_Navigate(url);
	else alert('[UrlIsEmpty]');
}

function _Page_Navigate(url){
	var navUrl=_MainPage.DefaultPage;
	if(_MainPage.Options.EncodeUrl)
	{
		navUrl+='?dec='+'1'+'&url=';
	    navUrl+=_Base64_encode(_MainPage.Page.UrlBox.value)+_Page_B64Unknowner;
	}
	else
	{
		navUrl+='?dec='+'0'+'&url=';
	    navUrl+=_MainPage.Page.UrlBox.value;
	}
	document.location=navUrl;
}
</script>

<div id="ASProxyMain" dir="[Direction]">
<table id="ASProxyMainTable" style="width: 100%; ">
<tr><td style="padding:0px; margin:0px;"><table style="width: 100%;border-width:0px;" cellpadding="0" cellspacing="0">
<tr><td class="Sides"><a href="." asproxydone="2">ASProxy <%=Consts.General.ASProxyVersion %></a></td><td style="font-size:small;"><strong>[PageHeader]</strong></td><td class="Sides">powered by SalarSoft</td></tr>
</table></td></tr><tr><td><!--This is ASProxy powered by SalarSoft. --><input name="txtUrl" type="text" size="60" id="txtUrl" dir="ltr" style="width:450px;" onkeyup="_Page_HandleTextKey(event)" value="<%=_ToDisplayUrl%>"/>
<input type="submit" name="btnASProxyDisplayButton" value="[btnDisplay]" id="btnASProxyDisplayButton" class="Button" style="height: 22px" onclick="_Page_SubmitForm()" />&nbsp;<br />
<span class="ASProxyCheckBox"><input id="chkUTF8" type="checkbox" name="chkUTF8" onclick="_Page_SaveOptions()"/><label for="chkUTF8">[chkUTF8]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkOrginalUrl" type="checkbox" name="chkOrginalUrl" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkOrginalUrl">[chkOrginalUrl]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkBackImage" type="checkbox" name="chkBackImage" checked="checked" onclick="_Page_SaveOptions()" /><label for="chkBackImage">[chkBackImage]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkRemoveScripts" type="checkbox" name="chkRemoveScripts" onclick="_Page_SaveOptions()"/><label for="chkRemoveScripts">[chkRemoveScripts]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkDisplayImages" type="checkbox" name="chkDisplayImages" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkDisplayImages">[chkDisplayImages]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkCookies" type="checkbox" name="chkCookies" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkCookies">[chkCookies]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkCompression" type="checkbox" name="chkCompression" onclick="_Page_SaveOptions()"/><label for="chkCompression">[chkCompression]</label></span>&nbsp;<a asproxydone="2" id="lnkMoreOpt" href="javascript:void(0);" onclick="toggleOpt(this);">[lnkMoreOpt]...<small>&gt;</small></a></td>
</tr><tr id="trMoreOpt" style="display: none;"><td id="tdMoreOpt"><span class="ASProxyCheckBox"><input id="chkIFrame" type="checkbox" name="chkIFrame" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkIFrame">[chkIFrame]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkPageTitle" type="checkbox" name="chkPageTitle" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkPageTitle">[chkPageTitle]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkForms" type="checkbox" name="chkForms" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkForms">[chkForms]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkProcessLinks" type="checkbox" name="chkProcessLinks" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkProcessLinks">[chkProcessLinks]</label></span>&nbsp;<span class="ASProxyCheckBox"><input id="chkProcessScripts" type="checkbox" name="chkProcessScripts" checked="checked" onclick="_Page_SaveOptions()"/><label for="chkProcessScripts">[chkProcessScripts]</label></span></td></tr>
<tr><td><a asproxydone="2" href="cookieman.aspx" target="_blank">[CookieManager]</a>&nbsp;&nbsp;<a asproxydone="2" href="download.aspx" target="_blank">[DownloadTool]</a>&nbsp;&nbsp;[GetFreeVersion]&nbsp;&nbsp;<span id="lblVersionNotifier"></span></td>
</tr></table>
<script type="text/javascript">
_Page_Initialize();
_Page_SetOptions();
</script>
<%if (_HasError){ %>
<span title="Error message" style="color:Red; font-weight:bold; font-family:Tahoma; font-size:10pt;"><%=_ErrorMessage%></span>
<%} %>
<noscript style="color:Maroon;font-weight:bold; font-family:Tahoma; font-size:11pt;">[JsIsDisabled]</noscript>
</div>
</body></html>

<div style="position: relative; left: 0px; top: 5px; width: 100%; height:auto;">
<%=_ResponseContent%>
</div>
<script type="text/javascript" src="scripts/versionchecker.js"></script>

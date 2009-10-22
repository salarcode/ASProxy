// ASProxy Surf page scripts
// Last update: 2009-10-19 coded by Salar Khalilzadeh //

// Page options
_XPage={};
function _Page_Initialize(){
	_XPage.UrlBox =document.getElementById('txtUrl');
	_XPage.ProcessLinks =document.getElementById('chkProcessLinks');
	_XPage.DisplayImages =document.getElementById('chkDisplayImages');
	_XPage.Forms =document.getElementById('chkForms');
	_XPage.Compression =document.getElementById('chkCompression');
	_XPage.Cookies =document.getElementById('chkCookies');
	_XPage.TempCookies =document.getElementById('chkTempCookies');
	_XPage.OrginalUrl =document.getElementById('chkOrginalUrl');
	_XPage.Frames =document.getElementById('chkFrames');
	_XPage.PageTitle =document.getElementById('chkPageTitle');
	_XPage.UTF8 =document.getElementById('chkUTF8');
	_XPage.RemoveScripts =document.getElementById('chkRemoveScripts');
	_XPage.EncodeUrl =document.getElementById('chkEncodeUrl');
	
	if(!_XPage.UrlBox) _XPage.UrlBox={};
	if(!_XPage.ProcessLinks) _XPage.ProcessLinks={};
	if(!_XPage.DisplayImages) _XPage.DisplayImages={};
	if(!_XPage.Forms) _XPage.Forms={};
	if(!_XPage.Compression) _XPage.Compression={};
	if(!_XPage.Cookies) _XPage.Cookies={};
	if(!_XPage.TempCookies) _XPage.TempCookies={};
	if(!_XPage.OrginalUrl) _XPage.OrginalUrl={};
	if(!_XPage.Frames) _XPage.Frames={};
	if(!_XPage.PageTitle) _XPage.PageTitle={};
	if(!_XPage.UTF8) _XPage.UTF8={};
	if(!_XPage.RemoveScripts) _XPage.RemoveScripts={};
	if(!_XPage.EncodeUrl) _XPage.EncodeUrl={};
}
function _Page_SaveOptions(){
	var cookieOpt=_Page_CookieName+"=";
	cookieOpt+="Links="+_XPage.ProcessLinks.checked;
	cookieOpt+="&Images="+_XPage.DisplayImages.checked;
	cookieOpt+="&SubmitForms="+_XPage.Forms.checked;
	cookieOpt+="&HttpCompression="+_XPage.Compression.checked;
	cookieOpt+="&Cookies="+_XPage.Cookies.checked;
	cookieOpt+="&TempCookies="+_XPage.TempCookies.checked;
	cookieOpt+="&OrginalUrl="+_XPage.OrginalUrl.checked;
	cookieOpt+="&Frames="+_XPage.Frames.checked;
	cookieOpt+="&PageTitle="+_XPage.PageTitle.checked;
	cookieOpt+="&ForceEncoding="+_XPage.UTF8.checked;
	cookieOpt+="&RemoveScripts="+_XPage.RemoveScripts.checked;
	cookieOpt+="&EncodeUrl="+_XPage.EncodeUrl.checked;
	//cookieOpt+="&RemoveObjects="+_XPage.RemoveObjects.checked;
	
	var dt=new Date();
	dt.setYear(dt.getFullYear()+1);
	
	cookieOpt+=" ;Path=/ ;expires="+dt.toString();
	document.cookie=cookieOpt;
}
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
		_Page_SubmitForm();		
	}
	return true;
}
function _Page_SubmitForm(){
	_Page_SaveOptions();
	var url=_XPage.UrlBox.value;
	if(url!='') {_Page_Navigate(url); return true;}
	else { alert(_Page_UrlIsEmpty); return false; }
}

function _Page_Navigate(url){
	var navUrl = _Page_XNav;
	if(_XPage.EncodeUrl.checked){
		navUrl+='?dec='+'1'+'&url=';
	    navUrl+=_Base64_encode(_XPage.UrlBox.value)+_Page_B64Unknowner;
	} else {
		navUrl+='?dec='+'0'+'&url=';
	    navUrl+=_XPage.UrlBox.value;
	}
	document.location=navUrl;
}
function _PageOnSubmit(){
	_Page_SubmitForm();
	return false;
}
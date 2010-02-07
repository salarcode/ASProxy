// ASProxy Surf page scripts
// Last update: 2010-02-07 coded by Salar.Kh //

// Page options
if (typeof (_XPage) == 'undefined')
	_XPage = {};
function _Page_Initialize(){
	_XPage.UrlBox =document.getElementById('txtUrl');
	_XPage.ProcessLinks =document.getElementById('chkProcessLinks');
	_XPage.DisplayImages =document.getElementById('chkDisplayImages');
	_XPage.Forms =document.getElementById('chkForms');
	_XPage.Compression =document.getElementById('chkCompression');
	_XPage.ImgCompressor =document.getElementById('chkImgCompressor');
	_XPage.Cookies =document.getElementById('chkCookies');
	_XPage.TempCookies =document.getElementById('chkTempCookies');
	_XPage.OrginalUrl =document.getElementById('chkOrginalUrl');
	_XPage.Frames =document.getElementById('chkFrames');
	_XPage.PageTitle =document.getElementById('chkPageTitle');
	_XPage.UTF8 =document.getElementById('chkUTF8');
	_XPage.RemoveScripts =document.getElementById('chkRemoveScripts');
	_XPage.RemoveObjects = document.getElementById('chkRemoveObjects');
	_XPage.EncodeUrl =document.getElementById('chkEncodeUrl');
	
	if(!_XPage.UrlBox) _XPage.UrlBox={};
	if(!_XPage.ProcessLinks) _XPage.ProcessLinks={};
	if(!_XPage.DisplayImages) _XPage.DisplayImages={};
	if(!_XPage.Forms) _XPage.Forms={};
	if(!_XPage.Compression) _XPage.Compression={};
	if(!_XPage.ImgCompressor) _XPage.ImgCompressor={};
	if(!_XPage.Cookies) _XPage.Cookies={};
	if(!_XPage.TempCookies) _XPage.TempCookies={};
	if(!_XPage.OrginalUrl) _XPage.OrginalUrl={};
	if(!_XPage.Frames) _XPage.Frames={};
	if(!_XPage.PageTitle) _XPage.PageTitle={};
	if(!_XPage.UTF8) _XPage.UTF8={};
	if(!_XPage.RemoveScripts) _XPage.RemoveScripts={};
	if(!_XPage.RemoveObjects) _XPage.RemoveObjects={};
	if(!_XPage.EncodeUrl) _XPage.EncodeUrl={};
}
function _Page_SaveOptions(){
	var cookieOpt=_Page_CookieName+"=";
	cookieOpt+="Links="+(_XPage.ProcessLinks.checked+0);
	cookieOpt+="&Img="+(_XPage.DisplayImages.checked+0);
	cookieOpt+="&Forms="+(_XPage.Forms.checked+0);
	cookieOpt+="&ZIP="+(_XPage.Compression.checked+0);
	cookieOpt+="&ImgZip="+(_XPage.ImgCompressor.checked+0);
	cookieOpt+="&Cookies="+(_XPage.Cookies.checked+0);
	cookieOpt+="&TempCookies="+(_XPage.TempCookies.checked+0);
	cookieOpt+="&FloatBar="+(_XPage.OrginalUrl.checked+0);
	cookieOpt+="&Frames="+(_XPage.Frames.checked+0);
	cookieOpt+="&Title="+(_XPage.PageTitle.checked+0);
	cookieOpt+="&PgEnc="+(_XPage.UTF8.checked+0);
	cookieOpt+="&RemScript="+(_XPage.RemoveScripts.checked+0);
	cookieOpt+="&EncUrl="+(_XPage.EncodeUrl.checked+0);
	cookieOpt+="&RemObj="+(_XPage.RemoveObjects.checked+0);
	
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
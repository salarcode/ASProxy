// ASProxy Dynamic Encoder
// ASProxy encoder for dynamically created objects //
// Last update: 2009-11-17 coded by Salar Khalilzadeh //

//_userConfig={EncodeUrl:true, OrginalUrl:true, Links:true, Images:true, Forms:true, Frames:true, Cookies:true};

//_reqInfo={
//	pageUrl:'http://blocked.com:8080/sub/hi/tester.html?param=1&hi=2',
//	pageUrlNoQuery:'http://blocked.com:8080/sub/hi/tester.html',
//	pagePath:'http://blocked.com:8080/sub/hi/',
//	rootUrl:'http://blocked.com:8080/', 
//	cookieName:'blocked_Cookies',
//	ASProxyUrl:'http://site.com:8080/asproxy/default.aspx', // or 'http://site.com/default.aspx';
//	ASProxyPath:'http://site.com:8080/asproxy/', // or 'http://site.com/';
//	ASProxyRoot:'http://site.com:8080/' ,
//	ASProxyPageName:'default.aspx',
//	UrlUnknowner:'B64Coded!'
//};

// regested site url information
//_reqInfo.location={ Hash:'#hi', Host:"site.com:8080", Hostname:"site.com", Pathname:"/dir/page.htm", Search:'?test=1', Port:"8080", Protocol:"http:" }

// ASProxy object
_ASProxy={
	Debug_UseAbsoluteUrl:false,
	TraceCookies:false,
	LogEnabled:false,
	Activities:{},
	Pages:{
		pgGetAny:'getany.ashx',
		pgGetHtml:'gethtml.ashx',
		pgGetJS:'getjs.ashx',
		pgImages:'images.ashx',
		pgDownload:'download.ashx',
		pgAuthorization:'authorization.aspx'
	},
	ClientSideUrls:["mailto:", "file://", "javascript:", "vbscript:", "jscript:", "vbs:","ymsgr:","data:"],
	NonVirtualUrls:["http://", "https://", "mailto:", "ftp://", "file://", "telnet://", "news://", "nntp://", "ldap://","ymsgr:", "javascript:", "vbscript:", "jscript:", "vbs:", "data:" ]
}

_ASProxy.EncodedUrls=[_reqInfo.ASProxyPageName+"?dec=",
		_ASProxy.Pages.pgGetHtml+"?dec=",_ASProxy.Pages.pgImages+"?dec=",_ASProxy.Pages.pgGetJS+"?dec=",
		_ASProxy.Pages.pgDownload+"?dec=",_ASProxy.Pages.pgAuthorization+"?dec=",_ASProxy.Pages.pgGetAny+"?dec="];

// original write methods should be on document
document.OriginalWrite = document.write;
document.OriginalWriteLn = document.writeln;
window.OriginalOpen=window.open;
window.LocationReplace=window.location.replace;
window.LocationAssign=window.location.assign;
window.LocationReload=window.location.reload;


// ---------------------------
// UrlEncoder required methods
// ---------------------------

// Content type constants
ENC_Page=0;
ENC_Images=1;
ENC_Any=2;
ENC_Frames=3;

// public: encodes urls
// type: 0-page, 1-images, 2-anything, 3-frames
// notCorrectLocalUrl: shouldn't correct url address to original site
// extraQuery: additional query to add to method
function __UrlEncoder(url,type,notCorrectLocalUrl,extraQuery){
	if(_ASProxy.IsClientSideUrl(url) || _ASProxy.IsSelfBookmarkUrl(url))
		return url;
	
	if(notCorrectLocalUrl!=true)
		url=_ASProxy.CorrectLocalUrlToOrginal(url);
	
	if(_ASProxy.IsVirtualUrl(url))
		url=_ASProxy.JoinUrls(url,_reqInfo.pagePath,_reqInfo.rootUrl);

	var asproxyBasePath;
	if(type == ENC_Images)
		asproxyBasePath=_ASProxy.Pages.pgImages; // images
	else if(type == ENC_Any)
		asproxyBasePath=_ASProxy.Pages.pgGetAny;
	else if(type == ENC_Frames)
		asproxyBasePath=_ASProxy.Pages.pgGetHtml;
	else
		asproxyBasePath=_reqInfo.ASProxyPageName;

	// UseAbsoluteUrl, only for debug
	if(typeof(_ASProxy.Debug_UseAbsoluteUrl)!='undefined' && _ASProxy.Debug_UseAbsoluteUrl)
		asproxyBasePath=_reqInfo.ASProxyPath + asproxyBasePath;

	// config parameters
	asproxyBasePath+='?dec='+(_userConfig.EncodeUrl+0)+'&url=';

	var bookmark;
	var result;
	bookmark=_ASProxy.ReturnBookmarkPart(url);

	if(bookmark!="")
		url=_ASProxy.RemoveBookmarkPart(url);

	result=asproxyBasePath;
	if(_userConfig.EncodeUrl)
		result+=_ASProxy.B64UnknownerAdd(_Base64_encode(url));
	else
		result+=url;

	if(extraQuery!=null)
		result+="&"+extraQuery;

	result=(result)+bookmark;
	return result;
}

// private: combine page url with base path and return full url
// exam: in "http://test.com/sub/" site, cenverts "/page.htm" to "http://test.com/page.htm" 
//	 and converts "page.htm" to "http://test.com/sub/page.htm"
_ASProxy.JoinUrls = function(url,pagePath,rootUrl)
{
	if(pagePath.lastIndexOf("/")!=pagePath.length-1)
		pagePath+="/";

	if(rootUrl.lastIndexOf("/")!=rootUrl.length-1)
		rootUrl+="/";

	var result;
	if(url.indexOf("/",0)==0)// check if slash is in the first
		result= rootUrl +"."+ url;
	else
		result= pagePath + url;
	return result;
}


// private: Is url client-side
_ASProxy.IsClientSideUrl=function(url){
	if (typeof (url) != "string") return false;
	url=url.toLowerCase();
	for (i = 0; i < _ASProxy.ClientSideUrls.length; i++)
	{
		if(_ASProxy.StrStartsWith(url,_ASProxy.ClientSideUrls[i]))
			return true;
	}
	return false;
}

// private: Is url virtual
_ASProxy.IsVirtualUrl=function(url){
	if (typeof (url) != "string") return true;
	url=url.toLowerCase();
	for (i = 0; i < _ASProxy.NonVirtualUrls.length; i++)
	{
		if(_ASProxy.StrStartsWith(url,_ASProxy.NonVirtualUrls[i]))
			return false;
	}
	return true;
}

// private: checks that url is a self bookmark in current page
_ASProxy.IsSelfBookmarkUrl=function(url){
	if (typeof (url) != "string") return false;
	url=url.toLowerCase();
	if(_ASProxy.StrStartsWith(url,'#'))
		return true;
	
	var location = window.location.href.toLowerCase()+'#';
	if(url.indexOf(location,0)==0)
		return true;
	return false;
}

// private: returns bookmark part of url
_ASProxy.ReturnBookmarkPart=function(url){
	if (typeof (url) != "string") return "";
	var pos=url.indexOf("#",0);
	if(pos!=-1){
		url=url.substring(pos,url.length);
		return url;
	}
	else return "";
}

// private: removes bookmark part of url
_ASProxy.RemoveBookmarkPart=function(url){
	if (typeof (url) != "string") return null;
	var result;
	var pos=url.indexOf("#",0);
	if(pos!=-1){
		url=url.substring(0,pos);
		return url;
	}
	else return url;
}

// private: convert url to requested url if started with proxy address AND checks if this is not a client url
_ASProxy.CorrectLocalUrlToOrginalCheck =function(url) {
	if(_ASProxy.IsClientSideUrl(url) || _ASProxy.IsSelfBookmarkUrl(url))
		return url;
	return _ASProxy.CorrectLocalUrlToOrginal(url);
}

// private: converts url to requested url if it is started with proxy address
_ASProxy.CorrectLocalUrlToOrginal = function(requestUrl){
	if (typeof (requestUrl) != "string") return null;
	var url=requestUrl.toLowerCase();
	var baseBasePath=_reqInfo.ASProxyRoot.toLowerCase();
	var basePath=_reqInfo.ASProxyPath.toLowerCase();
	var pagePath=_reqInfo.ASProxyPageName.toLowerCase();
	var willBeBase=_reqInfo.pageUrlNoQuery; // Current url without parameters
	var addSeperator=false;

	// Checking page url
	var position=url.indexOf(pagePath,0);
	var pathLength=pagePath.length;

	if(position==-1){
		// Checking site url
		position=url.indexOf(basePath,0);
		pathLength=basePath.length;
		willBeBase=_reqInfo.pagePath;
		addSeperator=true;
	}

	if(position==-1){
		// Checking host url
		position=url.indexOf(baseBasePath,0);
		pathLength=baseBasePath.length;
		willBeBase=_reqInfo.rootUrl; // root of current page
		addSeperator=true;
	}

	if(willBeBase.substr(willBeBase.length-1,1)=='/'){
		willBeBase=willBeBase.substr(0,willBeBase.length-1);
		addSeperator=true;
	}

	if(position==0){
		// If something found replace ASProxy url with orginal url
		var reqUrl=requestUrl.substr(pathLength,requestUrl.length);
		var seperator='/';
		if(reqUrl.substr(0,1)=='/')
			seperator="";
		if(addSeperator==false)
			seperator="";
			
		return willBeBase + seperator + reqUrl;
	}
	else
		return requestUrl;
}

// private: Adds url unknowner
_ASProxy.B64UnknownerAdd = function(url) {
	if (typeof (url) != "string") return _reqInfo.UrlUnknowner;
	var unknowner=_reqInfo.UrlUnknowner.toLowerCase();
	var urlAddr=url.toLowerCase();
	
	var add=!_ASProxy.StrEndsWith(urlAddr,unknowner);
	
	if(add) return url+_reqInfo.UrlUnknowner;
	else return url;
}

// ---------------------------
// END
// ---------------------------
_ASProxy.Log=function(){
if(_ASProxy.LogEnabled &&  typeof(console)!='undefined'){
	try{
		console.debug('ASProxy log:');
		var log='';
		for(var i=0;i<arguments.length;i++){
			log+=arguments[i]+'\n';
		}
		console.debug(log);
	}catch(e){}
}}
_ASProxy.IsEncodedByASProxy = function(url){
	if(typeof(url)!="string") return false;
	url=url.toLowerCase();
	var baseUrl=_reqInfo.ASProxyPath.toLowerCase();
	for (i = 0; i < _ASProxy.EncodedUrls.length; i++)
	{
		if(_ASProxy.StrStartsWith(url,_ASProxy.EncodedUrls[i]))
			return true;
		else if(_ASProxy.StrStartsWith(url,baseUrl+_ASProxy.EncodedUrls[i]))
			return true;
	}
	return false;
}

// private: return bookmark only if the link is for current page
_ASProxy.GetBookmarkOnlyForCurrentPage=function(url){
	var currUrl=document.location.href.toLowerCase();
	var reqUrl=url.toLowerCase();
	
	var markPos=url.indexOf("#",0);
	if(markPos!=-1){
		
		var urlPos=reqUrl.indexOf(currUrl,0);
		if(urlPos==-1)
			return url;
		else
			// we add current page url with bookmark part
			return _reqInfo.pageUrl + url.substring(markPos,url.length);
	}
	else return url;
}

// private: Event attacher
_ASProxy.AttachEvent = function(o, evType, f, capture) {
try{
	if(o == null) { return false; }
	if(o.addEventListener) {
		o.addEventListener(evType, f, capture);
		return true;
	} else if (o.attachEvent) {
		var r = o.attachEvent("on" + evType, f);
		return r;
	} else {
		try{ o["on" + evType] = f; }catch(e){_ASProxy.Log('AttachEvent"on"',e);}
	}
}catch(e){_ASProxy.Log('AttachEvent',e);}
}

_ASProxy.StrTrimLeft = function(str) {
	return str.replace(/^\s*/, "");
}
_ASProxy.StrTrimRight = function(str) {
	return str.replace(/\s*$/, "");
}
_ASProxy.StrTrim = function(str) {
	return str.replace(/^\s+|\s+$/g, '');
}
_ASProxy.StrStartsWith = function(str, check) {
	if (str.length == 0 || str.length < check.length) { return false; }
	return (str.substr(0, check.length) == check);
}
_ASProxy.StrEndsWith = function(str, check) {
	if (str.length == 0 || str.length < check.length) { return false; }
	return (str.substr(str.length - check.length) == check);
}


// ---------------------------
// Automated encoders
// ---------------------------
_ASProxy.Enc={};


// private: items collection
// contentType: 0-page, 1-images, 2-anything, 3-frames
// conditionProp,conditionValue: optional condition to proccess an element
// additionalKey,additionalValue: additional attribute to add to element
_ASProxy.Enc.EncodeElements=function(elementsArray,propName,contentType,applyFloatBar,conditionProp,conditionValue,additionalKey,additionalValue)
{
var i=0;
var item;
for(i=0;i<elementsArray.length;i++){
	item=elementsArray[i];
	
	// get element value
	var propValue=item.attributes[propName];
	var propValueFull=item[propName];
	
	if(propValue!=null)
		propValue=propValue.value;
	else
		propValue=propValueFull;

	// is element already coded?
	if(_ASProxy.IsEncodedByASProxy(propValue)==false){

		var applyEncoding=false;
		
		// checking for condition supplied
		if(conditionProp!=null && conditionValue!=null){
			var cValue=item.attributes[conditionProp];
			if(cValue!=null)
				cValue=cValue.value;
			else
				cValue=item[conditionProp];
			if(cValue!=null){
				if(cValue.toLowerCase() != conditionValue.toLowerCase())
				 continue;
			}
		}
		
		// status of ecoded by asproxy before
		var isDone=item.attributes["asproxydone"];
		if(isDone==null || (isDone.value!="1" && isDone.value!="2"))
		{
			applyEncoding=true;
		}
		else if( isDone.value=="1"){
			// last encodec url address
			var orgEncodedUrl=item.attributes["encodedurl"];
			if(orgEncodedUrl==null){	
				_ASProxy.CallOriginalSetAttr(item,"encodedurl",propValue);
				
				// if frames is proccessing and additional key is available
				if(contentType==3 && additionalKey){
					_ASProxy.CallOriginalSetAttr(item,additionalKey,additionalValue);
				}
				
				applyEncoding=true;
				//continue; // commented v5.0. do not ignore, proccess it!
			}
			else orgEncodedUrl=orgEncodedUrl.value;
			
			// checking previous address
			if(propValue!=orgEncodedUrl){
				if(propValue==window.location)
					continue;
				else
				{
					applyEncoding=true;
				}
			}
		}
		
		if(applyEncoding){
			// corrects address to site original address
			var newValue = _ASProxy.CorrectLocalUrlToOrginalCheck(propValue);
			propValueFull=_ASProxy.CorrectLocalUrlToOrginalCheck(propValueFull);
			
			// set encoding done flag
			_ASProxy.CallOriginalSetAttr(item,"asproxydone","1");
			
			// set original address
			_ASProxy.CallOriginalSetAttr(item,"originalurl",_ASProxy.GetBookmarkOnlyForCurrentPage(propValueFull));

			// if frames is proccessing and additional key is available
			if(contentType==3 && additionalKey){
				_ASProxy.CallOriginalSetAttr(item,additionalKey,additionalValue);
			}

			// set float bar variables
			if(applyFloatBar && _userConfig.OrginalUrl){
				if(typeof ORG_IN_!='undefined')
					_ASProxy.AttachEvent(item,"mouseover",function(){ORG_IN_(this)});
				if(typeof ORG_OUT_!='undefined')
					_ASProxy.AttachEvent(item,"mouseout",function(){ORG_OUT_()});
			}

			if(propValue==window.location && newValue==propValue)
				continue;
			else
				newValue= __UrlEncoder(newValue,contentType,true,null);
			
			// apply new value
			_ASProxy.CallOriginalSetAttr(item, propName , newValue);
			item[propName] = newValue;
			
			//Saving base64 coded url to monitor changes
			_ASProxy.CallOriginalSetAttr(item, "encodedurl" , newValue);
		}
		
	}
}}

// private: encodes all forms
_ASProxy.Enc.EncodeForms=function(){
if(_userConfig.Forms!=true) return;

var i=0; var frm;
for(i=0;i<document.forms.length;i++)
{
	frm=document.forms.item(i);

	var frmAction=frm.attributes["action"];
	if(frmAction!=null)
		frmAction=frmAction.value;
	else
		frmAction=frm.action;

	// is already encoded
	if(_ASProxy.IsEncodedByASProxy(frmAction)==false){

		var applyEncoding=false;
		var isDone = frm.attributes["asproxydone"];
		var frmMethod = frm.method;

		// is already encoded
		if(isDone==null || (isDone.value!="1" && isDone.value!="2"))
		{
			applyEncoding=true;
		}
		else if( isDone.value=="1"){
			var orgEncodedUrl=frm.attributes["encodedurl"];
			if(orgEncodedUrl == null)
				orgEncodedUrl = "";
			else
				orgEncodedUrl = orgEncodedUrl.value;

			frmMethod = frm.attributes["methodorginal"];
			if(frmMethod == null)
				frmMethod = frm.method;
			else
				frmMethod = frmMethod.value;
			
			var position=orgEncodedUrl.indexOf("://",0);
			if(position==-1){
				orgEncodedUrl=document.location.protocol+"//"+document.location.host+"/"+orgEncodedUrl;
			}

			// Checks only Action changes and does not support Method changes
			if(frmAction!=orgEncodedUrl)
			{
				applyEncoding=true;
			}
		}

		if(applyEncoding){
			_ASProxy.CallOriginalSetAttr(frm,"asproxydone","1");
			_ASProxy.CallOriginalSetAttr(frm,"methodorginal" , frmMethod);

			// encodes action
			var newFrmAction=__UrlEncoder(frmAction,ENC_Page,false,"method="+frmMethod);

			_ASProxy.CallOriginalSetAttr(frm,"action", newFrmAction);

			_ASProxy.CallOriginalSetAttr(frm,"encodedurl", newFrmAction);

			//override from method
			frm.method="POST";
		}
	}
}
//After 2 seconds
window.setTimeout("_ASProxy.Enc.EncodeForms();",2000);
}

// private: encode all frames
_ASProxy.Enc.EncodeFrames=function(){
	if(_userConfig.Frames!=true) return;

	var frames=document.documentElement.getElementsByTagName("iframe");
	try{
	_ASProxy.Enc.EncodeElements(frames,"src",3,false,null,null,"onload","_ASProxy.Enc.EncodeFrames()");
	}catch(e){_ASProxy.Log('Enc.EncodeFrames',e);}

	//After 4 seconds
	window.setTimeout("_ASProxy.Enc.EncodeFrames();",5000);
}

// private: encode all links
_ASProxy.Enc.EncodeLinks=function(){
	if(_userConfig.Links!=true) return;
	try{
	_ASProxy.Enc.EncodeElements(document.links,"href",0,true);
	}catch(e){_ASProxy.Log('Enc.EncodeLinks',e);}

	//After 1 second 
	window.setTimeout("_ASProxy.Enc.EncodeLinks();",1000);
}

// private: encode all images
_ASProxy.Enc.EncodeImages=function(){
	if(_userConfig.Images!=true) return;
	try{
	_ASProxy.Enc.EncodeElements(document.images,"src",1,true);
	}catch(e){_ASProxy.Log('Enc.EncodeImages',e);}

	//After 1 second
	window.setTimeout("_ASProxy.Enc.EncodeImages();",1000);
}

// private: encode all input images
_ASProxy.Enc.EncodeInputImages=function(){
	if(_userConfig.Images!=true) return;
	
	var inputImages=document.documentElement.getElementsByTagName("input");
	try{
	_ASProxy.Enc.EncodeElements(inputImages,"src",1,true,"type","image");
	}catch(e){_ASProxy.Log('Enc.EncodeInputImages',e);}

	//After 4 seconds
	window.setTimeout("_ASProxy.Enc.EncodeInputImages();",4000);
}

// private: encode all scripts
_ASProxy.Enc.EncodeScriptSources=function(){
	var scripts=document.documentElement.getElementsByTagName("script");
	try{
	_ASProxy.Enc.EncodeElements(scripts,"src",2,false);
	}catch(e){_ASProxy.Log('Enc.EncodeScriptSources',e);}

	//After 4 second 
	window.setTimeout("_ASProxy.Enc.EncodeScriptSources();",4000);
}
// ---------------------------
// END
// ---------------------------

// The location object will be replaced by this object
_ASProxy.LocationObject=function(){
	this.search= _reqInfo.location.Search;
	this.href= _reqInfo.pageUrl;
	this.hash=(window.location.hash!=null && window.location.hash!='')?window.location.hash: _reqInfo.location.Hash;
	this.host= _reqInfo.location.Host;
	this.hostname= _reqInfo.location.Hostname;
	this.pathname= _reqInfo.location.Pathname;
	this.port= _reqInfo.location.Port;
	this.protocol= _reqInfo.location.Protocol;
	this.replace=window.LocationReplace;
	this.assign=window.LocationAssign;
	
	this.replace=_ASProxy.LocationReplace;
	this.assign=_ASProxy.LocationAssign;
	this.reload=_ASProxy.LocationReload;
	
	this.URL= _reqInfo.pageUrl;
	
	this.toString=function(){return _reqInfo.pageUrl;};
	this.toLocaleString=function(){return _reqInfo.pageUrl;};
	this.length=this.href.length;
	//this.replace=this.href.replace;this.search=this.href.search;
	this.anchor=this.href.anchor;this.big=this.href.big;this.blink=this.href.blink;this.bold=this.href.bold;this.charAt=this.href.charAt;
	this.charCodeAt=this.href.charCodeAt;this.fixed=this.href.fixed;this.fontcolor=this.href.fontcolor;this.fontsize=this.href.fontsize;
	this.fromCharCode=this.href.fromCharCode;this.indexOf=this.href.indexOf;this.italics=this.href.italics;this.lastIndexOf=this.href.lastIndexOf;
	this.link=this.href.link;this.match=this.href.match;this.slice=this.href.slice;
	this.small=this.href.small;this.split=this.href.split;this.strike=this.href.strike;this.sub=this.href.sub;this.substr=this.href.substr;
	this.substring=this.href.substring;this.toLowerCase=this.href.toLowerCase;this.toUpperCase=this.href.toUpperCase;
};

// ---------------------------
// Client Cookies
// ---------------------------

// public: Returns current site cookies
function __CookieGet(_CookieName){
	return _ASProxy.GetDocumentCookie();
}

// public: Wrapping cookie operation in javascript
// sCookie: cookie string to set
function __CookieSet(sCookie) {
	return _ASProxy.SetDocumentCookie(sCookie);
}

_ASProxy.DocCookieString = document.cookie;
_ASProxy.GetDocumentCookie = function() {

	// document cookie
	_ASProxy.DocCookieString = document.cookie;

	var result = '';
	for (var i = 0; i < _reqInfo.appliedCookiesList.length; i++) {
		var cookieName = _reqInfo.appliedCookiesList[i];

		// our cookie for this name
		var cookie = _ASProxy.GetCookieByName(cookieName);

		if (cookie == null)
			continue;

		// now it's time to decode it!
		var cookieValue = _ASProxy.ParseASProxyCookie(cookie);

		if (cookieValue != null && cookieValue.length > 0)
		// add to the result
			result += cookieValue; //+ '; ';
	}

	return result;
}

// private: parses ASProxy cookie and returns standard cookie
_ASProxy.ParseASProxyCookie = function(asproxyCookie) {

	var result = '';

	// asproxy cookie parts are seperated by (&)
	var cookies = asproxyCookie.split("&");

	for (var i = 0; i < cookies.length; i++) {

		var cookieName = null;
		var cookieValue = null;
		var cookiePath = null;

		// the cookie
		var cookie = cookies[i];

		// cookie properties are seperated by (;)
		var cookieParts = cookie.split(';');

		for (var pIndex = 0; pIndex < cookieParts.length; pIndex++) {

			var cPart = cookieParts[pIndex];

			// Can't use split by equal sign method since 'cookie value' can contain equal sign (like in google, PREF='ID=...') and break this parsing,
			var equIndex = cPart.indexOf('=');

			var name = cPart.substr(0, equIndex).replace('+', '');
			name = _ASProxy.StrTrim(name);

			var value = _ASProxy.StrTrim(cPart.substr(equIndex + 1, cPart.length - equIndex - 1));

			if (name == "Name")
				cookieName = value;
			else if (name == "Value") {
				// this is first layer decode, required
				value = unescape(value);
				cookieValue = value;
			}
			else if (name == "Path") {
				cookiePath = value;
			}
		}

		// the cookie is parsed successfully
		if (cookieName != null && cookieValue != null) {
			// combine them
			var theVal = cookieName + '=' + cookieValue;

			// check cookiePath for current request path, if it is valid!
			if (cookiePath == null || cookiePath == "")
				cookiePath = "/";

			var cPath = cookiePath.toLowerCase();
			var reqPath = _reqInfo.location.Pathname.toLowerCase();

			// cookie should contain part of current request path
			if (reqPath.indexOf(cPath) != 0)
				continue;

			// add to result
			result += theVal + '; ';
		}
	}

	// done
	return result;
}


_ASProxy.SetDocumentCookie = function(cookieString) {
	if (cookieString == null || cookieString == '')
		return;

	// Parse the standard cookie and get asproxy cookie
	var asproxyCookieString = _ASProxy.ParseStandardCookieForSet(cookieString);
	return asproxyCookieString;
}

// private: parses standard cookie and returns ASProxy cookie
_ASProxy.ParseStandardCookieForSet = function(stdCookie) {

	var result = '';

	var cookieName = null;
	var cookieValue = null;
	var cookieExpires = null;
	var cookieMaxAge = null;
	var cookieDomain = null;
	var cookiePath = null;
	var cookieSecure = false;

	// the cookie set header which is going to "document.cookie" is seperated by (;)
	var cookieParts = stdCookie.split(";");
	for (var i = 0; i < cookieParts.length; i++) {

		// the cookie
		var cPart = cookieParts[i];

		// The seperator
		var equIndex = cPart.indexOf('=');

		// The name
		var name = cPart.substr(0, equIndex).replace('+', '');
		name = _ASProxy.StrTrim(name);

		// The value
		var value = _ASProxy.StrTrim(cPart.substr(equIndex + 1, cPart.length - equIndex - 1));


		if (i == 0) {
			// First pair is allways name=value
			cookieName = name;
			cookieValue = value;
		}
		else if (name == 'expires') {
			cookieExpires = value;
		}
		else if (name == 'max-age') {
			cookieMaxAge = value;
		}
		else if (name == 'domain') {
			cookieDomain = value;
		}
		else if (name == 'path') {
			cookiePath = value;
		}
		else if (name == 'secure') {
			cookieSecure = true;
		}
	}

	// the cookie to be saved
	var asproxyCookieString = '';

	// cookie name and value are required
	if (cookieName == null || cookieValue == null) {

		// no chance, go away!
		return null;
	}
	else {
		// building the cookie
		asproxyCookieString = 'Name=' + cookieName;

		// this includes first layer value encode
		asproxyCookieString += '; Value=' + escape(cookieValue);

		if (cookieExpires != null)
			asproxyCookieString += '; Expires=' + cookieExpires;

		if (cookieMaxAge != null)
			asproxyCookieString += '; Max-Age=' + cookieMaxAge;

		if (cookieDomain != null)
			asproxyCookieString += '; Domain=' + cookieDomain;

		if (cookiePath != null)
			asproxyCookieString += '; Path=' + cookiePath;

		if (cookieSecure)
			asproxyCookieString += '; Secure=True';
	}

	// default cookie name to save
	var toSaveCookieName = _reqInfo.cookieName;
	var toSaveCookieValue = '';
	var toSaveCookieString = '';

	// check if user have specifed domain
	if (cookieDomain != null && cookieDomain != '') {
		toSaveCookieName = _ASProxy.GetASProxyCookieName(cookieDomain);
	}

	// Starting to build the result
	toSaveCookieValue = asproxyCookieString;

	// Now we have to look for other cookies
	var otherSaveCookies = _ASProxy.GetCookieByName(toSaveCookieName);

	// if there is some previous cookies, we have to save them too
	if (otherSaveCookies != null && otherSaveCookies != '') {
		toSaveCookieValue += '& ' + otherSaveCookies;
	}

	// building 'document.cookie' friendly cookie
	// first we need encode the value, this is second layer encode
	toSaveCookieValue = escape(toSaveCookieValue);


	// the building the ASPrxy cookie as standard cookie
	// the name=value
	toSaveCookieString = toSaveCookieName + '=' + toSaveCookieValue;

	// only expires and max-age to save asproxy cookie
	if (cookieExpires != null)
		toSaveCookieString += '; expires=' + cookieExpires;

	if (cookieMaxAge != null)
		toSaveCookieString += '; max-age=' + cookieMaxAge;

	// To save ASProxy cookies the path should be always (/)
	toSaveCookieString += '; path=/';

	// return the result
	return toSaveCookieString;
}


_ASProxy.GetCookieByName = function(_cookieName) {
	var toGetCName = _cookieName;
	var aCookie = _ASProxy.DocCookieString.split(";");
	for (var i = 0; i < aCookie.length; i++) {
		var cParts = aCookie[i].split("=");
		var cName = _ASProxy.StrTrim(cParts[0]);
		if (toGetCName == cName)
			return unescape(cParts[1]);
	}
	return null;
}

_ASProxy.GetASProxyCookieName = function(domain) {
	return domain + _reqInfo.cookieNameExt;
}

// ---------------------------
// Client processors
// ---------------------------

// parses html codes
_ASProxy.ParseHtml=function(codes){
	if(typeof(codes)!='string')
		return codes;
		
	// can be anything
	var pattern = /\.(src)\s*=\s*([^;}]+)/ig;
	codes =codes.replace(pattern,".$1=__UrlEncoder($2,ENC_Any)");

	// goes to default page
	pattern = /\.(action|location|href)\s*=\s*([^;}]+)/ig;
	codes =codes.replace(pattern,".$1=__UrlEncoder($2)");

	// dynamic content change should call this method again
	pattern = /\.innerHTML\s*(\+)?=\s*([^};]+)\s*/ig;
	codes = codes.replace(pattern,'.innerHTML$1=_ASProxy.ParseHtml($2)');

	 // processing location attributes
	pattern = /\s(href|action)\s*=\s*(["']?)([^"'\s>]+)/ig;
	while ( match = pattern.exec(codes) ) {
		codes = codes.replace(match[0],' ' + match[1] + '=' + match[2] + __UrlEncoder(match[3]) );
	}

	 // processing attributes
	pattern = /\s(src|background)\s*=\s*(["']?)([^"'\s>]+)/ig;
	while ( match = pattern.exec(codes) ) {
		codes = codes.replace(match[0],' ' + match[1] + '=' + match[2] + __UrlEncoder(match[3],ENC_Any) );
	}

	// processing styles url
	pattern = /url\s*\(['"]?([^'"\)]+)['"]?\)/ig;
	while ( match = pattern.exec(codes) )
		codes = codes.replace(match[0],'url('+__UrlEncoder(match[1],ENC_Any)+')');

	// Css import rule
	pattern = /@import\s*['"]([^'"\(\)]+)['"]/ig;
	while ( match = pattern.exec(codes) )
		codes = codes.replace(match[0],'@import "'+__UrlEncoder(match[1],ENC_Any)+'"');

	return codes;
}

// parses javascript codes
_ASProxy.ParseJs=function(codes){

}

// ---------------------------
// Overriding standard methods
// ---------------------------

// private: calls original sett attribute
_ASProxy.CallOriginalSetAttr=function(element,attr,value){
if(element==null) return;
if (typeof element.OriginalSetAttribute == 'undefined')
	element.setAttribute(attr,value);
else
	element.OriginalSetAttribute(attr,value);
}

// private: overrides html elements setter
_ASProxy.OverrideHtmlSetters=function(){

// Only modern browsers
if (typeof window.__defineSetter__ == 'undefined')
	return;
// Tested and worked in Firefox 3+, Opera 9.6+, Chrome 2+

try{
	var interfaces = [
		HTMLTitleElement, HTMLTextAreaElement, HTMLTableSectionElement, HTMLTableRowElement, HTMLTableElement, 
		HTMLTableColElement, HTMLTableCellElement, HTMLTableCaptionElement, HTMLStyleElement, HTMLSelectElement, 
		HTMLScriptElement, HTMLParamElement, HTMLParagraphElement, HTMLOptionElement, HTMLOListElement, HTMLObjectElement, 
		HTMLMetaElement, HTMLMapElement, HTMLMapElement, HTMLLinkElement, HTMLLIElement, HTMLLegendElement, HTMLLabelElement, 
		HTMLIsIndexElement, HTMLInputElement, HTMLImageElement, HTMLIFrameElement, HTMLHtmlElement, HTMLHRElement, 
		HTMLHeadingElement, HTMLHeadElement, HTMLFrameSetElement, HTMLFrameElement, HTMLFormElement, HTMLFontElement, 
		HTMLFieldSetElement, HTMLEmbedElement, HTMLDocument, HTMLDListElement, HTMLDivElement, HTMLButtonElement, 
		HTMLBRElement, HTMLBodyElement, HTMLBaseFontElement, HTMLBaseElement, HTMLAreaElement, HTMLAnchorElement
	];

	try{
		// Try to add Html element interfaces which is not supported in all browsers
		// These Html element interfaces are not implemented by IE8 standard mode
		interfaces.push([HTMLElement, HTMLUListElement, HTMLQuoteElement, HTMLPreElement, HTMLModElement, HTMLMenuElement, HTMLDirectoryElement, HTMLAppletElement ]);
	}catch(e){_ASProxy.Log('OverrideHtmlSetters interfaces.push',e);}

	// Default attribute setter
	var _EncodeSetAttributeValue=function(attr,value,refTagName){
		if(_ASProxy.IsEncodedByASProxy(value))
			return value; // if alreay encoded don't change anything

		try{
			var attrName = attr.toLowerCase();
			var tag=refTagName;
			if(tag==null)
				tag=(this.tagName+'').toLowerCase();
			else
				tag=(tag+'').toLowerCase();
			
			if(attrName == 'src')
			{
				if(tag=='img')
					value = __UrlEncoder(value ,ENC_Images);
				else if(tag=='iframe' || tag=='frame')
					value = __UrlEncoder(value ,ENC_Frames);
				else // any other
					value = __UrlEncoder(value ,ENC_Any); 
			}
			else if(attrName == 'href')
			{
				if(tag=='a' || tag=='base')
					value = __UrlEncoder(value);
				else // any other
					value = __UrlEncoder(value ,ENC_Any); 
			}
			else if(attrName == 'background')
			{
				value = __UrlEncoder(value ,ENC_Images); 
			}
			else if(attrName == 'action')
			{
				if(tag=='form')
				{
					// the Setter_FormAction will be used instead
					value = value;
				}
				else // any other
					value = __UrlEncoder(value ,ENC_Any); 
			}
			else if(attrName == 'innerHtml')
			{
				value = _ASProxy.ParseHtml(value); 
			}
		}catch(e){_ASProxy.Log('_EncodeSetAttributeValue',e);}
		return value;
	}
	
	_ASProxy.SetAttribute= function(attr, value){
		try{ 
			if(attr.toLowerCase() == 'action' && this.tagName.toLowerCase()=='form'){
				_ASProxy.Setter_FormAction(this,value);
				// no any other action required
			}else{
				// encode the value
				value = _EncodeSetAttributeValue(attr, value, this.tagName);

				// call original method
				this.OriginalSetAttribute(attr, value);
			}
		}catch(e){_ASProxy.Log('_ASProxy.SetAttribute',e);}
	};
	
	// changes the form element and dosn't need any action
	_ASProxy.Setter_FormAction= function(element,value){
		// the element is a FORM element
		// value is ACTION of FORM element
		var frmMethod = element.attributes["methodorginal"];
		if(frmMethod == null)
			frmMethod = frmMethod.value;// attrib value
		else
			frmMethod = element.method;// FORM method value
			
		// setting done flag
		element.OriginalSetAttribute("asproxydone","1");
		element.OriginalSetAttribute("methodorginal" , frmMethod);
		
		var newFrmAction;
		
		// check if is encoded before
		if(_ASProxy.IsEncodedByASProxy(value)==false)
			// encodes action
			newFrmAction=__UrlEncoder(value,ENC_Page,false,"method="+frmMethod);
		else
			newFrmAction=value;

		// overriding the element
		//element.action=newFrmAction;
		element.OriginalSetAttribute("action", newFrmAction);
		element.method="POST";

		//Saving encoded url to monitor the changes
		element.OriginalSetAttribute("encodedurl", newFrmAction);
	};
	
	// Element setters
	_ASProxy.Setter_Src = function(value){this.OriginalSetAttribute('src',_EncodeSetAttributeValue('src',value,this.tagName));};
	_ASProxy.Setter_Href = function(value){this.OriginalSetAttribute('href',_EncodeSetAttributeValue('href',value,this.tagName));};
	_ASProxy.Setter_Background = function(value){this.OriginalSetAttribute('background',_EncodeSetAttributeValue('background',value,this.tagName));};
	_ASProxy.Setter_InnerHtml = function(value){this.OriginalSetAttribute('innerHtml',_EncodeSetAttributeValue('innerHtml',value,this.tagName));};
	_ASProxy.Setter_Action = function(value){
		try{ 
			if(this.tagName.toLowerCase()=='form'){
				_ASProxy.Setter_FormAction(this,value);
				// no any other action required
			}else{
				this.OriginalSetAttribute('action',_EncodeSetAttributeValue('action',value,this.tagName));
			}
		}catch(e){_ASProxy.Log('Setter_Action',e);}
	};
	
	// overriding elements properties
	for( i=0; i< interfaces.length ;i++ ){
		// element definition
		var elm = interfaces[i].prototype;
		
		// Does it have a prototype?
		if(elm==null) continue;
		
		// overridding element 'setAttribute' method
		elm.OriginalSetAttribute = elm.setAttribute;
		elm.setAttribute = _ASProxy.SetAttribute;

		// overridding element properties
		elm.__defineSetter__('src', _ASProxy.Setter_Src);
		elm.__defineSetter__('action', _ASProxy.Setter_Action);
		elm.__defineSetter__('href', _ASProxy.Setter_Href);
		elm.__defineSetter__('background', _ASProxy.Setter_Background);
		elm.__defineSetter__('innerHtml', _ASProxy.Setter_InnerHtml);

	}
}catch(e){_ASProxy.Log('OverrideHtmlSetters ALL',e);}
}

// Overriding standard functions
_ASProxy.OverrideStandardsDeclare=function(){ 
	_ASProxy.WindowOpen=function(url,name,features,replace){
		if(_ASProxy.IsEncodedByASProxy(url))
			return window.OriginalOpen(url,name,features,replace);
		else
			return window.OriginalOpen(__UrlEncoder(url),name,features,replace);
	}

	_ASProxy.LocationAssign = function(url) {
		// checking to see if it is not previously coded
		if(_ASProxy.IsEncodedByASProxy(url))
			window.location.href=url;
		else
		{
			url=__UrlEncoder(url);
			window.location.href=url;
		}
		return url;
	}

	_ASProxy.LocationReplace=function(url){
		var codedUrl;
		if(_ASProxy.IsEncodedByASProxy(url))
			codedUrl=url;
		else
			codedUrl=__UrlEncoder(url);
		if (window.location.replace == _ASProxy.LocationReplace)
			return window.LocationReplace(codedUrl);
		else
			return window.location.replace(codedUrl);
	}

	_ASProxy.LocationReload=function(){
		if (window.location.reload == _ASProxy.LocationReload)
			return window.LocationReload();
		else
			return window.location.reload();
	}

	_ASProxy.DocumentWrite=function(){
		var text=arguments[0];
		if(_ASProxy.ParseHtml){
			text=_ASProxy.ParseHtml(text);
			return document.OriginalWrite(text);
		}
	}

	_ASProxy.DocumentWriteLn=function(){
		var text=arguments[0];
		if(_ASProxy.ParseHtml){
			text=_ASProxy.ParseHtml(text);
			return document.OriginalWriteLn(text);
		}
	}
}
_ASProxy.OverrideStandards=function() {
	try{
		document.XDomain= _WindowLocation.host;
	}catch(e){_ASProxy.Log('document.XDOMAIN failed',e);}
	try{
		window.open=_ASProxy.WindowOpen;
	}catch(e){_ASProxy.Log('OVR window.open failed',e);}
	try{
		document.open=_ASProxy.WindowOpen;
	}catch(e){_ASProxy.Log('OVR document.open failed',e);}
	try{
		open=_ASProxy.WindowOpen;
	}catch(e){_ASProxy.Log('OVR open failed',e);}
	try{
		window.location.replace=_ASProxy.LocationReplace;
	}catch(e){_ASProxy.Log('OVR window.location failed',e);}
	try{
		location.replace=_ASProxy.LocationReplace;
	}catch(e){_ASProxy.Log('OVR location.replace failed',e);}
	
	try{
		document.write=_ASProxy.DocumentWrite;
	}catch(e){_ASProxy.Log('OVR document.write failed',e);}
	try{
		document.writeln=_ASProxy.DocumentWriteLn;
	}catch(e){_ASProxy.Log('OVR document.writeln failed',e);}
}

// ---------------------------
// END
// ---------------------------

// private: run encoders
_ASProxy.StartupDynamicEncoders=function() {
	//After 0.5 second 
	window.setTimeout("_ASProxy.Enc.EncodeLinks();",500);

	//After 0.7 second 
	window.setTimeout("_ASProxy.Enc.EncodeImages();",700);

	//After 2 second 
	window.setTimeout("_ASProxy.Enc.EncodeInputImages();",2000);

	//After 1 second 
	window.setTimeout("_ASProxy.Enc.EncodeForms();",1000);

	//After 2 second 
	window.setTimeout("_ASProxy.Enc.EncodeFrames();",2000);

	//After 4 second 
	window.setTimeout("_ASProxy.Enc.EncodeScriptSources();",4000);
}

_ASProxy.Initialize=function() {
	// Apply overriding
	_ASProxy.OverrideStandardsDeclare();
	
	// The replacement location object
	_WindowLocation=new _ASProxy.LocationObject();
	_ASProxy.ReqLocation = _WindowLocation;
	
	// Apply overriding
	_ASProxy.OverrideStandards();
	
	// Html setters
	_ASProxy.OverrideHtmlSetters();

	// start encoders
	_ASProxy.StartupDynamicEncoders();
}

// start immediately
_ASProxy.Initialize();

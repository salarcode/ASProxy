// ASProxy Dynamic Encoder
// ASProxy encoder for dynamically created objects //
// Last update: 2009-06-26 coded by Salar Khalilzadeh //

//_userConfig={EncodeUrl:true, OrginalUrl:true, Links:true, Images:true, Forms:true, Frames:true, Cookies:true};
//_reqInfo={pageUrl:'', pageUrlNoQuery:'', pagePath:'', rootUrl:'', cookieName:'',
//	ASProxyUrl:'', ASProxyPath:'', ASProxyRoot:'', ASProxyPageName:'', UrlUnknowner:''};
//_reqInfo.location={ Hash:'', Host:"", Hostname:"", Pathname:"", Search:'', Port:"", Protocol:"" };


// ASProxy object
_ASProxy={
	Debug:false,
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
//  UrlUnknowner:'B64Coded!'
//};

// regested site url information
//_reqInfo.location={
//	Hash:'#hi',
//	Host:"site.com:8080",
//	Hostname:"site.com",
//	Pathname:"/dir/page.htm",
//	Search:'?test=1',
//	Port:"8080",
//	Protocol:"http:"
//}

// ---------------------------
// UrlEncoder required methods
// ---------------------------

// public: encodes urls
// type: content type
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
	if(type==1)
		asproxyBasePath=_ASProxy.Pages.pgImages; // images
	else if(type==2)
		asproxyBasePath=_ASProxy.Pages.pgGetAny;
	else if(type==3)
		asproxyBasePath=_ASProxy.Pages.pgGetHtml;
	else
		asproxyBasePath=_reqInfo.ASProxyPageName;
	
	// use full usrl, only for debug
	if(typeof(__UseASProxyAbsoluteUrl)!='undefined' && __UseASProxyAbsoluteUrl)
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
	if(url==null) return false;
	url=url.toLowerCase();
	for (i = 0; i < _ASProxy.ClientSideUrls.length; i++)
	{
		if(_ASProxy.StringStartsWith(url,_ASProxy.ClientSideUrls[i]))
			return true;
	}
	return false;
}

// private: Is url virtual
_ASProxy.IsVirtualUrl=function(url){
	if(url==null) return true;
	url=url.toLowerCase();
	for (i = 0; i < _ASProxy.NonVirtualUrls.length; i++)
	{
		if(_ASProxy.StringStartsWith(url,_ASProxy.NonVirtualUrls[i]))
			return false;
	}
	return true;
}

// private: checks that url is a self bookmark in current page
_ASProxy.IsSelfBookmarkUrl=function(url){
	if(url==null) return false;
	url=url.toLowerCase();
	if(_ASProxy.StringStartsWith(url,'#'))
		return true;
	
	var location = window.location.href.toLowerCase()+'#';
	if(url.indexOf(location,0)==0)
		return true;
	return false;
}

// private: returns bookmark part of url
_ASProxy.ReturnBookmarkPart=function(url){
	if(url==null) return "";
	var pos=url.indexOf("#",0);
	if(pos!=-1){
		url=url.substring(pos,url.length);
		return url;
	}
	else return "";
}

// private: removes bookmark part of url
_ASProxy.RemoveBookmarkPart=function(url){
	if(url==null) return null;
	var result;
	var pos=url.indexOf("#",0);
	if(pos!=-1){
		url=url.substring(0,pos);
		return url;
	}
	else return url;
}

_ASProxy.StringStartsWith=function(str1,str2){
	if(str1.length == 0 || str1.length < str2.length) { return false; }
	return (str1.substr(0, str2.length) == str2);
}
_ASProxy.StringEndsWith=function(str1,str2){
	if(str1.length == 0 || str1.length < str2.length) { return false; }
	return (str1.substr(str1.length - str2.length) == str2);
}

// private: convert url to requested url if started with proxy address AND checks if this is not a client url
_ASProxy.CorrectLocalUrlToOrginalCheck =function(url) {
	if(_ASProxy.IsClientSideUrl(url) || _ASProxy.IsSelfBookmarkUrl(url))
		return requestUrl;
	return _ASProxy.CorrectLocalUrlToOrginal(url);
}

// private: converts url to requested url if it is started with proxy address
_ASProxy.CorrectLocalUrlToOrginal = function(requestUrl){
	if(requestUrl==null) return null;
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
	if(url==null) return _reqInfo.UrlUnknowner;
	var unknowner=_reqInfo.UrlUnknowner.toLowerCase();
	var urlAddr=url.toLowerCase();
	
	var add=!_ASProxy.StringEndsWith(urlAddr,unknowner);
	
	if(add) return url+_reqInfo.UrlUnknowner;
	else return url;
}

// ---------------------------
// END
// ---------------------------

_ASProxy.IsEncodedByASProxy = function(url){
	if(url==null) return false;
	url=url.toLowerCase();
	var baseUrl=_reqInfo.ASProxyPath.toLowerCase();
	for (i = 0; i < _ASProxy.EncodedUrls.length; i++)
	{
		if(_ASProxy.StringStartsWith(url,_ASProxy.EncodedUrls[i]))
			return true;
		else if(_ASProxy.StringStartsWith(url,baseUrl+_ASProxy.EncodedUrls[i]))
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
		try{ o["on" + evType] = f; }catch(e){}
	}
}catch(e){}
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
				item.setAttribute("encodedurl",propValue);
				
				// if frames is proccessing and additional key is available
				if(contentType==3 && additionalKey){
					item.setAttribute(additionalKey,additionalValue);
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
			item.setAttribute("asproxydone","1");
			
			// set original address
			item.setAttribute("originalurl",_ASProxy.GetBookmarkOnlyForCurrentPage(propValueFull));

			// if frames is proccessing and additional key is available
			if(contentType==3 && additionalKey){
				item.setAttribute(additionalKey,additionalValue);
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
				//newValue= ASProxyEncoder(newValue,contentType,false,null,true);
			
			// apply new value
			item.setAttribute(propName , newValue);
			item[propName] = newValue;
			
			//Saving base64 coded url to monitor changes
			item.setAttribute("encodedurl" , newValue);
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
			frm.setAttribute("asproxydone","1");
			frm.setAttribute("methodorginal" , frmMethod);

			var newFrmAction=__UrlEncoder(frmAction,0,false,"method="+frmMethod);
			//var newFrmAction=ASProxyEncoder(frmAction,0,true,frmMethod);

			frm.action=newFrmAction;
			frm.setAttribute("action", newFrmAction);

			frm.setAttribute("encodedurl", newFrmAction);//Saving encoded url to monitor the changes

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
	}catch(e){}

	//After 4 seconds
	window.setTimeout("_ASProxy.Enc.EncodeFrames();",5000);
}

// private: encode all links
_ASProxy.Enc.EncodeLinks=function(){
	if(_userConfig.Links!=true) return;
	try{
	_ASProxy.Enc.EncodeElements(document.links,"href",0,true);
	}catch(e){}

	//After 1 second 
	window.setTimeout("_ASProxy.Enc.EncodeLinks();",1000);
}

// private: encode all images
_ASProxy.Enc.EncodeImages=function(){
	if(_userConfig.Images!=true) return;
	try{
	_ASProxy.Enc.EncodeElements(document.images,"src",1,true);
	}catch(e){}

	//After 1 second
	window.setTimeout("_ASProxy.Enc.EncodeImages();",1000);
}

// private: encode all input images
_ASProxy.Enc.EncodeInputImages=function(){
	if(_userConfig.Images!=true) return;
	
	var inputImages=document.documentElement.getElementsByTagName("input");
	try{
	_ASProxy.Enc.EncodeElements(inputImages,"src",1,true,"type","image");
	}catch(e){}

	//After 4 seconds
	window.setTimeout("_ASProxy.Enc.EncodeInputImages();",4000);
}

// private: encode all scripts
_ASProxy.Enc.EncodeScriptSources=function(){
	var scripts=document.documentElement.getElementsByTagName("script");
	try{
	_ASProxy.Enc.EncodeElements(scripts,"src",2,false);
	}catch(e){}

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
	var toGetCName=_CookieName;
	if(toGetCName==null)
		toGetCName = _reqInfo.cookieName;
	var cookieString = document.cookie;
		
	var aCookie = cookieString.split("; ");
	for (var i=0; i < aCookie.length; i++)
	{
		var aCrumb = aCookie[i].split("=");
		if (toGetCName == aCrumb[0]) 
			return unescape(aCrumb[1]);
	}
	return "";
}

// public: Wrapping cookie operation in javascript
// sCookie: cookie string to set
function __CookieSet(sCookie){
var cookieSettingValues="";
function GetCookie(cookieString,sName){
	var aCookie = cookieString.split("; ");
	for (var i=0; i < aCookie.length; i++)
	{
		var aCrumb = aCookie[i].split("=");
		if (sName == aCrumb[0]) 
			return unescape(aCrumb[1]);
	}
	return null;
}
function EscapeCookie(value){
	return _reqInfo.cookieName + "=" + escape(value) + "; path=/;"+cookieSettingValues;
}
function JoinCookies(preCookie,newCookie){
	cookieSettingValues="";
	var aPreCookie = preCookie.split("; ");
	var aNewCookie = newCookie.split("; ");
	var resultCookie="";
	var tempColl=new Array();
	for (var i=0; i < aPreCookie.length; i++)
	{
		var preCrumb = aPreCookie[i].split("=");
		var preName=preCrumb[0];
		if(preName=="") continue;
		
		if(preName=="expires" || preName=="secure")
		{
			cookieSettingValues += aPreCookie[i] + "; ";
			continue;
		}
		
		var exists=false;
		for(var newI=0; newI < aNewCookie.length; newI++)
		{
			var newCrumb = aNewCookie[newI].split("=");
			var newName=newCrumb[0];
			if(preName==newName)
			{
				exists=true;
				break;
			}
		}
		if(exists==false)
			resultCookie += aPreCookie[i] + "; ";
	}
	
	for(var newI=0; newI < aNewCookie.length; newI++)
	{
		resultCookie += aNewCookie[newI] + "; ";
	}
	if(cookieSettingValues!="")
		cookieSettingValues="; "+cookieSettingValues;
	return resultCookie;	
}

var previousCookie=GetCookie(document.cookie, _reqInfo.cookieName);
if(previousCookie!="" && previousCookie!=null)
	previousCookie=(previousCookie);
else
	previousCookie="";

// join cookies
var __cookie=JoinCookies(previousCookie,sCookie);
return EscapeCookie(__cookie);
}
// ---------------------------
// END
// ---------------------------

// ---------------------------
// Client processors
// ---------------------------
// parses html codes
_ASProxy.ParseHtml=function(codes){
	if(typeof(codes)!='string')
		return codes;
		
	// can be anything
	var pattern = /\.(src)\s*=\s*([^;}]+)/ig;
	codes =codes.replace(pattern,".$1=__UrlEncoder($2,2)");

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
		codes = codes.replace(match[0],' ' + match[1] + '=' + match[2] + __UrlEncoder(match[3],2) );
	}

	// processing styles url
	pattern = /url\s*\(['"]?([^'"\)]+)['"]?\)/ig;
	while ( match = pattern.exec(codes) )
		codes = codes.replace(match[0],'url('+__UrlEncoder(match[1],2)+')');

    // Css import rule
    pattern = /@import\s*['"]([^'"\(\)]+)['"]/ig;
    while ( match = pattern.exec(codes) )
        codes = codes.replace(match[0],'@import "'+__UrlEncoder(match[1],2)+'"');

	return codes;
}

// parses javascript codes
_ASProxy.ParseJs=function(codes){

}

// ---------------------------
// END
// ---------------------------

// Overriding standard functions
_ASProxy.OverrideStandards=function() {
	_ASProxy.WindowOpen=function(url,name,features,replace){
		if(_ASProxy.IsEncodedByASProxy(url))
			return window.OriginalOpen(url,name,features,replace);
		else
			return window.OriginalOpen(__UrlEncoder(url),name,features,replace);
	}

	_ASProxy.LocationAssign=function(url){
		if(_ASProxy.IsEncodedByASProxy(url))
			return window.LocationAssign(url);
		else
			return window.LocationAssign(__UrlEncoder(url));
	}

	_ASProxy.LocationReplace=function(url){
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

	try{
		window.open=_ASProxy.WindowOpen;
	}catch(e){}
	try{
		document.open=_ASProxy.WindowOpen;
	}catch(e){}
	try{
		open=_ASProxy.WindowOpen;
	}catch(e){}
	try{
		window.location.replace=_ASProxy.LocationReplace;
	}catch(e){}
	try{
		location.replace=_ASProxy.LocationReplace;
	}catch(e){}
	
	try{
		document.write=_ASProxy.DocumentWrite;
	}catch(e){}
	try{
		document.writeln=_ASProxy.DocumentWriteLn;
	}catch(e){}
}

// private: override standards
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
	// The replacement location object
	_WindowLocation=new _ASProxy.LocationObject();
	_ASProxy.ReqLocation = _WindowLocation;
	
	// Apply overriding
	_ASProxy.OverrideStandards();

	// start encoders
	_ASProxy.StartupDynamicEncoders();
}

// start
_ASProxy.Initialize();
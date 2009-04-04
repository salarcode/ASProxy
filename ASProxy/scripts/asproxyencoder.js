// ASProxy Dynamic Encoder
// ASProxy encoder for dynamically created objects //
// Last update: 2009-01-17 coded by Salar Khalilzadeh //

//__ASProxyEncodeUrl=1;
//__ASProxyOriginalUrlEnabled=1;
//__ASProxyLinksEnabled=1;
//__ASProxyImagesEnabled=1;
//__ASProxyBackImagesEnabled=1;
//__ASProxyFormsEnabled=1;
//__ASProxyFramesEnabled=1;
//__ReqUrlFull					= 'http://blocked.com:8080/sub/hi/tester.html?param=1&hi=2';
//__ReqUrlNoParam				= 'http://blocked.com:8080/sub/hi/tester.html';
//__ReqUrlDir					= 'http://blocked.com:8080/sub/hi/';
//__ReqUrlBaseDir				= 'http://blocked.com:8080/';
//__ASProxyDefaultPagePath		= 'http://site.com:8080/asproxy/default.aspx'; or 'http://site.com/default.aspx';
//__ASProxySiteBaseDir		    = 'http://site.com:8080/asproxy/'; or 'http://site.com/';
//__ASProxySiteHostBaseDir      = 'http://site.com:8080/'; or 'http://site.com/'
//__ASProxyDefaultPage			= 'default.aspx';
//__B64Unknowner				= "B64Coded!";
//__ReqCookieName				= "localhost_Cookies"; // Cookie name used in javascript cookie operations

// Used to set full ASProxy urls, set true only for debug purposes!
//__UseASProxyAbsoluteUrl		=false;	

//Window location objects
//__WLocHash='#hi';__WLocHost="site.com:8080";__WLocHostname="site.com";__WLocPathname="/dir/page.htm";__WLocSearch='?test=1';__WLocPort="8080";__WLocProtocol="http:";

// Defining ASProxy object
if(typeof _ASProxy=='undefined')
	_ASProxy=function(){};

// global variables holds standard methods instance
var _OriginalWindowOpen=window.open;
var _OriginalLocationReplace=window.location.replace;
var _OriginalLocationAssign=window.location.assign;


// public: encodes urls
// type: content type
// addFormMethod: should http method be added, only for forms
// formMethod: http method value
// notCorrectLocalUrl: shouldn't correct url address to original site
function ASProxyEncoder(url,type,addFormMethod,formMethod,notCorrectLocalUrl){
	if(_ASProxy.IsClientSideUrl(url) || _ASProxy.IsSelfBookmarkUrl(url))
		return url;
	
	if(notCorrectLocalUrl!=true)
		url=_ASProxy.CorrectLocalUrlToOrginal(url);
	
	if(_ASProxy.IsVirtualUrl(url))
		url=_ASProxy.JoinUrls(__ReqUrlDir,__ReqUrlBaseDir,url);

	var asproxyBasePath;
	if(type==1)
		asproxyBasePath='images.aspx';
	else if(type==2)
		asproxyBasePath='getany.aspx';
	else if(type==3)
		asproxyBasePath='gethtml.aspx';
	else
		asproxyBasePath=__ASProxyDefaultPage;
	
	// use full usrl, only for debug
	if(typeof(__UseASProxyAbsoluteUrl)!='undefined' && __UseASProxyAbsoluteUrl)
		asproxyBasePath=__ASProxySiteBaseDir + asproxyBasePath;
		
	asproxyBasePath+='?decode='+__ASProxyEncodeUrl+'&url=';
	var bookmark;
	var result;
	bookmark=_ASProxy.ReturnBookmarkPart(url);

	if(bookmark!="")
		url=_ASProxy.RemoveBookmarkPart(url);

	result=asproxyBasePath;
	if(__ASProxyEncodeUrl)
		result+=_ASProxy.B64UnknownerAdd(_Base64_encode(url));
	else
		result+=url;
	if(addFormMethod)
		result+="&method="+(formMethod);
	result=(result)+bookmark;
	return result;
}

// public: encodes all forms
function ASProxyEncodeForms(){
if(__ASProxyFormsEnabled!=1)
	return;
var i=0; var frm;
for(i=0;i<document.forms.length;i++)
{
	frm=document.forms.item(i);

	var frmAction=frm.attributes["action"];
	if(frmAction!=null)
		frmAction=frmAction.value;
	else
		frmAction=frm.action;

	if(_ASProxy.IsEncodedByASProxy(frmAction)==false){
		
		var applyEncoding=false;
		var isDone=frm.attributes["asproxydone"];
		
		var frmMethod=frm.method;
		
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
			
			var newFrmAction=ASProxyEncoder(frmAction,0,true,frmMethod);
			frm.action=newFrmAction;
			frm.setAttribute("action", newFrmAction);
			
			frm.setAttribute("encodedurl", newFrmAction);//Saving encoded url to monitor the changes
			
			//override from method
			frm.method="POST";
		}
	}
}
//After 2 seconds
window.setTimeout("ASProxyEncodeForms();",2000);
}

// private: Collection of items encoder
// contentType: 0-page, 1-images, 2-anything, 3-frames
// conditionProp,conditionValue: optional condition to proccess an element
// additionalKey,additionalValue: additional attribute to add to element
_ASProxy.ASProxyEncodeElements=function(elementsArray,propName,contentType,applyFloatBar,conditionProp,conditionValue,additionalKey,additionalValue)
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
			item.setAttribute("originalurl",propValueFull);

			// if frames is proccessing and additional key is available
			if(contentType==3 && additionalKey){
				item.setAttribute(additionalKey,additionalValue);
			}

			// set float bar variables
			if(applyFloatBar && __ASProxyOriginalUrlEnabled){
				if(typeof ORG_IN_!='undefined')
					_ASProxy.AttachEvent(item,"mouseover",function(){ORG_IN_(this)});
				if(typeof ORG_OUT_!='undefined')
					_ASProxy.AttachEvent(item,"mouseout",function(){ORG_OUT_()});
			}

			if(propValue==window.location && newValue==propValue)
				continue;
			else
				newValue= ASProxyEncoder(newValue,contentType,false,null,true);
			
			// apply new value
			item.setAttribute(propName , newValue);
			item[propName] = newValue;
			
			//Saving base64 coded url to monitor changes
			item.setAttribute("encodedurl" , newValue);
		}
		
	}
}}

// public: Returns current site cookies
function ASProxyGetCookie(_CookieName){
	var toGetCName=_CookieName;
	if(toGetCName==null)
		toGetCName=__ReqCookieName;
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
function ASProxySetCookie(sCookie){
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
	return __ReqCookieName + "=" + escape(value) + "; path=/;"+cookieSettingValues;
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

var previousCookie=GetCookie(document.cookie,__ReqCookieName);
if(previousCookie!="" && previousCookie!=null)
	previousCookie=(previousCookie);
else
	previousCookie="";

// join cookies
var __cookie=JoinCookies(previousCookie,sCookie);
return EscapeCookie(__cookie);
}

// Overriding standard functions
_ASProxy.OverrideStandardFunctions=function()
{
	_ASProxy.WindowOpen=function(url,name,features,replace){
		if(_ASProxy.IsEncodedByASProxy(url))
			return _OriginalWindowOpen(url,name,features,replace);
		else
			return _OriginalWindowOpen(ASProxyEncoder(url),name,features,replace);
	}

	_ASProxy.LocationAssign=function(url){
		if(_ASProxy.IsEncodedByASProxy(url))
			return _OriginalLocationAssign(url);
		else
			return _OriginalLocationAssign(ASProxyEncoder(url));
	}

	_ASProxy.LocationReplace=function(url){
		// checking to see if it is not previously coded
		if(_ASProxy.IsEncodedByASProxy(url))
			window.location.href=url;
		else
		{
			url=ASProxyEncoder(url);
			window.location.href=url;
		}
		return url;
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
}

// The location object will be replaced by this object
_ASProxy.WindowLocationObject=function(){
	this.search=__WLocSearch;
	this.href=__ReqUrlFull;
	this.hash=(window.location.hash!=null && window.location.hash!='')?window.location.hash:__WLocHash;
	this.host=__WLocHost;
	this.hostname=__WLocHostname;
	this.pathname=__WLocPathname;
	this.port=__WLocPort;
	this.protocol=__WLocProtocol;
	this.replace=_ASProxy.LocationReplace;
	this.assign=_ASProxy.LocationAssign;
	this.URL=__ReqUrlFull;
	
	this.toString=function(){return __ReqUrlFull;};
	this.toLocaleString=function(){return __ReqUrlFull;};
	this.length=this.href.length;
	//this.replace=this.href.replace;this.search=this.href.search;
	this.anchor=this.href.anchor;this.big=this.href.big;this.blink=this.href.blink;this.bold=this.href.bold;this.charAt=this.href.charAt;
	this.charCodeAt=this.href.charCodeAt;this.fixed=this.href.fixed;this.fontcolor=this.href.fontcolor;this.fontsize=this.href.fontsize;
	this.fromCharCode=this.href.fromCharCode;this.indexOf=this.href.indexOf;this.italics=this.href.italics;this.lastIndexOf=this.href.lastIndexOf;
	this.link=this.href.link;this.match=this.href.match;this.slice=this.href.slice;
	this.small=this.href.small;this.split=this.href.split;this.strike=this.href.strike;this.sub=this.href.sub;this.substr=this.href.substr;
	this.substring=this.href.substring;this.toLowerCase=this.href.toLowerCase;this.toUpperCase=this.href.toUpperCase;
};

// Applying favicon
_ASProxy.ApplyFavicon=function(){
    var docHead=document.getElementsByTagName('head');
    if(docHead!=null && docHead.length>0)
		docHead=docHead[0];
	else return;

	function ApplyExisting(){
	var links = document.getElementsByTagName("link");
	for (var i=0; i<links.length; i++) {
		var link = links[i];
		var isType=(link.type=="image/x-icon");
		var isHref=(link.href.toLowerCase().lastIndexOf('.ico')!=-1);
		var isRel=(link.rel.toLowerCase().indexOf("icon")!=-1);
		if ((isType && isRel) || (isHref && isRel) || (isType && isHref)){
			if(_ASProxy.IsEncodedByASProxy(link.href)==false)
				link.href=ASProxyEncoder(link.href,1);
			docHead.appendChild(link);
			return true;
		}
	}
	return false;
	}

	if(ApplyExisting()==true) return;
	// not decided about yet
}

// public: encode all frames
function ASProxyEncodeFrames()
{
	if(__ASProxyFramesEnabled!=1) return;

	var frames=document.documentElement.getElementsByTagName("iframe");
	try{
	_ASProxy.ASProxyEncodeElements(frames,"src",3,false,null,null,"onload","ASProxyEncodeFrames()");
	}catch(e){}

	//After 4 seconds
	window.setTimeout("ASProxyEncodeFrames();",5000);
}

// public: encode all links
function ASProxyEncodeLinks()
{
	if(__ASProxyLinksEnabled!=1) return;
	try{
	_ASProxy.ASProxyEncodeElements(document.links,"href",0,true);
	}catch(e){}

	//After 1 second 
	window.setTimeout("ASProxyEncodeLinks();",1000);
}

// public: encode all images
function ASProxyEncodeImages()
{
	if(__ASProxyImagesEnabled!=1) return;
	try{
	_ASProxy.ASProxyEncodeElements(document.images,"src",1,true);
	}catch(e){}

	//After 1 second
	window.setTimeout("ASProxyEncodeImages();",1000);
}

// public: encode all input images
function ASProxyEncodeInputImages()
{
	if(__ASProxyImagesEnabled!=1) return;
	
	var inputImages=document.documentElement.getElementsByTagName("input");
	try{
	_ASProxy.ASProxyEncodeElements(inputImages,"src",1,true,"type","image");
	}catch(e){}

	//After 4 seconds
	window.setTimeout("ASProxyEncodeInputImages();",4000);
}


// ---------------------------------
// Internal functions

// private: Is url already encoded
_ASProxy.EncodedUrls=new Array(__ASProxyDefaultPage+"?decode=","images.aspx?decode=","getjs.aspx?decode=","getcss.aspx?decode=","gethtml.aspx?decode=","download.aspx?decode=","authorization.aspx?decode=","getany.aspx?decode=");
_ASProxy.IsEncodedByASProxy = function(url)
{
	if(url==null) return false;
	url=url.toLowerCase();
	var baseUrl=__ASProxySiteBaseDir.toLowerCase();
	for (i = 0; i < _ASProxy.EncodedUrls.length; i++)
	{
		if(_ASProxy.StringStartsWith(url,_ASProxy.EncodedUrls[i]))
			return true;
		else if(_ASProxy.StringStartsWith(url,baseUrl+_ASProxy.EncodedUrls[i]))
			return true;
	}
	return false;
}

// private: Adds url unknowner
_ASProxy.B64UnknownerAdd = function(url)
{
	if(url==null) return __B64Unknowner;
	var unknowner=__B64Unknowner.toLowerCase();
	var urlAddr=url.toLowerCase();
	
	var add=!_ASProxy.StringEndsWith(urlAddr,unknowner);
	
	if(add)
		return url+__B64Unknowner;
	else
		return url;
}


// private: Removes url unknowner
_ASProxy.B64UnknownerRemover = function(url)
{
	if(url==null) return null;
	var unknowner=__B64Unknowner.toLowerCase();
	var urlAddr=url.toLowerCase();
	var position=urlAddr.indexOf(unknowner);
	if(position>-1)
		return url.substring(0,position);
	else
		return url;
}

// private: convert url to requested url if started with proxy address AND checks if this is not a client url
_ASProxy.CorrectLocalUrlToOrginalCheck =function(requestUrl)
{
	if(_ASProxy.IsClientSideUrl(requestUrl) || _ASProxy.IsSelfBookmarkUrl(requestUrl))
		return requestUrl;
	return _ASProxy.CorrectLocalUrlToOrginal(requestUrl);
}


// private: converts url to requested url if it is started with proxy address
_ASProxy.CorrectLocalUrlToOrginal = function(requestUrl)
{
	if(requestUrl==null) return null;
	var url=requestUrl.toLowerCase();
	var baseBasePath=__ASProxySiteHostBaseDir.toLowerCase();
	var basePath=__ASProxySiteBaseDir.toLowerCase();
	var pagePath=__ASProxyDefaultPagePath.toLowerCase();
	var willBeBase=__ReqUrlNoParam; // Current url without parameters
	var addSeperator=false;

	// Checking page url
	var position=url.indexOf(pagePath,0);
	var pathLength=pagePath.length;

	if(position==-1){
		// Checking site url
		position=url.indexOf(basePath,0);
		pathLength=basePath.length;
		willBeBase=__ReqUrlDir;
		addSeperator=true;
	}

	if(position==-1){
		// Checking host url
		position=url.indexOf(baseBasePath,0);
		pathLength=baseBasePath.length;
		willBeBase=__ReqUrlBaseDir; // root of current page
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
			
		return willBeBase+seperator+reqUrl;
	}
	else
		return requestUrl;
}

// private: combine page url with base path and return full url
// exam: in "http://test.com/sub/" site, cenverts "/page.htm" to "http://test.com/page.htm" 
//	 and converts "page.htm" to "http://test.com/sub/page.htm"
_ASProxy.JoinUrls = function(pageBaseUrl,siteBaseUrl,requestPageUrl)
{
	if(pageBaseUrl.lastIndexOf("/")!=pageBaseUrl.length-1)
		pageBaseUrl+="/";

	if(siteBaseUrl.lastIndexOf("/")!=siteBaseUrl.length-1)
		siteBaseUrl+="/";

	var result;
	if(requestPageUrl.indexOf("/",0)==0)// check if slash is in the first
		result= siteBaseUrl+"."+requestPageUrl;
	else
		result= pageBaseUrl+requestPageUrl;
	return result;
}

// private: removes bookmark part of url
_ASProxy.RemoveBookmarkPart=function(url)
{
	if(url==null) return null;
	var result;
	var pos=url.indexOf("#",0);
	if(pos!=-1){
		url=url.substring(0,pos);
		return url;
	}
	else
		return url;
}

// private: returns bookmark part of url
_ASProxy.ReturnBookmarkPart=function(url)
{
	if(url==null) return "";
	var pos=url.indexOf("#",0);
	if(pos!=-1){
		url=url.substring(pos,url.length);
		return url;
	}
	else return "";
}

// private: Is url client-side
_ASProxy.ClientSideUrls = new Array( "mailto:", "file://", "javascript:", "vbscript:", "jscript:", "vbs:","ymsgr:","data:" );
_ASProxy.IsClientSideUrl=function(url)
{
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
_ASProxy.NotVirtualUrls= new Array( "http://", "https://", "mailto:", "ftp://", "file://", "telnet://", "news://", "nntp://", "ldap://","ymsgr:", "javascript:", "vbscript:", "jscript:", "vbs:", "data:"  );
_ASProxy.IsVirtualUrl=function(url)
{
	if(url==null) return true;
	url=url.toLowerCase();
	for (i = 0; i < _ASProxy.NotVirtualUrls.length; i++)
	{
		if(_ASProxy.StringStartsWith(url,_ASProxy.NotVirtualUrls[i]))
			return false;
	}
	return true;
}

// private: checks that url is a self bookmark in current page
_ASProxy.IsSelfBookmarkUrl=function(url)
{
	if(url==null) return false;
	url=url.toLowerCase();
	if(_ASProxy.StringStartsWith(url,'#'))
		return true;
	
	var location=window.location.href.toLowerCase()+'#';
	if(url.indexOf(location,0)==0)
		return true;
	return false;
}

_ASProxy.StringStartsWith=function(str1,str2) {
	if(str1.length == 0 || str1.length < str2.length) { return false; }
	return (str1.substr(0, str2.length) == str2);
}
_ASProxy.StringEndsWith=function(str1,str2) {
	if(str1.length == 0 || str1.length < str2.length) { return false; }
	return (str1.substr(str1.length - str2.length) == str2);
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

// --------------------------
// Apply overriding
_ASProxy.OverrideStandardFunctions();

// The replacement location object
_WindowLocation=new _ASProxy.WindowLocationObject();

// Apply favicon
window.setTimeout("_ASProxy.ApplyFavicon()",1000);
_ASProxy.AttachEvent(window,"load",function(){_ASProxy.ApplyFavicon()});

//After 0.5 second 
window.setTimeout("ASProxyEncodeLinks();",500);

//After 0.7 second 
window.setTimeout("ASProxyEncodeImages();",700);

//After 2 second 
window.setTimeout("ASProxyEncodeInputImages();",2000);

//After 1 second 
window.setTimeout("ASProxyEncodeForms();",1000);

//After 2 second 
window.setTimeout("ASProxyEncodeFrames();",2000);

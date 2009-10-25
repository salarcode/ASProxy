// ASProxy AJAX Wrapper Core
// Last update: 2009-06-26 coded by Salar Khalilzadeh //
var _AJAXWrapperHandler="ajax.ashx";Object.extend=function(dest,source,replace){for(var prop in source){if(replace==false&&dest[prop]!=null){continue;}
dest[prop]=source[prop];}
return dest;};function _ArrayAdd(arr,name,value){arr[name]=value;}
if(typeof XMLHttpRequest!='undefined')
_OriginalXMLHttpRequest=XMLHttpRequest;else
_OriginalXMLHttpRequest=null;_AJAXInternal=function(){try{return new _OriginalXMLHttpRequest();}catch(e){}
try{return new ActiveXObject("Msxml2.XMLHTTP.6.0")}catch(e){}
try{return new ActiveXObject("Msxml2.XMLHTTP.3.0")}catch(e){}
try{return new ActiveXObject("Msxml2.XMLHTTP")}catch(e){}
try{return new ActiveXObject("Microsoft.XMLHTTP")}catch(e){}};XMLHttpRequest=function(){};Object.extend(XMLHttpRequest.prototype,{_ajax:new _AJAXInternal()},true);Object.extend(XMLHttpRequest.prototype,{_headers:new Array(),_async:false,_reqUrl:'',_caller:null,_refresh:function(){_caller=this;this._attachAllEvent();},_attachAllEvent:function(){try{this._ajax.onreadystatechange=this._readystatechange;}catch(e){}
try{this._ajax.onload=this._load;}catch(e){}
try{if(this.onerror!=null)
this._ajax.onerror=this._error;}catch(e){}
try{if(this.onprogress!=null)
this._ajax.onprogress=this._progress;}catch(e){}
try{if(this.onabort!=null)
this._ajax.onabort=this._abort;}catch(e){}
try{if(this.ontimeout!=null)
this._ajax.ontimeout=this._timeout;}catch(e){}
try{if(this.onuploadprogress!=null)
this._ajax.onuploadprogress=this._uploadprogress;}catch(e){}
try{if(this.onloadstart!=null)
this._ajax.onloadstart=this._loadstart;}catch(e){}},_updateProperties:function(){try{this.responseText=this._ajax.responseText;}catch(e){}
try{this.responseXML=this._ajax.responseXML;}catch(e){}
try{this.status=this._ajax.status;}catch(e){}
try{this.statusText=this._ajax.statusText;}catch(e){}
try{this.readyState=this._ajax.readyState;}catch(e){}
try{this.responseBody=this._ajax.responseBody;}catch(e){}
try{this.multipart=this._ajax.multipart;}catch(e){}},_readystatechange:function(eventArgs){if(_caller!=null&&typeof _caller._updateProperties!='undefined')
{_caller._updateProperties();if(_caller.onreadystatechange!=null)
_caller.onreadystatechange(eventArgs);}},_load:function(eventArgs){if(_caller!=null&&typeof _caller._updateProperties!='undefined')
{_caller._updateProperties();if(_caller.onload!=null)
_caller.onload(eventArgs);}},_error:function(eventArgs){if(_caller!=null&&typeof _caller._updateProperties!='undefined')
{_caller._updateProperties();if(_caller.onerror!=null)
_caller.onerror(eventArgs);}},_progress:function(eventArgs){if(_caller!=null&&typeof _caller._updateProperties!='undefined')
{_caller._updateProperties();if(_caller.onprogress!=null)
_caller.onprogress(eventArgs);}},_abort:function(eventArgs){if(_caller!=null&&typeof _caller._updateProperties!='undefined')
{_caller._updateProperties();if(_caller.onabort!=null)
_caller.onabort(eventArgs);}},_timeout:function(eventArgs){if(_caller!=null&&typeof _caller._updateProperties!='undefined')
{_caller._updateProperties();if(_caller.ontimeout!=null)
_caller.ontimeout(eventArgs);}},_uploadprogress:function(eventArgs){if(_caller!=null&&typeof _caller._updateProperties!='undefined')
{_caller._updateProperties();if(_caller.onuploadprogress!=null)
_caller.onuploadprogress(eventArgs);}},_loadstart:function(eventArgs){if(_caller!=null&&typeof _caller._updateProperties!='undefined')
{_caller._updateProperties();if(_caller.onloadstart!=null)
_caller.onloadstart(eventArgs);}},UNSENT:0,OPENED:1,HEADERS_RECEIVED:2,LOADING:3,DONE:4,SECURITY_ERR:18,NETWORK_ERR:19,ABORT_ERR:20,abort:function(){this._refresh();this._ajax.abort();this._updateProperties();},getAllResponseHeaders:function(){this._refresh();this._updateProperties();return this._ajax.getAllResponseHeaders();},getResponseHeader:function(headerName){this._refresh();this._updateProperties();return this._ajax.getResponseHeader(headerName);},open:function(method,URL,async,userName,password){if(arguments.length<3)
async=true;this._reqUrl=URL;URL=this._EncodeAJAXUrl(method,URL,userName,password);method=this._ASProxyEncodeAJAXMethod(method);this._async=async;this._refresh();this._ajax.open(method,URL,async,userName,password);this._updateProperties();},send:function(content){var asproxyAJAXH=this._EncodeArray(this._headers);this._ajax.setRequestHeader("X-ASProxy-AJAX-Headers",asproxyAJAXH);this._ajax.setRequestHeader("X-ASProxy-AJAX-Referrer",_reqInfo.pageUrl);if(content&&content.nodeType){content=window.XMLSerializer?new window.XMLSerializer().serializeToString(content):content.xml;if(!this._headers["Content-Type"])
this._ajax.setRequestHeader("Content-Type","application/xml");}
this._refresh();this._ajax.send(content);this._updateProperties();},sendAsBinary:function(content){var asproxyAJAXH=this._EncodeArray(this._headers);this._ajax.setRequestHeader("X-ASProxy-AJAX-Headers",asproxyAJAXH);this._ajax.setRequestHeader("X-ASProxy-AJAX-Referrer",_reqInfo.pageUrl);this._refresh();this._ajax.sendAsBinary(content);this._updateProperties();},setRequestHeader:function(label,value){_ArrayAdd(this._headers,label,value);this._refresh();this._ajax.setRequestHeader(label,value);this._updateProperties();},overrideMimeType:function(mimetype){this._refresh();this._ajax.overrideMimeType(mimetype);this._updateProperties();},toString:function(){return'['+"XMLHttpRequest"+']';},readyState:0,responseText:"",responseXML:null,responseBody:0,status:0,statusText:"",channel:null,multipart:false,onreadystatechange:null,onload:null,onerror:null,onprogress:null,onabort:null,ontimeout:null,onuploadprogress:null,onloadstart:null},true);Object.extend(XMLHttpRequest.prototype,{_ASProxyEncodeAJAXMethod:function(method){if(method==null)return method;var m=method.toLowerCase();if(m=="post")
return"POST";else
return"GET";},_EncodeArray:function(arrObject){var result="{";for(var key in arrObject){var itemValue=arrObject[key];if(itemValue!=null&&typeof itemValue!='function')
result+='"'+key+'"|#|'+'"'+itemValue+'"|^|';}
result+="}";return result;},_ASProxyEncoderAJAXUrl:function(method,URL,userName,password){var url=URL;var result;if(_ASProxy.IsClientSideUrl(url)||_ASProxy.IsSelfBookmarkUrl(url))
return url;url=_ASProxy.CorrectLocalUrlToOrginal(url);if(_ASProxy.IsVirtualUrl(url))
url=_ASProxy.JoinUrls(url,_reqInfo.pagePath,_reqInfo.rootUrl);var asproxyBasePath=_AJAXWrapperHandler;asproxyBasePath+='?dec='+(_userConfig.EncodeUrl+0)+'&ajaxurl=';result=asproxyBasePath;if(_userConfig.EncodeUrl)
result+=_ASProxy.B64UnknownerAdd(_Base64_encode(url));else
result+=url;result+="&method="+(method);if(userName!=null)
result+="&use="+_ASProxy.B64UnknownerAdd(_Base64_encode(userName));if(password!=null)
result+="&pas="+_ASProxy.B64UnknownerAdd(_Base64_encode(password));return result;},_EncodeAJAXUrl:function(method,URL,userName,password){return this._ASProxyEncoderAJAXUrl(method,URL,userName,password);}},true);if(_OriginalXMLHttpRequest&&_OriginalXMLHttpRequest.wrapped)
XMLHttpRequest.wrapped=_OriginalXMLHttpRequest.wrapped;
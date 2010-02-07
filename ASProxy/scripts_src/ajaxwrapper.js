// ASProxy AJAX Wrapper Core
// Last update: 2010-01-23 coded by Salar.Kh //

// Utilities
Object.extend = function(dest, source, replace) {
    for (var prop in source) {
        if (replace == false && dest[prop] != null) { continue; }
        dest[prop] = source[prop];
    }
    return dest;
};

function _ArrayAdd(arr, name, value) {
    arr[name] = value;
}

// internal ajax object
if (typeof XMLHttpRequest != 'undefined')
    _ASProxyXMLHttpRequest = XMLHttpRequest;
else
    _ASProxyXMLHttpRequest = null;

_AJAXInternal = function() {
    try { return new _ASProxyXMLHttpRequest(); } catch (e) { }
    try { return new ActiveXObject("Msxml2.XMLHTTP.6.0") } catch (e) { }
    try { return new ActiveXObject("Msxml2.XMLHTTP.3.0") } catch (e) { }
    try { return new ActiveXObject("Msxml2.XMLHTTP") } catch (e) { }
    try { return new ActiveXObject("Microsoft.XMLHTTP") } catch (e) { }
};

// overriding XMLHttpRequest object
XMLHttpRequest = function() { };

Object.extend(XMLHttpRequest.prototype, {
    // internal ajax object
    _ajax: new _AJAXInternal()
}, true);

Object.extend(XMLHttpRequest.prototype, {
    //---Internal uses---

    _headers: new Array()
	,

    // Save async parameter 
    _async: false
	,

    // Save request url
    _reqUrl: ''
	,

    // a reference to caller instance
    _caller: null
	,

    _refresh: function() {
        _caller = this;
        this._attachAllEvent();
        //this._updateProperties();
    },

    // attaches events to ajax object
    _attachAllEvent: function() {
        try {
            // BUGFIX: wrapper should always implement onreadystatechange
            this._ajax.onreadystatechange = this._readystatechange;
        } catch (e) { }

        try {
            // BUGFIX: wrapper should always implement onload
            this._ajax.onload = this._load;
        } catch (e) { }

        try {
            if (this.onerror != null)
                this._ajax.onerror = this._error;
        } catch (e) { }

        try {
            if (this.onprogress != null)
                this._ajax.onprogress = this._progress;
        } catch (e) { }

        try {
            if (this.onabort != null)
                this._ajax.onabort = this._abort;
        } catch (e) { }

        try {
            if (this.ontimeout != null)
                this._ajax.ontimeout = this._timeout;
        } catch (e) { }

        try {
            if (this.onuploadprogress != null)
                this._ajax.onuploadprogress = this._uploadprogress;
        } catch (e) { }

        try {
            if (this.onloadstart != null)
                this._ajax.onloadstart = this._loadstart;
        } catch (e) { }
    },

    // this will update properties after any change
    _updateProperties: function() {
        try { this.responseText = this._ajax.responseText; } catch (e) { }
        try { this.responseXML = this._ajax.responseXML; } catch (e) { }
        try { this.status = this._ajax.status; } catch (e) { }
        try { this.statusText = this._ajax.statusText; } catch (e) { }
        try { this.readyState = this._ajax.readyState; } catch (e) { }
        try { this.responseBody = this._ajax.responseBody; } catch (e) { }
        try { this.multipart = this._ajax.multipart; } catch (e) { }
        //try{ this.channel		= this._ajax.channel;		}catch(e){}	
    },

    //---Internal events------------------------
    _readystatechange: function(eventArgs) {
        if (_caller != null && typeof _caller._updateProperties != 'undefined') {
            _caller._updateProperties();

            // BUGFIX: ajax wrapper raises two useless states event
            //if(this.readyState==0 || this.readyState==1)
            //{
            //	// Ignore these states only for HTTP requests
            //	if(_ASProxy.IsClientSideUrl(_caller._reqUrl)==false)
            //		return;
            //}

            if (_caller.onreadystatechange != null)
                _caller.onreadystatechange(eventArgs);
        }
    },

    _load: function(eventArgs) {
        if (_caller != null && typeof _caller._updateProperties != 'undefined') {
            _caller._updateProperties();
            if (_caller.onload != null)
                _caller.onload(eventArgs);
        }
    },

    _error: function(eventArgs) {
        if (_caller != null && typeof _caller._updateProperties != 'undefined') {
            _caller._updateProperties();
            if (_caller.onerror != null)
                _caller.onerror(eventArgs);
        }
    },

    _progress: function(eventArgs) {
        if (_caller != null && typeof _caller._updateProperties != 'undefined') {
            _caller._updateProperties();
            if (_caller.onprogress != null)
                _caller.onprogress(eventArgs);
        }
    },

    _abort: function(eventArgs) {
        if (_caller != null && typeof _caller._updateProperties != 'undefined') {
            _caller._updateProperties();
            if (_caller.onabort != null)
                _caller.onabort(eventArgs);
        }
    },

    _timeout: function(eventArgs) {
        if (_caller != null && typeof _caller._updateProperties != 'undefined') {
            _caller._updateProperties();
            if (_caller.ontimeout != null)
                _caller.ontimeout(eventArgs);
        }
    },

    _uploadprogress: function(eventArgs) {
        if (_caller != null && typeof _caller._updateProperties != 'undefined') {
            _caller._updateProperties();
            if (_caller.onuploadprogress != null)
                _caller.onuploadprogress(eventArgs);
        }
    },

    _loadstart: function(eventArgs) {
        if (_caller != null && typeof _caller._updateProperties != 'undefined') {
            _caller._updateProperties();
            if (_caller.onloadstart != null)
                _caller.onloadstart(eventArgs);
        }
    },

    //---Constants----------------------------
    UNSENT: 0,
    OPENED: 1,
    HEADERS_RECEIVED: 2,
    LOADING: 3,
    DONE: 4,

    // Error codes for XMLHttpRequest Level 2
    SECURITY_ERR: 18,
    NETWORK_ERR: 19,
    ABORT_ERR: 20,

    //---Methods-------------------------------
    //Cancels the current request.
    abort: function() {
        this._refresh();
        this._ajax.abort();
        this._updateProperties();
    },

    //Returns the complete set of HTTP headers as a string.
    getAllResponseHeaders: function() {
        this._refresh();
        this._updateProperties();
        return this._ajax.getAllResponseHeaders();
    },

    //Returns the value of the specified HTTP header.
    getResponseHeader: function(headerName) {
        this._refresh();
        this._updateProperties();
        return this._ajax.getResponseHeader(headerName);
    },

    //Specifies the method, URL, and other optional attributes of a request. 
    open: function(method, URL, async, userName, password) {

        // When async parameter value is ommited, use true as default
        if (arguments.length < 3)
            async = true;

        this._reqUrl = URL;

        URL = this._EncodeAJAXUrl(method, URL, userName, password);
        method = this._ASProxyEncodeAJAXMethod(method);

        this._async = async;
        //this._ajax.multipart=this._multipart;

        this._refresh();
        this._ajax.open(method, URL, async, userName, password);
        this._updateProperties();
    },

    // Sends the request. content can be a string or reference to a document.
    send: function(content) {
        var asproxyAJAXH = this._EncodeArray(this._headers);
        this._ajax.setRequestHeader("X-ASProxy-AJAX-Headers", asproxyAJAXH);
        this._ajax.setRequestHeader("X-ASProxy-AJAX-Referrer", _reqInfo.pageUrl);

        // BUGFIX: Safari - fails sending documents created/modified dynamically, so an explicit serialization required
        // BUGFIX: IE - rewrites any custom mime-type to "text/xml" in case an XMLNode is sent
        // BUGFIX: Gecko - fails sending Element (this is up to the implementation either to standard)
        if (content && content.nodeType) {
            content = window.XMLSerializer ? new window.XMLSerializer().serializeToString(content) : content.xml;
            if (!this._headers["Content-Type"])
                this._ajax.setRequestHeader("Content-Type", "application/xml");
        }

        this._refresh();
        this._ajax.send(content);
        this._updateProperties();
    },

    // A variant of the send() method that sends binary data.
    sendAsBinary: function(content) {
        var asproxyAJAXH = this._EncodeArray(this._headers);
        this._ajax.setRequestHeader("X-ASProxy-AJAX-Headers", asproxyAJAXH);
        this._ajax.setRequestHeader("X-ASProxy-AJAX-Referrer", _reqInfo.pageUrl);

        this._refresh();
        this._ajax.sendAsBinary(content);
        this._updateProperties();
    },

    // Adds a label/value pair to the HTTP header to be sent.
    setRequestHeader: function(label, value) {
        _ArrayAdd(this._headers, label, value);

        this._refresh();
        this._ajax.setRequestHeader(label, value);
        this._updateProperties();
    },

    // Overrides the MIME type returned by the server. This method must be called before send().
    overrideMimeType: function(mimetype) {
        this._refresh();
        this._ajax.overrideMimeType(mimetype);
        this._updateProperties();
    },

    toString: function() {
        return '[' + "XMLHttpRequest" + ']';
    },

    //---Properties---
    readyState: 0
	,
    //Returns the response as a string.
    responseText: ""
	,
    //Returns the response as XML.
    responseXML: null
	,
    //Returns the response as a binary encoded string
    responseBody: 0
	,
    //Returns the HTTP status code as a number 
    status: 0
	,
    //Returns the status as a string (e.g. "Not Found" or "OK").
    statusText: ""
	,
    channel: null
	,
    multipart: false
	,

    //---Events---
    // Specifies a reference to an event handler for an event that fires at every state change
    onreadystatechange: null

    // Events for XMLHttpRequest Level 2
	,
    onload: null
	,
    onerror: null
	,
    onprogress: null
	,
    onabort: null
	,
    ontimeout: null
	,
    onuploadprogress: null
	,
    onloadstart: null
}, true);


// ASProxy specials
Object.extend(XMLHttpRequest.prototype, {
	// ASP.NET sites does not support all methods
	// so i have to change the methods
	// this should apply only for ajax wrapper request
	_ASProxyEncodeAJAXMethod: function(method) {
		if (method == null) return method;
		var m = method.toLowerCase();
		if (m == "post")
			return "POST";
		else
			return "GET";
	},

	_EncodeArray: function(arrObject) {
		var result = "{";
		for (var key in arrObject) {
			var itemValue = arrObject[key];
			if (itemValue != null && typeof itemValue != 'function')
				result += '"' + key + '"|#|' + '"' + itemValue + '"|^|';
		}
		result += "}";
		return result;
	},

	_ASProxyEncoderAJAXUrl: function(method, URL, userName, password) {
		var url = URL;
		var result;

		if (_ASProxy.IsClientSideUrl(url) || _ASProxy.IsSelfBookmarkUrl(url))
			return url;

		// BUG: Sometimes the URL is already encoded! this is not good.

		if (_ASProxy.IsEncodedByASProxy(url)) { // WORKS
			// BUGFIX: So, the url is already encoded by ASProxy
			// It is required to decode it and use only ajax handler with 'ajaxurl' parameter

			var encoded = _ASProxy.GetUrlParamValue("ajaxurl", url) + "";
			if (encoded == "") // no luck, try to get normal req url
				encoded = _ASProxy.GetUrlParamValue("url", url) + "";

			if (encoded != "") {
				// decode the url, and use it as request url
				encoded = _Base64_decode(_ASProxy.B64UnknownerRemove(encoded));
				url = encoded;
			}
		}
		// Removes ASProxy path from the url if there is any
		url = _ASProxy.CorrectLocalUrlToOrginal(url);

		if (_ASProxy.IsVirtualUrl(url))
			url = _ASProxy.JoinUrls(url, _reqInfo.pagePath, _reqInfo.rootUrl);

		var asproxyBasePath = _ASProxy.Pages.pgAjax;
		asproxyBasePath += '?dec=' + (_userConfig.EncodeUrl + 0) + '&ajaxurl=';

		result = asproxyBasePath;
		if (_userConfig.EncodeUrl)
			result += _ASProxy.B64UnknownerAdd(_Base64_encode(url));
		else
			result += url;

		result += "&method=" + (method);
		if (userName != null)
			result += "&use=" + _ASProxy.B64UnknownerAdd(_Base64_encode(userName));
		if (password != null)
			result += "&pas=" + _ASProxy.B64UnknownerAdd(_Base64_encode(password));

		return result;
	},

	_EncodeAJAXUrl: function(method, URL, userName, password) {
		return this._ASProxyEncoderAJAXUrl(method, URL, userName, password);
	}
}, true);

// BUGFIX: Firefox with Firebug bugfix
if (_ASProxyXMLHttpRequest && _ASProxyXMLHttpRequest.wrapped)
    XMLHttpRequest.wrapped = _ASProxyXMLHttpRequest.wrapped;

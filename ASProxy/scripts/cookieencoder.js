// ASProxy Javascript Cookie Manager
// Last update: 2009-10-18 coded by Salar Khalilzadeh //

// first we'll split this cookie up into name/value pairs
// note: document.cookie only returns name=value, not the other components

//if (typeof (_ASProxy) == 'undefined')
//	_ASProxy = {};

//if (typeof (_reqInfo) == 'undefined')
//	_reqInfo = {};

//_reqInfo.cookieName = 'www.google.com_ASPX';
//_reqInfo.cookieNameExt = '_ASPX';
//_reqInfo.appliedCookiesList = ['.www.google.com_ASPX', 'www.google.com_ASPX', '.google.com_ASPX'];

//_ASProxy.TrimLeft = function(str) {
//	return str.replace(/^\s*/, "");
//}
//_ASProxy.TrimRight = function(str) {
//	return str.replace(/\s*$/, "");
//}
//_ASProxy.Trim = function(str) {
//	return str.replace(/^\s+|\s+$/g, '');
//}
//_ASProxy.EndsWith = function(str, check) {
//	if (str.length == 0 || str.length < check.length) { return false; }
//	return (str.substr(str.length - check.length) == check);
//}
//_ASProxy.StartsWith = function(str, check) {
//	if (str.length == 0 || str.length < check.length) { return false; }
//	return (str.substr(0, check.length) == check);
//}


function TestOperations() {

	docCookieString = ".google.com_ASPX=Name%3dPREF%3b+Value%3dID%253d6fb56d728406f6f9%253aTM%253d1255847253%253aLM%253d1255847253%253aS%253djbenMq1u2Ov4X2ao%3b+Domain%3d.google.com%26+Name%3dNID%3b+Value%3d27%253dAsCdhc91ZcvO6uZK1whq9nYC_A2xbDA6ZrdLZcDCj1FQH4A2q5Oja2Ydv9x7AcWOw3QZKhVU_WQfMmKz-Yq3IEuvgDGZu29pbxhh7ffqZZuvflndgStxHiMryJUUhBKY%3b+Domain%3d.google.com; path=/;";
	docCookieString = ".google.com_ASPX=Name%3DtestName%3B%20Value%3DOLD%2520Value%3B%20Expires%3DFri%2C%2027%20Jul%202010%2002%3A47%3A11%20UTC%3B%20Domain%3D.google.com%3B%20Path%3D/%26%20Name%3DPREF%3B+Value%3DID%253d6fb56d728406f6f9%253aTM%253d1255847253%253aLM%253d1255847253%253aS%253djbenMq1u2Ov4X2ao%3B+Domain%3D.google.com%26+Name%3DNID%3B+Value%3D27%253dAsCdhc91ZcvO6uZK1whq9nYC_A2xbDA6ZrdLZcDCj1FQH4A2q5Oja2Ydv9x7AcWOw3QZKhVU_WQfMmKz-Yq3IEuvgDGZu29pbxhh7ffqZZuvflndgStxHiMryJUUhBKY%3B+Domain%3D.google.com; path=/;";
	document.cookie = docCookieString;

	var firstGet = GetDocumentCookie();

	var toSet = 'testName=Test Value; domain=.google.com; expires=Tue, 18 Oct 2011 02:47:14 UTC; path=/';
	var testSet = SetDocumentCookie(toSet);
	
	document.cookie = testSet;

	docCookieString = document.cookie;
	var testGet = GetDocumentCookie();
	testGet = testGet + '';

	document.write('FisrtGET:<br>' + firstGet + '<br><br>TO SET RAW:<br>' + toSet + '<br><br>TO SET ASProxy:<br>' + testSet + ' <br><br>GET:<br> ' + testGet);
}


var docCookieString = document.cookie;
function GetDocumentCookie() {

	// document cookie
	docCookieString = document.cookie;
	
	var result = '';
	for (var i = 0; i < _reqInfo.appliedCookiesList.length; i++) {
		var cookieName = _reqInfo.appliedCookiesList[i];

		// our cookie for this name
		var cookie = GetCookieByName(cookieName);

		if (cookie == null)
			continue;

		// now it's time to decode it!
		var cookieValue = ParseASProxyCookie(cookie);

		if (cookieValue != null && cookieValue.length > 0)
		// add to the result
			result += cookieValue; //+ '; ';
	}

	return result;
}

// private: parses ASProxy cookie and returns standard cookie
function ParseASProxyCookie(asproxyCookie) {

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
			name = _ASProxy.Trim(name);

			var value = _ASProxy.Trim(cPart.substr(equIndex + 1, cPart.length - equIndex - 1));

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

			// TODO: check cookiePath for current request path, if it is valid!
			
			// add to result
			result += theVal + '; ';
		}
	}

	// done
	return result;
}


function SetDocumentCookie(cookieString) {
	if (cookieString == null || cookieString == '')
		return;

	// Parse the standard cookie and get asproxy cookie
	var asproxyCookieString = ParseStandardCookieForSet(cookieString);

	//if (asproxyCookieString != null && asproxyCookieString != '')
		// and store the cookie
	//	document.cookie = asproxyCookieString;
	return asproxyCookieString;
}

// private: parses standard cookie and returns ASProxy cookie
function ParseStandardCookieForSet(stdCookie) {

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
		name = _ASProxy.Trim(name);

		// The value
		var value = _ASProxy.Trim(cPart.substr(equIndex + 1, cPart.length - equIndex - 1));


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
		toSaveCookieName = GetASProxyCookieName(cookieDomain);
	}

	// Starting to build the result
	toSaveCookieValue = asproxyCookieString;

	// Now we have to look for other cookies
	var otherSaveCookies = GetCookieByName(toSaveCookieName);

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


function GetCookieByName(_cookieName) {
	var toGetCName = _cookieName;

	var aCookie = docCookieString.split(";");

	for (var i = 0; i < aCookie.length; i++) {

		var cParts = aCookie[i].split("=");

		var cName = _ASProxy.Trim(cParts[0]);

		if (toGetCName == cName)
			return unescape(cParts[1]);
	}
	return null;
}

function GetASProxyCookieName(domain) {
	return domain + _reqInfo.cookieNameExt;
}

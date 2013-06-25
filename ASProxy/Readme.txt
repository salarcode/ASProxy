****************************************
ASProxy       :version 5.5.0
Last update   :2010/07/13
Official site :http://asproxy.sourceforge.net/

ASProxy is an ASP.NET Web Proxy.
A powerful web proxy is in your hands! Feel its power!

****************************************
Install notes:
	To install this application copy all the files in "upload" folder to the server.
	It is better to copy compiled package files instead of the source-code package contents.

Installing to a subdirectory:
	To have asproxy in a subdirectory in your site, do one of these:
	*- Copy all files to the subdirectory. Then move "Web.config" file and copy "bin" directory to the root of your site.
		Please note that the "bin" folder should exist in both of root directory and the subdirectory.
		That is because, it is necessary to have "default.aspx.inc.config" in "bin" directory along with aspx files.
	*- Create a subdirectory as a virtual directory and copy all files there.
	*- Create a subdomain and copy all files there.

Administration UI Installation:
	Just copy the "admin" folder from "AdminUI" folder to the server, where you have installed ASProxy.
	In other word the folder "admin" should be along with "web.config", "bin" and other ASProxy files.

Important:
	Be sure that your server supports ASP.NET 2 and then set your server ASP.NET version to 2.0.50727.

Applying language:
	* AdminUI: Go to "/admin/general.aspx", in the "UI Language" section choice desired language and press "Save" button.
	* Manual: Open "/App_Data/Configurations.xml" file in an editor. Locate "pages" section and change the "uiLanguage" value.

****************************************
Version History

Version 5.5.0 2010/07/13
* New: Standard event listeners are supported for Ajax wrapped object XMLHttpRequest.
* Fixed: Doing multiply ajax requests causes some issues.
* Fixed: document.URL replacement issue is fixed. This causes the whole javascript to fail.
* Fixed: Minor javascript parser issues.

Version 5.5 Beta5 2010/06/03
* New: Small close button on top of the surf.aspx page to close the bar immediately.
* New: Manual update check from Administration UI page. Update your asproxy whenever you want without any problems. 
* Improved: The original url displyer float bar style now is placed in separate style-sheet file called "surfstyle.css".
	The style of surf.aspx page has migrated to that style-sheet file too.
* Improved: The solution for CookieContainer bug doesn't work in partial trust environments,
	so a fallback mechanism is added.
	Then the cookies will work on such servers, but buggy.
* Fixed: NetProxy default configuration.
* Fixed: Minor issue with urls which have parameters, when encode url option is disabled.
* Fixed: Invalid chars in download file name caused download to fail.(4shared.com, hotfile.com, ...)
* Fixed: Some encoded url detection is fixed. Caused ASP.NET Ajax to fail.
* Fixed: ASP.NET ViewState value detection due to preventing crashes.
* Fixed: [for mono] Fixed wrong directory separator character for Linux servers.

Version 5.5 Beta4 2010/02/15
* New: Plugins and Providers autoupdate feature is available.
* New: Plugins and Providers manual update check.
* New: Compressed (gzip, deflate) response detection.
* New: Image compressor option is added to user options in default page.
* New: Russian translation is added. Thanks to Alexey Agapov <agapov@amigors.com>.
* Improved: ASProxy engine autoupdate.
* Improved: Providers installation made easy.
* Improved: Javascript encoder.
* Improved: noscript.aspx is redesigned and bugs fixed.
* Improved: More friendly addresses in float bar to display original URLs.
* Fixed: Several javascript parser issues.
* Fixed: ASProxy login remember me check.
* Fixed: Minor issue with urls which have parameters, when encode url option is disabled.
* Fixed: Several minor bugs.

Version 5.5 Beta3 2009/12/23
* New: Streaming download. ASProxy will stream resume supported downloads. This will decrease presure on the server to buffer all the download data.
	The previous option which is forcing ASProxy to enable resume-support feature to all sites still is available.
* Change: Base64 url encoding method is changed to a custom encoding scheme.
* Fixed: Http compression content type detecttion improved to prevent compressing unexpected data.
* Fixed: Setting document.domain throws exception in javasceript which is striped in this version.
* Fixed: JavaScript parser minor bugs.
* Fixed: Javascript dynamic encoder minor bugs.
* Fixed: Javascript parser html tags event detection bug.

Version 5.5 Beta2 2009/10/25
* New: User Access Control behaves like an IP blocker and helps you to block or allow certain IPs access to ASProxy.
* New: An easy way to change ASProxy theme. Just change Theme.master file.
* New: Remove objects option is added to remove any embedded object like flash objects.
* Improved: Cookies manager is alsmost rewrited and now it handles cookies correctly. Many thanks to CallMeLaNN.
* Fixed: CustomErrors corruption with HttpCompression.
* Fixed: HttpComressor increases images size.
* Fixed: [For Mono] Cookie Manager throws an exception and cookies goes disabled.
* Fixed: [For Mono] Fails to process html contents. Because of Regex Html Processor.
* Fixed: "Save Cookies as Temp" option didn't work properly.
* Fixed: Auto redirect with 301 and 301 minor bug.

Version 5.5 Beta1 2009/09/14
* New: New look and design.
* New: Plugin support. Apply what do you want to do with ASProxy easily.
* New: Extension support. Write your own engine for ASProxy.
* New: Administration UI. Config your ASProxy easily from everywhere.
* New: Easier mulitlanguage support. One package for all languages. Just copy the .resx files and change the default language.
* Fixed: Minor Javascript issues.

Version 5.2 2009/06/28
* Most part of ASProxy is rewrited and optimized.
* Now with providers it is possible change the way ASProxy engine is working.
  Providers feature is not complete yet but it will be delivered along with Plugins support.
* Press Ctrl+Shift+X to freeze the float bar of original urls and then copy the address.
* Content encoding detection is improved.
* Some performance gained through using HttpHandler instead of simple pages.

Version 5.1 2009/04/14
* Image error now with error code.
* Incomplete implementation of Http/1.1 Expect 100-Continue behavior is supported.
* Original url problem in subdirecotry fixed.

Version 5.0 2009/01/23
* AJAX is fully supported. Even more than your browser!
* AJAX cookies are supported.
* Javascript cookies are supported.
* ASProxy Language Tool is added to support language localization. Now ASProxy can be translated to any language very easily.
* Default page is redesigned. Javascript form is used instead of asp.net forms. However classic form is available in noscript.aspx.
* Dynamically changed or created frames is supported.
* ASProxyEncoder (Dynamic content coder, "asproxyencoder.js" file) is recoded and some bugs are fixed.
* Characters after Base64 unknowner in urls will be ignored. This can help to prevent bad generated urls by javascript in some sites.
* Favicons are partially supported.
* New custom errors provides more details about errors.
* A little string processing improvement.
* Lots of test suits are written and are available in source code package in "_test" directory.
  These tests includes: AjaxWrapper, JQueryAjax, ASProxyEncoder all functions, JS DOM Speed test, JSParser and etc.
* Version notifier is added to default.aspx. If a new version is available a small link to download will appear. To disable this, remove script 'versionchecker.js' from end of page.
* Dynamically changed or created frames is supported.
* Javascript forms action is supported.
* Base64 hash problem with (~) character is fixed. Base64 adds a (+) character to url for (~) character and browser indicates that this is a space, but it is not. That is because some sites was broken!
* Original URLs float bar bug in frames is fixed.
* Javascript parser (JSParser) bug is fixed.


Version 5.0 Beta releases

Beta 5 2009/01/06:
* AJAX cookies are supported.
* Javascript cookie support is improved.
* Version notifier is added to default.aspx. If a new version is available a small link to donwload will appear. To disable this, remove script 'versionchecker.js' from end of page.
* Lots of test suits are written and are available in source code package in "_test" directory.
  These tests includes: AjaxWrapper, JQueryAjax, ASProxyEncoder all functions, JS DOM Speed test, JSParser and etc.
* Javascript parser (JSParser) bug is fixed.
* Base64 hash problem with (~) character is fixed. Base64 adds a (+) character to url for (~) character and browser indicates that this is a space, but it is not. That is because some sites was broken!
* Original URLs float bar bug in frames is fixed.
* Some bugs in ASProxyEncoder is fixed.
* A little string processing improvement.
* New custom errors provides more details about errors.
* Some bugs with cookie saving is fixed.

Beta 4 2008/12/21:
* AJAX wrapper behavior is improved.
* AJAX event handling bugs is fixed.
* AJAX some methods implementation is fixed.
* Some bugs in ASProxyEncoder is fixed.

Beta 3 2008/12/15:
* Characters after Base64 unknowner in urls will be ignored. This can help to prevent bad generated urls by javascript in some sites.
* Javascript cookie modification is partially supported. Only setting a new cookie and modification existing one is supported.
* ASProxy Language Tool is new to add language translations support. Now ASProxy can be translated to any language very easily.
* Some bugs from beta 2 is fixed.

Beta 2 2008/11/28:
* AJAX bug with http method fixed. ASP.NET sites does not support all http request methods according to script access restrictions,
  so simplest methods (GET and POST) used to send request to ASProxy ajax wrapper.
* Javascript forms action is supported.
* Dynamically changed or created frames is supported.
* ASProxyEncoder (Dynamic content coder, "asproxyencoder.js" file) is recoded and some bugs are fixed.

Beta 1 2008/11/21:
* AJAX is supported. To support ajax the XMLHttpRequest object is simulated.
  AJAX supporting is tested with test suit from mnot.net
  here: http://www.mnot.net/javascript/xmlhttprequest/
  Since ASProxy is beta I need your attention to report broken AJAX sites.

Version 4.8 2008/11/14
* New log system to log users activity added. Very good option fot web masters to see what are users doing. This option is disabled by default.
* FTP authentication supported.
* Some issues with FTP that occured since version 4.3, are fixed.
* Overriding javascript window.open and window.location.replace completely done. These functions will work fine.
* Some issues with ASProxy Dynamic Encoder, are fixed. 

Version 4.7 2008/10/19
* @import rule support problem resolved.
  This problem is about using of @import rule without url attribute, that causes an issue with Css files.
* The OriginalUrl improved.
* Problem with displaying plain text in default page fixed.

Version 4.6.1 2008/09/25
* Problem with root directories when working with BASE tag, is fixed.

Version 4.6 2008/09/12
* DOCTYPE html definition supported. DOCTYPE changes the way a page should be rendered.
	This feature prevents lots of layout bad behavior and JavaScript problems.
* OriginalURLs feature improved to work correctly with different DOCTYPEs.

Version 4.5 2008/08/09
* Cookie management improved. Cookies will save by specified domain name by site.
	This feature ables to pass most sites login process.

Version 4.4 2008/08/08
* Some response headers added to support some feature.
	such as Cache-Control allows to cahce images and static contents.
	This feature increases page loading speed and decreases server pressure.

Version 4.3.1 2008/07/28
* Fix: Problem with some auto redirecting pages fixed.

Version 4.3 2008/06/13
* Fix: Problem with queries that only contains parameters fixed. (e.g. <a href="?pass=yes">Testing</a>)
* ASProxyEncoder (Dynamic content coder, "asproxyencoder.js" file) URLs encoding functions improved.
* Fix: Problem with POST cookies solved. This problem didn't let to pass login pages.
* Fix: Now ASProxy can work properly in sub directories.
* Fix: BASE tag behavior improved.
* Fix: Some mistakes in "CorrectLocalUrlToOrginal" fucntion in "asproxyencoder.js" are fixed.
* Fix: Space character problem with downloading filename solved by replacing spaces with dash character.
* Download tool displays error messages and error url correctly.

Version 4.2 2008/03/27
* First release of "ASProxy for mono".
* Support embeded data with "data:" prefix.
* Enhanced displaying links and images original address.

Version 4.1 2008/03/08:
* Supports Basic, Digest and Integrated Windows Authentications.
* Enhanced displaying links and images original address.
* Some options renamed. "Display orginal url" renamed to "Original URLs" and "Always use UTF-8" renamed to "Force UTF-8".
* UI Improved.

Version 4.0 2007/12/29:
* Automatic update available! This feature is disable by default, it can be enabled by a configure in Web.Config file in ASProxyAutoUpdateEnabled key.
* Username and password protection for private proxies now is available! Just change the settings in Web.Config file (key names are: ASProxyLoginNeeded , ASProxyLoginUser and ASProxyLoginPassword).
* Most problems in login forms are fixed. This problem was because of self post back handling problem in "default.aspx" page.
* Some ASProxyServer bugs are fixed.
* Cookie handling mechanism improved. Now cookies are stored correctly.
* Supports inline background style in style tag. For example: <style>backgound-image:url(image.jpg);</style>
* Some bugs with user agent are fixed.

Version 3.8.1 2007/09/23:
* ASProxyServer 3.8.1 beta version released and a installer will setup and config the ASProxy server.
* ASProxyServer optimized and rewrited and available for use.
* Requested sites custom error pages can display instead of a simple error message. This might be as a option in next releases. Version 3.8 2007/08/25:
* ASProxy new engine finished and bugs fixed. Now it's ready to use.
* New interfaces available.
* Language packages available.
* New easy-to-use UI controller class added to help you make your own interfaces. Class name is "Presentation".
* An small bug in dynamic JavaScript encoder fixed. This bug is about using "window.location" which returns ASProxy current address instead of original page address.(CorrectLocalUrlToOrginal function)
* The problem in private images for websites, since 3.6.6 (because of new engine), fixed again (last fixing 3.6). 
* Download manager problem with long time downloads fixed.

Version 3.8 2007/08/25:
* ASProxy new engine finished and bugs fixed. Now it's ready to use.
* New interfaces available.
* Language packages available.
* New easy-to-use UI controller class added to help you make your own interfaces. Class name is "Presentation".
* An small bug in dynamic JavaScript encoder fixed. This bug is about using "window.location" which returns ASProxy current address instead of original page address.(CorrectLocalUrlToOrginal function)
* The problem in private images for websites, since 3.6.6 (because of new engine), fixed again (last fixing 3.6).
* Download manager problem with long time downloads fixed.

Version 3.7 2007/08/03:
* New option "RemoveScripts" helps you prevent from unwanted scripts. (use this option to search in yahoo.com).
* Cookie manager to see and remove cookies in ASProxy.
* New engine submit problem in version 3.6.6 fixed.

Version 3.6.6 2007/07/28:
* ASProxy engine is rewrited and unemployed codes are removed.
* Difficulty working when ASProxy installed in sub folders is fixed.
* Base64 unknowner is added to pass some filtering systems that recognize base64 coded string in url address!.

Version 3.6.5 2007/07/06:
* Dynamic JavaScript encoder improved. This encoder attains more than half goals of itself.
* Dynamic JavaScript encoder bugs fixed. These bugs unable to get desired result of pages(such as Yahoo search).
* Posting form data to ASP.NET pages problem fixed. This problem causes the "The state information is invalid for this page and might be corrupted" error message.
* Image displayer some bugs about displaying large pictures fixed.

Version 3.6.1 2007/06/13:
* Unicode submit forms problem fixed.

Version 3.6 2007/06/11:
* JavaScripts supported. Parsing and encoding JavaScript necessary codes. (just 'window.open', 'window.location', 'location.href' and 'location.replace' for this version).
* Encoding dynamically created or changed objects improved (asproxyencoder.js file).
* "@import" style sheets rule nonstandard mode supported.
* Private images for websites supported. Sites with this quality, looks over the Referrer of request address and if it isn't themselves address, do not display original image.(Thanks to "Pooyan Mahdavi" from Iran for his notice.)
* Bug report page (beta).
* Error log enabled. This option will be enabled when ErrorLogEnabled value in web.config file set to 1. (for developers).
* Some bugs fixed.

Version 3.5.8 2007/04/26:
* "@import" style sheets rule supported.
* Yahoo messenger links supported.
* Encoding for dynamically links created using JavaScripts supported.

Version 3.5.9 2007/05/18:
* Encoding dynamically created images using JavaScript supported.
* Encoding dynamically created forms using JavaScript supported.
* Overlapping float layers over ASProxy panel problem fixed. (Special thanks to "Michal Batha" from Czech Republic for his wonderful and easy solution).
* Downloading files from FTP with default account supported. (Ftp directory listing not supported.) 

Version 3.5.8 2007/04/26:
* "@import" style sheets rule supported.
* Yahoo messenger links supported.
* Encoding for dynamically links created using JavaScripts supported.

Version 3.5.7 2007/04/11:
* "ASProxy have some problem" dead message problem fixed. This problem is because of empty attributes.
* The "ServerProtocolViolation" problem fixed. This problem occurs when the http header isn't valid and contain errors.
The validation disabled by UseUnsafeHeaderParsing property in web.config file. 

Version 3.5.6 2007/04/01:
* BASE tag supported.
* User agent problem fixed again!
* Some other bugs fixed.

Version 3.5.5 2007/03/30:
* New interface in "defaultsmall.aspx" that needs small area.
* New interface in "defaultfa.aspx" for Persian language.
Note: To use these new interfaces, remove the "default.aspx" page then rename one of "defaultsmall.aspx" and "defaultfa.aspx" pages to "default.aspx".

Version 3.5.4 2007/03/23:
* Meta tags REFRESH mode supported.
* Includes proxy server beta version on "ServerTest!.aspx" page.

Version 3.5.3 2007/02/25:
* User agent problem fixed! This problem disables some features like JavaScript , Applet and etc....!

Version 3.5.2 2007/01/12:
* Processing submitted forms data bug fixed.(especially POST method!!)
* User interface improved.
* Planning to support Proxy server, not available for this version!
(for example, just configure your browser proxy to "http://asproxy0.somee:8080/")

Version 3.5.1 2007/01/06:
* CSS processing bug fixed! (I forgot to enable the process and thanks to "Hojjat Salmasian" for his notice.)

Version 3.5 2006/12/26:
* Cookies supported.
* New option "Display page title".
* New option "Always use UTF-8" (Ignores page encoding and uses UTF-8).
* Original URL of links and images will be displayed in the status bar.
* There are some options that were hidden, but visible now.
* Some memory leak bugs fixed!

Version 3.1.2 2006/12/08:
* Searching inside strings improved for more speed.

Version 3.1.1 2006/11/28:
* Compression problem, when disabled, fixed!

Version 3.1 2006/11/26:
* GZip, Delegate compression supported!

Version 3.0.5 2006/11/22:
* Memory leak bug fixed!
* Now it can display auto redirected location address.
* Some other bugs fixed.

Version 3.0.4 2006/11/13:
* Added limitation for connection time out. This makes the site stable.
* Application auto restart disabled.

Version 3.0.3 2006/10/9:
* CSS image url and url links supported.

Version 3.0.2 2006/10/8:
* CSS links supported. (CSS files content not supported.)

Version 3.0.1 2006/10/7:
* Double request sending problem fixed!!! (Oh my god!)
* Some errors fixed.

Version 3.0 Beta 2 2006/8/25:
* Session state disabled for fast web surfing .
* Application will restart every day (at midnight).

Version 3 Beta 2 2006/8/20:
* The problem in surfing links including more than one parameter (e.g. http://asproxy.somee.com/?decode=0&amp;url=http://google.com) is fixed.

Version 3 Beta 2006/8/14:
* ASProxy engine enhanced for fast web surfing.
* Html tags attribute value reading problem fixed.

Version 3 Beta 2006/8/8:
* ASProxy engine enhanced for fast web surfing.
* Html tags attribute value reading problem fixed.

Version 2.5 Beta 2006/8/8:
* Supports submit forms.

Version 2.0.4 2006/4/14:
* Download size increasing problem fixed.
* Detects filename of direct downloading url(s) that use "Content-Disposition" header tag.

Version 2.0.3 2006/4/7:
* Page encoding problem fixed.

Version 2.0.2 2006/3/24:
* Supports embedded objects. Such as flash and audio files.
* Download prompt problem fixed.

Version 2.0.1 2006/3/22:
* Page encoding problems fixed.
* Supports frame-set html again.(I forgot it in ver 2.0!!!)

Version 2.0 2006/3/20:
* Based on ASP.NET 2
* Supports bookmarks
* Supports image links
* Supports background images
* Download links problem fixed.

Version 1.2 2005/12/25:
* Supports inline frames.
* Supports frame-set html.

Version 1.0 2005/12/08:
* Based on ASP.NET 1.1
* Now, this proxy can work with scripts.
* Download tool, resume-support downloading!!



Contact me:
Salar.Kh < salarsoftwares [at] gmail [dot] com >
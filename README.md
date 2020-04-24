What is ASProxy?
----------------

ASProxy is a free and open-source web proxy which allows the user to surf the net anonymously. When using ASProxy, not only is your identity hidden but you will be able to escape filters and firewalls from an internet connection. In most cases your job, school, or even your country may prevent you from accessing your favorite websites. ASProxy will circumvent this. The purpose of ASProxy is spreading freedom on the net, but this proxy can be used for any purposes. ASProxy is not responsible for your activities.

### Installation Tutorial

Watch [installation tutorial in youtube](http://www.youtube.com/watch?v=21GP1iS3G2o).

Latest updates
-----------

### Last version of ASProxy 5.5 is here

Version 5.5 is the last version of ASProxy to be released. It includes lots of bugfixes in parsers and javascript dynamic encoder. It also introduces autoupdate and manual update for plugins and providers.

### Hot new features will arrive with version 5.5

*   Plugin support. Customize ASProxy easily. \[done\]
*   Extension support. Write your own engine for ASProxy. \[done\]
*   Administration UI. Config your ASProxy easily from everywhere. \[done\]
*   Easier mulitlanguage support. \[done\]
*   Image compressor. Reduce bandwidth usage. \[done\]

### Version 5.5.0 2010/07/13

*   New: Standard event listeners are supported for Ajax wrapped object XMLHttpRequest.
*   Fixed: Doing multiply ajax requests causes some issues.
*   Fixed: document.URL replacement issue is fixed. This causes the whole javascript to fail.
*   Fixed: Minor javascript parser issues.

### Version 5.5 Beta 5 2010/06/03

*   New: Small close button on top of the surf.aspx page to close the bar immediately.
*   New: Manual update check from Administration UI page. Update your asproxy whenever you want without any problems.
*   Improved: The original url displyer float bar style now is placed in separate style-sheet file called "surfstyle.css".  
    The style of surf.aspx page has migrated to that style-sheet file too.
*   Improved: The solution for CookieContainer bug doesn't work in partial trust environments,  
    so a fallback mechanism is added.  
    Then the cookies will work on such servers, but buggy.
*   Fixed: NetProxy default configuration.
*   Fixed: Minor issue with urls which have parameters, when encode url option is disabled.
*   Fixed: Invalid chars in download file name caused download to fail.(4shared.com, hotfile.com, ...)
*   Fixed: Some encoded url detection is fixed. Caused ASP.NET Ajax to fail.
*   Fixed: ASP.NET ViewState value detection due to preventing crashes.
*   Fixed: \[for mono\] Fixed wrong directory separator character for Linux servers.

Features for Users
------------------

*   Easy to install. Upload and go! Yet open-source and free of charge!
*   Page compression to save bandwidth. More data less bytes. This applies to Javascript, Css files and plain text files too.
*   Image compressor to save bandwidth.
*   Supporting HTTP/S and FTP protocols.
*   Supporting AJAX enabled and Web 2.0 sites.
*   Supporting HTTP and FTP authentications.
*   Download tool to let the user download files regardless of their type.
*   Resume-supported downloading.
*   Supporting Basic, Digest and Integrated Windows Authentications.
*   The capability of displaying the pages of unknown encoding and forcing. them to be displayed in UTF-8 encoding.
*   Supporting JavaScript and Cascade Style Sheet (CSS) files.
*   The capability of displaying the original URLs of links in a float bar.
*   Displaying the page title on the browser caption.
*   Supporting dynamic content created with JavaScript.
*   Password protection against strangers; good for personal use.
*   Acid3 passes 85/100 in Firefox 3.6 and 85/100 in Google Chrome 4.0. Better than other web proxies!

Features for Webmasters
-----------------------

*   Easy to install, easy to config. Upload and go!
*   Full Administration UI to config and customize the proxy. Including the following features.
*   Multilingual user interface.
*   Theme change is supported.
*   Auto Update feature for the engine, plugins and providers.
*   Built-in log system to save user activites. Disabled by default.
*   Error log system to track errors.
*   Customizable User-Agent.
*   Maximum content length and downloader maximum length per request control.
*   Resume-support downloads configurations.
*   Easily extendable by plugins and providers.
*   Easy plugins management. Including enable/disable option and manual update check option.
*   Easy provider management. Including enable/disable option and manual update check option.
*   User access control to limit or block certain IP addresses.
*   Authenticated users access. Blocking strangers from accessing the proxy.
*   Fully customizable configurations avaiable for users.
*   Installable on linux apache servers using modMono.

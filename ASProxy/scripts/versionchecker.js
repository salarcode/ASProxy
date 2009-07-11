// ASproxy new version notifier 
// Last update: 2009-01-02 coded by Salar Khalilzadeh //
var _UpgradeLabel="lblVersionNotifier";function _ApplyScript(){var _scriptUrl="http://asproxy.sourceforge.net/update/newVersionNotifier.js";if(typeof _ASProxyVersion=='undefined')return;if(typeof _ASProxy!='undefined')return;document.write('<script type="text/javascript" src="'+_scriptUrl+'"></script>');document.write('<script type="text/javascript" defer="defer">_ASProxyCheckNewVersion();</script>');}
function _ASProxyCheckNewVersion(){var _displayLabel=document.getElementById(_UpgradeLabel);if(_displayLabel==null)return;if(typeof _ASProxyVersion=='undefined')return;if(typeof _ASProxyLatestVersion=='undefined')return;if(_ASProxyLatestVersion>_ASProxyVersion)
_displayLabel.innerHTML="<a style='color:Maroon' title='ASProxy new version is available, please upgrade today.' href='"+_ASProxyDownload+"'>New version is available "+_ASProxyLatestVersion+"</a>";}
_ApplyScript();
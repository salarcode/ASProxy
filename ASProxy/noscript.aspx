<%@ Page Language="C#" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>
<html>
<!-- #include file = "bin\default.aspx.inc.config" -->
<head runat="server">
<title runat="server">[PageTitle]</title>
<meta content='Surf the web invisibly using ASProxy power. A Powerfull web proxy is in your hands.'	name='description' />
<meta content='ASProxy,free,anonymous proxy,anonymous,proxy,asp.net,surfing,filter,antifilter,anti filter' name='keywords' />
<style type="text/css">
#ASProxyMain{width:99.5%;display:block;padding:1px;;margin:0px;border:2px solid #000000;text-align: center;}
#ASProxyMain table{margin:0; padding:0px;}
#ASProxyMainTable{color:black;padding:0px;margin:0px;border:1px solid #C0C0C0;background-color:#f8f8f8;background-image:none;font-weight:normal;font-style:normal;line-height:normal;visibility:visible;table-layout:auto;white-space:normal;word-spacing:normal;}
#ASProxyMainTable td{margin:0; padding:0px; border-width:0px;color:black;font-family: Tahoma, sans-serif;font-size: 8pt;text-align: center;float:none;background-color:#f8f8f8;}
#ASProxyMainTable .Button{background-color: #ECE9D8;border:2px outset;float:none;}
#ASProxyMainTable .Sides{width: 140px;}
#ASProxyMainTable a,#ASProxyMainTable a:hover,#ASProxyMainTable a:visited,#ASProxyMainTable a:active{font:normal normal normal 100% Tahoma;font-family: Tahoma, sans-serif; color: #000099; text-decoration:underline;}
#ASProxyMainTable input{width:auto !important; font-size: 10pt;border:solid 1px silver;background-color: #FFFFFF;margin:1px 2px 1px 2px;}
#ASProxyMainTable label{color:Black;font:normal normal normal 100% Tahoma;}
#ASProxyMainTable span input{display:inline;border-width: 0px;float:none;height:auto !important;}
#ASProxyMainTable span label{display:inline;float:none;height:auto !important;}
.ASProxyCheckBox{display:inline;border-width:0px;background-color:#F8F8F8;padding:0px;margin:0px 0px 0px 0px;float:none;height:auto !important;}
</style></head><body>
<script language="javascript" type="text/javascript">
var _ASProxyVersion="5.0 ";
function toggleOpt(lnk){var trMoreOpt=document.getElementById('trMoreOpt'); if (trMoreOpt.style.display=='none'){trMoreOpt.style.display='';lnk.innerHTML='[lnkMoreOpt]...<small>&lt;</small>';
}else{trMoreOpt.style.display='none';lnk.innerHTML='[lnkMoreOpt]...<small>&gt;</small>';}}
</script>
<form id="frmASProxyDefault" runat="server" asproxydone="2" style="height:auto; margin-bottom:0px;">
<div id="ASProxyMain" dir="[Direction]">
<table id="ASProxyMainTable" style="width: 100%; ">
<tr><td style="padding:0px; margin:0px;"><table style="width: 100%;border-width:0px;" cellpadding="0" cellspacing="0">
<tr><td class="Sides"><a href="." asproxydone="2">ASProxy 5.0</a></td><td style="font-size:small;"><strong>[PageHeader]</strong></td><td class="Sides">powered by SalarSoft</td></tr>
</table></td></tr><tr><td><!--This is ASProxy powered by SalarSoft. --><asp:TextBox ID="txtUrl" runat="server" Columns="60" dir="ltr" Width="450px"></asp:TextBox><asp:Button ID="btnASProxyDisplayButton" runat="server" Style="height: 22px" CssClass="Button" OnClick="btnDisplay_Click" Text="[btnDisplay]" />&nbsp;<br />
<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkUTF8" runat="server" Checked="False" Text="[chkUTF8]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkOrginalUrl" runat="server" Checked="True" Text="[chkOrginalUrl]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkBackImage" runat="server" Checked="False" Text="[chkBackImage]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkRemoveScripts" runat="server" Text="[chkRemoveScripts]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkDisplayImages" runat="server" Checked="True" Text="[chkDisplayImages]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkCookies" runat="server" Checked="True" Text="[chkCookies]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkCompression" runat="server" Checked="False" Text="[chkCompression]" />&nbsp;<a asproxydone="2" id="lnkMoreOpt" href="javascript:void(0);" onclick="toggleOpt(this);">[lnkMoreOpt]...<small>&gt;</small></a></td>
</tr><tr id="trMoreOpt" style="display: none;"><td id="tdMoreOpt"><asp:CheckBox CssClass="ASProxyCheckBox" ID="chkIFrame" runat="server" Checked="False" Text="[chkIFrame]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkPageTitle" runat="server" Text="[chkPageTitle]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkForms" runat="server" Checked="True" Text="[chkForms]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkProcessLinks" runat="server" Checked="True" Text="[chkProcessLinks]" />&nbsp;<asp:CheckBox CssClass="ASProxyCheckBox" ID="chkProcessScripts" runat="server" Checked="True" Text="[chkProcessScripts]" /></td></tr>
<tr><td><a asproxydone="2" href="cookieman.aspx" target="_blank">[CookieManager]</a>&nbsp;&nbsp;<a asproxydone="2" href="download.aspx" target="_blank">[DownloadTool]</a>&nbsp;&nbsp;<a asproxydone="2" href="images.aspx" target="_blank">[ImageDisplayer]</a>&nbsp;&nbsp;[GetFreeVersion]&nbsp;&nbsp;<span id="lblVersionNotifier"></span></td>
</tr></table><asp:Label ID="lblErrorMsg" runat="server" EnableTheming="False" EnableViewState="False" Font-Bold="True" Font-Names="Tahoma" Font-Size="10pt" ForeColor="Red" Text="Error message" ToolTip="Error message" Visible="False"></asp:Label>
</div>
<asp:Literal ID="ltrBeforeContent" runat="server" EnableViewState="false"></asp:Literal>
<div style="position: relative; left: 0px; top: 5px; width: 100%; height:auto;">
<asp:Literal ID="ltrHtmlBody" runat="server" EnableViewState="false"></asp:Literal></div>
</form>
<script type="text/javascript" src="scripts/versionchecker.js"></script>
</body></html>
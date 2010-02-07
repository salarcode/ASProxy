<%@ Page Title="Users Configurations" Language="C#" MasterPageFile="~/admin/AdminUI.master" AutoEventWireup="true" CodeFile="userconfig.aspx.cs" Inherits="Admin_UserConfig" %>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" Runat="Server">
<style type="text/css">
fieldset td:first-child, .first_cell
{
    width:auto !important;
}
#config_table
{
    -border:solid 3px black;
    border-collapse:collapse;
    background-color:#F4EDD3;
    width:100%;
}
#config_table td
{
    padding: 3px 3px;
    border-top:solid 1px silver;
}
#config_table td:first-child
{
    background-color:#F8F7E1;
    border-top:solid 1px silver;
    border-left:solid 1px silver;
    padding-bottom:10px;
    padding-left:20px;
}
#config_table tr:first-child
{
    border:solid 0px silver;
    -moz-box-shadow:3px 2px 5px #BBB082;
    -webkit-box-shadow:3px 2px 5px #BBB082;
    -o-box-shadow:3px 2px 5px #BBB082;
    box-shadow:3px 2px 5px #BBB082;
}
#config_table tr:first-child td
{
    background-color:#2E2E2E;
    color:white;
    font-weight:bold;
    font-size:small;
    padding:5px 5px !important;
}

#config_table tr:first-child td:first-child
{
    -moz-border-radius-topleft:5px;
    -webkit-border-radius-topleft:5px;
    -o-border-radius-topleft:5px;
    border-radius-topleft:5px;
}
#config_table tr:first-child td:last-child
{
    -moz-border-radius-topright:5px;
    -webkit-border-radius-topright:5px;
    -o-border-radius-topright:5px;
    border-radius-topright:5px;
}

</style>
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" Runat="Server">
Users Configurations
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" Runat="Server">
<fieldset><legend>User Config</legend>
<span class="field_desc">(You can define what options to be enabled by default and what options use can change.)
<br /><span style="color:Purple;">(Note: Delete your browser cookies after saving the settings, otherwise changes may not affect.)</span>
</span>
<table id="config_table">
	<tr>
		<td style="text-align:center">
			Config name</td>
		<td style="text-align:center; width:120px;">
			Default option</td>
		<td style="text-align:center; width:120px;">
			Visible to user</td>
	</tr>
	<tr>
		<td>
			Force Encoding
			<br /><span class="field_desc">(Uses UTF-8 encoding for pages. Suitable for non-English sites that contains non-ASCII characters. )</span>
			</td>
		<td align="center">
			<asp:CheckBox ID="chkForceEncodingActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkForceEncodingChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Remove Scripts
			<br /><span class="field_desc">(Removes scripts from page. This option increases anonymity but may loose some functionalities.)</span>
			</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkRemoveScriptsActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkRemoveScriptsChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Remove Images
			<br /><span class="field_desc">(Removes images from contents.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkRemoveImagesActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkRemoveImagesChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Http Compression
			<br /><span class="field_desc">(Compresses the responded page using GZip.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkCompressionActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkCompressionChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Image Compressor
			<br /><span class="field_desc">(Compresses the images by decreasing their quality specified in 'General Options' tab.<br />
			Please note that enabling this options may cause some overhead to server. You can enable this config from 'General Options' tab.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkImgCompressorActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkImgCompressorChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Encode Url
			<br /><span class="field_desc">(Encodes original site address and hides it. This is highly recommended option to pass the blockers on the internet connection.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkEncodeUrlActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkEncodeUrlChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Cookies
			<br /><span class="field_desc">(Enables cookie support.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkCookiesActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkCookiesChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Temp Cookies
			<br /><span class="field_desc">(Saves cookies only for current session, and they will not be available after browser is closed.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkTempCookiesActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkTempCookiesChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Images
			<br /><span class="field_desc">(Displays images.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkImagesActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkImagesChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Links
			<br /><span class="field_desc">(Processes links and encodes them.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkLinksActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkLinksChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Frames
			<br /><span class="field_desc">(Enables inline frames.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkFramesActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkFramesChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Submit Forms
			<br /><span class="field_desc">(Processes submit forms.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkSubmitFormsActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkSubmitFormsChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Remove Objects
			<br /><span class="field_desc">(Removes embedded objects.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkRemoveObjectsActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkRemoveObjectsChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Orginal Url
			<br /><span class="field_desc">(Displays original URL address in a float bar on the top of page.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkOrginalUrlActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkOrginalUrlChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Page Title
			<br /><span class="field_desc">(Displays page title in browser title.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkPageTitleActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkPageTitleChangeable" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			Document Type
			<br /><span class="field_desc">(Automatically apply back-end site document type, so it will show correctly in correct format.)</span></td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkDocTypeActive" runat="server" /> 
		</td>
		<td style="text-align:center">
			<asp:CheckBox ID="chkDocTypeChangeable" runat="server" />
		</td>
	</tr>
</table>
</fieldset>
	<asp:Button ID="btnSave" CssClass="submit_button" runat="server" Text="Save" OnClick="btnSave_Click" />
	<input id="btnCancel" class="submit_button" type="button" value="Cancel"  onclick="document.location='default.aspx'" />

</asp:Content>
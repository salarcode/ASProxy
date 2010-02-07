<%@ Page Language="C#" MasterPageFile="~/admin/AdminUI.master" AutoEventWireup="true" CodeFile="back-end.aspx.cs" Inherits="Admin_BackEnd" Title="Back-End Site Communication Config" %>


<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	Back-End
    Site Communication Config 
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<div class="error_list">
    <asp:Literal ID="ltErrorsList" EnableViewState="false" runat="server"></asp:Literal>
	<asp:ValidationSummary ID="vldErrorsList" runat="server" EnableViewState="false" />
	</div>
	<fieldset>
		<legend>Web Data</legend>
		<table style="width: 100%;">
			<tr>
				<td>Max content length
				</td>
				<td><asp:TextBox ID="txtMaxContentLength" runat="server"></asp:TextBox>
					<asp:RegularExpressionValidator ID="vldMaxContentLength" runat="server" 
                        ErrorMessage="Max content length is invalid" 
                        ControlToValidate="txtMaxContentLength" Display="Dynamic" 
                        ValidationExpression="(\-1|\d{1,18})" CssClass="field_error">Invalid number</asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ID="reqMaxContentLength"  
                        ControlToValidate="txtMaxContentLength" Display="Dynamic" runat="server" 
                        CssClass="field_error" ErrorMessage="Max content length is required">Required</asp:RequiredFieldValidator>
					<br /><span class="field_desc">(in bytes, -1 for unlimited)</span></td>
			</tr>
			<tr>
				<td>Request timeout
				</td>
				<td><asp:TextBox ID="txtRequestTimeout" runat="server"></asp:TextBox>
					<asp:RegularExpressionValidator ID="vldRequestTimeout" runat="server" 
                        ErrorMessage="Max content length is invalid" 
                        ControlToValidate="txtRequestTimeout" Display="Dynamic" 
                        ValidationExpression="\d{1,18}" CssClass="field_error">Invalid number</asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ID="reqRequestTimeout" Display="Dynamic" 
                        ControlToValidate="txtRequestTimeout" runat="server" 
                        CssClass="field_error" ErrorMessage="Request timeout is required">Required</asp:RequiredFieldValidator>
				<br /><span class="field_desc">(In milliseconds, before the request times out.)</span>
				</td>
			</tr>
			<tr>
				<td>Request read write timeout
				</td>
				<td><asp:TextBox ID="txtRequestReadWriteTimeOut" runat="server"></asp:TextBox>
					<asp:RegularExpressionValidator ID="vldRequestReadWriteTimeOut" runat="server" 
                        ErrorMessage="Max content length is invalid" 
                        ControlToValidate="txtRequestReadWriteTimeOut" Display="Dynamic" 
                        ValidationExpression="\d{1,18}" CssClass="field_error">Invalid number</asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ID="reqRequestReadWriteTimeOut"  
                        ControlToValidate="txtRequestReadWriteTimeOut" Display="Dynamic" 
                        runat="server" CssClass="field_error" 
                        ErrorMessage="Request read write timeout is required">Required</asp:RequiredFieldValidator>
				<br /><span class="field_desc">(In milliseconds when writing to or reading from a stream (Back-end site).)</span></td>
			</tr>
			<tr>
				<td>Send signature
				</td>
				<td><asp:CheckBox ID="chkSendSignature" runat="server" Text="Enabled" />
				<br /><span class="field_desc">(Send ASProxy signature header to Back-end and Front-end users.)</span></td>
			</tr>
			<tr>
				<td>Preferred local encoding
				</td>
				<td><asp:TextBox ID="txtPreferredLocalEncoding" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="reqPreferredLocalEncoding"  
                        ControlToValidate="txtPreferredLocalEncoding" Display="Dynamic" 
                        runat="server" CssClass="field_error" 
                        ErrorMessage="Request read write timeout is required">Required</asp:RequiredFieldValidator>
				<br /><span class="field_desc">(Preferred encoding for unrecognized page encodings.)</span></td>
			</tr>
			</table>
	</fieldset>
	<fieldset>
		<legend>User Agent</legend>
		<table style="width: 100%;">
			<tr>
				<td>Useragent mode
				</td>
				<td><asp:DropDownList ID="cmbUserAgentMode" runat="server">
					</asp:DropDownList>
					<br /><span class="field_desc">(Back-end UserAgent behavior.)</span>
				</td>
			</tr>
			<tr>
				<td>Custom UserAgent
				</td>
				<td><asp:TextBox ID="txtCustomUserAgent" runat="server"></asp:TextBox>
				<br /><span class="field_desc">(UserAgent for Back-End.)</span></td>
			</tr>
			</table>
	</fieldset>
	<fieldset>
		<legend>Downloader</legend>
		<table style="width: 100%;">
			<tr>
				<td>Resume-support
				</td>
				<td><asp:CheckBox ID="chkResumeSupport" runat="server" Text="Enabled" />
				<br /><span class="field_desc">(Downloader Resume-Support feature.)</span></td>
			</tr>
			<tr>
				<td>Max content length
				</td>
				<td><asp:TextBox ID="txtDownMaxContentLength" runat="server"></asp:TextBox>
					<asp:RegularExpressionValidator ID="vldDownMaxContentLength" runat="server" 
                        ErrorMessage="Max content length is invalid" 
                        ControlToValidate="txtDownMaxContentLength" Display="Dynamic" 
                        ValidationExpression="(\-1|\d{1,18})" CssClass="field_error">Invalid number</asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ID="reqDownMaxContentLength"  
                        ControlToValidate="txtDownMaxContentLength" Display="Dynamic" 
                        runat="server" CssClass="field_error">Required</asp:RequiredFieldValidator>
				<br /><span class="field_desc">(Downloader specific. In bytes, -1 for unlimited.)</span></td>
			</tr>
			<tr>
				<td>Request timeout
				</td>
				<td><asp:TextBox ID="txtDownRequestTimeout" runat="server"></asp:TextBox>
					<asp:RegularExpressionValidator ID="vldDownRequestTimeout" runat="server" 
                        ErrorMessage="Max content length is invalid" 
                        ControlToValidate="txtDownRequestTimeout" Display="Dynamic" 
                        ValidationExpression="\d{1,18}" CssClass="field_error">Invalid number</asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ID="reqDownRequestTimeout" Display="Dynamic" 
                        ControlToValidate="txtDownRequestTimeout" runat="server" 
                        CssClass="field_error">Required</asp:RequiredFieldValidator>
				<br /><span class="field_desc">(Downloader specific. In milliseconds, before the request times out.)</span></td>
			</tr>
			<tr>
				<td>Request read write timeout
				</td>
				<td><asp:TextBox ID="txtDownRequestReadWriteTimeOut" runat="server"></asp:TextBox>
					<asp:RegularExpressionValidator ID="vldDownRequestReadWriteTimeOut" runat="server" 
                        ErrorMessage="Max content length is invalid" 
                        ControlToValidate="txtDownRequestReadWriteTimeOut" Display="Dynamic" 
                        ValidationExpression="\d{1,18}" CssClass="field_error">Invalid number</asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ID="reqDownRequestReadWriteTimeOut" Display="Dynamic" 
                        ControlToValidate="txtDownRequestReadWriteTimeOut" runat="server" 
                        CssClass="field_error">Required</asp:RequiredFieldValidator>
				<br /><span class="field_desc">(Downloader specific. In milliseconds when writing to or reading from a stream (Back-end site).)</span></td>
			</tr>
		</table>
	</fieldset>
	<fieldset>
		<legend>Network proxy</legend>
		<table style="width: 100%;">
			<tr>
				<td>Enabled
				</td>
				<td><asp:DropDownList ID="cmbNetProxyMode" runat="server"></asp:DropDownList>
				<br /><span class="field_desc">(Determines proxy configuration to connect to the back-end websites.)</span></td>
			</tr>
			<tr>
				<td>Custom proxy address:</td>
				<td><asp:TextBox ID="txtNetCustomProxyAddress" runat="server"></asp:TextBox>
				<br /><span class="field_desc">(Proxy address for custom configuration.)</span></td>
			</tr>
			<tr>
				<td>Custom proxy port:
				</td>
				<td><asp:TextBox ID="txtNetCustomProxyPort" runat="server"></asp:TextBox>
					<asp:RegularExpressionValidator ID="vldNetCustomProxyPort" runat="server" 
                        ErrorMessage="Custom proxy port is invalid" 
                        ControlToValidate="txtNetCustomProxyPort" Display="Dynamic" 
                        ValidationExpression="\d{1,10}" CssClass="field_error">Invalid number</asp:RegularExpressionValidator>
				<br /><span class="field_desc">(Proxy port for custom configuration.)</span></td>
			</tr>
			<tr>
				<td>Authentication required</td>
				<td><asp:CheckBox ID="chkAuthentication" runat="server"  Text="Enabled" />
				<br /><span class="field_desc">(Is authentication required for specified custom proxy.)</span></td>
			</tr>
			<tr>
				<td>Authentication Username:</td>
				<td><asp:TextBox ID="txtAuthUsername" runat="server"></asp:TextBox>
				<br /><span class="field_desc">(Custom proxy username.)</span></td>
			</tr>
			<tr>
				<td>Authentication Password:</td>
				<td><asp:TextBox ID="txtAuthPassword" runat="server"></asp:TextBox>
				<br /><span class="field_desc">(Custom proxy password.)</span></td>
			</tr>
		</table>
	</fieldset>
	<asp:Button ID="btnSave" runat="server" Text="Save" CssClass="submit_button" 
        onclick="btnSave_Click" />
	<input type="button" value="Cancel" class="submit_button" onclick="document.location='default.aspx'" />
</asp:Content>

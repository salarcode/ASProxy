<%@ Page Language="C#" MasterPageFile="~/admin/AdminUI.master" Title="Backend Page" %>

<script runat="server">

</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	Back-end
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<fieldset>
		<legend>Web Data</legend>
		<table style="width: 100%;">
			<tr>
				<td>Max content length
				</td>
				<td><asp:TextBox ID="txtMaxContentLength" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td>Request timeout
				</td>
				<td><asp:TextBox ID="txtRequestTimeout" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td>Request read write timeout
				</td>
				<td><asp:TextBox ID="txtRequestReadWriteTimeOut" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td>Send signature
				</td>
				<td><asp:CheckBox ID="chkSendSignature" runat="server" />
				</td>
			</tr>
			<tr>
				<td>Preferred local encoding
				</td>
				<td><asp:TextBox ID="txtPreferredLocalEncoding" runat="server"></asp:TextBox>
				</td>
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
				</td>
			</tr>
			<tr>
				<td>Custom UserAgent
				</td>
				<td><asp:TextBox ID="txtCustomUserAgent" runat="server"></asp:TextBox>
				</td>
			</tr>
			</table>
	</fieldset>
	<fieldset>
		<legend>Downloader</legend>
		<table style="width: 100%;">
			<tr>
				<td>Resume-support
				</td>
				<td><asp:CheckBox ID="chkResumeSupport" runat="server" />
				</td>
			</tr>
			<tr>
				<td>Max content length
				</td>
				<td>
					<asp:TextBox ID="txtDownMaxContentLength" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td>Request timeout
				</td>
				<td><asp:TextBox ID="txtDownRequestTimeout" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td>Request read write timeout
				</td>
				<td><asp:TextBox ID="txtDownRequestReadWriteTimeOut" runat="server"></asp:TextBox>
				</td>
			</tr>
		</table>
	</fieldset>
	<fieldset>
		<legend>Network proxy</legend>
		<table style="width: 100%;">
			<tr>
				<td>Enabled
				</td>
				<td><asp:DropDownList ID="cmbUserAgentMode0" runat="server">
					</asp:DropDownList>
				</td>
			</tr>
			<tr>
				<td>Custom proxy address:</td>
				<td>
					<asp:TextBox ID="txtNetProxyAddress" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td>Custom proxy port:
				</td>
				<td><asp:TextBox ID="txtNetProxyPort1" runat="server"></asp:TextBox>
										</td>
			</tr>
			<tr>
				<td>Authentication required</td>
				<td>
					<asp:CheckBox ID="CheckBox1" runat="server" />
				</td>
			</tr>
			<tr>
				<td>Authentication Username:</td>
				<td><asp:TextBox ID="txtNetProxyPort" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td>Authentication Password:</td>
				<td><asp:TextBox ID="txtNetProxyPort0" runat="server"></asp:TextBox>
				</td>
			</tr>
		</table>
	</fieldset>
	<asp:Button ID="btnSave" runat="server" Text="Save" />
	<input type="button" value="Cancel" />
</asp:Content>

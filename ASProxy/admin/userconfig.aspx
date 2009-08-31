<%@ Page Language="C#" MasterPageFile="~/admin/AdminUI.master" Title="Untitled Page" %>

<script runat="server">

	private void RealodFormData()
	{
		//SalarSoft.ASProxy.Configurations.UserOptions.
	}
	
	protected void Page_Load(object sender, EventArgs e)
	{

	}
</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" Runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" Runat="Server">
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" Runat="Server">
<fieldset><legend>User Config</legend>
	<table style="width: 100%;">
		<tr>
			<td>
				Force Encoding</td>
			<td>
				<asp:CheckBox ID="chkForceEncodingActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkForceEncodingChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Remove Scripts</td>
			<td>
				<asp:CheckBox ID="chkRemoveScriptsActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkRemoveScriptsChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Remove Images</td>
			<td>
				<asp:CheckBox ID="chkRemoveImagesActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkRemoveImagesChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Http Compression</td>
			<td>
				<asp:CheckBox ID="chkCompressionActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkCompressionChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Encode Url</td>
			<td>
				<asp:CheckBox ID="chkEncodeUrlActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkEncodeUrlChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Cookies</td>
			<td>
				<asp:CheckBox ID="chkCookiesActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkCookiesChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Temp Cookies</td>
			<td>
				<asp:CheckBox ID="chkTempCookiesActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkTempCookiesChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Images</td>
			<td>
				<asp:CheckBox ID="chkImagesActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkImagesChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Links</td>
			<td>
				<asp:CheckBox ID="chkLinksActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkLinksChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Frames</td>
			<td>
				<asp:CheckBox ID="chkFramesActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkFramesChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Submit Forms</td>
			<td>
				<asp:CheckBox ID="chkSubmitFormsActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkSubmitFormsChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Remove Objects</td>
			<td>
				<asp:CheckBox ID="chkRemoveObjectsActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkRemoveObjectsChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Orginal Url</td>
			<td>
				<asp:CheckBox ID="chkOrginalUrlActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkOrginalUrlChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Page Title</td>
			<td>
				<asp:CheckBox ID="chkPageTitleActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkPageTitleChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
		<tr>
			<td>
				Document Type</td>
			<td>
				<asp:CheckBox ID="chkDocTypeActive" Text="Active" runat="server" /> 
				<asp:CheckBox ID="chkDocTypeChangeable" runat="server" Text="Changeable" />
			</td>
		</tr>
	</table>
</fieldset>
	<asp:Button ID="btnSave" runat="server" Text="Save" />
	<input id="btnCancel" type="button" value="Cancel" />

</asp:Content>
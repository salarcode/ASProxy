<%@ Page Language="C#" MasterPageFile="~/admin/AdminUI.master" Title="General Page" %>

<script runat="server">

</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" Runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" Runat="Server">
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" Runat="Server">
<fieldset><legend>UI Language</legend>UI Language:
<asp:DropDownList ID="cmbUiLanguage" runat="server"></asp:DropDownList>
</fieldset>
	<asp:Button ID="btnSave" runat="server" Text="Save" />
	<input id="btnCancel" type="button"
		value="Cancel" />
</asp:Content>
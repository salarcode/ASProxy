<%@ Page Language="C#" MasterPageFile="~/admin/AdminUI.master" Title="Log system Page" %>

<script runat="server">

</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	Log system
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<fieldset>
		<legend>Activity log</legend>shit
		<asp:DropDownList ID="cmbActivityList" runat="server">
		</asp:DropDownList>
		<asp:Button ID="btnAView" runat="server" Text="View" />
		<asp:Button ID="btnADownload" runat="server" Text="Download" />
		<asp:Button ID="btnADelete" runat="server" Text="Delete" />
		<asp:Button ID="btnAClear" runat="server" Text="Delete all" />
	</fieldset>
	<fieldset>
		<legend>Error log</legend>shit
		<asp:DropDownList ID="cmbErrorsList"  runat="server">
		</asp:DropDownList>
		<asp:Button ID="btnEView" runat="server" Text="View" />
		<asp:Button ID="btnEDownload" runat="server" Text="Download" />
		<asp:Button ID="btnEDelete" runat="server" Text="Delete" />
		<asp:Button ID="btnEClear" runat="server" Text="Delete all" />
	</fieldset>
	<fieldset>
		<asp:TextBox ID="TextBox1" runat="server" Rows="20" TextMode="MultiLine" 
			Width="100%"></asp:TextBox>
	
	</fieldset>
</asp:Content>

<%@ Page Title="Saved logs by Log System" Language="C#" MasterPageFile="~/admin/AdminUI.master" AutoEventWireup="true" CodeFile="logsystem.aspx.cs" Inherits="Admin_Logsystem" %>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	Saved logs by Log System
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
<span>You can enable/disable log system from General Options tab.</span>
	<fieldset>
		<legend>Activity log</legend>
		Log file:
		<asp:DropDownList ID="cmbActivityList" runat="server" Width="200">
		</asp:DropDownList>
		<asp:Button class="submit_button" ID="btnAView" runat="server" Text="View" 
            OnClientClick="DeleteLog()" onclick="btnAView_Click" />
		<asp:Button class="submit_button" ID="btnADownload" runat="server" 
            Text="Download" OnClientClick="DeleteLog()" onclick="btnADownload_Click" />
		<asp:Button class="submit_button" ID="btnADelete" runat="server" Text="Delete" 
            OnClientClick="DeleteLog()" onclick="btnADelete_Click" />
		<asp:Button class="submit_button" ID="btnAClear" runat="server" 
            Text="Delete all" OnClientClick="DeleteLog()" onclick="btnAClear_Click" />
	</fieldset>
	<fieldset>
		<legend>Error log</legend>
		Log file:
		<asp:DropDownList ID="cmbErrorsList" runat="server" Width="200">
		</asp:DropDownList>
		<asp:Button class="submit_button" ID="btnEView" runat="server" Text="View" 
            OnClientClick="DeleteLog()" onclick="btnEView_Click" />
		<asp:Button class="submit_button" ID="btnEDownload" runat="server" 
            Text="Download" OnClientClick="DeleteLog()" onclick="btnEDownload_Click" />
		<asp:Button class="submit_button" ID="btnEDelete" runat="server" Text="Delete" 
            OnClientClick="DeleteLog()" onclick="btnEDelete_Click" />
		<asp:Button class="submit_button" ID="btnEClear" runat="server" 
            Text="Delete all" OnClientClick="DeleteLog()" onclick="btnEClear_Click" />
	</fieldset>
	<fieldset>
	
	<script type="text/javascript">
	    function DeleteLog() {
	        var txtViewLog = '<%=txtViewLog.ClientID%>';
	        document.getElementById(txtViewLog).value = '';
	    }
	</script>
		<asp:TextBox ID="txtViewLog" EnableViewState="false" runat="server" Rows="20" TextMode="MultiLine" 
			Width="100%"></asp:TextBox>
	
	</fieldset>
</asp:Content>

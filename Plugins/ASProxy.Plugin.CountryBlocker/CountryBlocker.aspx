<%@ Page Title="Country Blocker Configigurations" Language="C#" MasterPageFile="~/admin/AdminUI.master"
	AutoEventWireup="true" CodeFile="CountryBlocker.aspx.cs" Inherits="Admin_CountryBlocker" %>

<script runat="server">

</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
	<style type="text/css">
		</style>
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	Country Blocker Configurations
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<div class="error_list">
		<asp:Literal ID="ltErrorsList" EnableViewState="false" runat="server"></asp:Literal>
		<asp:ValidationSummary ID="vldErrorsList" runat="server" EnableViewState="false" />
	</div>
	<fieldset>
		<legend>Add a new rule</legend><span class="field_desc">(You can allow or block your desigred countries access here.)</span>
		<table style="width: 100%;">
			<tr>
				<td>
					Rule mode:
				</td>
				<td>
					<asp:RadioButton ID="rbtnAllow" runat="server" Text="Allow the country" GroupName="RuleMode" />
					<br />
					<asp:RadioButton ID="rbtnBlock" runat="server" Text="Block the country" GroupName="RuleMode" />
				</td>
			</tr>
			<tr>
				<td>
					Country:
				</td>
				<td>
					<asp:DropDownList EnableViewState="false" ID="cmbAddCountry" runat="server">
					</asp:DropDownList>
				</td>
			</tr>
			<tr>
				<td>
					<asp:Button ID="btnAddRule" CssClass="submit_button" runat="server" Text="Add Rule"
						OnClick="btnAddRule_Click" />
				</td>
				<td>
					<b>Note:</b> If you specify an allow rule, all the block rules will be ignored!
				</td>
			</tr>
		</table>
	</fieldset>
	<fieldset>
		<legend>Allowed countries</legend>
		<table style="width: 100%;">
			<tr>
				<td>
					Manage allowed countries
				</td>
				<td>
					<asp:ListBox ID="lstAllowedCountries" runat="server" CausesValidation="True"></asp:ListBox>
					<br />
					<span class="field_desc">(Here is the allowed countries list and you can delete them
						here.)</span>
					<br />
					<asp:Button ID="btnDeleteAllowed" CssClass="submit_button" runat="server" Text="Delete"
						OnClick="btnDeleteAllowed_Click" />
					<asp:CheckBox ID="chkDeleteAllowedSure" runat="server" Text="Are you sure?" />
				</td>
			</tr>
		</table>
	</fieldset>
	<fieldset>
		<legend>Blocked countries</legend>
		<table style="width: 100%;">
			<tr>
				<td>
					Manage blocked countries
				</td>
				<td>
					<asp:ListBox ID="lstBlockedCountries" runat="server" CausesValidation="True"></asp:ListBox>
					<br />
					<span class="field_desc">(Here is the blocked countries list and you can delete them
						here.)</span>
					<br />
					<asp:Button ID="btnDeleteBlocked" CssClass="submit_button" runat="server" Text="Delete"
						OnClick="btnDeleteBlocked_Click" />
					<asp:CheckBox ID="chkDeleteBlockedSure" runat="server" Text="Are you sure?" />
				</td>
			</tr>
		</table>
	</fieldset>
</asp:Content>

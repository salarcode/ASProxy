<%@ Page Title="ASProxy Users Authentication" Language="C#" MasterPageFile="~/admin/AdminUI.master"
	AutoEventWireup="true" CodeFile="users.aspx.cs" Inherits="Admin_Users" %>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	ASProxy Users Authentication
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<div class="error_list">
		<asp:Literal ID="ltErrorsList" EnableViewState="false" runat="server"></asp:Literal>
		<asp:ValidationSummary ID="vldErrorsList" runat="server" EnableViewState="false" />
	</div>
	<fieldset>
		<legend>Authentication</legend>
		<table style="width: 100%;">
			<tr>
				<td>
					Login required to ASProxy
				</td>
				<td>
					<asp:CheckBox ID="chkLoginEnabled" runat="server" Text="Login is Required" Enabled="False" />
					<asp:Button ID="btnLoginEnable" runat="server" Text="Enable" Style="padding: 0px;
						margin: 0px; font-size: x-small;" OnClick="btnLoginEnable_Click" />/<asp:Button ID="btnLoginDisable"
							runat="server" Text="Disable" Style="padding: 0px; margin: 0px; font-size: x-small;"
							OnClick="btnLoginDisable_Click" />
					<br />
					<span class="field_desc">(Helps you to prevent strangers to access your hosted ASProxy,
						and save bandwidth!)</span>
				</td>
			</tr>
			<tr>
				<td>
					Add new user:
				</td>
				<td>
					<fieldset>
						<legend>New ASProxy user</legend><span class="field_desc">(Specify users who can access
							to ASProxy. Also you can specify user limitations.)</span>
						<table style="width: 100%;">
							<tr>
								<td>
									Username:
								</td>
								<td>
									<asp:TextBox ID="txtNewUsername" runat="server"></asp:TextBox>
									<asp:RequiredFieldValidator ID="reqNewUsername" runat="server" ControlToValidate="txtNewUsername"
										ErrorMessage="Username is required" ValidationGroup="NewUser">Required</asp:RequiredFieldValidator>
								</td>
							</tr>
							<tr>
								<td>
									Password:
								</td>
								<td>
									<asp:TextBox ID="txtNewPassword" runat="server"></asp:TextBox>
									<asp:RequiredFieldValidator ID="reqNewPassword" runat="server" ControlToValidate="txtNewPassword"
										ErrorMessage="Password is required" ValidationGroup="NewUser">Required</asp:RequiredFieldValidator>
								</td>
							</tr>
							<tr>
								<td>
								</td>
								<td>
									<asp:CheckBox ID="chkNewPages" runat="server" Text="Pages" Checked="True" />
									<asp:CheckBox ID="chkNewImages" runat="server" Text="Images" Checked="True" />
									<asp:CheckBox ID="chkNewDownloads" runat="server" Text="Downloads" Checked="True" />
									<br />
									<span class="field_desc">(Specify three kind of access that a user can have.)</span>
								</td>
							</tr>
							<tr>
								<td>
									<asp:Button CssClass="submit_button" ID="btnNewUserAdd" runat="server" Text="Add user"
										OnClick="btnNewUserAdd_Click" ValidationGroup="NewUser" />
								</td>
								<td>
								</td>
							</tr>
						</table>
					</fieldset>
				</td>
			</tr>
			<tr>
				<td>
					Users list:
				</td>
				<td>
					<asp:ListBox ID="lstUsersList" runat="server" CausesValidation="True"></asp:ListBox>
					<br />
					<span class="field_desc">(You can delete the users from here. Note that at least one
						user should be there to this feature work properly.)</span>
					<br />
					<asp:Button ID="btnDeleteUser" CssClass="submit_button" runat="server" Text="Delete"
						OnClick="btnDeleteUser_Click" />
					<asp:CheckBox ID="chkDeleteUserSure" runat="server" Text="Are you sure?" />
				</td>
			</tr>
		</table>
	</fieldset>
	<fieldset>
		<legend>User Access Control</legend>
		<table style="width: 100%;">
			<tr>
				<td>
					User Access Control</td>
				<td>
					<asp:CheckBox ID="chkUACEnabled" runat="server" Text="UAC is Enabled" Enabled="false"/>
					<asp:Button ID="btnUACEnable" runat="server" Text="Enable" Style="padding: 0px;
						margin: 0px; font-size: x-small;" onclick="btnUACEnable_Click" />/<asp:Button 
						ID="btnUACDisable" Enabled="false"
							runat="server" Text="Disable" Style="padding: 0px; margin: 0px; font-size: x-small;" 
						onclick="btnUACDisable_Click" />
					
					<br />
					<span class="field_desc">(Enables you to block or allow certain IP addresses from/to accessing to your ASProxy.)</span>
				</td>
			</tr>
			<tr>
				<td>
					Add
				</td>
				<td>
					<fieldset>
						<legend>Add new rule</legend><span class="field_desc">(Allow/Block access to ASProxy
							from a specified IP address or from a range of addresses.)</span>
						<table style="width: 100%;">
							<tr>
								<td>
									Rule mode:
								</td>
								<td>
									<asp:RadioButton ID="rbtnUacAllow" runat="server" Text="Allow the address(es)" GroupName="RuleMode"
										Checked="True" />
									<br />
									<asp:RadioButton ID="rbtnUacBlock" runat="server" Text="Block the address(es)" GroupName="RuleMode" />
								</td>
							</tr>
							<tr>
								<td>
									<asp:RadioButton ID="rbtnUacAddIP" runat="server" Text="Single IP" GroupName="Range"
										Checked="True" />
								</td>
								<td>
									IP Address:<br />
									<asp:TextBox ID="txtSingleIP" runat="server"></asp:TextBox>
								</td>
							</tr>
							<tr>
								<td>
									<asp:RadioButton ID="rbtnUacAddRange" runat="server" Text="IP Range" GroupName="Range" />
								</td>
								<td>
									Low range:<br />
									<asp:TextBox ID="txtLowRangeIP" runat="server"></asp:TextBox>
									<br />
									High range:<br />
									<asp:TextBox ID="txtHighRangeIP" runat="server"></asp:TextBox>
								</td>
							</tr>
							<tr>
								<td>
									<asp:Button ID="btnUacAddRule" CssClass="submit_button" runat="server" Text="Add Rule"
										OnClick="btnUacAddRule_Click" />
								</td>
								<td>
									<b>Note:</b> If you specify an allow rule, all the block rules will be ignored!
								</td>
							</tr>
						</table>
					</fieldset>
				</td>
			</tr>
			<tr>
				<td>
					Rules
				</td>
				<td>
					<asp:ListBox ID="lstUACList" runat="server" CausesValidation="True"></asp:ListBox>
					<br />
					<span class="field_desc">(You can delete rules from here.)</span>
					<br />
					<asp:Button CssClass="submit_button" ID="btnUACDelete" runat="server" Text="Delete"
						OnClick="btnUACDelete_Click" />
					<asp:CheckBox ID="chkUACDeleteSure" runat="server" Text="Are you sure?" />
				</td>
			</tr>
		</table>
	</fieldset>
</asp:Content>

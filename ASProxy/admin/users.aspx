<%@ Page Language="C#" MasterPageFile="~/admin/AdminUI.master" Title="Users Page" %>

<script runat="server">

</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	authentication
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<fieldset>
		<legend>Authentication</legend>
		<table style="width: 100%;">
			<tr>
				<td>
					Authentication enabled
				</td>
				<td>
					<asp:CheckBox ID="chkEnabled" runat="server" Text="enabled"/>
				</td>
			</tr>
			<tr>
				<td>
					Add user:</td>
				<td>
				
					<table style="width: 100%;">
						<tr>
							<td>
								Username:</td>
							<td>
					<asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
							</td>
						</tr>
						<tr>
							<td>
								Password:</td>
							<td>
					<asp:TextBox ID="TextBox3" runat="server"></asp:TextBox>
							</td>
						</tr>
						<tr>
							<td colspan="2">
								<asp:CheckBox ID="chkPages" runat="server" Text="Pages"/>
								<asp:CheckBox ID="chkImages" runat="server" Text="Images"/>
								<asp:CheckBox ID="chkDownloads" runat="server" Text="Downloads"/>
							</td>
						</tr>
						<tr>
							<td>
								&nbsp;</td>
							<td>
								<asp:Button ID="btnAdd" runat="server" Text="Add user" />
							</td>
						</tr>
						</table>
				</td>
			</tr>
			<tr>
				<td>
					Users list:</td>
				<td>
					<asp:ListBox ID="lstUsersList" runat="server" CausesValidation="True">
					</asp:ListBox>
					<asp:Button ID="btnDeleteUser" runat="server" Text="Delete" />
				</td>
			</tr>
			<tr>
				<td>
					&nbsp;</td>
				<td>
					&nbsp;</td>
			</tr>
			<tr>
				<td>
					&nbsp;</td>
				<td>
					&nbsp;</td>
			</tr>
		</table>
	</fieldset>
	<fieldset>
		<legend>User Access Control</legend>
		<table style="width: 100%;">
			<tr>
				<td>
					Add</td>
				<td>
					<table style="width:100%;">
						<tr>
							<td>
								<asp:RadioButton ID="rbtnUacAddIP" runat="server" Text="Single IP" GroupName="Range" />
							</td>
							<td>
								IP Name:<br />
								<asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
													</td>
													<td>
														&nbsp;</td>
												</tr>
												<tr>
													<td>
														<asp:RadioButton ID="rbtnUacAddRange" runat="server" Text="IP Range" GroupName="Range" />
													</td>
													<td>
														Low range:<br />
														<asp:TextBox ID="TextBox5" runat="server"></asp:TextBox>
														<br />
														High range:<br />
														<asp:TextBox ID="TextBox6" runat="server"></asp:TextBox>
													</td>
													<td>
														&nbsp;</td>
												</tr>
												<tr>
													<td>
														&nbsp;</td>
													<td>
								<asp:Button ID="btnUacAddRule" runat="server" Text="Add Rule" />
													</td>
													<td>
														&nbsp;</td>
												</tr>
											</table>
				</td>
			</tr>
			<tr>
				<td>
					Rules</td>
				<td>
					<asp:ListBox ID="lstUACList" runat="server" CausesValidation="True">
					</asp:ListBox>
					<asp:Button ID="btnUACDelete" runat="server" Text="Delete" />
				</td>
			</tr>
		</table>
	</fieldset>
	
</asp:Content>

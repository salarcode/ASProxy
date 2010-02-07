<%@ Page Title="Login to Administration UI" Language="C#" MasterPageFile="~/admin/AdminUI.master"
	AutoEventWireup="true" CodeFile="adminlogin.aspx.cs" Inherits="Admin_AdminLogin" %>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	<asp:Label ID="lblTitleLogin" Visible="true" runat="server" Text="Administration UI User Login" EnableViewState="false"></asp:Label>
	<asp:Label ID="lblTitleRegister" Visible="false" runat="server" Text="Administration UI User Register" EnableViewState="false"></asp:Label>
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<div class="error_list">
		<asp:Literal ID="ltErrorsList" EnableViewState="false" runat="server"></asp:Literal>
	</div>
	<asp:Label ID="lblRegisterMessage" runat="server" Text="This is first time you are opening Administation UI. To prevent strangers from changing your ASProxy configuration you should specify username and password." EnableViewState="false" Visible="false"></asp:Label>
	<asp:Label ID="lblLoginMessage" runat="server" Text="To be able to see and change the configurations you should login with administration account." EnableViewState="false" Visible="false"></asp:Label>
	<fieldset>
		<table style="width: 100%;">
			<tr>
				<td>
					Username:
				</td>
				<td>
					<asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
					<asp:RequiredFieldValidator ID="reqUsername" runat="server" ControlToValidate="txtUsername"
						ErrorMessage="Username is required" ValidationGroup="NewUser">Required</asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					Password:
				</td>
				<td>
					<asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox>
					<asp:RequiredFieldValidator ID="reqPassword" runat="server" ControlToValidate="txtPassword"
						ErrorMessage="Password is required" ValidationGroup="NewUser">Required</asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					&nbsp;
				</td>
				<td>
					<asp:Button CssClass="submit_button" ID="btnAdminLogin" runat="server" Text="Login"
						ValidationGroup="NewUser" Visible="false" OnClick="btnAdminLogin_Click" />
					<asp:Button CssClass="submit_button" ID="btnAdminRegister" runat="server" Text="Register"
						ValidationGroup="NewUser" Visible="false" OnClick="btnAdminRegister_Click" />
				</td>
			</tr>
		</table>
	</fieldset>
</asp:Content>

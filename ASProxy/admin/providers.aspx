<%@ Page Language="C#" MasterPageFile="~/admin/AdminUI.master" Title="Providers" %>

<script runat="server">

	protected void btnEnableDisableClick(object sender, EventArgs e)
	{

	}
</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	Providers
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<fieldset>
		<legend>Providers</legend>
		<table style="width: 100%;">
			<tr>
				<td>
					Providers canBeOverwritten
				</td>
				<td>
					<asp:CheckBox ID="chkCanBeOverwritten" runat="server" />
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
					plugins
				</td>
				<td>
					<asp:CheckBox ID="chkEnabled" Text="Plugins Enabled" runat="server" />
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
		</table>
	</fieldset>
		<fieldset>
		<legend>Auto Update</legend>
		<table style="width: 100%;">
			<tr>
				<td>
					ASProxy autoupdate
				</td>
				<td>
					<asp:CheckBox ID="chkASProxyUpdate" runat="server" Text="Enabled" />
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
					ASProxy update info Url
				</td>
				<td>
					<asp:TextBox ID="txtUpdateInfoUrl" runat="server" Text="http://asproxy.sourceforge.net/update/autoupdate.xml"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
					Providers autoupdate
				</td>
				<td>
					<asp:CheckBox ID="chkUpdateProviders" runat="server" Text="Enabled" />
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
					Plugins autoupdate
				</td>
				<td>
					<asp:CheckBox ID="chkUpdatePlugins" Text="Enabled" runat="server" />
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
		</table>
</fieldset>
	<fieldset>
		<legend>Plugins</legend>
		<asp:Repeater ID="rptPlugins" runat="server">
			<HeaderTemplate>
				<table cellpadding="0" cellspacing="0" class="tblOptions">
					<tr class="option">
						<td>
						Details
						</td>
						<td>
							State
						</td>
						<td>
							
							Options
						</td>
					</tr>
			</HeaderTemplate>
			<FooterTemplate></table></FooterTemplate>
			<ItemTemplate>
				<tr class="option">
					<td class="name">
						State
					</td>
					<td class="desc">dddddddddddddddddddddddddddd
					</td>
					<td>
						<asp:Button CssClass="button" ID="btnEnableDisable" runat="server" OnClick="btnEnableDisableClick"
							CommandName="EnableDisable" CommandArgument="<%# Container.DataItem.ToString() %>"
							Text="Enable/Disable" />
					</td>
				</tr>
			</ItemTemplate>
		</asp:Repeater>
	</fieldset>
</asp:Content>

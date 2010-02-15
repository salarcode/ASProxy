<%@ Page Title="Engine Extensions" Language="C#" MasterPageFile="~/admin/AdminUI.master"
	AutoEventWireup="true" CodeFile="providers.aspx.cs" Inherits="Admin_Providers" %>

<%@ Import Namespace="SalarSoft.ASProxy" %>

<script runat="server">
</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
	<style type="text/css">
		fieldset td:first-child, .first_cell
		{
			width: 150px !important;
		}
	</style>
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	ASProxy Engine Extensions
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<fieldset>
		<legend>Engine Extensions</legend>
		<table class="options_table" style="width: 100%;">
			<tr>
				<td>
					Providers
				</td>
				<td>
					<asp:CheckBox ID="chkCanBeOverwritten" runat="server" Text="Built-in providers can be overridden by third-party providers" />
					<br />
					<span class="field_desc">(Enables third-party providers. Providers are a complete replacement
						of ASProxy built-in engines. So if this option is enabled and there is a thirt-party
						provider for a specified section, ASProxy will initialize and use that third-party
						provider. The providers can change the whole pipeline.)</span>
				</td>
			</tr>
			<tr>
				<td>
					Plugins
				</td>
				<td>
					<asp:CheckBox ID="chkPluginsEnabled" Text="Plugins Enabled" runat="server" />
					<br />
					<span class="field_desc">(Enables plugin support. Plugins are third-party extensions
						which can extend or change the way ASProxy works. They can not change the whole
						pipeline, but they can affect it.)</span>
				</td>
			</tr>
		</table>
		<asp:Button ID="btnSaveProviders" CssClass="submit_button" runat="server" Text="Save"
			OnClick="btnSaveProviders_Click" />
	</fieldset>
	<fieldset>
		<legend><strong>Providers</strong></legend>
		<fieldset runat="server" id="pgUpdateProvider" visible="false">
			<legend>Updating a provider</legend>
			<asp:Label ID="lblUpProviderStatus" runat="server" Text="Update status"></asp:Label>
			<br />
			Provider name:
			<asp:Label ID="lblUpProviderName" runat="server" Text=""></asp:Label>
			<br />
			Update version:
			<asp:Label ID="lblUpProviderVersion" runat="server" Text=""></asp:Label>
			<br />
			<asp:Button ID="btnUpProviderUpdate" CssClass="submit_button" runat="server" 
				Text="Update" onclick="btnUpProviderUpdate_Click" />
			<asp:Button ID="btnUpProviderDismiss" CssClass="submit_button" runat="server" 
				Text="Dismiss" onclick="btnUpProviderDismiss_Click" />
		</fieldset>
		<span class="field_desc">(Here is installed providers. You can enable or disable them
			easily from here. Each change requires restart.)</span>
		<asp:Repeater ID="rptProviders" runat="server" OnItemDataBound="rptProviders_ItemDataBound">
			<HeaderTemplate>
				<table cellpadding="0" cellspacing="0" class="tblOptions">
					<tr class="option option_first_row">
						<td>
							Name<small> / version</small>
						</td>
						<td>
							Details
						</td>
						<td style="width: 90px;">
							State
						</td>
						<td style="width: 180px;">
							Options
						</td>
						<td style="width: 50px;">
							Update
						</td>
					</tr>
			</HeaderTemplate>
			<FooterTemplate>
				</table></FooterTemplate>
			<ItemTemplate>
				<tr class="option">
					<td class="name">
						<%#string.IsNullOrEmpty(((ProviderInfo)Container.DataItem).ConfigUiUrl)==false
							?HtmlTags.LinkTag(((ProviderInfo)Container.DataItem).Name+" "+((ProviderInfo)Container.DataItem).Version,
								this.ResolveUrl(((ProviderInfo)Container.DataItem).ConfigUiUrl),"Config this provider")
							: ((ProviderInfo)Container.DataItem).Name + " " + ((ProviderInfo)Container.DataItem).Version%>
					</td>
					<td class="desc">
						<%#((ProviderInfo)Container.DataItem).Description%>
						<br />
						<%#((ProviderInfo)Container.DataItem).Author%>
					</td>
					<td style="text-align: center" class="desc">
						<%#((ProviderInfo)Container.DataItem).Disabled ? "Disabled" : "Enabled"%>
						/
						<%#((ProviderInfo)Container.DataItem).Loaded ? "Loaded" : "Not-loaded"%>
					</td>
					<td style="text-align: center" class="desc">
						<asp:Button CssClass="button" ID="btnProviderEnable" runat="server" OnClick="btnProviderEnableClick"
							CommandName="Enable" CommandArgument="<%# ((ProviderInfo)Container.DataItem).Name + ((ProviderInfo)Container.DataItem).Author %>"
							Text="Enable" />
						<asp:Button CssClass="button" ID="btnProviderDisable" runat="server" OnClick="btnProviderDisableClick"
							CommandName="Disable" CommandArgument="<%# ((ProviderInfo)Container.DataItem).Name + ((ProviderInfo)Container.DataItem).Author %>"
							Text="Disable" />
					</td>
					<td class="desc">
						<asp:Button CssClass="button" ID="btnProviderUpdateCheck" runat="server" Text="Update" OnClick="btnProviderUpdateCheckClick"
							CommandName="Update" CommandArgument="<%# ((ProviderInfo)Container.DataItem).Name%>" Enabled="<%#((ProviderInfo)Container.DataItem).UpdateEnabled %>"/>
					</td>
				</tr>
			</ItemTemplate>
		</asp:Repeater>
		<span><span style="color: Red;">*Note:</span> The changes won't apply and display until
			you restart the app. Go to admin home page to restart the app.</span>
		<asp:Literal ID="ltNoProvider" EnableViewState="false" Text="&lt;br /&gt;There is no Provider installed."
			Visible="false" runat="server"></asp:Literal>
	</fieldset>
	<fieldset>
		<legend><strong>Plugins</strong></legend>
		<fieldset runat="server" id="pgUpdatePlugin" visible="false">
			<legend>Updating a plugin</legend>
			<asp:Label ID="lblUpPluginStatus" runat="server" Text="Update status"></asp:Label>
			<br />
			Plugin name:
			<asp:Label ID="lblUpPluginName" runat="server" Text=""></asp:Label>
			<br />
			Update version:
			<asp:Label ID="lblUpPluginVersion" runat="server" Text=""></asp:Label>
			<br />
			<asp:Button ID="btnUpPluginUpdate" CssClass="submit_button" runat="server" 
				Text="Update" onclick="btnUpPluginUpdate_Click" />
			<asp:Button ID="btnUpPluginDismiss" CssClass="submit_button" runat="server" 
				Text="Dismiss" onclick="btnUpPluginDismiss_Click" />
		</fieldset>
		
		<span class="field_desc">(Here is all installed
			plugins list. You can enable or disable them easily from here.)</span>
		<asp:Repeater ID="rptPlugins" runat="server" OnItemDataBound="rptPlugins_ItemDataBound">
			<HeaderTemplate>
				<table cellpadding="0" cellspacing="0" class="tblOptions">
					<tr class="option option_first_row">
						<td>
							Name<small> / version</small>
						</td>
						<td>
							Details
						</td>
						<td style="width: 90px;">
							State
						</td>
						<td style="width: 180px;">
							Options
						</td>
						<td style="width: 50px;">
							Update
						</td>
					</tr>
			</HeaderTemplate>
			<FooterTemplate>
				</table></FooterTemplate>
			<ItemTemplate>
				<tr class="option">
					<td class="name">
						<%#string.IsNullOrEmpty(((PluginInfo)Container.DataItem).ConfigUiUrl) == false
							? HtmlTags.LinkTag(((PluginInfo)Container.DataItem).Name + " " + ((PluginInfo)Container.DataItem).Version,
								this.ResolveUrl(((PluginInfo)Container.DataItem).ConfigUiUrl), "Config this plugin")
							: ((PluginInfo)Container.DataItem).Name + " " + ((PluginInfo)Container.DataItem).Version%>
					</td>
					<td class="desc">
						<%#((PluginInfo)Container.DataItem).Description%>
						<br />
						<%#((PluginInfo)Container.DataItem).Author%>
					</td>
					<td style="text-align: center" class="desc">
						<%#((PluginInfo)Container.DataItem).Disabled ?  "Disabled" : "Enabled"%>
					</td>
					<td style="text-align: center" class="desc">
						<asp:Button CssClass="button" ID="btnPluginEnable" runat="server" OnClick="btnPluginEnableClick"
							CommandName="Enable" CommandArgument="<%# ((PluginInfo)Container.DataItem).Name + ((PluginInfo)Container.DataItem).Author %>"
							Text="Enable" />
						<asp:Button CssClass="button" ID="btnPluginDisable" runat="server" OnClick="btnPluginDisableClick"
							CommandName="Disable" CommandArgument="<%# ((PluginInfo)Container.DataItem).Name + ((PluginInfo)Container.DataItem).Author %>"
							Text="Disable" />
					</td>
					<td class="desc">
						<asp:Button CssClass="button" ID="btnPluginUpdateCheck" runat="server" Text="Update" OnClick="btnPluginUpdateCheckClick"
						CommandName="Update" CommandArgument="<%# ((PluginInfo)Container.DataItem).Name%>"  Enabled="<%#((PluginInfo)Container.DataItem).UpdateEnabled %>"/>
				</tr>
			</ItemTemplate>
		</asp:Repeater>
		<span><span style="color: Red;">*Note:</span> The changes won't apply and display until
			you restart the app. Go to admin home page to restart the app.</span>
		<asp:Literal ID="ltNoPlugin" EnableViewState="False" Text="&lt;br /&gt;There is no plug-in installed."
			Visible="False" runat="server"></asp:Literal>
	</fieldset>
</asp:Content>

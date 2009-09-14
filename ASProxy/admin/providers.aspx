<%@ Page Title="Engine Extensions" Language="C#" MasterPageFile="~/admin/AdminUI.master"
    AutoEventWireup="true" CodeFile="providers.aspx.cs" Inherits="Admin_Providers" %>

<%@ Import Namespace="SalarSoft.ASProxy" %>

<script runat="server">
</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
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
        <asp:Button ID="btnSaveProviders" CssClass="submit_button" runat="server" 
            Text="Save" onclick="btnSaveProviders_Click" />
    </fieldset>
    <fieldset>
        <legend>Provider</legend>
        <span class="field_desc">(Here is installed providers. It is read-only list.)</span>
        <asp:Repeater ID="rptProvider" runat="server">
            <HeaderTemplate>
                <table cellpadding="0" cellspacing="0" class="tblOptions">
                    <tr class="option">
                        <td>
                            Details
                        </td>
                        <td>
                            State
                        </td>
                    </tr>
            </HeaderTemplate>
            <FooterTemplate>
                </table></FooterTemplate>
            <ItemTemplate>
                <tr class="option">
                    <td class="name">
                        State
                    </td>
                    <td class="desc">
                        
                    </td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
        <asp:Literal ID="ltNoProvider" EnableViewState="false" Text="&lt;br /&gt;There is no Provider installed." Visible="false" runat="server"></asp:Literal>
    </fieldset>
    <fieldset>
        <legend>Plugins</legend>
        <span class="field_desc">(Here is all installed plugins list. You can enable or disable them easily from here.)</span>
        <asp:Repeater ID="rptPlugins" runat="server">
            <HeaderTemplate>
                <table cellpadding="0" cellspacing="0" class="tblOptions">
                    <tr class="option option_first_row">
                        <td>
                            Name<small> / version</small>
                        </td>
                        <td>
                            Details
                        </td>
                        <td style="width: 100px;">
                            State
                        </td>
                        <td style="width: 200px;">
                            Options
                        </td>
                    </tr>
            </HeaderTemplate>
            <FooterTemplate>
                </table></FooterTemplate>
            <ItemTemplate>
                <tr class="option">
                    <td class="name">
                        <%#((PluginInfo)Container.DataItem).Name%>
                        <%#((PluginInfo)Container.DataItem).Version%>
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
                            CommandName="Enable" CommandArgument="<%# ((PluginInfo)Container.DataItem).Name + ((PluginInfo)Container.DataItem).Author %>" Text="Enable" />
                        <asp:Button CssClass="button" ID="btnPluginDisable" runat="server" OnClick="btnPluginDisableClick"
                            CommandName="Disable" CommandArgument="<%# ((PluginInfo)Container.DataItem).Name + ((PluginInfo)Container.DataItem).Author %>"
                            Text="Disable" />
                    </td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
        <span><span style="color:Red;">*Note:</span> The changes won't apply and display until you restart the app. Go to default page to restart the app.</span>
        <asp:Literal ID="ltNoPlugin" EnableViewState="False" 
            Text="&lt;br /&gt;There is no plug-in installed." Visible="False" 
            runat="server"></asp:Literal>
    </fieldset>
</asp:Content>

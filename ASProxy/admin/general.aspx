<%@ Page Title="General Configurations" Language="C#" MasterPageFile="~/admin/AdminUI.master" AutoEventWireup="true"
    CodeFile="general.aspx.cs" Inherits="Admin_General" %>

<script runat="server">

</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
<style type="text/css">
</style>
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	General Configurations 
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
    <fieldset>
        <legend>UI Language</legend>UI Language:
        <asp:DropDownList ID="cmbUiLanguage" runat="server">
        </asp:DropDownList>
        <br /><span class="field_desc">(Requires website restart.)</span>
    </fieldset>
    <fieldset>
        <legend>Image Compressor</legend>
        <asp:CheckBox ID="chkImageCompressor" runat="server" Text="Image compressor config is enabled" />
        <br /><span class="field_desc">(Enables image compression with specified quality. Be aware this feature may have some overhead to your server. To apply this config please refer to 'User Options' tab.)</span>
        
        <br />
        <br />
        Quality:<asp:TextBox ID="txtImgQuality" runat="server" Text="10"></asp:TextBox>
        
        <asp:RangeValidator ID="vldImgQuality" runat="server" CssClass="field_error" 
            ErrorMessage="Quality is invalid. Quality should be between 1 to 100." 
            ControlToValidate="txtImgQuality" Display="Dynamic" MaximumValue="100" 
            MinimumValue="1" Type="Integer">Invalid quality range. Quality should be between 1 to 100.</asp:RangeValidator>
                    <asp:RequiredFieldValidator ID="reqImgQuality"  
                        ControlToValidate="txtImgQuality" Display="Dynamic" 
                        runat="server" CssClass="field_error" 
                        ErrorMessage="Quality number is required.">Required</asp:RequiredFieldValidator>
        <br /><span class="field_desc">(Quality of compression.)</span>
    </fieldset>
    <fieldset >
    <legend>Auto Update</legend>
    <span class="field_desc">(ASProxy can update it self to the latest version. It is disabled by default to respect your choice and privacy.)</span>
        <table class="options_table" style="width:100%; ">
            <tr>
                <td style="width:200px;">
                    Update information Url:</td>
                <td>
                    <asp:TextBox ID="txtUpdateInfoUrl" runat="server" Columns="70"></asp:TextBox>
                    <br /><span class="field_desc">(ASProxy engine update information url. The format of these information should be recognizable by ASProxy, otherwise the update will fail.)</span>
                </td>
            </tr>
            <tr>
                <td>
                    Update ASProxy engine:</td>
                <td>
                    <asp:CheckBox ID="chkUpdateEngine" runat="server" Text="Enabled" />
                    <br /><span class="field_desc">(Specifies if updating ASProxy engine is enabled.)</span>
                </td>
            </tr>
            <tr>
                <td>
                    Update third-party providers:</td>
                <td>
                    <asp:CheckBox ID="chkUpdateProviders" runat="server" Text="Enabled" />
                    <br /><span class="field_desc">(The provider which gives update information will be updated if this option is enabled.)</span>
                </td>
            </tr>
            <tr>
                <td>
                    Update third-party plugins:</td>
                <td>
                    <asp:CheckBox ID="chkUpdatePlugins" runat="server" Text="Enabled" />
                    <br /><span class="field_desc">(The plug-ins which gives update information will be updated if this option is enabled.)</span>
                </td>
            </tr>
        </table>
    </fieldset>
    
    <fieldset>
    <legend>Log System</legend>
        <table class="options_table">
            <tr>
                <td>
                    Max file size:</td>
                <td>
                    <asp:TextBox ID="txtLogMaxFileSize" runat="server"></asp:TextBox>
                    <br /><span class="field_desc">(Maximum size of a log file. Files will split if the log file size is more than this.)</span>
                </td>
            </tr>
            <tr>
                <td>
                    File format:</td>
                <td>
                    <asp:TextBox ID="txtLogFileFormat" runat="server"></asp:TextBox>
                    <br /><span class="field_desc">(The format of log file. It uses dates in file format and can alos include minutes (mm) and seconds (ss).)</span>
                </td>
            </tr>
        </table>
    <fieldset><legend>Activity Log</legend>
    
    <span class="field_desc">(User activities can be logged. Here is logging settings.)</span>
        <table  class="options_table">
            <tr>
                <td>Activity log enabled</td>
                <td><asp:CheckBox ID="chkActivityLogEnabled" runat="server"  Text="Enabled"/>
                <br /><span class="field_desc">(Specifies if user activities logging is enabled ot 
                    not.)</span>
                </td>
            </tr>
            <tr>
                <td>Location</td>
                <td><asp:TextBox ID="chkActivityLogLocation" runat="server" Columns="50"></asp:TextBox>
                <br /><span class="field_desc">(The location to store log files. It must be 
                    relative.)</span></td>
            </tr>
            <tr>
                <td>Pages</td>
                <td><asp:CheckBox ID="chkActivityLogPages" runat="server" Text="Enabled" />
                <br /><span class="field_desc">(Determine if visiting pages should be logged.)</span></td>
            </tr>
            <tr>
                <td>Images</td>
                <td><asp:CheckBox ID="chkActivityLogImages" runat="server" Text="Enabled" />
                <br /><span class="field_desc">(Determine if displayed images should be logged.)</span></td>
            </tr>
            </table>
    </fieldset>
    <fieldset><legend>Error Log</legend>
    <span class="field_desc">(Developers who are debugging ASProxy should know what error details is. This feature is for them. Saves error details in specified location.)</span>
        <table  class="options_table">
            <tr>
                <td>Error log enabled</td>
                <td><asp:CheckBox ID="chkErrorLogEnabled" runat="server" Text="Enabled" />
                <br /><span class="field_desc">(Specifies errors should be logged or not.)</span></td>
            </tr>
            <tr>
                <td>Location</td>
                <td><asp:TextBox ID="chkErrorLogLocation" runat="server" Columns="50"></asp:TextBox>
                <br /><span class="field_desc">(The location to store log files. It must be 
                    relative.)</span></td>
            </tr>
            </table>
    </fieldset>
    </fieldset>
    <asp:Button CssClass="submit_button" ID="btnSave" runat="server" Text="Save" 
        onclick="btnSave_Click" />
    <input class="submit_button" id="btnCancel" type="button" value="Cancel"  onclick="document.location='default.aspx'" />
</asp:Content>

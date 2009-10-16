<%@ Page Language="C#" MasterPageFile="~/admin/AdminUI.master" Title="Welcome to ASProxy Administration UI" %>

<%@ Import Namespace="SalarSoft.ASProxy" %>

<script runat="server">

	string DisplayEnabledOption(bool enabled)
	{
		return enabled ? "Enabled" : "Disabled";
	}

	protected void btnRestartApp_Click(object sender, EventArgs e)
	{
		if (chkRestartApp.Checked)
		{
			HttpRuntime.UnloadAppDomain();
			Response.Redirect(Request.Url.ToString());
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (Common.IsRunningOnMono())
		{
			chkRestartApp.Enabled = false;
			btnRestartApp.Enabled = false;		
		}
	}
</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" runat="Server">
     <style type="text/css">
     <!--[if IE 6]>
     #IE6Support{ display:block !important;}
     <![endif]-->
     </style>
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" runat="Server">
	Welcome to ASProxy Administration UI
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" runat="Server">
	<div id="IE6Support" style="display: none; color: Red; font-weight: bold;">
		Warning, IE6 is not supported by ASProxy Administration UI. Please use <a href="http://www.getfirefox.com/"
			target="_blank">Firefox</a> instead.</div>
<fieldset>
<legend>Configurations Summary</legend>
<table class="options_table">
	<tr>
		<td>
			UI language:
		</td>
		<td>
			<%= Configurations.Pages.GetUiLanguage().DisplayName%>
		</td>
	</tr>
	<tr>
		<td>
			Image compressor:
		</td>
		<td>
			<%=DisplayEnabledOption( Configurations.ImageCompressor.Enabled)%>
		</td>
	</tr>
	<tr>
		<td>
			Engine auto-update
		</td>
		<td>
			<%=DisplayEnabledOption(Configurations.AutoUpdate.Engine)%>
		</td>
	</tr>
	<tr>
		<td>
			Activity log:
		</td>
		<td>
			<%=DisplayEnabledOption(Configurations.LogSystem.ActivityLog_Enabled)%>
		</td>
	</tr>
	<tr>
		<td>
			Resume-support download:
		</td>
		<td>
			<%=DisplayEnabledOption(Configurations.WebData.Downloader_ResumeSupport)%>
		</td>
	</tr>
	<tr>
		<td>
			Network proxy mode:
		</td>
		<td>
			<%=Configurations.NetProxy.Mode.ToString()%>
		</td>
	</tr>
	<tr>
		<td>
			Plugins:
		</td>
		<td>
			<%=DisplayEnabledOption(Configurations.Providers.PluginsEnabled)%>
		</td>
	</tr>
	<tr>
		<td>
			Login is required to ASProxy:
		</td>
		<td>
			<%=DisplayEnabledOption(Configurations.Authentication.Enabled)%>
		</td>
	</tr>
	<tr>
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
<legend>Application Operations</legend>
<table>
	<tr>
		<td>
			Restart Application:
		</td>
		<td>
			<asp:Button ID="btnRestartApp" runat="server" Text="Restart Application" 
				onclick="btnRestartApp_Click" /><asp:CheckBox
				ID="chkRestartApp" runat="server" Text="Are you sure?" />
			<br /><span class="field_desc">(Some ASProxy configurations applies after application restart. If you restart the application the configurations will load again. Also this may cause some current connections to fail.)
			<br /><span style="color:Red;">Warning, this option won't work with Mono, try to restart the app manually.</span>
			</span>
		</td>
	</tr>
	</table>
</fieldset>
</asp:Content>

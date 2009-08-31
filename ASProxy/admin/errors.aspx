<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

	string ErrosPath()
	{
		string _errorLog_FileFormat = Server.MapPath(SalarSoft.ASProxy.Configurations.LogSystem.ErrorLog_location);

		return System.IO.Path.GetDirectoryName(_errorLog_FileFormat);

	}

	protected void Page_Load(object sender, EventArgs e)
	{
		string path = ErrosPath();
		if (System.IO.Directory.Exists(path))
		{
			string[] files = System.IO.Directory.GetFiles(ErrosPath(), "*.log", System.IO.SearchOption.TopDirectoryOnly);
			rptErrors.DataSource = files;
			rptErrors.DataBind();

		}

		string str = Request.QueryString["log"];
		if (!string.IsNullOrEmpty(str) && System.IO.File.Exists(str))
		{
			txtContent.Text = System.IO.File.ReadAllText(str);
		}
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Errors Page</title>
</head>
<body>
	<form id="frmErrors" runat="server">
	<div>
		<asp:Repeater ID="rptErrors" runat="server">
			<ItemTemplate>
				<a href="?log=<%#Container.DataItem %>">ErrorFile
					<%#Container.ItemIndex+1 %></a><br />
			</ItemTemplate>
		</asp:Repeater>
	</div>
	<asp:TextBox ID="txtContent" EnableViewState="false" runat="server" Height="249px"
		TextMode="MultiLine" Width="552px"></asp:TextBox>
	</form>
</body>
</html>

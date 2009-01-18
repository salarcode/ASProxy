<%@ Page Language="C#" %>

<%@ Import Namespace="SalarSoft.ASProxy" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	protected void btnASProxyDisplayButton_Click(object sender, EventArgs e)
	{
		txtUrl.Text = UrlProvider.CorrectInputUrl(txtUrl.Text);
		string redir = UrlProvider.AddArgumantsToUrl(FilesConsts.DefaultPage, txtUrl.Text, true);
		Response.Redirect(redir);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		bool decode = false;
		string url = "";
		try
		{
			UrlProvider.GetUrlArguments(Page.Request.QueryString, out decode, out url);
			if (!string.IsNullOrEmpty(url))
			{
				if (decode)
					url = UrlProvider.DecodeUrl(url);
				txtUrl.Text = url;
				lblRequestedUrl.Text = url;

                if (LogSystem.Enabled)
                    LogSystem.Log(LogEntity.AuthorizationRequired, url);
			}
			else
			{
				Response.Redirect(FilesConsts.DefaultPage, false);
			}
		}
		catch (System.Threading.ThreadAbortException)
		{
		}
		catch (Exception err)
		{
            if (LogSystem.Enabled)
                LogSystem.Log(LogEntity.Error, url, err.Message);
            
            ASProxyExceptions.LogException(err, "Error while authorization: " + url);
			lblErrorMsg.Text = err.Message;
			lblErrorMsg.Visible = true;
			return;
		}

	}

	protected void wcLogin_Authenticate(object sender, AuthenticateEventArgs e)
	{
		bool decode = false;
		string url = "";
		try
		{
			UrlProvider.GetUrlArguments(Page.Request.QueryString, out decode, out url);
			if (!string.IsNullOrEmpty(url))
			{
				if (decode)
					url = UrlProvider.DecodeUrl(url);

				ASProxyCredentialCache.AddCertification(url, wcLogin.UserName, wcLogin.Password);

				string redir = UrlProvider.AddArgumantsToUrl(FilesConsts.DefaultPage, url, true);
				Response.Redirect(redir, true);

				e.Authenticated = true;
			}
			else
			{
				Response.Redirect(FilesConsts.DefaultPage, false);
			}
		}
		catch (System.Threading.ThreadAbortException)
		{
		}
		catch (Exception err)
		{
			if (LogSystem.Enabled)
				LogSystem.Log(LogEntity.Error, Request.Url.ToString(), err.Message);

			e.Authenticated = false;
			ASProxyExceptions.LogException(err, "Error while authorization: " + url);
			lblErrorMsg.Text = err.Message;
			lblErrorMsg.Visible = true;
			return;
		}
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>[PageTitle]</title>
</head>
<body>
<form id="frmAuthorization" runat="server" dir="[Direction]">
<table cellspacing="0" dir="[Direction]" style="color: black; border: solid 2px black; height: 0px;
width: 100%; text-align: center;">
<tr>
<td>
<table style="border: solid 1px silver; width: 100%; height: 0px; background-color: #f8f8f8;
	color: Black;">
	<tr>
		<td class="asproxy_td">
			<!--This is ASProxy that powered by SalarSoft.-->
			<span style="color: Navy; font-size:small;">[VersionFamily]</span>&nbsp;<asp:TextBox ID="txtUrl" runat="server"
				Columns="50" CssClass="asproxy_textbox" dir="ltr"></asp:TextBox><asp:Button ID="btnASProxyDisplayButton"
					runat="server" CssClass="asproxy_input" Style="height: 22px" Text="Display" OnClick="btnASProxyDisplayButton_Click" /></td>
	</tr>
</table>
<asp:Label ID="lblErrorMsg" runat="server" EnableTheming="False" EnableViewState="False"
	Font-Bold="True" Font-Names="Tahoma" Font-Size="10pt" ForeColor="Red" Text="Error message"
	ToolTip="Error message" Visible="False"></asp:Label><br />
<span style="font-size: 10pt; font-family: Verdana">[Description]<br />
	<asp:Label ID="lblRequestedUrl" runat="server"></asp:Label></span></td>
</tr>
</table>
<center>
<br />
<asp:Login ID="wcLogin" runat="server" BackColor="#F7F6F3" BorderColor="Gray" BorderPadding="4"
BorderStyle="Solid" BorderWidth="2px" Font-Names="Verdana" Font-Size="0.8em"
ForeColor="#333333" RememberMeSet="True" TitleText="[PageHeader]"
Width="300px" DisplayRememberMe="False" OnAuthenticate="wcLogin_Authenticate" FailureText="[LoginFailed]" LoginButtonText="[LogIn]" PasswordLabelText="[Password]" PasswordRequiredErrorMessage="[PasswordRequired]" UserNameLabelText="[UserName]" UserNameRequiredErrorMessage="[UserNameRequired]">
<TitleTextStyle BackColor="#5D7B9D" Font-Bold="True" Font-Size="0.9em" ForeColor="White"
Height="20px" />
<InstructionTextStyle Font-Italic="True" ForeColor="Black" />
<TextBoxStyle Font-Size="0.8em" />
<LoginButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px"
Font-Names="Verdana" Font-Size="Small" ForeColor="#284775" />
</asp:Login>
</center>
</form>
</body></html>
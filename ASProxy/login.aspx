<%@ Page Language="C#" meta:resourcekey="Page" Inherits="SalarSoft.ASProxy.PageInMasterLocale"%>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<script runat="server">
protected void aspLogin_Authenticate(object sender, AuthenticateEventArgs e)
{
	if (Configurations.Authentication.IsUserAuthenticated(aspLogin.UserName, aspLogin.Password))
	{
		e.Authenticated = true;
		if (Systems.LogSystem.ActivityLogEnabled)
			Systems.LogSystem.Log(LogEntity.ASProxyLoginPassed, "Username=" + aspLogin.UserName);
		FormsAuthentication.RedirectFromLoginPage(aspLogin.UserName, aspLogin.RememberMeSet);
	}
	else
		e.Authenticated = false;
}
</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Please login to get access</title>
	<style type="text/css">
		body
		{
			font-family: tahoma;
			font-size: 10pt;
		}
	</style>
</head>
<body>
<form id="frmLogin" runat="server" dir="<%=Resources.Languages.TextDirection%>">
<asp:Login style="margin:0 auto;" ID="aspLogin" Width="350px" runat="server" OnAuthenticate="aspLogin_Authenticate"
BackColor="#F7F6F3" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px"
TitleText="Please login to get access" BorderPadding="4" Font-Names="Verdana" Font-Size="0.8em"
ForeColor="#333333" meta:resourcekey="aspLogin">
<TitleTextStyle BackColor="#5D7B9D" BorderColor="LightGray" BorderWidth="1px" Font-Bold="True"
Font-Size="9pt" ForeColor="White" Height="20px" />
<InstructionTextStyle Font-Italic="True" ForeColor="Black" />
<TextBoxStyle Font-Size="0.8em" />
<LoginButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px"
Font-Names="Verdana" Font-Size="9pt" ForeColor="#284775" />
</asp:Login>
</form>
</body></html>
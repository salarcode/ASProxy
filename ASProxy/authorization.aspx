<%@ Page Language="C#" meta:resourcekey="Page"%>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
	protected void btnASProxyDisplayButton_Click(object sender, EventArgs e)
	{
		txtUrl.Text = UrlProvider.CorrectInputUrl(txtUrl.Text);
		string redir = UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageDefault_Dynamic, txtUrl.Text, true);
		Response.Redirect(redir);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		bool decode = false;
		string url = "";
		try
		{
            UrlProvider.ParseASProxyPageUrl(Page.Request.QueryString, out decode, out url);
			if (!string.IsNullOrEmpty(url))
			{
				if (decode)
					url = UrlProvider.DecodeUrl(url);
				txtUrl.Text = url;
				lblRequestedUrl.Text = url;

                if ( Systems.LogSystem.ActivityLogEnabled)
                    Systems.LogSystem.Log(LogEntity.AuthorizationRequired, url);
			}
			else
			{
				Response.Redirect(Consts.FilesConsts.PageDefault_Dynamic, false);
			}
		}
		catch (System.Threading.ThreadAbortException)
		{
		}
		catch (Exception ex)
		{
            if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, url);
            
			lblErrorMsg.Text = ex.Message;
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
            UrlProvider.ParseASProxyPageUrl(Page.Request.QueryString, out decode, out url);
			if (!string.IsNullOrEmpty(url))
			{
				if (decode)
					url = UrlProvider.DecodeUrl(url);

				Systems.CredentialCache.AddCertification(url, wcLogin.UserName, wcLogin.Password);

				string redir = UrlProvider.GetASProxyPageUrl(Consts.FilesConsts.PageDefault_Dynamic, url, true);
				Response.Redirect(redir, true);

				e.Authenticated = true;
			}
			else
			{
				Response.Redirect(Consts.FilesConsts.PageDefault_Dynamic, false);
			}
		}
		catch (System.Threading.ThreadAbortException)
		{
		}
		catch (Exception ex)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, url);

			e.Authenticated = false;

			lblErrorMsg.Text = ex.Message;
			lblErrorMsg.Visible = true;
			return;
		}
	}
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head><title>Authorization is required for this site</title>
</head>
<body dir="<%=Resources.Languages.TextDirection%>"><form id="frmAuthorization" runat="server">
<table cellspacing="0" dir="<%=Resources.Languages.TextDirection%>" style="color: black; border: solid 2px black; height: 0px;width: 100%; text-align: center;">
<tr><td>
<table style="border: solid 1px silver; width: 100%; height: 0px; background-color: #f8f8f8;color: Black;">
<tr><td class="asproxy_td"><!--This is ASProxy that powered by SalarSoft.--><span style="color: Navy; font-size:small;">ASProxy <%=Consts.General.ASProxyVersion %></span>&nbsp;<asp:TextBox 
ID="txtUrl" runat="server" Columns="50" CssClass="asproxy_textbox" dir="ltr" meta:resourcekey="txtUrl"></asp:TextBox><asp:Button ID="btnASProxyDisplayButton"
runat="server" CssClass="asproxy_input" Style="height: 22px" Text="Display" 
OnClick="btnASProxyDisplayButton_Click" meta:resourcekey="btnASProxyDisplayButton" /></td>
</tr></table>
<asp:Label ID="lblErrorMsg" runat="server" EnableTheming="False" EnableViewState="False" Font-Bold="True" Font-Names="Tahoma" Font-Size="10pt" ForeColor="Red" Text="Error message"
ToolTip="Error message" Visible="False" meta:resourcekey="lblErrorMsg"></asp:Label><br />
<span style="font-size: 10pt; font-family: Verdana">
<asp:Literal ID="lblDesc" runat="server" EnableViewState="False" meta:resourcekey="lblDesc" 
Text="The requested resource requires authentication. Please enter username and password.&lt;br /&gt;
The entered username and password will store in memory and wll not be available after this session."></asp:Literal>
<br />
<asp:Label ID="lblRequestedUrl" runat="server" meta:resourcekey="lblRequestedUrl"></asp:Label></span></td>
</tr></table><center><br /><asp:Login ID="wcLogin" runat="server" BackColor="#F7F6F3" BorderColor="Gray" BorderPadding="4"
BorderStyle="Solid" BorderWidth="2px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#333333" RememberMeSet="True" TitleText="Log In to authorization required site"
Width="300px" DisplayRememberMe="False" OnAuthenticate="wcLogin_Authenticate" meta:resourcekey="wcLogin">
<TitleTextStyle BackColor="#5D7B9D" Font-Bold="True" Font-Size="0.9em" ForeColor="White" Height="20px" />
<InstructionTextStyle Font-Italic="True" ForeColor="Black" />
<TextBoxStyle Font-Size="0.8em" />
<LoginButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="Small" ForeColor="#284775" />
</asp:Login>
</center></form>
</body></html>
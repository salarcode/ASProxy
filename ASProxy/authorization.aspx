<%@ Page Title="Authorization is required for this site" Inherits="SalarSoft.ASProxy.PageInMasterLocale" Language="C#" MasterPageFile="~/Theme.master" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<script runat="server">
	
	protected void btnDisplay_Click(object sender, EventArgs e)
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
				//lblRequestedUrl.Text = url;

                if ( Systems.LogSystem.ActivityLogEnabled)
                    Systems.LogSystem.Log(LogEntity.AuthorizationRequired, url);
			}
			else
			{
				//Response.Redirect(Consts.FilesConsts.PageDefault_Dynamic, false);
			}
		}
		catch (System.Threading.ThreadAbortException)
		{
		}
		catch (Exception ex)
		{
            if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, ex.Message, url);
            
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
				Systems.LogSystem.LogError(ex, ex.Message, url);

			e.Authenticated = false;

			lblErrorMsg.Text = ex.Message;
			lblErrorMsg.Visible = true;
			return;
		}
	}
</script>

<asp:Content ContentPlaceHolderID="plhHeadMeta" Runat="Server">
<title>Authorization is required for this site</title>
<style type="text/css">
.tblLogin
{
	margin-top:10px;
}
</style>
</asp:Content>


<asp:Content ContentPlaceHolderID="plhMainBar" Runat="Server">
<div class="urlBar">
<asp:Literal ID="lblUrl" EnableViewState="False" runat="server" meta:resourcekey="lblUrl" Text="Enter URL:"></asp:Literal>
<br /><asp:TextBox ID="txtUrl" CssClass="urlText" runat="server" Columns="70" dir="ltr" Width="550px" meta:resourcekey="txtUrl"></asp:TextBox>
<asp:Button CssClass="button" ID="btnDisplay" runat="server" OnClick="btnDisplay_Click" Text="Display" meta:resourcekey="btnASProxyDisplayButton" />
</div>
</asp:Content>


<asp:Content ContentPlaceHolderID="plhMainTitle" Runat="Server">
Authorization is required
</asp:Content>


<asp:Content ContentPlaceHolderID="plhContent" Runat="Server">
<asp:Label ID="lblErrorMsg" runat="server" EnableTheming="False" EnableViewState="False" Font-Bold="True" Font-Names="Tahoma" Font-Size="10pt" ForeColor="Red" Text="Error message"
ToolTip="Error message" Visible="False" meta:resourcekey="lblErrorMsg"></asp:Label>

<asp:Literal ID="lblDesc" runat="server" EnableViewState="False" meta:resourcekey="lblDesc" 
Text="The requested resource requires authentication. Please enter username and password.&lt;br /&gt;
The entered username and password will store in memory and will not be available after this session."></asp:Literal>

<center>
<asp:Login ID="wcLogin" CssClass="tblLogin" runat="server" BackColor="#F7F7DE" BorderColor="#CCCC99"
BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="10pt" 
		RememberMeSet="True" TitleText="Log In to authorization required site"
Width="300px" DisplayRememberMe="False" OnAuthenticate="wcLogin_Authenticate" 
		meta:resourcekey="wcLogin">
<TitleTextStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" 
		Height="25px" />
</asp:Login>
</center>
</asp:Content>


<asp:Content ContentPlaceHolderID="plhOptionsTitle" Runat="Server">
</asp:Content>


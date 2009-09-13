using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;

public partial class admin_adminlogin : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		btnAdminRegister.Visible = false;
		btnAdminLogin.Visible = false;
		if(Configurations.AdminUI.IsAdminStarted==false)
			btnAdminRegister.Visible = true;
		else
			btnAdminLogin.Visible = true;
	}


	void ReturnToRedirectedPage()
	{
		string returnUrl = Request.QueryString["ReturnUrl"];
		if (string.IsNullOrEmpty(returnUrl))
			returnUrl = "default.aspx";
		Response.Redirect(returnUrl, true);
	}


	protected void btnAdminLogin_Click(object sender, EventArgs e)
	{
		txtPassword.Text = txtPassword.Text.Trim().ToLower();
		txtUsername.Text = txtUsername.Text.Trim().ToLower();
		if (Configurations.AdminUI.IsAdminStarted)
		{
			if (txtUsername.Text == Configurations.AdminUI.UserName.ToLower()
				&& txtPassword.Text == Configurations.AdminUI.Password.ToLower())
			{
				Admin_AdminUI.LoginTheUser();
				ReturnToRedirectedPage();
			}
		}
	}
	protected void btnAdminRegister_Click(object sender, EventArgs e)
	{
		txtPassword.Text = txtPassword.Text.Trim().ToLower();
		txtUsername.Text = txtUsername.Text.Trim().ToLower();
		if (Configurations.AdminUI.IsAdminStarted == false)
		{
			Configurations.AdminUI.Password = txtPassword.Text;
			Configurations.AdminUI.UserName = txtUsername.Text;
			Configurations.AdminUI.IsAdminStarted = true;
			Configurations.SaveSettings();

			ReturnToRedirectedPage();
		}
	}
}

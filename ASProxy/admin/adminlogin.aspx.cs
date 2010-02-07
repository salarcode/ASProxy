using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;

public partial class Admin_AdminLogin : System.Web.UI.Page
{
	List<string> _errorsList = new List<string>();
	protected void Page_Load(object sender, EventArgs e)
	{
		btnAdminRegister.Visible = false;
		btnAdminLogin.Visible = false;
		lblRegisterMessage.Visible = false;
		lblLoginMessage.Visible = false;
		lblTitleRegister.Visible = false;
		lblTitleLogin.Visible = false;

		if (Configurations.AdminUI.IsAdminStarted == false)
		{
			Page.Title = "Register Your Administration UI User";
			lblTitleRegister.Visible = true;
			btnAdminRegister.Visible = true;
			lblRegisterMessage.Visible = true;
		}
		else
		{
			Page.Title = "Login to Administration UI";
			lblTitleLogin.Visible = true;
			btnAdminLogin.Visible = true;
			lblLoginMessage.Visible = true;
		}
	}
	void DisplayErrors(List<string> errorsList)
	{
		if (errorsList.Count == 0)
			return;
		ltErrorsList.Visible = true;
		string display = "<ul style='color:Red;'>";
		foreach (string item in errorsList)
		{
			display += "<li>" + item + "</li>";
		}
		display += "</ul>";
		ltErrorsList.Text = display;
	}


	void AddError(string errorMessage)
	{
		_errorsList.Add(errorMessage);
	}

	void ReturnToRedirectedPage()
	{
		string returnUrl = Request.QueryString["ReturnUrl"];
		if (string.IsNullOrEmpty(returnUrl))
			returnUrl = "default.aspx";
		Response.Redirect(returnUrl, true);
	}

	protected override void OnPreRender(EventArgs e)
	{
		DisplayErrors(_errorsList);

		base.OnPreRender(e);
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
			else
				AddError("Invalid username or password");
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

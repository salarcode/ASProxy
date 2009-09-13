using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;

public partial class Admin_AdminUI : System.Web.UI.MasterPage
{
	const string _AdminLogin = "AdminUISession";

	public static void LoginTheUser()
	{
		HttpContext.Current.Session[_AdminLogin] = true;
	}
	public static void LogoutTheUser()
	{
		HttpContext.Current.Session[_AdminLogin] = false;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (Configurations.AdminUI.IsAdminStarted)
		{
			if (!IsAdminLoggedIn())
				RedirectToLoginPage();
		}
		else
			RedirectToRegisterPage();
	}


	void RedirectToLoginPage()
	{
		string url = Request.Url.AbsolutePath.ToLower();
		if (url.EndsWith("adminlogin.aspx") == false)
		{
			string redirect = "admin/adminlogin.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery);
			redirect = CurrentContext.MapAppVPath(redirect);
			Response.Redirect(redirect, true);
		}
	}

	void RedirectToRegisterPage()
	{
		string url = Request.Url.AbsolutePath.ToLower();
		if (url.EndsWith("adminlogin.aspx") == false)
		{
			string redirect = "admin/adminlogin.aspx?usr=init&ReturnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery);
			redirect = CurrentContext.MapAppVPath(redirect);
			Response.Redirect(redirect, true);
		}
	}

	bool IsAdminLoggedIn()
	{
		// if sessions is enabled
		if (Session.Mode != System.Web.SessionState.SessionStateMode.Off)
		{
			object adminObj = Session[_AdminLogin];
			if (adminObj != null)
				return (bool)adminObj;
			else
				return false;
		}
		else
			return true;
	}
}

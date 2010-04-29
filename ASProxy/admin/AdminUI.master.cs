//**************************************************************************
// Product Name: ASProxy
// Description:  SalarSoft ASProxy is a free and open-source web proxy
// which allows the user to surf the net anonymously.
//
// MPL 1.1 License agreement.
// The contents of this file are subject to the Mozilla Public License
// Version 1.1 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS"
// basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
// License for the specific language governing rights and limitations
// under the License.
// 
// The Original Code is ASProxy (an ASP.NET Proxy).
// 
// The Initial Developer of the Original Code is Salar.Kh (SalarSoft).
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.Kh < salarsoftwares [@] gmail[.]com > (original author)
//
//**************************************************************************

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

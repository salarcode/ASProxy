<%@ Application Language="C#" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>

<script RunAt="server">
	
	void Application_Start(object sender, EventArgs e)
	{
		// Since V5.1, Some site didn't implement Expect 100-Continue behavior .
		// The Expect 100-Continue behavior is fully described in IETF RFC 2616 Section 10.1.1. 
		System.Net.ServicePointManager.Expect100Continue = false;

		// auto update reminder
		if (Configurations.AutoUpdate.Engine)
		{
		    SalarSoft.ASProxy.Updater.AddUpdateReminder();
		}
	}

	protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
	{
		// Page ui
		System.Threading.Thread.CurrentThread.CurrentUICulture = Configurations.Pages.GetUiLanguage();
			
		
		if (Configurations.Authentication.Enabled)
		{
			if (Request.IsAuthenticated == false)
			{
				string url = Request.Url.AbsolutePath.ToLower();
				if (url.EndsWith("login.aspx") == false)
				{
					string redirect = "login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery);
					redirect = CurrentContext.MapAppVPath(redirect);
					Response.Redirect(redirect, true);
				}
			}
		}
	}

	protected void Application_Error(object sender, EventArgs e)
	{
		Exception ex = Server.GetLastError();
		if (Systems.LogSystem.ErrorLogEnabled)
		{
			Systems.LogSystem.LogError(ex, Request, "Application_Error", "");
		}

		// shows a custom error message instead of ASP.NET general error
		CustomErrors.HandleCustomErrors(ex);
	}
</script>


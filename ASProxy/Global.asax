<%@ Application Language="C#" %>

<script RunAt="server">
	void Application_Start(object sender, EventArgs e)
	{
		//SalarSoft.ASProxy.ASProxyExceptions.LogException(new Exception(), "Application_Start");
		if (SalarSoft.ASProxy.ASProxyConfig.ASProxyAutoUpdateEnabled)
		{
			SalarSoft.ASProxy.Update.Updater.AddUpdateReminder();
		}
	}

	protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
	{
		if (SalarSoft.ASProxy.ASProxyConfig.ASProxyLoginNeeded)
		{
			if (Request.IsAuthenticated == false)
			{
				string url = Request.Url.AbsolutePath.ToLower();
				if (url.EndsWith(".aspx"))
				{
					if (url.EndsWith("login.aspx") == false)
					{
						string redirect = "login.aspx?ReturnUrl=" + Request.Url.PathAndQuery;
						Response.Redirect(redirect, true);
					}
				}
			}
		}
	}

	protected void Application_Error(object sender, EventArgs e)
	{
		Exception ex = Server.GetLastError();
		SalarSoft.ASProxy.ASProxyExceptions.LogException(ex, "Application_Error");
		
		if (SalarSoft.ASProxy.LogSystem.Enabled)
			SalarSoft.ASProxy.LogSystem.Log(SalarSoft.ASProxy.LogEntity.Error, ex.Message);

		SalarSoft.ASProxy.ASProxyExceptions.HandleCustomErrors(ex);
	}
</script>


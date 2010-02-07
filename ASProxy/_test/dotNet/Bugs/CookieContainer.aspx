<%@ Page Language="C#" %>

<%@ Import Namespace="System.Net" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	CookieContainer getContainer()
	{
		CookieContainer result = new CookieContainer();

		Uri uri = new Uri("http://extra.sub.site.com");
		string cookieH;
		//cookieH = @"Test0=val; domain=test.site.com; path=/";
		//result.SetCookies(uri, cookieH);

		cookieH = @"Test1=val; domain=extra.sub.site.com; path=/;";
		result.SetCookies(uri, cookieH);

		// Cookies2
		//cookieH = @"Test2=val; $domain=.site.com; $path=/";
		//result.SetCookies(uri, cookieH);

		cookieH = @"Test2=val; domain=.sub.site.com; path=/";
		result.SetCookies(uri, cookieH);

		cookieH = @"Test3=val; domain=sub.site.com; path=/;";
		result.SetCookies(uri, cookieH);

		return result;
	}

	void Test()
	{
		CookieContainer cookie = getContainer();

		bool hackCookieContainer = true;

		if (hackCookieContainer)
		{
			//#warning Dirty cookie hack
			Hashtable table = (Hashtable)cookie.GetType().InvokeMember("m_domainTable",
									   System.Reflection.BindingFlags.NonPublic |
									   System.Reflection.BindingFlags.GetField |
									   System.Reflection.BindingFlags.Instance,
									   null,
									   cookie,
									   new object[] { });
			ArrayList keys = new ArrayList(table.Keys);
			foreach (object keyObj in keys)
			{
				string key = (keyObj as string);
				if (key[0] == '.')
				{
					string newKey = key.Remove(0, 1);
					table[newKey] = table[keyObj];
				}
			}
		}

		lblResult.Text += "<br>Total cookies count: " + cookie.Count + " &nbsp;&nbsp; expected: 3";

		Uri uri = new Uri("http://extra.sub.site.com");
		CookieCollection coll ;//= cookie.GetCookies(uri);
		coll = GetCookies(cookie,uri);
		lblResult.Text += "<br>For " + uri + " Cookie count: " + coll.Count + " &nbsp;&nbsp; expected: 2";
		lblResult.Text += "<br>";
		foreach (Cookie c in coll)
		{
			lblResult.Text += " Name:" + c.Name + " ";
		}
		lblResult.Text += "<br>";

		uri = new Uri("http://xyz.sub.site.com");
		//coll = cookie.GetCookies(uri);
		coll = GetCookies(cookie, uri);
		lblResult.Text += "<br>For " + uri + " Cookie count: " + coll.Count + " &nbsp;&nbsp; expected: 2";
		lblResult.Text += "<br>";
		foreach (Cookie c in coll)
		{
			lblResult.Text += " Name:" + c.Name + " ";
		}
		lblResult.Text += "<br>";

		uri = new Uri("http://sub.site.com");
		//coll = cookie.GetCookies(uri);
		coll = GetCookies(cookie, uri);
		lblResult.Text += "<br>For " + uri + " Cookie count: " + coll.Count + " &nbsp;&nbsp; expected: 2";
		lblResult.Text += "<br>";
		foreach (Cookie c in coll)
		{
			lblResult.Text += " Name:" + c.Name + " ";
		}
		lblResult.Text += "<br>";

	}

	private CookieCollection GetCookies(CookieContainer container, Uri reqUri)
	{
		return container.GetCookies(reqUri);

		string url = reqUri.Scheme + "://" + DateTime.Now.Ticks.ToString()+"." + reqUri.Authority + reqUri.PathAndQuery;
		Uri cookieUri = new Uri(url);
		CookieCollection reqUriColl = container.GetCookies(reqUri);
		CookieCollection correctedUriColl = container.GetCookies(cookieUri);
		correctedUriColl.Add(reqUriColl);
		return correctedUriColl;
	}


	protected void Page_Load(object sender, EventArgs e)
	{
		Test();
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>CookieContainer Test Page</title>
</head>
<body>
	<form id="frmTest" runat="server">
	<asp:Label ID="lblResult" EnableViewState="false" runat="server"></asp:Label>
	</form>
</body>
</html>

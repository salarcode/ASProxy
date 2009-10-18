<%@ Page Language="C#" %>

<%@ Import Namespace="System.Net" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	
	bool _applyBugFix = true;
	
    CookieContainer getContainer()
    {
        CookieContainer result = new CookieContainer();

        Uri uri = new Uri("http://sub.site.com");
        string cookieH ;//= @"Test1=val; domain=sub.site.com; path=/";
		//result.SetCookies(uri, cookieH);
		
		Cookie c = new Cookie("Test1", "val1", "/", "sub.site.com");
		result.Add(c);

		cookieH = @"Test4=val4; domain=sub.site.com; path=/";
		result.SetCookies(uri, cookieH);

		//cookieH = @"Test2=val; domain=.site.com; path=/";
		//result.SetCookies(uri, cookieH);
		c = new Cookie("Test2", "val", "/", ".site.com");
		result.Add(c);

		//cookieH = @"Test3=val; domain=site.com; path=/";
		//result.SetCookies(uri, cookieH);
		c = new Cookie("Test2", "val", "/", "site.com");
		result.Add(c);

        return result;
    }

    void Test()
    {
        CookieContainer cookie = getContainer();

		if (_applyBugFix)
		{
			BugFix_AddDotCookieDomain(cookie);
		}
		
        lblResult.Text += "<br>Total cookies count: " + cookie.Count + " &nbsp;&nbsp; expected: 3";

        Uri uri = new Uri("http://sub.site.com");
        CookieCollection coll = cookie.GetCookies(uri);
        lblResult.Text += "<br>For " + uri + " Cookie count: " + coll.Count + " &nbsp;&nbsp; expected: 2";
		lblResult.Text += "<br>";
		foreach (Cookie c in coll)
		{
			lblResult.Text += " Name:" + c.Name + " ";
		}
		lblResult.Text += "<br>";

		
        uri = new Uri("http://other.site.com");
        coll = cookie.GetCookies(uri);
        lblResult.Text += "<br>For " + uri + " Cookie count: " + coll.Count + " &nbsp;&nbsp; expected: 2";
		lblResult.Text += "<br>";
		foreach (Cookie c in coll)
		{
			lblResult.Text += " Name:" + c.Name + " ";
		}
		lblResult.Text += "<br>";

		
        uri = new Uri("http://site.com");
        coll = cookie.GetCookies(uri);
        lblResult.Text += "<br>For " + uri + " Cookie count: " + coll.Count + " &nbsp;&nbsp; expected: 2";
		lblResult.Text += "<br>";
		foreach (Cookie c in coll)
		{
			lblResult.Text += " Name:" + c.Name + " ";
		}
		lblResult.Text += "<br>";

    }

	private static Type _cookieContainerType = Type.GetType("System.Net.CookieContainer, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
	private static Type _pathListType = Type.GetType("System.Net.PathList, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
	/// <summary>
	/// This method is aimed to fix a goddamn CookieContainer issue,
	/// In CookieContainer adds missed path for cookies that is not started with dot.
	/// This is a dirty hack
	/// </summary>
	/// <remarks>
	/// This method is only for .NET 2.0 which is used by .NET 3.0 and 3.5 too.
	/// The issue will be fixed in .NET 4, I hope!
	/// </remarks>
	/// <autor>Many thanks to CallMeLaNN "dot-net-expertise.blogspot.com" to complete this method</autor>
	private void BugFix_AddDotCookieDomain(CookieContainer cookieContainer)
	{
		Hashtable table = (Hashtable)_cookieContainerType.InvokeMember("m_domainTable",
										 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance,
										 null,
										 cookieContainer,
										 new object[] { });

		ArrayList keys = new ArrayList(table.Keys);

		object pathList1;
		object pathList2;

		SortedList sortedList1;
		SortedList sortedList2;
		ArrayList pathKeys;

		CookieCollection cookieColl1;
		CookieCollection cookieColl2;

		foreach (string key in keys)
		{
			if (key[0] == '.')
			{
				string nonDotKey = key.Remove(0, 1);
				// Dont simply code like this:
				// table[nonDotKey] = table[key];
				// instead code like below:

				// This codes will copy all cookies in dot domain key into nondot domain key.

				pathList1 = table[key];
				pathList2 = table[nonDotKey];
				if (pathList2 == null)
				{
					pathList2 = Activator.CreateInstance(_pathListType); // Same as PathList pathList = new PathList();
					lock (cookieContainer)
					{
						table[nonDotKey] = pathList2;
					}
				}

				// merge the PathList, take cookies from table[keyObj] copy into table[nonDotKey]
				sortedList1 = (SortedList)_pathListType.InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList1, new object[] { });
				sortedList2 = (SortedList)_pathListType.InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList2, new object[] { });

				pathKeys = new ArrayList(sortedList1.Keys);

				foreach (string pathKey in pathKeys)
				{

					cookieColl1 = (CookieCollection)sortedList1[pathKey];
					cookieColl2 = (CookieCollection)sortedList2[pathKey];
					if (cookieColl2 == null)
					{
						cookieColl2 = new CookieCollection();
						sortedList2[pathKey] = cookieColl2;
					}

					foreach (Cookie c in cookieColl1)
					{
						lock (cookieColl2)
						{
							cookieColl2.Add(c);
						}
					}
				}
			}
		}
	}

    protected void Page_Load(object sender, EventArgs e)
    {
        Test();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>CookieContainer Test Page</title>
</head>
<body>
    <form id="frmTest" runat="server">
    <asp:Label ID="lblResult" EnableViewState="false" runat="server"></asp:Label>
    </form>
</body>
</html>

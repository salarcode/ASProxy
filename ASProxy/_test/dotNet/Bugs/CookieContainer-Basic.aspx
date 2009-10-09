<%@ Page Language="C#" %>

<%@ Import Namespace="System.Net" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	
	bool _applyBugFix = true;
	
    CookieContainer getContainer()
    {
        CookieContainer result = new CookieContainer();

        Uri uri = new Uri("http://sub.site.com");
        string cookieH = @"Test1=val; domain=sub.site.com; path=/";
        result.SetCookies(uri, cookieH);

        cookieH = @"Test2=val; domain=.site.com; path=/";
        result.SetCookies(uri, cookieH);

        cookieH = @"Test3=val; domain=site.com; path=/";
        result.SetCookies(uri, cookieH);

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

        uri = new Uri("http://other.site.com");
        coll = cookie.GetCookies(uri);
        lblResult.Text += "<br>For " + uri + " Cookie count: " + coll.Count + " &nbsp;&nbsp; expected: 2";

        uri = new Uri("http://site.com");
        coll = cookie.GetCookies(uri);
        lblResult.Text += "<br>For " + uri + " Cookie count: " + coll.Count + " &nbsp;&nbsp; expected: 2";

    }

	Type _ContainerType = typeof(CookieContainer);
	/// <summary>
	/// CookieContainer "Domain" BugFix
	/// </summary>
	/// <param name="responseCookies"></param>
	private void BugFix_AddDotCookieDomain(CookieContainer cookieContainer)
	{
		// here is the hack: http://social.microsoft.com/Forums/en-US/netfxnetcom/thread/1297afc1-12d4-4d75-8d3f-7563222d234c
		// and: http://channel9.msdn.com/forums/TechOff/260235-Bug-in-CookieContainer-where-do-I-report/
		Hashtable table = (Hashtable)_ContainerType.InvokeMember("m_domainTable",
								   System.Reflection.BindingFlags.NonPublic |
								   System.Reflection.BindingFlags.GetField |
								   System.Reflection.BindingFlags.Instance,
								   null,
								   cookieContainer,
								   new object[] { });
		ArrayList keys = new ArrayList(table.Keys);
		foreach (string keyObj in keys)
		{
			string key = (keyObj as string);
			if (key[0] == '.')
			{
				string newKey = key.Remove(0, 1);
				table[newKey] = table[keyObj];
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

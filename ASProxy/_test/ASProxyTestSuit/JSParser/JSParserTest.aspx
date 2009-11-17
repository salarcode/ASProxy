<%@ Page Language="C#" %>

<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="SalarSoft.ASProxy.BuiltIn" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	
	string GetRangeString(string source, TextRange resultRange)
	{
		try
		{
			return source.Substring(resultRange.Start, resultRange.End - resultRange.Start);

		}
		catch (Exception ex)
		{
			return "Exception : " + ex.Message + " TextRange.Start=" + resultRange.Start + " TextRange.End=" + resultRange.End;
		}
	}

	string AssertResult(string source, TextRange result, TextRange expected)
	{
		if (result.IsEqual(expected))
			return Succeed("Succeed with: " + GetRangeString(source, result));
		else
			return Failed("Failed with: " + GetRangeString(source, result), " Expected: " + GetRangeString(source, expected));
	}

	string AssertResult(string result, string expected)
	{
		if (result == expected)
			return Succeed("Succeed with: " + result);
		else
			return Failed("Failed with: " + result, " Expected: " + expected);
	}

	string AssertResult(string result, string expected, string acceptable)
	{
		if (result == expected)
			return Succeed("Succeed with: " + result);
		else if (result == acceptable)
			return Acceptable("Failed with: " + result, " Expected: " + expected);
		else
			return Failed("Failed with: " + result, " Expected: " + expected);
	}

	string Succeed()
	{
		return "<span style='color:green'>Succeed</span>";
	}

	string Succeed(string message)
	{
		return "<span style='color:green'>" + message + "</span>";
	}

	string Failed(string failMessage)
	{
		return "<span style='color:red'>" + failMessage + "</span>";
	}

	string Failed(string failMessage, string expected)
	{
		return "<span style='color:red'>" + failMessage + "</span> <span style='color:maroon'>" + expected + "</span> ";
	}

	string Acceptable(string failMessage, string expected)
	{
		return "<span style='color:red'>" + failMessage + "</span> <span style='color:maroon'>" + expected + "</span> <span style='color:green'>Acceptable anyway!</span>";
	}

	#region Tests

	#region FindMethodParametersRange
	string FindMethodParametersRange_Test1()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window.open('Hello() ','parameter2',function3('dude'+foo()),End parameter)";
			string expected = "'Hello() ','parameter2',function3('dude'+foo()),End parameter";


			return AssertResult(GetRangeString(test, JSParser.FindMethodParametersRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindMethodParametersRange_Test2()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window.open('Hello() ','parameter2'); method2();";
			string expected = "'Hello() ','parameter2'";


			return AssertResult(GetRangeString(test, JSParser.FindMethodParametersRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindMethodParametersRange_Test3()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window.open('Hello() '+call())";
			string expected = "'Hello() '+call()";


			return AssertResult(GetRangeString(test, JSParser.FindMethodParametersRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindMethodParametersRange_Test4()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window.open('Hello() '+function(){var a=1+2; return 'test1'+inside();})";
			string expected = "'Hello() '+function(){var a=1+2; return 'test1'+inside();}";


			return AssertResult(GetRangeString(test, JSParser.FindMethodParametersRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindMethodParametersRange_Test5()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window.open({return '';}())";
			string expected = "{return '';}()";


			return AssertResult(GetRangeString(test, JSParser.FindMethodParametersRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	#endregion

	#region FindMethodFirstParameterRange
	string FindMethodFirstParameterRange_Test1()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window  \t  .open('Hello, '+test(\" hel,lo \"),param2,param3))";
			string expected = "'Hello, '+test(\" hel,lo \")";


			return AssertResult(GetRangeString(test, JSParser.FindMethodFirstParameterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindMethodFirstParameterRange_Test2()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window     .open('Hello() '+function(){var a=goin(11,22); return 'test1'+inside(33,55);},param2)";
			string expected = "'Hello() '+function(){var a=goin(11,22); return 'test1'+inside(33,55);}";


			return AssertResult(GetRangeString(test, JSParser.FindMethodFirstParameterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindMethodFirstParameterRange_Test3()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window.   open(,,,,,)";
			string expected = "";


			return AssertResult(GetRangeString(test, JSParser.FindMethodFirstParameterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindMethodFirstParameterRange_Test4()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window\n.\topen('Hi, are you ok?',second+1)";
			string expected = "'Hi, are you ok?'";


			return AssertResult(GetRangeString(test, JSParser.FindMethodFirstParameterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindMethodFirstParameterRange_Test5()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window    .     open('Hi, are \"y,o,u\" ok?',second+2)";
			string expected = "'Hi, are \"y,o,u\" ok?'";


			return AssertResult(GetRangeString(test, JSParser.FindMethodFirstParameterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindMethodFirstParameterRange_Test6()
	{
		try
		{
			string searchFor = "window.open";
			string test = "window  \r .  \t \n open(\"Hi, are 'YOU' ok?\",second+2)";
			string expected = "\"Hi, are 'YOU' ok?\"";


			return AssertResult(GetRangeString(test, JSParser.FindMethodFirstParameterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	#endregion

	#region FindPropertyRange
	string FindPropertyRange_Test1()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window.location= hello()+what+'ok();'+'continue?' ;";
			string expected = " hello()+what+'ok();'+'continue?' ";
			string acceptable = "hello()+what+'ok();'+'continue?'";

			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected, acceptable);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test2()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window.location=(hello()+what+'ok();');";
			string expected = "(hello()+what+'ok();')";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test3()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window.location= function(){hello()+what+'ok();'+'continue?'};";
			string expected = " function(){hello()+what+'ok();'+'continue?'}";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test4()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window.location= function(){hello()+what+'ok();'+'continue?';};";
			string expected = " function(){hello()+what+'ok();'+'continue?';}";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test5()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window.location= hello(param1,p2,p4);";
			string expected = " hello(param1,p2,p4)";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test6()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window \t.\r\n\n location= window.location+q1;";
			string expected = " window.location+q1";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test7()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window.location= \"hello();\"+'ok();'+'continue?' ;";
			string expected = " \"hello();\"+'ok();'+'continue?' ";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test8()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window.location= \"hello(;);\"+'ok();'+'continue?' ;";
			string expected = " \"hello(;);\"+'ok();'+'continue?' ";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test9()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window.location= function(){var goF=function(){return 'ab';} var go=1;return goF()+go+'2';};";
			string expected = " function(){var goF=function(){return 'ab';} var go=1;return goF()+go+'2';}";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test10()
	{
		try
		{
			string searchFor = "window.location";
			string test = "var location='test.htm'; if(location=='test.htm'){ window.location=location+'?done=1';}";
			string expected = "location+'?done=1'";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test11()
	{
		try
		{
			string searchFor = "window.location";
			string test = "if(window.location=='http://test.htm'){ window.location=window.location+'?done=1';}";
			string expected = "window.location+'?done=1'";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test12()
	{
		try
		{
			string searchFor = "location.href";
			string test = "if(window.location.href==uri){window.location.reload();} else window.location.href='test.htm';";
			string expected = "'test.htm'";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyRange_Test13()
	{
		try
		{
			string searchFor = "location.href";
			string test = @"var str=' location.href=\""hel'lo.htm\""; ';";
			string expected = @"\""hel'lo.htm\""";


			return AssertResult(GetRangeString(test, JSParser.FindPropertySetterRange(ref test, searchFor, 0)), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	#endregion

	#region FindPropertyGetterRange
	string FindPropertyGetterRange_Test1()
	{
		try
		{
			string searchFor = "window.location";
			string test = "if(window.location=='http://test.htm'){ window.location=window.location+'?done=1';}";

			TextRange expected = new TextRange(
				test.IndexOf("window.location+'?done=1"),
				test.IndexOf("window.location+'?done=1") + searchFor.Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchFor, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyGetterRange_Test2()
	{
		try
		{
			string searchFor = "window.location";
			string test = "var ok=window.location+'ok=1';";

			TextRange expected = new TextRange(
				test.IndexOf("window.location+'ok=1'"),
				test.IndexOf("window.location+'ok=1'") + searchFor.Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchFor, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyGetterRange_Test3()
	{
		try
		{
			string searchFor = "window.location";
			string test = "var ok=window.location.indexOf('ok',1);";

			TextRange expected = new TextRange(
				test.IndexOf("window.location.indexOf"),
				test.IndexOf("window.location.indexOf") + searchFor.Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchFor, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyGetterRange_Test4()
	{
		try
		{
			string searchFor = "window.location";
			string test = "window.location \t = \r \n   window.location;";

			TextRange expected = new TextRange(
				test.IndexOf("window.location;"),
				test.IndexOf("window.location;") + searchFor.Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchFor, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyGetterRange_Test5()
	{
		try
		{
			string searchFor = "window.location";
			string test = "alert(window.location+'?gohere=1');";

			TextRange expected = new TextRange(
				test.IndexOf("window.location+"),
				test.IndexOf("window.location+") + searchFor.Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchFor, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	#endregion

	#region FindPropertyGetterRange2
	string FindPropertyGetterRange2_Test1()
	{
		try
		{
			string[] searchForPart1 = new string[] { "window", "document" };
			string[] searchForPart2 = new string[] { "location" };
			string test = "alert(window.location+'?gohere=1');";

			TextRange expected = new TextRange(
				test.IndexOf("window.location+"),
				test.IndexOf("window.location+") + "window.location".Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchForPart1, searchForPart2, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyGetterRange2_Test2()
	{
		try
		{
			string[] searchForPart1 = new string[] { "window", "document" };
			string[] searchForPart2 = new string[] { "location" };
			string test = "alert(document.location+'?gohere=1');";

			TextRange expected = new TextRange(
				test.IndexOf("document.location+"),
				test.IndexOf("document.location+") + "document.location".Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchForPart1, searchForPart2, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyGetterRange2_Test3()
	{
		try
		{
			string[] searchForPart1 = new string[] { "location", "document" };
			string[] searchForPart2 = new string[] { "href", "location" };
			string test = "alert(document.location.href+'?gohere=1'+location.href);";

			TextRange expected = new TextRange(
				test.IndexOf("document.location.href"),
				test.IndexOf("document.location.href") + "document.location".Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchForPart1, searchForPart2, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyGetterRange2_Test4()
	{
		try
		{
			string[] searchForPart1 = new string[] { "window", "document", "location" };
			string[] searchForPart2 = new string[] { "href", "location" };
			string test = "alert(document.href.location+'?gohere=1'+location.href);";

			TextRange expected = new TextRange(
				test.IndexOf("document.href.location"),
				test.IndexOf("document.href.location") + "document.href".Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchForPart1, searchForPart2, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	string FindPropertyGetterRange2_Test5()
	{
		try
		{
			string[] searchForPart1 = new string[] { "window", "document", "location" };
			string[] searchForPart2 = new string[] { "href", "location" };
			string test = "alert(location.href+'?gohere=1'+document.location);";

			TextRange expected = new TextRange(
				test.IndexOf("location.href+"),
				test.IndexOf("location.href+") + "location.href".Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchForPart1, searchForPart2, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	
	string FindPropertyGetterRange2_Test6()
	{
		try
		{
			string[] searchForPart1 = new string[] { "window", "document", "location" };
			string[] searchForPart2 = new string[] { "href", "location" };
			string test = "if(window.location=='go'){location.href=document.location+'?done=1';}";

			TextRange expected = new TextRange(
				test.IndexOf("document.location+"),
				test.IndexOf("document.location+") + "document.location".Length);

			return AssertResult(test, JSParser.FindPropertyGetterRange(ref test, searchForPart1, searchForPart2, 0), expected);
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}
	#endregion

	string TestN()
	{
		try
		{

			return "";
		}
		catch (Exception ex)
		{
			return Failed(ex.Message);
		}
	}


	#endregion

	protected void Page_Load(object sender, EventArgs e)
	{

	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>JSParser Test Suit</title>
</head>
<body>
<h2>Attention, this test suit shouldn't run in ASProxy. This is a stand alone test.</h2>

	<form id="frmJSParser" runat="server">
		<h2>
			Welcome to JSParser test suit</h2>
		<b>Author: Salar Khalilzadeh</b><br />
		<b>Update: 2008-12-24</b>
		<br />
		<br />
		<!--================================-->
		<h3>
			Test: FindMethodParametersRange method.</h3>
		<p>
			This function locates hole text (including parameters and etc.) between start parenthesis
			and end parenthesis of method.</p>
		Test 1:
		<%=FindMethodParametersRange_Test1()%>
		<br />
		Test 2:
		<%=FindMethodParametersRange_Test2()%>
		<br />
		Test 3:
		<%=FindMethodParametersRange_Test3()%>
		<br />
		Test 4:
		<%=FindMethodParametersRange_Test4()%>
		<br />
		Test 5:
		<%=FindMethodParametersRange_Test5()%>
		<br />
		<!--================================-->
		<h3>
			Test: FindPropertySetterRange method.</h3>
		<p>
			This function locates property set location.</p>
		Test 1:
		<%=FindPropertyRange_Test1()%>
		<br />
		Test 2:
		<%=FindPropertyRange_Test2()%>
		<br />
		Test 3:
		<%=FindPropertyRange_Test3()%>
		<br />
		Test 4:
		<%=FindPropertyRange_Test4()%>
		<br />
		Test 5:
		<%=FindPropertyRange_Test5()%>
		<br />
		Test 6:
		<%=FindPropertyRange_Test6()%>
		<br />
		Test 7:
		<%=FindPropertyRange_Test7()%>
		<br />
		Test 8:
		<%=FindPropertyRange_Test8()%>
		<br />
		Test 9:
		<%=FindPropertyRange_Test9() %>
		<br />
		Test 10:
		<%=FindPropertyRange_Test10()%>
		<br />
		Test 11:
		<%=FindPropertyRange_Test11()%>
		<br />
		Test 12:
		<%=FindPropertyRange_Test12()%>
		<br />
		Test 13:
		<%=FindPropertyRange_Test13()%>
		<br />
		<!--================================-->
		<h3>
			Test: FindPropertyGetterRange method.</h3>
		<p>
			This function locates property get location.</p>
		Test 1:
		<%=FindPropertyGetterRange_Test1()%>
		<br />
		Test 2:
		<%=FindPropertyGetterRange_Test2()%>
		<br />
		Test 3:
		<%=FindPropertyGetterRange_Test3()%>
		<br />
		Test 4:
		<%=FindPropertyGetterRange_Test4()%>
		<br />
		Test 5:
		<%=FindPropertyGetterRange_Test5()%>
		<br />
		<!--================================-->
		<h3>
			Test: FindPropertyGetterRange2 method.</h3>
		<p>
			This function locates property get location by a range of items.</p>
		Test 1:
		<%=FindPropertyGetterRange2_Test1()%>
		<br />
		Test 2:
		<%=FindPropertyGetterRange2_Test2()%>
		<br />
		Test 3:
		<%=FindPropertyGetterRange2_Test3()%>
		<br />
		Test 4:
		<%=FindPropertyGetterRange2_Test4()%>
		<br />
		Test 5:
		<%=FindPropertyGetterRange2_Test5()%>
		<br />
		Test 6:
		<%=FindPropertyGetterRange2_Test6()%>
		<br />
		
		<!--================================-->
		<h3>
			Test: FindMethodFirstParameterRange method</h3>
		<p>
			Find the first parameter of a method.</p>
		Test 1:
		<%=FindMethodFirstParameterRange_Test1()%>
		<br />
		Test 2:
		<%=FindMethodFirstParameterRange_Test2()%>
		<br />
		Test 3:
		<%=FindMethodFirstParameterRange_Test3()%>
		<br />
		Test 4:
		<%=FindMethodFirstParameterRange_Test4()%>
		<br />
		Test 5:
		<%=FindMethodFirstParameterRange_Test5()%>
		<br />
		Test 6:
		<%=FindMethodFirstParameterRange_Test6()%>
		<br />
	</form>
</body>
</html>

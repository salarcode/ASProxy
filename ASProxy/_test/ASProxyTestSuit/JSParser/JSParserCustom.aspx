<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="SalarSoft.ASProxy.BuiltIn" %>
<script runat="server">
	private DateTime timer1 = DateTime.Now;

	private void StartProcess()
	{
		timer1 = DateTime.Now;
	}
	private void EndProcess()
	{
		TimeSpan span = DateTime.Now.Subtract(timer1);


		lblProcessTime.Text = span.TotalSeconds+" S";
	}
	
	protected void btnParseIt_Click(object sender, EventArgs e)
	{
		try
		{
			TextRange result;
			string js = txtJS.Text;
			
			StartProcess();
			result = JSParser.FindMethodParametersRange(ref js, txtMethodName.Text,0);
			EndProcess();
			
			lblResult.Text = "Start: " + result.Start.ToString() + "    End: " + result.End.ToString();
			try
			{
				lblParsedText.Text ="{"+ js.Substring(result.Start, result.End - result.Start)+"}";
			}
			catch (Exception ex)
			{
				lblParsedText.Text = ex.ToString();
			}
		}
		catch (Exception ex)
		{
			lblResult.Text = ex.ToString();
		}
	}

	protected void txtParsProperty_Click(object sender, EventArgs e)
	{
		try
		{
			TextRange result;
			string js = txtTestProperty.Text;
			
			StartProcess();
			result = JSParser.FindPropertySetterRange(ref js, txtPropertyName.Text, 0);
			EndProcess();

			lblPropertyContain.Text = "Start: " + result.Start.ToString() + "    End: " + result.End.ToString();
			try
			{
				lblPropertyPos.Text = "{" + js.Substring(result.Start, result.End - result.Start) + "}";
			}
			catch (Exception ex)
			{
				lblPropertyPos.Text = ex.ToString();
			}
		}
		catch (Exception ex)
		{
			lblPropertyContain.Text = ex.ToString();
		}
	}

	protected void btnParseMethod_Click(object sender, EventArgs e)
	{
		try
		{
			TextRange result;
			string js = txtParseMethod.Text;
			
			StartProcess();
			result = JSParser.FindMethodFirstParameterRange(ref js, txtParseMethodName.Text, 0);
			EndProcess();

			lblParseMethodPosition.Text = "Start: " + result.Start.ToString() + "    End: " + result.End.ToString();
			try
			{
				lblParseMethodContain.Text = "{" + js.Substring(result.Start, result.End - result.Start) + "}";
			}
			catch (Exception ex)
			{
				lblParseMethodContain.Text = ex.ToString();
			}
		}
		catch (Exception ex)
		{
			lblParseMethodPosition.Text = ex.ToString();
		}
	}

	protected void btnReplaceBetweenString_Click(object sender, EventArgs e)
	{
		try
		{
			//string str = txtParseMethod.Text;
			
			//int start = txtParseMethod.Text.IndexOf('(') + 1;
			//int End = JSParser.FindMethodParameterEndRange(ref str, start, txtParseMethod.Text.Length);

			//lblParseMethodPosition.Text = "Start: " + start.ToString() + "    End: " + End.ToString();
			//try
			//{
			//    lblParseMethodContain.Text = "{" + str.Substring(start, End - start) + "}";
			//}
			//catch (Exception ex)
			//{
			//    lblParseMethodContain.Text = ex.ToString();
			//}
		}
		catch (Exception ex)
		{
			lblParseMethodPosition.Text = ex.ToString();
		}


	}

	protected void btnFindPropertySetPosition_Click(object sender, EventArgs e)
	{
		try
		{
			//string str = txtTestProperty.Text;

			//int start = txtTestProperty.Text.IndexOf('=') + 1;
			//int End = JSParser.FindPropertySetEndRange(ref str, start, txtTestProperty.Text.Length);

			//lblPropertyPos.Text = "Start: " + start.ToString() + "    End: " + End.ToString();
			//try
			//{
			//    lblPropertyContain.Text = "{" + str.Substring(start, End - start) + "}";
			//}
			//catch (Exception ex)
			//{
			//    lblPropertyContain.Text = ex.ToString();
			//}
		}
		catch (Exception ex)
		{
			lblPropertyPos.Text = ex.ToString();
		}
	}

	protected void btnFindMethodHoleEndRange_Click(object sender, EventArgs e)
	{
		try
		{
			//TextRange result;
			//string str = txtJS.Text;

			//int start = txtJS.Text.IndexOf('(') + 1;
			//int End = JSParser.FindMethodHoleEndRange(ref str, start, txtJS.Text.Length);

			//lblResult.Text = "Start: " + start.ToString() + "    End: " + End.ToString();
			//try
			//{
			//    lblParsedText.Text = "{" + str.Substring(start, End - start) + "}";
			//}
			//catch (Exception ex)
			//{
			//    lblParsedText.Text = ex.ToString();
			//}
		}
		catch (Exception ex)
		{
			lblResult.Text = ex.ToString();
		}

	}

	protected void DottedPos_Click(object sender, EventArgs e)
	{
		//try
		//{
		//    TextRange result;
		//    string str = txtTestProperty.Text;

		//    result = JSParser.FindDottedCommandStartPosition(ref str, txtPropertyName.Text, 0, '=');

		//    lblPropertyContain.Text = "Start: " + result.Start.ToString() + "    End: " + result.End.ToString();

		//}
		//catch (Exception ex)
		//{
		//    lblPropertyContain.Text = ex.ToString();
		//}
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body style="font-family:Tahoma; font-size:small;">
<h2>Attention, this test suit shouldn't run in ASProxy. This is a stand alone test.</h2>

    <form id="form1" runat="server">
		<table style="width:100%" cellpadding="10">
		<tr><td>
		<asp:TextBox ID="txtMethodName" runat="server" Width="342px">window.open</asp:TextBox><br />
		<asp:Button ID="btnParseIt" runat="server" Text="ParseIt" OnClick="btnParseIt_Click" />
			<asp:Button ID="btnFindMethodHoleEndRange" runat="server" OnClick="btnFindMethodHoleEndRange_Click"
				Text="FindMethodParametersRange" /><br />
		<asp:TextBox ID="txtJS" runat="server" Height="112px" TextMode="MultiLine" Width="100%">window.open('Hello() ','parameter2',function3('dude'+foo()),End parameter)</asp:TextBox><br />
		<asp:Label ID="lblResult" EnableViewState="false" runat="server" Text="Label"></asp:Label><br />
		<asp:Label ID="lblParsedText" runat="server" EnableViewState="false" Text="Label"></asp:Label></td><td>
		<asp:TextBox ID="txtPropertyName" runat="server" Width="342px">window.location</asp:TextBox><br />
			<asp:Button ID="btnParsProperty"
			runat="server" Text="Parse property" OnClick="txtParsProperty_Click" />
					<asp:Button ID="btnFindPropertySetPosition" runat="server"
						Text="FindPropertySetPosition" OnClick="btnFindPropertySetPosition_Click" /><br />
			<asp:TextBox ID="txtTestProperty" runat="server" Height="105px"
				TextMode="MultiLine" Width="100%">window.location=function(){return hello()+what+'ok();'+'continue?';}; otherf();</asp:TextBox><br />
			<asp:Button ID="DottedPos" runat="server" OnClick="DottedPos_Click" Text="DottedPos" />
			<asp:Label ID="lblPropertyContain" runat="server" EnableViewState="false" Text="Label"></asp:Label><br />
		<asp:Label ID="lblPropertyPos" runat="server" EnableViewState="false" Text="Label"></asp:Label></td></tr>
			<tr>
				<td>
					<asp:TextBox ID="txtParseMethodName" runat="server" Width="342px">window.open</asp:TextBox><br />
					<asp:Button ID="btnParseMethod" runat="server" Text="Parse method" OnClick="btnParseMethod_Click" />
					<asp:Button ID="btnReplaceBetweenString" runat="server" Text="FindMethodParameterEndRange" OnClick="btnReplaceBetweenString_Click" /><br />
					<asp:TextBox ID="txtParseMethod" runat="server" Height="112px" TextMode="MultiLine" Width="100%">window.open('Hello, '+test(&quot; hel,lo &quot;),param2,param3))</asp:TextBox><br />
					<asp:Label ID="lblParseMethodPosition" runat="server" EnableViewState="false" Text="Label"></asp:Label><br />
					<asp:Label
						ID="lblParseMethodContain" runat="server" EnableViewState="false" Text="Label"></asp:Label></td>
				<td>
					Process time:
					<asp:Label ID="lblProcessTime" runat="server" Text="Label"></asp:Label></td>
			</tr>
			<tr>
				<td>
				</td>
				<td>
				</td>
			</tr>
			<tr>
				<td>
				</td>
				<td>
				</td>
			</tr>
		</table>

    </form>
</body>
</html>

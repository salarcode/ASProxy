<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

	protected void btnJSProcessorExecute_Click(object sender, EventArgs e)
	{
		try
		{
            SalarSoft.ASProxy.BuiltIn.JSProcessor js = new SalarSoft.ASProxy.BuiltIn.JSProcessor();
            //js.UserOptions.Scripts = true;
			//js.UserOptions.RemoveScripts = false;
			//txtResult.Text = js.Execute(txtJSCode.Text);

		}
		catch (Exception ex)
		{
			txtResult.Text = ex.ToString();
		}
	}
</script>
<script type="text/javascript">

//var va1=document.location;
//va1+="\n"+document.location.href;
//va1+="\n"+window.location;
//va1+="\n"+window.location.hostname;
//va1+="\n"+document.URL;


//va1+="\n document.location.href='test.htm';";
//va1+="\n document.location.replace('test.htm');";
//va1+="\n window.location.replace('test.htm');";
//va1+="\n location.replace('test.htm');";
//va1+="\n location.assign('test.htm');";
//va1+="\n location.bold();";

//alert(va1);

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>JSProcessor Complete Test</title>
</head>
<body>
	<h2>
		Attention, this test suit shouldn't run in ASProxy. This is a stand alone test.</h2>
	<form id="frmJSProcessor" runat="server" style="height: 95%">
	<asp:TextBox ID="txtJSCode" runat="server"  TextMode="MultiLine" 
		Width="719px" Rows="18"> 
var va1=document.location;
va1+="\n"+document.location.href;
va1+="\n"+window.location;
va1+="\n"+window.location.hostname;
va1+="\n"+document.URL;


va1+="\n location.replace('replace.htm');";
va1+="\n location.assign('assign.htm');";
va1+="\n document.location.href='test.htm';";
va1+="\n document.location.replace('replace.htm');";
va1+="\n window.location.replace('replace.htm');";
va1+="\n location.bold();";

alert(va1);
	</asp:TextBox>
	<br />
	<asp:Button ID="btnJSProcessorExecute" runat="server" Text="JSProcessor Execute"
		OnClick="btnJSProcessorExecute_Click" />
	<br />
	<asp:TextBox ID="txtResult" runat="server" ReadOnly="True" TextMode="MultiLine"
		Width="717px" Rows="18"></asp:TextBox>
	</form>
</body>
</html>

<%@ Page Language="C#" %>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
void SurfingOnLoading()
{
ASProxyEngine engine = null;
try
{
	if (!Page.IsPostBack)
	{
		if (UrlProvider.IsASProxyAddressUrlIncluded(Request.QueryString))
		{
			engine = new ASProxyEngine(ProcessTypeForData.CSS,true);
			engine.RequestInfo.ContentType = MimeContentType.text_css;
			engine.Initialize(Request);
			engine.Execute(Response);

			if (engine.LastStatus== LastActivityStatus.Error)
			{
                if (LogSystem.Enabled)
                    LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);
                
                Response.Clear();
				SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(Response);
				Response.ContentType = "text/html";
				Response.Write("/* " + engine.LastErrorMessage + " */");
				Response.End();
			}
		}
		else
			Response.Redirect(FilesConsts.DefaultPage, false);
	}
}
catch (System.Threading.ThreadAbortException)
{
}
catch (Exception err)
{
    if (LogSystem.Enabled)
        LogSystem.Log(LogEntity.Error, Request.Url.ToString(), err.Message);
    
    Response.Clear();
	SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(Response);
	Response.ContentType = "text/html";
	Response.Write("/* Error: " + err.Message + " */");
	Response.End();
}
finally
{
	if (engine != null)
		engine.Dispose();
}
}

protected void Page_Load(object sender, EventArgs e)
{
	SurfingOnLoading();
}
</script>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head><title>GetCSS</title></head>
<body><form id="frmGetCSS" runat="server"></form>
</body></html>
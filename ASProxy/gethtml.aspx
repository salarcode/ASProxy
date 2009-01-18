<%@ Page Language="C#"%>
<%@ Import Namespace="SalarSoft.ASProxy" %>
<%@ Import Namespace="System.Threading" %>
<script runat="server">
void SurfingOnLoading()
{
ASProxyEngine engine=null;
try  
{
	if (!Page.IsPostBack)
	{
		if (UrlProvider.IsASProxyAddressUrlIncluded(Request.QueryString))
		{
			engine = new ASProxyEngine(ProcessTypeForData.HTML,true);
			engine.RequestInfo.ContentType = MimeContentType.text_html;
			engine.Initialize(Request);
			engine.Execute(Response);

			//engine = new ASProxyResults(Request, true, true, false, MimeContentType.text_html);
			//engine.DoResponse(Response);

			if (engine.LastStatus== LastActivityStatus.Error)
			{
                if (LogSystem.Enabled)
                    LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);
                
                Response.Clear();
				SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(Response);
				Response.ContentType = "text/html";
				Response.Write("//" + engine.LastErrorMessage);
				Response.End();
			}
		}
		else
			Response.Redirect(FilesConsts.DefaultPage, false);
	}
}
catch (ThreadAbortException)
{
}
catch (Exception err)
{
    if (LogSystem.Enabled)
        LogSystem.Log(LogEntity.Error, Request.Url.ToString(), err.Message);

	Response.Clear();
	SalarSoft.ASProxy.Common.ClearASProxyRespnseHeader(Response);
	Response.ContentType = "text/html";
	Response.Write("<center><b><font color='red'>Error: " + err.Message + "</font></b></center>");
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
</script><html>
<head></head><body><form id="frmGetHtml" runat="server"  asproxydone="2">
</form></body></html>
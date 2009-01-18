<%@ WebHandler Language="C#" Class="SalarSoft.ASProxy.AjaxWrapper" %>

using System;
using System.Web;

namespace SalarSoft.ASProxy
{
	public class AjaxWrapper : IHttpHandler
	{

		public void ProcessRequest(HttpContext context)
		{
			AjaxEngine engine = new AjaxEngine(context);
			try
			{
				// load options
				engine.Options = ASProxyConfig.GetCookieOptions();

				// initilize
				engine.Initialize();

				// test for error
				if (engine.LastStatus == LastActivityStatus.Error)
				{
					if (LogSystem.Enabled)
						LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

					context.Response.Write(engine.LastErrorMessage);
					context.Response.StatusDescription = engine.LastErrorMessage;
					context.Response.StatusCode = (int)engine.LastErrorStatusCode();
					return;
				}

				// execute the request
				engine.Execute();
				
				// test for error
				if (engine.LastStatus == LastActivityStatus.Error)
				{
					if (LogSystem.Enabled)
						LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, engine.LastErrorMessage);

					context.Response.Write(engine.LastErrorMessage);
					context.Response.StatusDescription = engine.LastErrorMessage;
					context.Response.StatusCode = (int)engine.LastErrorStatusCode();
					return;
				}

				// apply to response
				engine.ApplyToResponse(context.Response);
				
			}
			catch (System.Threading.ThreadAbortException) { }
			catch (Exception ex)
			{
				if (LogSystem.Enabled)
					LogSystem.Log(LogEntity.Error, engine.RequestInfo.RequestUrl, ex.Message);
				
				context.Response.Write(ex.Message);
				context.Response.StatusDescription = ex.Message;
				context.Response.StatusCode = (int)engine.LastErrorStatusCode();
			}
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

	}
}
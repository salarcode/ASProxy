<%@ WebHandler Language="C#" Class="ajax" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using SalarSoft.ASProxy.Exposed;
using SalarSoft.ASProxy.BuiltIn;

public class ajax : IHttpHandler
{

	public void ProcessRequest(HttpContext context)
	{
		AjaxEngine engine = new AjaxEngine();
		try
		{
			engine.DataTypeToProcess = DataTypeToProcess.None;

			//TODO: Test content type affect
			//engine.RequestInfo.ContentType = MimeContentType.application;

			// load options
			engine.UserOptions = UserOptions.ReadFromRequest();

			// initilize
			engine.Initialize(context.Request);

			// initialize the request
			engine.ExecuteHandshake();

			// test for error
			if (engine.LastStatus == LastStatus.Error)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(engine.LastException, engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

				context.Response.Write(engine.LastErrorMessage);
				context.Response.StatusDescription = engine.LastErrorMessage;
				context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);
				return;
			}

			// execute the request
			engine.ExecuteToResponse(context.Response);

			// test for error
			if (engine.LastStatus == LastStatus.Error)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(engine.LastException, engine.LastErrorMessage, engine.RequestInfo.RequestUrl);

				context.Response.Write(engine.LastErrorMessage);
				context.Response.StatusDescription = engine.LastErrorMessage;
				context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);
				return;
			}

		}
		catch (System.Threading.ThreadAbortException) { }
		catch (Exception ex)
		{
			if (Systems.LogSystem.ErrorLogEnabled)
				Systems.LogSystem.LogError(ex, ex.Message, engine.RequestInfo.RequestUrl);


			context.Response.Write(ex.Message);
			context.Response.StatusDescription = ex.Message;
			context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(engine.LastException);
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
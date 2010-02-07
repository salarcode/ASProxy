using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using System.Threading;
using System.Net;

namespace SalarSoft.ASProxy
{
	public class CustomErrors
	{
		internal static string GetCustomErrorFileAddr()
		{
			return HttpRuntime.AppDomainAppPath + Consts.FilesConsts.PageCustomErrors;
		}

		public static void HandleCustomErrors(Exception ex)
		{
			try
			{
				HttpContext context = HttpContext.Current;

				// if custom error is enabled
				if (context.IsCustomErrorEnabled == false)
					return;

				string errorFile = GetCustomErrorFileAddr();
				string errorPattern = File.ReadAllText(errorFile);
				string errorDetails = GetCustomErrorDetails(context.Request, ex);

				errorPattern = errorPattern.Replace("[ErrorDetails]", errorDetails);

				// clear all previous data
				context.Response.ClearContent();

				// BugFixed 5.5b2: Content-Encding:Gzip is required
				Common.ClearHeadersButSaveEncoding(context.Response);

				// write to response
				context.Response.ContentType = "text/html";
				context.Response.ContentEncoding = Encoding.UTF8;
				context.Response.Write(errorPattern);
				context.Response.StatusCode = (int)Common.GetExceptionHttpErrorCode(ex);
				context.Response.StatusDescription = ex.Message;

				// clear previous errors
				context.ClearError();

				// finish current request
				context.Response.End();
			}
			catch (ThreadAbortException) { }
			catch (Exception) { }
		}

		private static string GetCustomErrorDetails(HttpRequest req, Exception ex)
		{
			if (ex == null) return "";
			string result;

			result = "\nMessage: " + ex.Message;
			result += "\n\nUserAgent: " + req.UserAgent;
			if (req.UrlReferrer != null)
				result += "\nReferrer: " + req.UrlReferrer.ToString();
			result += "\nError location: " + req.Url.ToString();
			result += "\nHttp method: " + req.HttpMethod;
			result += "\nIs authenticated: " + req.IsAuthenticated.ToString();
			result += "\nIs secure connection: " + req.IsSecureConnection.ToString();
			result += "\nIs local: " + req.IsLocal.ToString();
			result += "\ndotNet Runtime: " + Environment.Version.ToString();
			result += "\nServer OS Version: " + Environment.OSVersion.ToString();

			if (ex is WebException || ex is HttpException)
			{
				HttpStatusCode code = Common.GetExceptionHttpErrorCode(ex);
				result += "\nStatus code: " + ((int)code).ToString() + " " + code.ToString();
			}

			result += "\n\n\nException details: " + ex.ToString();
			return result;
		}
	}
}

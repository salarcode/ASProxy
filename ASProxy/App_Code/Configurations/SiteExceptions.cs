using System;
using System.IO;
using System.Web;
using System.Net;
using System.Threading;

namespace SalarSoft.ASProxy
{
	public class ASProxyExceptions : Exception
	{
		public ASProxyExceptions()
		{
			LogException(this, "");
		}
		public ASProxyExceptions(string ErrorMessage)
		{
			LogException(this, ErrorMessage);
		}
		public ASProxyExceptions(string ErrorMessage, Exception ex)
		{
			LogException(ex, ErrorMessage);
		}

		internal static string GetCustomErrorFileAddr()
		{
			return HttpRuntime.AppDomainAppPath + FilesConsts.CustomErrorsPage;
		}

		internal static string GetExceptionFileAddr()
		{
			string result = "exceptions.xml";
			result = HttpRuntime.AppDomainAppPath + FilesConsts.Dir_Bin + "\\" + result;
			return result;
		}

		public static void LogException(Exception ex)
		{
			LogException(ex, "");
		}

		public static void LogException(Exception ex, string ErrorMessage)
		{
			if (!ASProxyConfig.ErrorLogEnabled)
				return;

			if (ex is HttpException)
			{
				if (ex.Message.ToLower().LastIndexOf("does not exist") != -1)
					return;
			}
			else if (ex is System.Threading.ThreadAbortException)
				return;

			if (ErrorMessage == null)
				ErrorMessage = "";

			StreamWriter exFile = null;
			try
			{
				string exceptionFile = GetExceptionFileAddr();
				exFile = File.AppendText(exceptionFile);

				exFile.WriteLine();
				exFile.WriteLine("===============");
				exFile.WriteLine("Date time:     " + DateTime.Now.ToString());

				exFile.WriteLine("Error message: " + ErrorMessage);
				exFile.WriteLine("Error details: ");
				if (ex != null)
					exFile.Write(ex.ToString());
				else
					exFile.Write("No details, exception is NULL");
			}
			catch { }
			finally
			{
				if (exFile != null)
					exFile.Close();
			}
		}

		public static void HandleCustomErrors(Exception ex)
		{
			try
			{
				HttpContext context = HttpContext.Current;
				string errorFile = GetCustomErrorFileAddr();
				string errorPattern = File.ReadAllText(errorFile);
				string errorDetails = GetCustomErrorDetails(context.Request, ex);

				errorPattern = errorPattern.Replace("[ErrorDetails]", errorDetails);

				// clear all previous data
				context.Response.ClearContent();
				context.Response.ClearHeaders();

				// write to response
				context.Response.ContentType = "text/html";
				context.Response.Write(errorPattern);
				context.Response.StatusCode = (int)GetExceptionStatusCode(ex);
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

			if (ex is WebException || ex is HttpException)
			{
				HttpStatusCode code = GetExceptionStatusCode(ex);
				result += "\nStatus code: " + ((int)code).ToString()+" " + code.ToString();
			}

			result += "\n\n\nException details: " + ex.ToString();
			return result;
		}

		private static HttpStatusCode GetExceptionStatusCode(Exception ex)
		{
			if (ex != null)
			{
				if (ex is WebException)
				{
					WebException webEx = (WebException)ex;
                    if (webEx.Response != null && webEx.Response is HttpWebResponse)
                    {
                        return ((HttpWebResponse)webEx.Response).StatusCode;
                    }
                    //else
                    //{
                    //    // since v5.1: Added to save more info
                    //    return (HttpStatusCode)(-(int)webEx.Status);
                    //}
				}
				else if (ex is HttpException)
				{
					return (HttpStatusCode)((HttpException)ex).GetHttpCode();
				}
			}
			return HttpStatusCode.InternalServerError;
		}

	}
}
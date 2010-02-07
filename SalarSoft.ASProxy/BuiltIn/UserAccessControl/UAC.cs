using System;
using System.Collections.Generic;
using System.Text;
using SalarSoft.ASProxy.Exposed;
using System.Web;
using System.Threading;
using System.Net;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class UAC : ExUAC
	{
		private const string _forbiddenMessage = "The administrator has blocked your access to this service.";
		private bool _isPluginAvailable;

		public UAC()
		{
			// getting plugin availablity state
			_isPluginAvailable = Plugins.IsPluginAvailable(PluginHosts.IPluginUAC);
		}


		/// <summary>
		/// Validates the request context for UAC
		/// </summary>
		public override bool ValidateContext(HttpContext context)
		{
			// executing plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginUAC,
					PluginMethods.IPluginUAC.ValidateContext,
					this, context);

			if (!Configurations.UserAccessControl.Enabled)
				return true;

			// user ip
			string userIP = context.Request.UserHostAddress;

			if (Configurations.UserAccessControl.AllowedList != null ||
				Configurations.UserAccessControl.AllowedRange != null)
			{
				// Search in single ip addresses
				if (Configurations.UserAccessControl.AllowedList != null)
						// Is the IP allowed?
					if (Configurations.UserAccessControl.AllowedList.Contains(userIP))
						return true;

				if (Configurations.UserAccessControl.AllowedRange != null)
					foreach (Configurations.UserAccessControlConfig.IPRange range in Configurations.UserAccessControl.AllowedRange)
					{
						// Is between the range?
						if (CompareIPs.IsGreaterOrEqual(userIP, range.Low) && CompareIPs.IsLessOrEqual(userIP, range.High))
						{
							return true;
						}
					}

				// No match found, end app anyway!
				EndApplication(context);
				return false;
			}

			// Search in single blocked ip addresses
			if (Configurations.UserAccessControl.BlockedList != null)
				// Is the IP blocked?
				if (Configurations.UserAccessControl.BlockedList.Contains(userIP))
				{
					EndApplication(context);
					return false;
				}

			if (Configurations.UserAccessControl.BlockedRange != null)
				foreach (Configurations.UserAccessControlConfig.IPRange range in Configurations.UserAccessControl.BlockedRange)
				{
					// Is between the range?
					if (CompareIPs.IsGreaterOrEqual(userIP, range.Low) && CompareIPs.IsLessOrEqual(userIP, range.High))
					{
						EndApplication(context);
						return false;
					}
				}

			return true;
		}

		private void EndApplication(HttpContext context)
		{
			try
			{
				Common.ClearHeadersButSaveEncoding(context.Response);
				
				// write to response
				context.Response.ContentType = "text/html";
				context.Response.ContentEncoding = Encoding.UTF8;
				context.Response.Write(_forbiddenMessage);
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				context.Response.StatusDescription = _forbiddenMessage;

				context.Response.End();
			}
			catch (ThreadAbortException) { }
		}
	}
}

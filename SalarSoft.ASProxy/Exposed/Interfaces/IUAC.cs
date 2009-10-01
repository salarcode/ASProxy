using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy.Exposed
{
	/// <summary>
	/// User Access Control
	/// </summary>
	public interface IUAC
	{
		/// <summary>
		/// Validates the request context for UAC
		/// </summary>
		bool ValidateContext(HttpContext context);
	}
}

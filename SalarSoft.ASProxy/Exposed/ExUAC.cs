using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Exposed
{
	/// <summary>
	/// User Access Control
	/// </summary>
	public abstract class ExUAC : IUAC
	{
		public abstract bool ValidateContext(System.Web.HttpContext context);
	}
}

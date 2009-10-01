using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginUAC
		{
			ValidateContext
		}
	}

	public interface IPluginUAC
	{
		/// <summary>
		/// 
		/// </summary>
		void ValidateContext(IUAC uac, HttpContext context);
	}
}

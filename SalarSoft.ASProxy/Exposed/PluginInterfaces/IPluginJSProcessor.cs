using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginJSProcessor
		{
			BeforeExecute,
			AfterExecute
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IPluginJSProcessor
	{
		/// <summary>
		/// 
		/// </summary>
		void BeforeExecute(IJSProcessor cssProcessor, ref string codes, string pageUrl, string pageUrlNoQuery, string pagePath, string rootUrl);

		/// <summary>
		/// 
		/// </summary>
		void AfterExecute(IJSProcessor cssProcessor, ref string codes, string pageUrl, string pageUrlNoQuery, string pagePath, string rootUrl);
	}
}

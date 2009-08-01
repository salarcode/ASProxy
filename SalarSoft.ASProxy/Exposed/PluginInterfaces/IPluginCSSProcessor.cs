using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginCSSProcessor
		{
			BeforeExecute,
			AfterExecute
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IPluginCSSProcessor
	{
		/// <summary>
		/// 
		/// </summary>
		void BeforeExecute(ICssProcessor cssProcessor, ref string codes, string pageUrl, string pageUrlNoQuery, string pagePath, string rootUrl);

		/// <summary>
		/// 
		/// </summary>
		void AfterExecute(ICssProcessor cssProcessor, ref string codes, string pageUrl, string pageUrlNoQuery, string pagePath, string rootUrl);
	}
}
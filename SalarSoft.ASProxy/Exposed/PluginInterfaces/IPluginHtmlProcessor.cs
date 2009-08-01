using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginHtmlProcessor
		{
			BeforeExecute,
			AfterExecute
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IPluginHtmlProcessor
	{
		/// <summary>
		/// 
		/// </summary>
		void BeforeExecute(IHtmlProcessor cssProcessor, ref string codes, string pageUrl, string pageUrlNoQuery, string pagePath, string rootUrl);

		/// <summary>
		/// 
		/// </summary>
		void AfterExecute(IHtmlProcessor cssProcessor, ref string codes, string pageUrl, string pageUrlNoQuery, string pagePath, string rootUrl);
	}
}

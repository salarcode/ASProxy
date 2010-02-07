using System;
using System.Collections.Generic;
using System.Text;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class Processors
	{
		private static IHtmlProcessor _HtmlProcessor;
		private static ICssProcessor _CssProcessor;
		private static IJSProcessor _JSProcessor;

		static Processors()
		{
			InitlizeClasses();
		}

		static void InitlizeClasses()
		{
			_HtmlProcessor = (IHtmlProcessor)Providers.GetProvider(ProviderType.IHtmlProcessor);
			_CssProcessor = (ICssProcessor)Providers.GetProvider(ProviderType.ICssProcessor);
			_JSProcessor = (IJSProcessor)Providers.GetProvider(ProviderType.IJSProcessor);
		}

		public static IHtmlProcessor HtmlProcessor { get { return _HtmlProcessor; } }
		public static ICssProcessor CssProcessor { get { return _CssProcessor; } }
		public static IJSProcessor JSProcessor { get { return _JSProcessor; } }
	}
}

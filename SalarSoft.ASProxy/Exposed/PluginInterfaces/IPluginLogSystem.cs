using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginLogSystem
		{
			BeforeLog,
			BeforeLogError
		}
	}

	public interface IPluginLogSystem
	{
		/// <summary>
		/// 
		/// </summary>
		void BeforeLog(ILogSystem logSystem, LogEntity entity, HttpRequest request, string requestedUrl, params object[] optionalData);
		
		/// <summary>
		/// 
		/// </summary>
		void BeforeLog(ILogSystem logSystem, LogEntity entity, params object[] optionalData);
		
		/// <summary>
		/// 
		/// </summary>
		void BeforeLogError(ILogSystem logSystem, Exception ex, HttpRequest request, string message, string requestedUrl, params object[] optionalData);
	}
}
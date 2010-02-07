using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginHost
		{
			RegisterPlugin
		}
	}

	/// <summary>
	/// The plugin base class to get started.
	/// The plugin should register its host classes in constructor.
	/// </summary>
	public interface IPluginHost
	{
		/// <summary>
		/// Registers the plugin classes
		/// </summary>
		void RegisterPlugin();
	}
}

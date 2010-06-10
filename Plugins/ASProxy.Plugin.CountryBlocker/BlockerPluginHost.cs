using SalarSoft.ASProxy;
using SalarSoft.ASProxy.Exposed;

namespace ASProxy.Plugin.CountryBlocker
{
	public class BlockerPluginHost :IPluginHost
	{
		public BlockerPluginHost()
		{
			BlockedCountries.Initialize();
		}

		public void RegisterPlugin()
		{
			// Registring the plugin
			Plugins.RegisterHost(PluginHosts.IPluginEngine, typeof(BlockerPluginEngine));
		}
	}
}

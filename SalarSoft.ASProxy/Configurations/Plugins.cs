using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Web;
using System.Reflection;
using System.Collections;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy
{
    /// <summary>
    /// Plugins provider
    /// </summary>
    public class Plugins
    {
        internal struct PluginStore
        {
            public Type ClassType;
            public object Instance;
        }

        /// <summary>
        /// List of available plugins to load
        /// </summary>
        static List<PluginInfo> _loadedPlugins;

        /// <summary>
        /// List of installed plugins in ASProxy
        /// </summary>
        static List<PluginInfo> _installedPlugins;

        static Dictionary<PluginHosts, ArrayList> _pluginsClassType;
        static bool _pluginsEnabled;

        static Plugins()
        {
            _loadedPlugins = new List<PluginInfo>();
            _installedPlugins = new List<PluginInfo>();
            _pluginsClassType = new Dictionary<PluginHosts, ArrayList>();

            _pluginsEnabled = Configurations.Providers.PluginsEnabled;
            if (_pluginsEnabled)
                LoadPlugins();
        }

        /// <summary>
        /// Loaded plugins which will apply to the engine
        /// </summary>
        internal static List<PluginInfo> LoadedPlugins
        {
            get { return _loadedPlugins; }
        }

        /// <summary>
        /// All installed plugins, includes disabled and failed to load plugins
        /// </summary>
        internal static List<PluginInfo> InstalledPlugins
        {
            get
            {
                return _installedPlugins;
            }
        }
        /// <summary>
        /// The loaded plugins stores in current request's context.
        /// </summary>
        private static Dictionary<PluginHosts, List<PluginStore>> LoadedPluginsList
        {
            get
            {
                const string contextItemStoreKey = "Plugins.LoadedPlugins";
                HttpContext context = HttpContext.Current;
                if (context != null)
                {
                    Dictionary<PluginHosts, List<PluginStore>> loadedList =
                        (Dictionary<PluginHosts, List<PluginStore>>)context.Items[contextItemStoreKey];
                    if (loadedList == null)
                    {
                        loadedList = new Dictionary<PluginHosts, List<PluginStore>>();
                        context.Items[contextItemStoreKey] = loadedList;
                    }
                    return loadedList;
                }
                return null;
            }
        }


        #region public methods

        /// <summary>
        /// Registers plugin host
        /// </summary>
        public static void RegisterHost(PluginHosts hostType, Type pluginClass)
        {
            ArrayList added;

            // added plugins list
            if (!_pluginsClassType.TryGetValue(hostType, out added))
                added = new ArrayList();

            added.Add(pluginClass);
            _pluginsClassType[hostType] = added;
        }

        /// <summary>
        /// Removes the registration of a plugin host class
        /// </summary>
        public static void UnRegisterHost(PluginHosts hostType, Type pluginClass)
        {
            ArrayList pluginsList;
            if (_pluginsClassType.TryGetValue(hostType, out pluginsList))
            {
                // remove from registered list
                pluginsList.Remove(pluginClass);
            }

            // remove from loaded list
            Dictionary<PluginHosts, List<PluginStore>> loadedList = LoadedPluginsList;
            List<PluginStore> plugins;
            if (loadedList != null &&
                loadedList.TryGetValue(hostType, out plugins))
            {
                for (int i = 0; i < plugins.Count; i++)
                {
                    PluginStore plugin = plugins[i];
                    if (plugin.ClassType == pluginClass)
                    {
                        plugins.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes the registration of a plugin host class
        /// </summary>
        private static void UnRegisterHost(PluginHosts hostType)
        {
            // remove from registered list
            _pluginsClassType.Remove(hostType);

            // remove from loaded list
            Dictionary<PluginHosts, List<PluginStore>> loadedList = LoadedPluginsList;
            if (loadedList != null)
                loadedList.Remove(hostType);
        }
        #endregion


        #region internal methods

        /// <summary>
        /// Changes plugin enabled status
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="enabled"></param>
        internal static void SetPluginEnableStatus(string pluginName, bool enabled)
        {
            for (int i = 0; i < _loadedPlugins.Count; i++)
            {
                PluginInfo plugin = _loadedPlugins[i];
                if (plugin.Name == pluginName)
                {
                    plugin.Disabled = !enabled;
                    //_loadedPlugins[i] = plugin; // required for structs
                }
            }

            for (int i = 0; i < _installedPlugins.Count; i++)
            {
                PluginInfo plugin = _installedPlugins[i];
                if (plugin.Name == pluginName)
                {
                    plugin.Disabled = !enabled;
					//_installedPlugins[i] = plugin; // required for structs
                }
            }
        }

		/// <summary>
		/// Finds plugin
		/// </summary>
		internal static PluginInfo FindPlugin(List<PluginInfo> plugins, string pluginName)
		{
			foreach (PluginInfo plugin in plugins)
			{
				if (plugin.Name == pluginName)
				{
					return plugin; 
				}
			}
			return null;
		}

        /// <summary>
        /// Calls specified method of all specified hosts.
        /// </summary>
        internal static void CallPluginMethod(PluginHosts hostType, Enum methodNameEnum, params object[] arguments)
        {
            // Gets the available plugins list
            List<PluginStore> plugins = GetPluginsInstances(hostType);
            if (plugins != null && plugins.Count > 0)
            {
                // method name as enum
                string methodName = methodNameEnum.ToString();

                for (int i = 0; i < plugins.Count; i++)
                {
                    PluginStore pluginStore = plugins[i];
                    try
                    {
                        // getting requested method info using reflection
                        MethodInfo info = pluginStore.ClassType
                            .GetMethod(methodName,
                            BindingFlags.Instance | BindingFlags.Public);

                        // call plugin with specifed arguments
                        info.Invoke(pluginStore.Instance, arguments);
                    }
                    catch (EPluginStopRequest)
                    {
                        // The plugin requested to stop the process
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // the plugin requested to stop any operation
                        if (ex.InnerException is EPluginStopRequest)
                            throw;

                        if (Systems.LogSystem.ErrorLogEnabled)
                            Systems.LogSystem.LogError(ex, "Plugin method failed. IPluginEngine Plugin=" + plugins[i].ClassType + " MethodName=" + methodName, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if there is any plugin class registered for specified type
        /// </summary>
        internal static bool IsPluginAvailable(PluginHosts hostType)
        {
            if (!_pluginsEnabled)
                return false;

            if (_pluginsClassType.ContainsKey(hostType))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region private methods
        /// <summary>
        /// Creates and stores plugin class instances, then returns the their list
        /// </summary>
        private static List<PluginStore> GetPluginsInstances(PluginHosts hostType)
        {
            // reads loaded plugins list from context
            Dictionary<PluginHosts, List<PluginStore>> loadedPlugins = LoadedPluginsList;

            List<PluginStore> loaded;

            // if the request host is already created
            if (loadedPlugins.TryGetValue(hostType, out loaded))
            {
                // if there is any return it
                if (loaded.Count > 0)
                    return loaded;
            }


            ArrayList available;

            // the plugin class is not created
            // trying to get the plugin class type if avaialble
            if (_pluginsClassType.TryGetValue(hostType, out available))
            {
                // new store list
                loaded = new List<PluginStore>();

                for (int i = 0; i < available.Count; i++)
                {
                    Type classType = (Type)available[i];
                    object classObj;
                    try
                    {
                        // creatig a new instance of class type
                        classObj = InvokeDefaultCreateInstance(classType);

                        PluginStore store;
                        store.ClassType = classType;
                        store.Instance = classObj;

                        loaded.Add(store);
                    }
                    catch (Exception ex)
                    {
                        if (Systems.LogSystem.ErrorLogEnabled)
                            Systems.LogSystem.LogError(ex, "Error in create new instance of plugin host: hostName=" + hostType, string.Empty);
                    }
                }

                // adding stored plugin in a collection
                loadedPlugins[hostType] = loaded;

                // and the result
                return loaded;
            }

            // nothing found
            return null;
        }

        /// <summary>
        /// Load providers list from xml file
        /// </summary>
        private static void LoadPlugins()
        {
            string[] pluginsList = Directory.GetFiles(PluginsLocation,
                                     Consts.FilesConsts.File_PluginInfoExt,
                                     SearchOption.TopDirectoryOnly);


            if (_installedPlugins == null)
                _installedPlugins = new List<PluginInfo>();
            else
                _installedPlugins.Clear();

            for (int i = 0; i < pluginsList.Length; i++)
            {
                // reads plugin info from xml file
                PluginInfo info = ReadPluginInfo(pluginsList[i]);

                // and adds to the available plugins list
                _installedPlugins.Add(info);
            }

            LoadPluginInfo(_installedPlugins);
        }


        private static void LoadPluginInfo(List<PluginInfo> pluginsList)
        {
            try
            {
                PluginInfo info;
                for (int i = 0; i < pluginsList.Count; i++)
                {
                    info = pluginsList[i];

                    // if plugin is disabled don't do anything
                    if (info.Disabled)
                        continue;

                    Type classType;
                    try
                    {
                        // load specified class name
                        classType = Type.GetType(info.ClassTypeName);
                    }
                    catch (Exception ex)
                    {
                        if (Systems.LogSystem.ErrorLogEnabled)
                            Systems.LogSystem.LogError(ex, "Failed to load plugin: Name=" + info.Name + " TypeName=" + info.ClassTypeName, string.Empty);

                        // continue to next plugin
                        continue;
                    }

                    try
                    {
                        // calls plugin initialization
                        object plugObj = InvokeDefaultCreateInstance(classType);

						// Call plugin register method
						InvokePluginHostRegister(classType, plugObj);

                        // everything is ok
                        // Plugin is added to successfully loaded plugins
                        _loadedPlugins.Add(info);
                    }
                    catch (Exception ex)
                    {
                        if (Systems.LogSystem.ErrorLogEnabled)
                            Systems.LogSystem.LogError(ex, "Error in create new instance of a plugin: Name=" + info.Name, string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Systems.LogSystem.ErrorLogEnabled)
                    Systems.LogSystem.LogError(ex, "Error in loading plugins!", string.Empty);
            }
        }

        /// <summary>
        /// Creates a new instance of specified type by calling its default constructor
        /// </summary>
        private static object InvokeDefaultCreateInstance(Type type)
        {
            // Get the default constructor
            ConstructorInfo constructor = type.GetConstructor(new Type[] { });
            if (constructor != null)
            {
                return constructor.Invoke(new object[] { });
            }
            return false;
        }

		/// <summary>
		/// Calling plugin RegisterPlugin method
		/// </summary>
		static void InvokePluginHostRegister(Type pluginClassType, object pluginInstance)
		{
			string methodName = PluginMethods.IPluginHost.RegisterPlugin.ToString();
			try
			{
				// getting requested method info using reflection
				MethodInfo info = pluginClassType
					.GetMethod(methodName,
					BindingFlags.Instance | BindingFlags.Public);

				// call plugin with specifed arguments
				info.Invoke(pluginInstance, new object[] { });
			}
			catch (EPluginStopRequest)
			{
				// The plugin requested to stop the process
				throw;
			}
			catch (Exception ex)
			{
				// the plugin requested to stop any operation
				if (ex.InnerException is EPluginStopRequest)
					throw;

				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, "Plugin method failed. IPluginEngine Plugin=" + pluginClassType + " MethodName=" + methodName, string.Empty);
			}
		}

        /// <summary>
        /// Reads plugin info from specifed plugin xml file
        /// </summary>
        private static PluginInfo ReadPluginInfo(string pluginXmlFile)
        {
            PluginInfo result;
            try
            {
                XmlDocument xml = new XmlDocument();

                // load the plugin file
                xml.Load(pluginXmlFile);
                XmlNode rootNode = xml.SelectSingleNode("plugin");

                // read data from xml data file
                result = PluginInfo.ReadFromXml(rootNode);

                // disabled state
                result.Disabled = Configurations.Providers.IsPluginDisabled(result.Name);

                return result;
            }
            catch (Exception ex)
            {
                if (Systems.LogSystem.ErrorLogEnabled)
                    Systems.LogSystem.LogError(ex, "Failed to load a plugin: xmlFile= " + Path.GetFileName(pluginXmlFile), string.Empty);
            }
            return new PluginInfo();
        }

        private static string PluginsLocation
        {
            get
            {
                return CurrentContext.MapAppPath(Consts.FilesConsts.Dir_Plugins);
            }
        }

        #endregion
    }
}
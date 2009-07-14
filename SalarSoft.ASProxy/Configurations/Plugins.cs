using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using SalarSoft.ASProxy.Definitions;
using System.Reflection;
using System.Collections;

namespace SalarSoft.ASProxy
{
	public class Plugins
	{
		static List<PluginInfo> _loadedPlugins;
		static Dictionary<PluginHosts, ArrayList> _addedPlugins;
		static Dictionary<PluginHosts, ArrayList> _createdPlugins;

		static Plugins()
		{
			_loadedPlugins = new List<PluginInfo>();
			_addedPlugins = new Dictionary<PluginHosts, ArrayList>();
			_createdPlugins = new Dictionary<PluginHosts, ArrayList>();

			if (Configurations.Providers.PluginsEnabled)
				// the default providers will be used
				LoadPlugins();

		}


		#region public methods
		public static void RegisterHost(PluginHosts hostType, Type pluginClass)
		{
			// added plugins list
			ArrayList added = _addedPlugins[hostType];
			if (added == null)
				added = new ArrayList();

			added.Add(pluginClass);
			_addedPlugins[hostType] = added;
		}

		public static void UnRegisterHost(PluginHosts hostType)
		{
			// added plugins list
			_addedPlugins.Remove(hostType);
		}
		#endregion


		#region internal methods
		internal static bool IsPluginEnabled(PluginHosts hostType)
		{
			// added plugins list
			ArrayList added = _addedPlugins[hostType];
			if (added == null)
				return false;
			else
				return added.Count > 0;
		}

		internal static ArrayList GetPluginsInstance(PluginHosts hostType)
		{
			ArrayList created = _createdPlugins[hostType];
			if (created != null && created.Count > 0)
			{
				return created;
			}
			else
			{
				ArrayList added = _addedPlugins[hostType];
				created = new ArrayList();

				for (int i = 0; i < added.Count; i++)
				{
					Type classType = (Type)added[i];
					object classObj;
					try
					{
						classObj = InvokeDefaultCreateInstance(classType);
						created.Add(classObj);
					}
					catch (Exception)
					{
						if (Systems.LogSystem.ErrorLogEnabled)
							Systems.LogSystem.LogError("Error in create new instance of plugin host: hostName=" + hostType);
					}
				}
				_createdPlugins[hostType] = created;

				return created;
			}
		}
		#endregion

		#region private methods

		/// <summary>
		/// Load providers list from xml file
		/// </summary>
		private static void LoadPlugins()
		{
			string[] pluginsList = Directory.GetFiles(PluginsLocation,
									 Consts.FilesConsts.File_PluginInfoExt,
									 SearchOption.TopDirectoryOnly);

			List<PluginInfo> loaded = new List<PluginInfo>();

			for (int i = 0; i < pluginsList.Length; i++)
			{
				PluginInfo info = ReadPluginInfo(pluginsList[i]);
				loaded.Add(info);
			}

			LoadPluginInfo(loaded);
		}


		private static void LoadPluginInfo(List<PluginInfo> pluginsList)
		{
			try
			{
				PluginInfo info;
				for (int i = 0; i < pluginsList.Count; i++)
				{
					info = pluginsList[i];
					Type classType;
					try
					{
						// load specified class name
						classType = Type.GetType(info.ClassTypeName);
					}
					catch (Exception)
					{
						if (Systems.LogSystem.ErrorLogEnabled)
							Systems.LogSystem.LogError("Failed to load plugin: Name=" + info.Name + " TypeName=" + info.ClassTypeName);

						// continue to next plugin
						continue;
					}

					try
					{
						// calls plugin initialization
						object plugObj = InvokeDefaultCreateInstance(classType);

						// everything is ok
						// Plugin is added to successfully loaded plugins
						_loadedPlugins.Add(info);
					}
					catch (Exception)
					{
						if (Systems.LogSystem.ErrorLogEnabled)
							Systems.LogSystem.LogError("Error in create new instance of a plugin: Name=" + info.Name);
					}
				}
			}
			catch (Exception)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError("Error in loading plugins!");
			}
		}


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

				return result;
			}
			catch (Exception)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError("Failed to load a plugin: xmlFile= " + Path.GetFileName(pluginXmlFile));
			}
			return new PluginInfo();
		}

		private static string PluginsLocation
		{
			get
			{
				return CurrentContext.MapAppPath(Consts.FilesConsts.Dir_Plugins); ;
			}
		}

		#endregion
	}
}
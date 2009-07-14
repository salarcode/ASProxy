using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SalarSoft.ASProxy.Definitions
{
	public struct PluginInfo
	{
		public string ASProxyVersion;
		public string Name;
		public string Author;
		public string Version;
		public string Description;
		public bool UpdateEnabled;
		internal string UpdateInfoUrl;
		internal string ClassTypeName;

		static internal PluginInfo ReadFromXml(XmlNode rootNode)
		{
			PluginInfo result;
			// info
			XmlNode node = rootNode.SelectSingleNode("info");
			result.ASProxyVersion = node.Attributes["ASProxyVersion"].Value;
			result.Name = node.SelectSingleNode("Name").Value;
			result.Author = node.SelectSingleNode("Author").Value;
			result.Version = node.SelectSingleNode("Version").Value;
			result.Description = node.SelectSingleNode("Description").Value;

			// update
			node = rootNode.SelectSingleNode("update");
			result.UpdateEnabled = Convert.ToBoolean(node.Attributes["enabled"].Value);
			result.UpdateInfoUrl = node.Attributes["updateInfoUrl"].Value;

			// classType
			node = rootNode.SelectSingleNode("classType");
			result.ClassTypeName = node.Attributes["typeName"].Value;

			return result;
		}
	}
}

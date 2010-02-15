using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SalarSoft.ASProxy
{
	public class PluginInfo
	{
		public string ASProxyVersion;
		public string Name;
		public string Author;
		public string Version;
		public string Description;
		public string ConfigUiUrl;
		public bool UpdateEnabled;
		public bool Disabled;
		internal string UpdateInfoUrl;
		internal string ClassTypeName;

		static internal PluginInfo ReadFromXml(XmlNode rootNode)
		{
			PluginInfo result = new PluginInfo();
			// info
			XmlNode node = rootNode.SelectSingleNode("info");
			result.ASProxyVersion = node.Attributes["ASProxyVersion"].Value;
			result.Name = node.SelectSingleNode("Name").InnerText;
			result.Author = node.SelectSingleNode("Author").InnerText;
			result.Version = node.SelectSingleNode("Version").InnerText;
			result.Description = node.SelectSingleNode("Description").InnerText;
			result.ConfigUiUrl = node.SelectSingleNode("ConfigUiUrl").InnerText;

			// update
			node = rootNode.SelectSingleNode("update");
			result.UpdateEnabled = Convert.ToBoolean(node.Attributes["enabled"].Value);
			result.UpdateInfoUrl = node.Attributes["updateInfoUrl"].Value;

			// classType
			node = rootNode.SelectSingleNode("classType");
			result.ClassTypeName = node.Attributes["typeName"].Value;

			result.Disabled = false;

			return result;
		}
	}
}

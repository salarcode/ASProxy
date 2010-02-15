using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;

namespace SalarSoft.ASProxy
{
	public class ProviderInfo
	{
		public string ASProxyVersion;
		public string Name;
		public string Author;
		public string Version;
		public string Description;
		public string ConfigUiUrl;
		public bool UpdateEnabled;
		public bool Disabled;
		public bool Loaded;
		public Dictionary<ProviderType, string> Providers;
		internal string UpdateInfoUrl;

		static internal ProviderInfo ReadFromXml(XmlNode rootNode)
		{
			ProviderInfo result = new ProviderInfo();

			// info
			XmlNode infoNode = rootNode.SelectSingleNode("info");
			result.ASProxyVersion = infoNode.Attributes["ASProxyVersion"].Value;
			result.Name = infoNode.SelectSingleNode("Name").InnerText;
			result.Author = infoNode.SelectSingleNode("Author").InnerText;
			result.Version = infoNode.SelectSingleNode("Version").InnerText;
			result.Description = infoNode.SelectSingleNode("Description").InnerText;
			result.ConfigUiUrl = infoNode.SelectSingleNode("ConfigUiUrl").InnerText;

			// update
			XmlNode updateNode = rootNode.SelectSingleNode("update");
			result.UpdateEnabled = Convert.ToBoolean(updateNode.Attributes["enabled"].Value);
			result.UpdateInfoUrl = updateNode.Attributes["updateInfoUrl"].Value;

			// registred providers list
			result.Providers = new Dictionary<ProviderType, string>();

			XmlNode providersNode = rootNode.SelectSingleNode("providers");
			Type ProviderTypeEnum = typeof(ProviderType);
			foreach (XmlNode provider in providersNode.ChildNodes)
			{
				if (provider.Name != "add")
					continue;

				// Parse privider name
				string providerName = provider.Attributes["providerName"].Value;
				ProviderType type = (ProviderType)Enum.Parse(ProviderTypeEnum, providerName);

				string typeName = provider.Attributes["typeName"].Value;

				// add to registred list
				result.Providers.Add(type, typeName);
			}

			result.Disabled = false;
			result.Loaded = false;
			return result;
		}
	}

}

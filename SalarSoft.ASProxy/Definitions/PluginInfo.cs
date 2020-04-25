//**************************************************************************
// Product Name: ASProxy
// Description:  SalarSoft ASProxy is a free and open-source web proxy
// which allows the user to surf the net anonymously.
//
// MPL 1.1 License agreement.
// The contents of this file are subject to the Mozilla Public License
// Version 1.1 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS"
// basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
// License for the specific language governing rights and limitations
// under the License.
// 
// The Original Code is ASProxy (an ASP.NET Proxy).
// 
// The Initial Developer of the Original Code is Salar.K.
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.K https://github.com/salarcode (original author)
//
//**************************************************************************

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

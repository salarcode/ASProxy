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
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class JSProcessor : ExJSProcessor
	{
		#region variables
		UserOptions _UserOptions;
		#endregion

		#region properties
		#endregion

		#region public methods
		public JSProcessor()
		{
			_UserOptions = CurrentContext.UserOptions;
		}

		public override string Execute()
		{
			if (_UserOptions.RemoveScripts)
			{
				// no script is required
				return string.Empty;
			}
			else
			{
				Encoding _encoding;
				string resultCodes = StringStream.GetString(
					WebData.ResponseData,
					WebData.ResponseInfo.ContentType,
					_UserOptions.ForceEncoding,
					false,
					out _encoding);
				ContentEncoding = _encoding;

				IWebData webData = (IWebData)WebData;

				// Execute the result
				Execute(ref resultCodes,
					webData.ResponseInfo.ResponseUrl,
					null, null,
					webData.ResponseInfo.ResponseRootUrl);

				// the result
				return resultCodes;
			}
		}

		public override void Execute(ref string codes,
			string pageUrl,
			string pageUrlNoQuery,
			string pagePath,
			string rootUrl)
		{
			try
			{
				// 1- executing plugins
				if (Plugins.IsPluginAvailable(PluginHosts.IPluginJSProcessor))
					Plugins.CallPluginMethod(PluginHosts.IPluginJSProcessor,
						PluginMethods.IPluginJSProcessor.BeforeExecute,
						this, (object)codes, pageUrl, pageUrlNoQuery, pagePath, rootUrl);

				// BugFix: setting document.domain will cause error in javascript
				// Also checking the value of document.domain may cause bad behaviours
				// so both are going ot be disabled here
				JSReplacer.ReplacePropertyUsages(ref codes,
					"document.domain", Consts.ClientContent.JSEncoder_ASProxyLocationXDomain);


				// New location should be encoded!
				JSReplacer.AddEncoderMethodToPropertySet(ref codes,
					new string[] { "location.href", "window.location",
                                    "document.location", "top.location",
                                    "self.location", "parent.location" },
					Consts.ClientContent.JSEncoder_ASProxyEncoderMethodName);


				// Cookie set
				JSReplacer.AddEncoderMethodToPropertySet(ref codes,
					"document.cookie",
					Consts.ClientContent.JSEncoder_ASProxySetCookieMethodName);

				// Cookie get, Since v5.0
				JSReplacer.AddEncoderMethodToPropertyGet(ref codes,
					"document.cookie",
					Consts.ClientContent.JSEncoder_ASProxyGetCookieMethodName, true);


				// Replaces these properties by __XUrl
				// top.location , parent.location and self.location
				JSReplacer.AddEncoderMethodToPropertyGet(ref codes,
					new string[] { "top", "parent", "self" },
					new string[] { "location" },
					Consts.ClientContent.JSEncoder_ASProxyWindowLocOverrider,
					false);


				// document.URL is only getter and its string
				JSReplacer.AddEncoderMethodToPropertyGet(ref codes,
					new string[] { "document" },
					new string[] { "URL" },
					Consts.ClientContent.JSEncoder_ASProxyWindowLocHref,
					false);

				// fisrt test location with base objects
				JSReplacer.AddEncoderMethodToPropertyGet(ref codes,
					new string[] { "window", "document", "location" },
					new string[] { "location" },
					Consts.ClientContent.JSEncoder_ASProxyWindowLocOverrider,
					false);

				// then location attributes test 
				JSReplacer.AddEncoderMethodToPropertyGetFirstPart(ref codes,
					new string[] { "location" },
					new string[] { "href", "search", "hash", "host", "hostname", "pathname", "port", "protocol", "replace", "assign" },
					Consts.ClientContent.JSEncoder_ASProxyWindowLocOverrider,
					false);

				JSReplacer.AddEncoderMethodToMethodFirstParameter(ref codes,
					"location.replace",
					Consts.ClientContent.JSEncoder_ASProxyEncoderMethodName);

				// It is not common to use "open" method directly.
				// So i ignore to handle it here
				//JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "open", Consts.JSEncoder_ASProxyEncoderMethodName);

				// Don't proccess these
				// The "open" method will proccess in JavaScript encoder in "asproxyencoder.js" file.
				// Since v4.8
				//JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "window.open", Consts.JSEncoder_ASProxyEncoderMethodName);
				//JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "location.open", Consts.JSEncoder_ASProxyEncoderMethodName);


				// 2- executing plugins
				if (Plugins.IsPluginAvailable(PluginHosts.IPluginJSProcessor))
					Plugins.CallPluginMethod(PluginHosts.IPluginJSProcessor,
						PluginMethods.IPluginJSProcessor.AfterExecute,
						this, (object)codes, pageUrl, pageUrlNoQuery, pagePath, rootUrl);

			}
			catch (Exception ex)
			{
				// error logs
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, pageUrl);

				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
				LastErrorMessage = "ASProxy has some errors!";


				codes = "/* ASProxy has some errors! \n"
					+ ex.Message + " */"
					+ codes;
			}
		}
		#endregion
	}
}

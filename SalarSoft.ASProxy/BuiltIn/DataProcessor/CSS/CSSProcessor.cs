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
// The Initial Developer of the Original Code is Salar.Kh (SalarSoft).
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.Kh < salarsoftwares [@] gmail[.]com > (original author)
//
//**************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class CSSProcessor : ExCssProcessor
	{
		#region variables
		private UserOptions _UserOptions;
		#endregion

		#region public methods
		public CSSProcessor()
		{
			_UserOptions = CurrentContext.UserOptions;
		}

		public override string Execute()
		{
			Encoding _encoding;
			string resultCodes = StringStream.GetString(
				WebData.ResponseData,
				WebData.ResponseInfo.ContentType, 
				_UserOptions.ForceEncoding,
				false,
				out _encoding);
			ContentEncoding = _encoding;


			if (_UserOptions.Images)
			{

				// Page url. E.G. http://Site.com/users/profile.aspx?uid=90
				string pageUrl = WebData.ResponseInfo.ResponseUrl;

				// this is page path, used in processing relative paths in source html
				// for example the pageRootUrl for "http://Site.com/users/profile.aspx" will be "http://Site.com/users/"
				// gets page root Url
				string pagePath = UrlProvider.GetPagePath(pageUrl);

				// the page Url without any query parameter, used in processing relative query parameters
				// the pageUrlNoQuery for "http://Site.com/profile.aspx?uid=90" will be "http://Site.com/profile.aspx"
				// Gets page Url without any query parameter
				string pageUrlNoQuery = UrlProvider.GetPageAbsolutePath(pageUrl);


				// Execute the result
				Execute(ref resultCodes,
					pageUrl,
					pageUrlNoQuery,
					pagePath,
					WebData.ResponseInfo.ResponseRootUrl);
			}

			// the result
			return resultCodes;
		}

		public override void Execute(ref string codes, string pageUrl, string pageUrlNoQuery, string pagePath, string rootUrl)
		{
			try
			{
				// 1- executing plugins
				if (Plugins.IsPluginAvailable(PluginHosts.IPluginCSSProcessor))
					Plugins.CallPluginMethod(PluginHosts.IPluginCSSProcessor,
						PluginMethods.IPluginCSSProcessor.BeforeExecute,
						this, (object)codes, pageUrl, pageUrlNoQuery, pagePath, rootUrl);

				// ASProxy pages url formats generator
				ASProxyPagesFormat pages = new ASProxyPagesFormat(_UserOptions.EncodeUrl);

				// For @Import rule
				CSSReplacer.ReplaceCSSClassStyleUrl(ref codes,
					pageUrlNoQuery,
					pages.PageAnyType,
					pagePath,
					rootUrl,
					_UserOptions.EncodeUrl,
					true);

				// For backgrounds
				CSSReplacer.ReplaceCSSClassStyleUrl(ref codes,
					pageUrlNoQuery,
					pages.PageAnyType,
					pagePath,
					rootUrl,
					_UserOptions.EncodeUrl,
					false);


				// 2- executing plugins
				if (Plugins.IsPluginAvailable(PluginHosts.IPluginCSSProcessor))
					Plugins.CallPluginMethod(PluginHosts.IPluginCSSProcessor,
						PluginMethods.IPluginCSSProcessor.AfterExecute,
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

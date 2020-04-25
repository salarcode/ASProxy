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

namespace SalarSoft.ASProxy
{
	public class ASProxyPagesFormat
	{
		private bool _EncodeUrl;

		public ASProxyPagesFormat(bool encodeUrls)
		{
			_EncodeUrl = encodeUrls;
			GeneratePages();
		}

		#region Pages
		public string PageDefault;
		public string PageHtml;
		public string PageAnyType;
		public string PageJS;
		public string PageImage;
		public string PageDownload;
		//public string PageCss;
		//public string PageData;
		#endregion

		private void GeneratePages()
		{
			string pagesFormat = "{0}?" + Consts.Query.Decode + "=" + Convert.ToInt32(_EncodeUrl) + "&" +
								Consts.Query.UrlAddress + "={1}";

			PageDefault = string.Format(pagesFormat, Consts.FilesConsts.PageDefault_Dynamic, "{0}");
			PageHtml = string.Format(pagesFormat, Consts.FilesConsts.PageDirectHtml, "{0}");
			PageAnyType = string.Format(pagesFormat, Consts.FilesConsts.PageAnyType, "{0}");
			PageDownload = string.Format(pagesFormat, Consts.FilesConsts.PageDownload, "{0}");
			//PageData = string.Format(pagesFormat, Consts.FilesConsts.PageDirectData, "{0}");
			//PageCss = string.Format(pagesFormat, Consts.FilesConsts.PageDirectCSS, "{0}");
			PageImage = string.Format(pagesFormat, Consts.FilesConsts.PageImages, "{0}");
			PageJS = string.Format(pagesFormat, Consts.FilesConsts.PageDirectJS, "{0}");
		}
	}
}

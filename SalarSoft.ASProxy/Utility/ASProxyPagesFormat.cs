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

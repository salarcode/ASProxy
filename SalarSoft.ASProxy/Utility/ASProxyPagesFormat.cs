using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy
{
    public class ASProxyPagesFormat
    {
        private UserOptions _userOptions;

        public ASProxyPagesFormat(UserOptions userOptions)
        {
            _userOptions = userOptions;
            GeneratePages();
        }

        #region Pages
        public string PageCss;
        public string PageDefault;
        public string PageJS;
        public string PageHtml;
        public string PageAnyType;
        public string PageImage;
        public string PageData;
        public string PageDownload;
        #endregion

        private void GeneratePages()
        {
            string pagesFormat = "{0}?" + Consts.Query.Decode + "=" + Convert.ToInt32(_userOptions.EncodeUrl) + "&" +
                                Consts.Query.UrlAddress + "={1}";

            PageCss = string.Format(pagesFormat, Consts.FilesConsts.PageDirectCSS, "{0}");
            PageDefault = string.Format(pagesFormat, Consts.FilesConsts.PageDefault, "{0}");
            PageJS = string.Format(pagesFormat, Consts.FilesConsts.PageDirectJS, "{0}");
            PageHtml = string.Format(pagesFormat, Consts.FilesConsts.PageDirectHtml, "{0}");
            PageAnyType = string.Format(pagesFormat, Consts.FilesConsts.PageAnyType, "{0}");
            PageImage = string.Format(pagesFormat, Consts.FilesConsts.PageImages, "{0}");
            PageData = string.Format(pagesFormat, Consts.FilesConsts.PageDirectData, "{0}");
            PageDownload = string.Format(pagesFormat, Consts.FilesConsts.PageDownload, "{0}");

        }
    }
}

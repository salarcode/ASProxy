﻿using System;

namespace SalarSoft.ASProxy.Exposed
{

    /// <summary>
    /// Summary description for ExHtmlProcessor
    /// </summary>
    public abstract class ExHtmlProcessor : ExDataProcessor, IHtmlProcessor
    {
        #region variables
        private string _docType;
        private bool _isFrameSet;
        private string _pageTitle;
        #endregion

        #region properties
        public string DocType { get { return _docType; } set { _docType = value; } }
        public bool IsFrameSet { get { return _isFrameSet; } set { _isFrameSet = value; } }
        public string PageTitle { get { return _pageTitle; } set { _pageTitle = value; } }
        #endregion
    }
}
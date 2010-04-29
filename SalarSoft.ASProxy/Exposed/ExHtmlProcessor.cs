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
		public virtual string DocType { get { return _docType; } set { _docType = value; } }
		public virtual bool IsFrameSet { get { return _isFrameSet; } set { _isFrameSet = value; } }
		public virtual string PageTitle { get { return _pageTitle; } set { _pageTitle = value; } }
        #endregion
    }
}
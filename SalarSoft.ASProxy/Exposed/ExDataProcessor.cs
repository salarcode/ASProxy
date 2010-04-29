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
using System.Data;
using System.Configuration;
using System.Web;
using System.Text;

namespace SalarSoft.ASProxy.Exposed
{
	/// <summary>
	/// Summary description for ExDataProcessor
	/// </summary>
	public abstract class ExDataProcessor : IDataProcessor
	{
		#region variables
		private IWebData _webData;
		private Encoding _contentEncoding;
		private string _extraCodesForPage;
		private string _extraCodesForBody;
		private LastStatus _lastStatus;
		private Exception _lastException;
		private string _lastErrorMessage;
		#endregion

		#region properties
		public virtual IWebData WebData { get { return _webData; } set { _webData = value; } }
		public virtual Encoding ContentEncoding { get { return _contentEncoding; } set { _contentEncoding = value; } }
		public virtual string ExtraCodesForPage { get { return _extraCodesForPage; } set { _extraCodesForPage = value; } }
		public virtual string ExtraCodesForBody { get { return _extraCodesForBody; } set { _extraCodesForBody = value; } }

		public virtual LastStatus LastStatus { get { return _lastStatus; } set { _lastStatus = value; } }
		public virtual Exception LastException { get { return _lastException; } set { _lastException = value; } }
		public virtual string LastErrorMessage { get { return _lastErrorMessage; } set { _lastErrorMessage = value; } }
		#endregion

		#region public methods
		public abstract string Execute();
		public abstract void Execute(ref string codes, string pageUrl, string pageUrlNoQuery,string pagePath, string rootUrl);
		#endregion

	}
}
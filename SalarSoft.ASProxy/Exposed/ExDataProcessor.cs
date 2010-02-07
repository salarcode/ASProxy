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
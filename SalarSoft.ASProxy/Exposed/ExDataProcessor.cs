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
		private UserOptions _userOptions;
		private Encoding _contentEncoding;
		private string _pageInitializerCodes;
		private LastStatus _lastStatus;
		private Exception _lastException;
		private string _lastErrorMessage;
		#endregion

		#region properties
		public IWebData WebData { get { return _webData; } set { _webData = value; } }
		public UserOptions UserOptions { get { return _userOptions; } set { _userOptions = value; } }
		public Encoding ContentEncoding { get { return _contentEncoding; } set { _contentEncoding = value; } }
		public string PageInitializerCodes { get { return _pageInitializerCodes; } set { _pageInitializerCodes = value; } }
		public LastStatus LastStatus { get { return _lastStatus; } set { _lastStatus = value; } }
		public Exception LastException { get { return _lastException; } set { _lastException = value; } }
		public string LastErrorMessage { get { return _lastErrorMessage; } set { _lastErrorMessage = value; } }
		#endregion

		#region public methods
		public abstract string Execute();
		public abstract void Execute(ref string codes, string pageUrl, string rootUrl);
		#endregion

	}
}
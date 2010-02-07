using System;
using System.Web;
using SalarSoft.ASProxy;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Summary description for ExEngine
    /// </summary>
    public abstract class ExEngine : IEngine
    {
        #region variables
        protected IWebData _webData;
        private DataTypeToProcess _dataTypeToProcess;
        private bool _applyContentFileName;
        private LastStatus _lastStatus;
        private Exception _lastException;
        private string _lastErrorMessage;
        protected UserOptions _userOptions;
        protected EngineRequestInfo _requestInfo;
        protected EngineResponseInfo _responseInfo;
        #endregion

        #region properties
		public virtual IWebData WebData
        {
            get { return _webData; }
            set { _webData = value; }
        }

		public virtual EngineRequestInfo RequestInfo
        {
            get { return _requestInfo; }
            set { _requestInfo = value; }
        }
		public virtual EngineResponseInfo ResponseInfo
        {
            get { return _responseInfo; }
            set { _responseInfo = value; }
        }
		public virtual DataTypeToProcess DataTypeToProcess
        {
            get { return _dataTypeToProcess; }
            set { _dataTypeToProcess = value; }
        }

		public virtual bool ApplyContentFileName
        {
            get { return _applyContentFileName; }
            set { _applyContentFileName = value; }
        }

		public virtual LastStatus LastStatus
        {
            get { return _lastStatus; }
            set { _lastStatus = value; }
        }

		public virtual Exception LastException
        {
            get { return _lastException; }
            set { _lastException = value; }
        }

		public virtual string LastErrorMessage
        {
            get { return _lastErrorMessage; }
            set { _lastErrorMessage = value; }
        }
		public virtual UserOptions UserOptions
        {
            get { return _userOptions; }
            set { _userOptions = value; }
        }
        #endregion

        #region public methods
        public void Dispose()
        {
			if (_webData != null)
				_webData.Dispose();
			_webData = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void Initialize(HttpRequest httpRequest);

        /// <summary>
        /// 
        /// </summary>
        public abstract void Initialize(string requestUrl);

        /// <summary>
        /// 
        /// </summary>
        public abstract void ExecuteHandshake();

        /// <summary>
        /// 
        /// </summary>
        public abstract string ExecuteToString();

        /// <summary>
        /// 
        /// </summary>
        public abstract void ExecuteToResponse(HttpResponse httpResponse);
        
        /// <summary>
        /// 
        /// </summary>
        public abstract void ExecuteToStream(Stream stream);
        #endregion

        #region static methods
        #endregion

        #region protected methods
		protected virtual void Redirect(string url, HttpStatusCode httpStatusCode)
        {
            try
            {
                HttpResponse response = HttpContext.Current.Response;
                response.StatusCode = (int)httpStatusCode;
                response.RedirectLocation = url;
                response.End();
            }
            catch (ThreadAbortException) { }
        }

		protected virtual void Redirect(string url, HttpStatusCode httpStatusCode, bool abortRequest)
        {
            try
            {
                HttpResponse response = HttpContext.Current.Response;
                response.StatusCode = (int)httpStatusCode;
                response.RedirectLocation = url;

                if (abortRequest)
                    response.End();
                else
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (ThreadAbortException) { }
        }
        #endregion

        #region private methods
        #endregion

        #region Type defination
        #endregion
    }
}
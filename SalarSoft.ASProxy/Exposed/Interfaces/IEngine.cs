using System;
using System.Data;
using System.Configuration;
using System.Web;
using SalarSoft.ASProxy;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Generalize the Engine
    /// </summary>
    public interface IEngine : IDisposable, IExeptionHandled
    {
        ///<summary>
        /// Type of data to is requested to be processed.
        ///</summary>
        DataTypeToProcess DataTypeToProcess { get; set; }

        /// <summary>
        /// User defined options
        /// </summary>
        UserOptions UserOptions { get; set; }

        EngineRequestInfo RequestInfo { get; set; }

        EngineResponseInfo ResponseInfo { get; set; }
		IWebData WebData { get; set; }


        void Initialize(HttpRequest httpRequest);

        void Initialize(string requestUrl);

        void ExecuteHandshake();

        string ExecuteToString();

        void ExecuteToResponse(HttpResponse httpResponse);

        /// <summary>
        /// 
        /// </summary>
        void ExecuteToStream(System.IO.Stream stream);

    }
}
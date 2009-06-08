using System;
using System.Data;
using System.Configuration;
using System.Web;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Generalize the WebData
    /// </summary>
    public interface IWebData: IDisposable,IExeptionHandled
    {
        /// <summary>
        /// Response information
        /// </summary>
        WebDataResponseInfo ResponseInfo { get; set; }
        /// <summary>
        /// Request information
        /// </summary>
        WebDataRequestInfo RequestInfo { get; set; }
        /// <summary>
        /// Confirms the request and gets the data from the web.
        /// </summary>
        void Execute();
        System.IO.Stream ResponseData { get; set; }
    }
}
using System;
using System.Web;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Generalize the HandledExeption
    /// </summary>
    public interface IExeptionHandled
    {
        /// <summary>
        /// Last activity status
        /// </summary>
        LastStatus LastStatus { get; set; }
        
        ///<summary>
        ///
        ///</summary>
        Exception LastException { get; set; }

        ///<summary>
        ///
        ///</summary>
        string LastErrorMessage { get; set; }

    }
}
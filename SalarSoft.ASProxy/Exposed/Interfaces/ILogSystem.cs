using System;
using System.Web;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Generalize the LogSystem
    /// </summary>
    public interface ILogSystem
    {
        /// <summary>
        /// Enable/Disable the error log system
        /// </summary>
        bool ErrorLogEnabled { get; }

        /// <summary>
        /// Enable/Disable the activity log system
        /// </summary>
        bool ActivityLogEnabled { get; }

        /// <summary>
        /// Logs specified items in specified log entity
        /// </summary>
        void Log(LogEntity entity, params object[] optionalData);

        /// <summary>
        /// Logs http request, requested url and specified items
        /// </summary>
        void Log(LogEntity entity, HttpRequest request, string requestedUrl, params object[] optionalData);

        void Log(LogEntity entity, string requestedUrl, params object[] optionalData);

		void LogError(Exception ex, string requestedUrl, params object[] optionalData);

		void LogError(Exception ex, string message, string requestedUrl, params object[] optionalData);
        
        void LogError(Exception ex, HttpRequest request, string message, string requestedUrl, params object[] optionalData);
        
        void LogError(string message, string requestedUrl, params object[] optionalData);
        
        void LogError(string message, params object[] optionalData);
    }
}
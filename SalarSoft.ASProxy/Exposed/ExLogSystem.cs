using System;
using System.Web;
using System.IO;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Summary description for ExLogSystem
    /// </summary>
    public abstract class ExLogSystem : ILogSystem
    {
        #region properties
        public abstract bool ActivityLogEnabled { get; }
        public abstract bool ErrorLogEnabled { get; }
        #endregion

        #region public methods
        public abstract void Log(LogEntity entity, params object[] optionalData);
        public abstract void Log(LogEntity entity, string requestedUrl, params object[] optionalData);
        public abstract void Log(LogEntity entity, HttpRequest request, string requestedUrl, params object[] optionalData);
        public abstract void LogError(Exception ex, string requestedUrl, params object[] optionalData);
		public abstract void LogError(Exception ex, string message, string requestedUrl, params object[] optionalData);
        public abstract void LogError(Exception ex, HttpRequest request, string message, string requestedUrl, params object[] optionalData);
        public abstract void LogError(string message, string requestedUrl, params object[] optionalData);
        public abstract void LogError(string message, params object[] optionalData);
        #endregion
	}
}
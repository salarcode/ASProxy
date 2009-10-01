using System;
using System.Web;
using System.IO;
using System.Text;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy.BuiltIn
{
	/// <summary>
	/// ASproxy built-in log-system
	/// </summary>
	public class LogSystem : ExLogSystem
	{
		protected const string _strEntityFormat = "\r\n<Entry type=\"{0}\">\r\n{1}</Entry>\r\n";
		protected const string _strUrlFormat = "<RequestedUrl>{0}</RequestedUrl>\r\n";
		protected const string _strASProxyRequestUrl = "<ASProxyRequestUrl>{0}</ASProxyRequestUrl>\r\n";
		protected const string _strIPFormat = "<IP>{0}</IP>\r\n";
		protected const string _strDataFormat = "<Param>{0}</Param>\r\n";
		protected const string _strDateTimeFormat = "<Date>{0}</Date>\r\n";
		protected const string _strMessage = "<Message>{0}</Message>\r\n";
		protected const string _strError = "<Error>{0}</Error>\r\n";

		#region variables
		protected string _activityLogs_FileFormat;
		protected string _errorLog_FileFormat;
		protected string _activityLogs_LastFileName;
		protected string _errorLog_LastFileName;
		private bool _isPluginAvailable;
		#endregion

		#region properties
		/// <summary>
		/// Enable/Disable the log system
		/// </summary>
		public override bool ActivityLogEnabled
		{
			get { return Configurations.LogSystem.ActivityLog_Enabled; }
		}

		/// <summary>
		/// Enable/Disable the log system
		/// </summary>
		public override bool ErrorLogEnabled
		{
			get { return Configurations.LogSystem.ErrorLog_Enabled; }
		}
		#endregion

		#region public methods
		public LogSystem()
		{
			HttpContext context = HttpContext.Current;
			if (Configurations.LogSystem.ActivityLog_Enabled)
			{
				try
				{
					_activityLogs_FileFormat = context.Server.MapPath(Configurations.LogSystem.ActivityLog_Location);

					string path = Path.GetDirectoryName(_activityLogs_FileFormat);
					System.IO.Directory.CreateDirectory(path);
				}
				catch (Exception)
				{
					// call stack overflow!
					//Configurations.LogSystem.ActivityLog_Enabled = false;
				}
			}

			if (Configurations.LogSystem.ErrorLog_Enabled)
			{
				try
				{
					_errorLog_FileFormat = context.Server.MapPath(Configurations.LogSystem.ErrorLog_location);

					string path = Path.GetDirectoryName(_errorLog_FileFormat);
					System.IO.Directory.CreateDirectory(path);
				}
				catch (Exception)
				{
					// call stack overflow!
					//Configurations.LogSystem.ErrorLog_Enabled = false;
				}
			}

			// getting plugin availablity state
			_isPluginAvailable = Plugins.IsPluginAvailable(PluginHosts.IPluginLogSystem);
		}

		/// <summary>
		/// Logs specified items in specified log entity
		/// </summary>
		public override void Log(LogEntity entity, params object[] optionalData)
		{
			if (ActivityLogEnabled == false)
				return;
			if (entity == LogEntity.ImageRequested && Configurations.LogSystem.ActivityLog_Images == false)
				return;

			// executing plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginLogSystem,
					PluginMethods.IPluginLogSystem.BeforeLog,
					this, entity, optionalData);


			StringBuilder builder = new StringBuilder();
			builder.AppendFormat(_strIPFormat, HttpContext.Current.Request.UserHostAddress);
			builder.AppendFormat(_strDateTimeFormat, DateTime.Now.ToString());

			for (int i = 0; i < optionalData.Length; i++)
			{
				builder.AppendFormat(_strDataFormat, optionalData[i]);
			}

			string result = string.Format(_strEntityFormat, entity, builder.ToString());

			try
			{
				SaveToActivityFile(result);
			}
			catch { }
		}

		public override void Log(LogEntity entity, string requestedUrl, params object[] optionalData)
		{
			Log(entity, null, requestedUrl, optionalData);
		}

		/// <summary>
		/// Logs http request, requested url and specified items
		/// </summary>
		public override void Log(LogEntity entity, HttpRequest request, string requestedUrl, params object[] optionalData)
		{
			if (ActivityLogEnabled == false)
				return;
			if (entity == LogEntity.ImageRequested && Configurations.LogSystem.ActivityLog_Images == false)
				return;

			// executing plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginLogSystem,
					PluginMethods.IPluginLogSystem.BeforeLog,
					this, request, requestedUrl, optionalData);


			StringBuilder builder = new StringBuilder();
			builder.AppendFormat(_strUrlFormat, requestedUrl);

			if (request != null)
				builder.AppendFormat(_strIPFormat, request.UserHostAddress);
			builder.AppendFormat(_strDateTimeFormat, DateTime.Now.ToString());

			for (int i = 0; i < optionalData.Length; i++)
			{
				builder.AppendFormat(_strDataFormat, optionalData[i]);
			}

			string result = string.Format(_strEntityFormat, entity, builder.ToString());

			try
			{
				SaveToActivityFile(result);
			}
			catch { }
		}

		protected virtual void SaveToActivityFile(string data)
		{
			string fileFormat = Configurations.LogSystem.FileFormat;
			fileFormat = DateTime.Now.ToString(fileFormat);
			string fileName = string.Format(_activityLogs_FileFormat, fileFormat);
			long maxSize = Configurations.LogSystem.MaxFileSize * 1024;

			if (string.IsNullOrEmpty(_activityLogs_LastFileName))
			{
				_activityLogs_LastFileName = fileName;
			}

			FileInfo info = new FileInfo(_activityLogs_LastFileName);
			if (info.Exists && info.Length > maxSize)
			{
				int i = 1;
				do
				{
					fileName = string.Format(_activityLogs_FileFormat, fileFormat + "_" + i);
					info = new FileInfo(fileName);

					i++;
				}
				while (info.Exists && info.Length > maxSize);
			}


			try
			{
				File.AppendAllText(fileName, data);
			}
			catch { }
		}

		protected virtual void SaveToErrorFile(string data)
		{
			string fileFormat = Configurations.LogSystem.FileFormat;
			fileFormat = DateTime.Now.ToString(fileFormat);
			string fileName = string.Format(_errorLog_FileFormat, fileFormat);
			long maxSize = Configurations.LogSystem.MaxFileSize * 1024;

			if (string.IsNullOrEmpty(_errorLog_LastFileName))
			{
				_errorLog_LastFileName = fileName;
			}

			FileInfo info = new FileInfo(_errorLog_LastFileName);
			if (info.Exists && info.Length > maxSize)
			{
				int i = 1;
				do
				{
					fileName = string.Format(_errorLog_FileFormat, fileFormat + "_" + i);
					info = new FileInfo(fileName);

					i++;
				}
				while (info.Exists && info.Length > maxSize);
			}


			try
			{
				File.AppendAllText(fileName, data);
			}
			catch { }
		}


		/// <summary>
		/// Logs exception, request host, requested url and specified items
		/// </summary>
		public override void LogError(Exception ex, HttpRequest request, string message, string requestedUrl, params object[] optionalData)
		{
			if (ErrorLogEnabled == false)
				return;

			// executing plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginLogSystem,
					PluginMethods.IPluginLogSystem.BeforeLogError,
					this, ex, request, message, requestedUrl, optionalData);

			StringBuilder builder = new StringBuilder();
			if (!string.IsNullOrEmpty(requestedUrl))
				builder.AppendFormat(_strUrlFormat, requestedUrl);

			if (request != null)
			{
				builder.AppendFormat(_strIPFormat, request.UserHostAddress);
				builder.AppendFormat(_strASProxyRequestUrl, request.Url.PathAndQuery);
			}

			if (!string.IsNullOrEmpty(message))
				builder.AppendFormat(_strMessage, message);

			builder.AppendFormat(_strDateTimeFormat, DateTime.Now.ToString());
			if (ex != null)
				builder.AppendFormat(_strError, ex.ToString());

			for (int i = 0; i < optionalData.Length; i++)
			{
				builder.AppendFormat(_strDataFormat, optionalData[i]);
			}

			string result = string.Format(_strEntityFormat, LogEntity.Error, builder.ToString());

			try
			{
				SaveToErrorFile(result);
			}
			catch { }
		}

		public override void LogError(Exception ex, string message, string requestedUrl, params object[] optionalData)
		{
			LogError(ex, null, message, requestedUrl, optionalData);
		}

		/// <summary>
		/// Logs exception, requested url and specified items
		/// </summary>
		public override void LogError(Exception ex, string requestedUrl, params object[] optionalData)
		{
			LogError(ex, null, null, requestedUrl, optionalData);
		}

		/// <summary>
		/// Logs exception message, requested url and specified items
		/// </summary>
		public override void LogError(string message, string requestedUrl, params object[] optionalData)
		{
			LogError(null, null, message, requestedUrl, optionalData);
		}

		/// <summary>
		/// Logs exception message and specified items
		/// </summary>
		public override void LogError(string message, params object[] optionalData)
		{
			LogError(null, null, message, null, optionalData);
		}

		#endregion


	}
}
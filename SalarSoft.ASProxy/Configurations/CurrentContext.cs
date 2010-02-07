using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy
{
	public class CurrentContext
	{

		/// <summary>
		/// Current request user options
		/// </summary>
		public static UserOptions UserOptions
		{
			get
			{
				object context = HttpContext.Current.Items[Consts.General.ContextUserOptionsKey];
				UserOptions result;
				if (context == null)
				{
					result = UserOptions.ReadFromRequest();
					HttpContext.Current.Items[Consts.General.ContextUserOptionsKey] = result;
				}
				else
				{
					result = (UserOptions)context;
					if (result.LoadedFromSource == false)
					{
						result = UserOptions.ReadFromRequest();
						HttpContext.Current.Items[Consts.General.ContextUserOptionsKey] = result;
					}
				}
				return result;
			}
			set
			{
				HttpContext.Current.Items[Consts.General.ContextUserOptionsKey] = value;
			}
		}


		/// <summary>
		/// Generate full physical path of a file or page.
		/// </summary>
		/// <param name="path">File name or page name</param>
		/// <returns>Full physical path</returns>
		public static string MapAppPath(string path)
		{
			if (path == "" || path == null)
				return path;
			string apppath = AppPhysicalPath;
			string result = path;
			if (result[0] == '\\' || result[0] == '/')
				result = result.Remove(0, 1);
			if (apppath[apppath.Length - 1] != '\\')
				result = apppath + "\\" + path;
			else
				result = apppath + path;
			return result;
		}

		/// <summary>
		/// Current application phycical path.
		/// </summary>
		public static string AppPhysicalPath
		{
			get { return HttpRuntime.AppDomainAppPath; }
		}

		/// <summary>
		/// Generate full virtual path of a page or url file address.
		/// </summary>
		/// <param name="path">Page name or url file</param>
		/// <returns>Full virtual path. ex: /SalarSoft/topics/articles.aspx</returns>
		/// <example>
		/// In Local ApplicationPath is: localhost
		/// <br />
		/// In server ApplicationPath is: /
		/// </example>
		public static string MapAppVPath(string path)
		{
			string result = path;
			string apppath = AppVirtualPath;

			if (result[0] != '/' && result[0] != '\\')
				result = '/' + result;

			if (apppath.Length > 1)
				result = apppath + result;

			if (result[0] != '/' && result[0] != '\\')
				result = '/' + result;
			return result;
		}

		/// <summary>
		/// Current application virtual path.
		/// </summary>
		public static string AppVirtualPath
		{
			get { return HttpRuntime.AppDomainAppVirtualPath; }
		}

	}
}

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Text;

namespace SalarSoft.ASProxy.Exposed
{
	/// <summary>
	/// Generalize the DataProcessor
	/// </summary>
	public interface IDataProcessor : IExeptionHandled
	{
		/// <summary>
		/// Extra codes that will be trasfered before the content
		/// </summary>
		string ExtraCodesForPage { get; set; }

		/// <summary>
		/// For html contents, extra codes which should run in the body of page
		/// </summary>
		string ExtraCodesForBody { get; set; }
		Encoding ContentEncoding { get; set; }
		IWebData WebData { get; set; }

		string Execute();

		void Execute(ref string codes, string pageUrl, string pageUrlNoQuery ,string pagePath, string rootUrl);
	}
}
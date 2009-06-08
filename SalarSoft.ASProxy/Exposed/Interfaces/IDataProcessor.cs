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
		string PageInitializerCodes { get; set; }
		Encoding ContentEncoding { get; set; }
		UserOptions UserOptions { get; set; }
		IWebData WebData { get; set; }

		string Execute();

		void Execute(ref string codes, string pageUrl, string rootUrl);

	}
}
using SalarSoft.ASProxy.Exposed;
using System.Web;
using SalarSoft.ASProxy;
using System.Net;
using MaxMind;

namespace ASProxy.Plugin.CountryBlocker
{
	public class BlockerPluginEngine : IPluginEngine
	{
		public void AfterInitialize(IEngine engine)
		{
			HttpContext context = HttpContext.Current;
			if (context != null)
			{
				string userIP = context.Request.UserHostAddress;

				if (BlockedCountries.IsIpBlocked(userIP))
				{
					throw new EPluginStopRequest((int)HttpStatusCode.Forbidden,
						"Sorry, the administrator has blocked your country access.");
				}
			}
		}

		public void BeforeHandshake(IEngine engine, IWebData webData)
		{
			//SalarSoft.ASProxy.UrlProvider.CorrectInputUrl
		}

		public void AfterHandshake(IEngine engine, IWebData webData)
		{
		}

		public void BeforeProcessor(IEngine engine, IDataProcessor dataProcessor)
		{
		}

		public void AfterProcessor(IEngine engine)
		{
		}

		public void AfterExecuteToResponse(IEngine engine, HttpResponse httpResponse)
		{
		}
	}
}

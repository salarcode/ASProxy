<%@ WebHandler Language="C#" Class="parser" %>

using System;
using System.Web;
using SalarSoft.ASProxy;
using SalarSoft.ASProxy.Exposed;

public class parser : IHttpHandler
{
	private enum ParseRequest { JavaScript, Html }
	private string _requestUrl = "";
	private ParseRequest _parseType;
	private string _requestCodes;

	public void ProcessRequest(HttpContext context)
	{
		// Read the request info
		IntializeRequestInfo(context.Request);
		if (string.IsNullOrEmpty(_requestUrl) || string.IsNullOrEmpty(_requestCodes))
		{
			context.Response.Write("");
			return;
		}

		ProccessRequest(context.Response);
	}

	void ProccessRequest(HttpResponse httpResponse)
	{
		// this is page path, used in processing relative paths in source html
		// for example the pageRootUrl for "http://Site.com/users/profile.aspx" will be "http://Site.com/users/"
		// gets page root Url
		string pagePath = UrlProvider.GetPagePath(_requestUrl);

		// the page Url without any query parameter, used in processing relative query parameters
		// the pageUrlNoQuery for "http://Site.com/profile.aspx?uid=90" will be "http://Site.com/profile.aspx"
		// Gets page Url without any query parameter
		string pageUrlNoQuery = UrlProvider.GetPageAbsolutePath(_requestUrl);

		// http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/
		string rootUrl = UrlProvider.GetRootPath(_requestUrl);

		IDataProcessor dataProcessor = null;

		switch (_parseType)
		{
			case ParseRequest.JavaScript:
				dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.IJSProcessor);
				break;
			case ParseRequest.Html:
				dataProcessor = (IDataProcessor)Providers.GetProvider(ProviderType.IHtmlProcessor);
				break;
			default:
				return;
		}

		// execute the proccessor		
		dataProcessor.Execute(ref _requestCodes, _requestUrl, pageUrlNoQuery, pagePath, rootUrl);
		
		// set the response parameters
		httpResponse.ContentType = "text/plain";
		
		// write the response
		httpResponse.Write(_requestCodes);
	}

	void IntializeRequestInfo(HttpRequest httpRequest)
	{
		bool tmpBool = false;
		_requestCodes = StringStream.GetString(httpRequest.InputStream);

		// Get requested url
		_requestUrl = httpRequest.QueryString[Consts.Query.UrlAddress];

		// if url is provided
		if (!string.IsNullOrEmpty(_requestUrl))
		{
			string decode = httpRequest.QueryString[Consts.Query.Decode];
			if (!string.IsNullOrEmpty(decode))
			{
				try
				{
					tmpBool = Convert.ToBoolean(Convert.ToInt32(decode));
				}
				catch
				{
					tmpBool = false;
				}
			}

			// If url is encoded, decode it
			if (tmpBool)
				_requestUrl = UrlProvider.DecodeUrl(_requestUrl);
		}


		// Get request post method state
		string query;
		if (UrlProvider.GetRequestQuery(httpRequest.QueryString, "type", out query))
		{
			query = query.Trim().ToLower();
			if (query == "js")
				_parseType = ParseRequest.JavaScript;
			else
				_parseType = ParseRequest.Html;
		}
	}

	public bool IsReusable
	{
		get
		{
			return false;
		}
	}

}
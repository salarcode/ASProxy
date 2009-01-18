using System;
namespace SalarSoft.ASProxy
{
	/// <summary>
	/// Range of a string in main text
	/// </summary>
	public struct TextRange
	{
		public int Start;
		public int End;
		public TextRange(int start, int end)
		{
			Start = start;
			End = end;
		}
		public TextRange(int start)
		{
			Start = start;
			End = -1;
		}
		public bool IsEqual(TextRange txtRange)
		{
			return (txtRange.End == End && txtRange.Start == Start);
		}
	}

	/// <summary>
	/// Type of referrer used in ASProxy
	/// </summary>
	public enum ReferrerType
	{
		None,
		ASProxySite,
		Referrer,
		RequesterAsReferrer
	}


	public enum AutoRedirectType
	{
		/// <summary>
		/// Redirects to another page of current site
		/// </summary>
		RequestInternal,

		/// <summary>
		/// Redirects to out of currect requested site
		/// </summary>
		External,

		/// <summary>
		/// Redirects to ASProxy's another page
		/// </summary>
		ASProxyPages
	}

	/// <summary>
	/// Last activity state
	/// </summary>
	public enum LastActivityStatus
	{
		Normal,
		Error,
		NormalErrorPage
	}

	public enum MimeContentType
	{
		text_html,
		text_plain,
		text_css,
		text_javascript,
		image_jpeg,
		image_gif,
		application
	}

}

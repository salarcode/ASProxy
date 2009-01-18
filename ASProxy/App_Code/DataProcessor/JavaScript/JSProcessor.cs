using System;
using System.Text;

namespace SalarSoft.ASProxy
{
	/// <summary>
	/// This class will proccess the javascript codes
	/// </summary>
	public class JSProcessor
	{
		private WebDataCore fData;
		public OptionsType Options = OptionsType.GetDefault(true);
		private Encoding fResponsePageEncoding = Encoding.UTF8;


		public JSProcessor(WebDataCore urlData)
		{
			fData = urlData;
		}

		public Encoding ResponsePageEncoding
		{
			get { return fResponsePageEncoding; }
		}


		/// <summary>
		/// Process the codes
		/// </summary>
		/// <returns>Processed codes</returns>
		public string Execute()
		{
			string html = Processors.GetString(fData, Options.IgnorePageEncoding, out fResponsePageEncoding);
			return Execute(html);
		}

		/// <summary>
		/// Process the codes
		/// </summary>
		/// <returns>Processed codes</returns>
		public string Execute(string html)
		{
			try
			{
				if (Options.Scripts)
				{
					JSReplacer.AddEncoderMethodToPropertySet(ref html, "location.href", Consts.JSEncoder_ASProxyEncoderMethodName);

					JSReplacer.AddEncoderMethodToPropertySet(ref html, "window.location", Consts.JSEncoder_ASProxyEncoderMethodName);
					JSReplacer.AddEncoderMethodToPropertySet(ref html, "document.location", Consts.JSEncoder_ASProxyEncoderMethodName);

					JSReplacer.AddEncoderMethodToPropertySet(ref html, "document.cookie", Consts.JSEncoder_ASProxySetCookieMethodName);

					// Cookie get, Since v5.0
					JSReplacer.AddEncoderMethodToPropertyGet(ref html, "document.cookie", Consts.JSEncoder_ASProxyGetCookieMethodName);

					JSReplacer.AddEncoderMethodToPropertyGet(ref html, new string[] { "window", "document", "location" }
						, new string[] {"location","URL"}, Consts.JSEncoder_ASProxyWindowLocOverrider, false);

					JSReplacer.AddEncoderMethodToPropertyGetFirstPart(ref html, new string[] { "location" }
						, new string[] { "href", "search", "hash", "host", "hostname", "pathname", "port", "protocol", "replace", "assign" }, Consts.JSEncoder_ASProxyWindowLocOverrider, false);

					JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "location.replace", Consts.JSEncoder_ASProxyEncoderMethodName);

					// It is not common to use "open" method directly.
					// So i ignore to handle it here
					//JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "open", Consts.JSEncoder_ASProxyEncoderMethodName);

					// Don't proccess these
					// The "open" method will proccess in JavaScript encoder in "asproxyencoder.js" file.
					// Since v4.8
					//JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "window.open", Consts.JSEncoder_ASProxyEncoderMethodName);
					//JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "location.open", Consts.JSEncoder_ASProxyEncoderMethodName);
				}

				return html;
			}
			catch (Exception err)
			{
				ASProxyExceptions.LogException(err);

				return "/* ASProxy has some errors! \n" + err.Message + " */"
					+ html;
			}
		}
	}
}

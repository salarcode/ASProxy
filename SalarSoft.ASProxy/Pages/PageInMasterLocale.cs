using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace SalarSoft.ASProxy
{
	public class PageInMasterLocale : Page
	{
		protected override void InitializeCulture()
		{
			// Page UI
			System.Threading.Thread.CurrentThread.CurrentUICulture = Configurations.Pages.GetUiLanguage();

			base.InitializeCulture();
		}
	}
}
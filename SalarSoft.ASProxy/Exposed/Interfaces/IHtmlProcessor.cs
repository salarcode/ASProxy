using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Exposed
{
    public interface IHtmlProcessor : IDataProcessor
    {
        string PageTitle { get; set; }
        bool IsFrameSet { get; set; }
        string DocType { get; set; }
    }
}

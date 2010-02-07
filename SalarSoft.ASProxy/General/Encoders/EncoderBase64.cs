using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Encoders
{
    public class EncoderBase64 : Encoder3To4
    {
        public EncoderBase64()
        {
            _CodingTable = Base64.CodeTable;
            _FillChar = '=';
        }
    }

}

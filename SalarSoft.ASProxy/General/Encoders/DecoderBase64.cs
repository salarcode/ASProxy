using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Encoders
{
    public class DecoderBase64 : Decoder3To4
    {
        public DecoderBase64()
        {
            _DecodeTable = Base64.DecodeTable;
            _CodingTable = Base64.CodeTable;
            _FillChar = '=';
        }
    }

}

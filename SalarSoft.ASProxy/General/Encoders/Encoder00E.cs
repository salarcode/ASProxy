using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SalarSoft.ASProxy.Encoders
{
    public class Encoder00E : Encoder3To4
    {
        public override string Encode(Stream ASrcStream, int ABytes)
        {
            int LStart = (int)ASrcStream.Position;
            string TempResult = base.Encode(ASrcStream, ABytes);
            return _CodingTable[(int)ASrcStream.Position - LStart + 1] + TempResult;
        }
    }

}

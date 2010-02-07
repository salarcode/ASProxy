using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Encoders
{
    public sealed class XXE
    {
        private XXE()
        {
        }

        static XXE()
        {
            Decoder00E.ConstructDecodeTable(CodeTable, out DecodeTable);
        }

        public const string CodeTable = "+-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        public static readonly byte[] DecodeTable;
    }

}

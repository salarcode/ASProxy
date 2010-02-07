using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy.Encoders
{
    public sealed class ASProxyBase64
    {
        private ASProxyBase64()
        {
        }

        static ASProxyBase64()
        {
            DecodeTable = new byte[CodeTable.Length * 2];
            Decoder3To4.ConstructDecodeTable(CodeTable, out DecodeTable);

        }

        //public const string CodeTable = "+-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        public const string CodeTable = "abcdefghijkml012345nopqrstuvwxyz6789ABCDEFGHIJKLMNOPQRSTUVWXYZ+/";
        public static readonly byte[] DecodeTable;// = new byte[128];
    }

    public class ASProxyDecoderBase64 : Decoder3To4
    {
        public ASProxyDecoderBase64()
        {
            _DecodeTable = ASProxyBase64.DecodeTable;
            _CodingTable = ASProxyBase64.CodeTable;
            _FillChar = '=';
        }
    }

    public class ASProxyEncoderBase64 : Encoder3To4
    {
        public ASProxyEncoderBase64()
        {
            _CodingTable = ASProxyBase64.CodeTable;
            _FillChar = '=';
        }
    }
}

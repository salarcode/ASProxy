//**************************************************************************
// Product Name: ASProxy
// Description:  SalarSoft ASProxy is a free and open-source web proxy
// which allows the user to surf the net anonymously.
//                     
// MPL 1.1 License agreement.
// The contents of this file are subject to the Mozilla Public License
// Version 1.1 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS"
// basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
// License for the specific language governing rights and limitations
// under the License.
// 
// The Original Code is ASProxy (an ASP.NET Proxy).
// 
// The Initial Developer of the Original Code is Salar.Kh (SalarSoft).
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.Kh < salarsoftwares [@] gmail[.]com > (original author)
//
//**************************************************************************

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



		public static string EncodeToASProxyBase64(string input)
		{
			var encoder = new ASProxyEncoderBase64();
			return encoder.Encode(input);
		}
		public static string DecodeFromASProxyBase64(string b64Input)
		{
			var decoder = new ASProxyDecoderBase64();
			return decoder.DecodeString(b64Input);
		}
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

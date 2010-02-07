using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SalarSoft.ASProxy.Encoders;

namespace SalarSoft.ASProxy
{
    public static class UrlEncoders
    {
        public static string EncodeToASProxyBase64(string input)
        {
            ASProxyEncoderBase64 encoder = new ASProxyEncoderBase64();
            return encoder.Encode(input);
        }
        public static string DecodeFromASProxyBase64(string b64input)
        {
            ASProxyDecoderBase64 decoder = new ASProxyDecoderBase64();
            return decoder.DecodeString(b64input);
        }
    }
}

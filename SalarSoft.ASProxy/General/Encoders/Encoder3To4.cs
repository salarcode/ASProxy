using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SalarSoft.ASProxy.Encoders
{
    public abstract class EncoderBase
    {
        private System.Text.Encoding mEncoding = System.Text.Encoding.ASCII;
        public string Encode(string ASrc)
        {
            using (MemoryStream ms = new MemoryStream(this.Encoding.GetBytes(ASrc)))
            {
                return Encode(ms);
            }
        }

        public string Encode(Stream ASrcStream)
        {
            return Encode(ASrcStream, Int32.MaxValue);
        }

        public abstract string Encode(Stream ASrcStream, int ABytes);

        public System.Text.Encoding Encoding
        {
            get
            {
                return mEncoding;
            }
            set
            {
                mEncoding = value;
            }
        }
    }

    public class Encoder3To4 : EncoderBase
    {
        protected string _CodingTable;
        protected char _FillChar;

        protected byte[] EncodeBytes(byte[] ABuffer)
        {
            int LOutSize = ((ABuffer.Length + 2) / 3) * 4;
            int LLen = 0;
            int LPos = 0;
            int LBufSize = ABuffer.Length;
            int LBufDataLen;
            byte LIn1;
            byte LIn2;
            byte LIn3;
            int LSize;
            byte[] LUnit;
            byte[] TempResult = new byte[LOutSize];

            while (LPos <= LBufSize)
            {
                LBufDataLen = LBufSize - LPos;
                if (LBufDataLen > 3)
                {
                    LIn1 = (byte)ABuffer[LPos];
                    LIn2 = (byte)ABuffer[LPos + 1];
                    LIn3 = (byte)ABuffer[LPos + 2];
                    LSize = 3;
                    LPos += 3;
                }
                else
                {
                    if (LBufDataLen > 2)
                    {
                        LIn1 = (byte)ABuffer[LPos];
                        LIn2 = (byte)ABuffer[LPos + 1];
                        LIn3 = (byte)ABuffer[LPos + 2];
                        LSize = 3;
                        LPos = LBufSize + 1;
                    }
                    else
                    {
                        if (LBufDataLen > 1)
                        {
                            LIn1 = (byte)ABuffer[LPos];
                            LIn2 = (byte)ABuffer[LPos + 1];
                            LIn3 = 0;
                            LSize = 2;
                            LPos = LBufSize + 1;
                        }
                        else
                        {
                            LIn1 = (byte)ABuffer[LPos];
                            LIn2 = 0;
                            LIn3 = 0;
                            LSize = 1;
                            LPos = LBufSize + 1;
                        }
                    }
                }
                LUnit = new byte[4];
                LUnit[0] = (byte)_CodingTable[((LIn1 >> 2) & 63)];
                LUnit[1] = (byte)_CodingTable[(((LIn1 << 4) | (LIn2 >> 4)) & 63)];
                LUnit[2] = (byte)_CodingTable[(((LIn2 << 2) | (LIn3 >> 6)) & 63)];
                LUnit[3] = (byte)_CodingTable[(LIn3 & 63)];
                if (LLen + 4 > TempResult.Length)
                {
                    throw new Exception("EncoderThreeToFour.Encode: Calculated length exceeded (expected " +
                                        (4 * ((LBufSize + 2) / 3)).ToString() +
                                        ", about to go " +
                                        (LLen + 4).ToString() +
                                        " at offset " +
                                        LPos.ToString() +
                                        " of " + LBufSize.ToString());
                }
                Array.Copy(LUnit, 0, TempResult, LLen, 4);
                LLen += 4;
                if (LSize < 3)
                {
                    TempResult[LLen - 1] = (byte)FillChar;
                    if (LSize == 1)
                    {
                        TempResult[LLen - 2] = (byte)FillChar;
                    }
                }
            }
            if (LLen != (4 * ((LBufSize + 2) / 3)))
            {
                throw new Exception("EncoderThreeToFour.Encode: Calculated length not met (expected " +
                                    (4 * (LBufSize + 2) / 3).ToString() +
                                    ", finished at " +
                                    (LLen + 4).ToString() +
                                    ", BufSize = " + LBufSize.ToString());
            }
            return TempResult;
        }

        public override string Encode(Stream ASrcStream, int ABytes)
        {
            // TODO: Make this more efficient
            byte[] LBuffer = null; ;
            int LBufSize = (int)(ASrcStream.Length - ASrcStream.Position);
            byte[] LOutgoing = null;
            if (LBufSize > ABytes)
            {
                LBufSize = ABytes;
            }
            if (LBufSize == 0)
            {
                return "";
            }
            LBuffer = new byte[LBufSize];
            ASrcStream.Read(LBuffer, 0, LBufSize);
            LOutgoing = EncodeBytes(LBuffer);
            return Encoding.GetString(LOutgoing);
        }

        public void EncodeUnit(byte AIn1, byte AIn2, byte AIn3, out byte[] AOut)
        {
            AOut = new byte[4];
            AOut[0] = (byte)_CodingTable[((AIn1 >> 2) & 63)];
            AOut[1] = (byte)_CodingTable[(((AIn1 << 4) | (AIn2 >> 4)) & 63)];
            AOut[2] = (byte)_CodingTable[(((AIn2 << 2) | (AIn3 >> 6)) & 63)];
            AOut[3] = (byte)_CodingTable[(AIn3 & 63)];
        }

        public string CodingTable
        {
            get
            {
                return _CodingTable;
            }
        }

        public char FillChar
        {
            get
            {
                return _FillChar;
            }
            set
            {
                _FillChar = value;
            }
        }
    }
}

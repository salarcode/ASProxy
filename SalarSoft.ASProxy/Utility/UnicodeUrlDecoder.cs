using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy
{
	/// <summary>
	/// Specially optiomized encoder to decode Unicode characters only in a URL
	/// </summary>
	public class UnicodeUrlDecoder
	{
		private class InternalDecoder
		{
			// Fields
			private int _bufferSize;
			private byte[] _byteBuffer;
			private char[] _charBuffer;
			private Encoding _encoding;
			private int _numBytes;
			private int _numChars;

			// Methods
			internal InternalDecoder(int bufferSize, Encoding encoding)
			{
				this._bufferSize = bufferSize;
				this._encoding = encoding;
				this._charBuffer = new char[bufferSize];
			}

			internal void AddByte(byte b)
			{
				if (this._byteBuffer == null)
				{
					this._byteBuffer = new byte[this._bufferSize];
				}
				this._byteBuffer[this._numBytes++] = b;
			}

			internal void AddChar(char ch)
			{
				if (this._numBytes > 0)
				{
					this.FlushBytes();
				}
				this._charBuffer[this._numChars++] = ch;
			}

			private void FlushBytes()
			{
				if (this._numBytes > 0)
				{
					this._numChars += this._encoding.GetChars(this._byteBuffer, 0, this._numBytes, this._charBuffer, this._numChars);
					this._numBytes = 0;
				}
			}

			internal string GetString()
			{
				if (this._numBytes > 0)
				{
					this.FlushBytes();
				}
				if (this._numChars > 0)
				{
					return new string(this._charBuffer, 0, this._numChars);
				}
				return string.Empty;
			}
		}

		public static string UrlDecode(string str)
		{
			if (str == null)
			{
				return null;
			}
			return UrlDecodeEncoding(str, Encoding.UTF8);
		}

		private static string UrlDecodeEncoding(string str, Encoding enc)
		{
			int length = str.Length;

			InternalDecoder decoder = new InternalDecoder(length, enc);
			for (int i = 0; i < length; i++)
			{
				char ch = str[i];
				if ((ch == '%') && (i < (length - 2)))
				{
					if ((str[i + 1] == 'u') && (i < (length - 5)))
					{
						int next2 = HexCharToInt(str[i + 2]);
						int next3 = HexCharToInt(str[i + 3]);
						int next4 = HexCharToInt(str[i + 4]);
						int next5 = HexCharToInt(str[i + 5]);

						if (((next2 < 0) || (next3 < 0)) || ((next4 < 0) || (next5 < 0)))
						{
							//just add the char and continue
							decoder.AddChar(ch);
							continue;
						}

						ch = (char)((((next2 << 12) | (next3 << 8)) | (next4 << 4)) | next5);

						// step to next 5 char
						i += 5;

						decoder.AddChar(ch);
						continue;
					}

					int nextC1 = HexCharToInt(str[i + 1]);
					int nextC2 = HexCharToInt(str[i + 2]);
					if ((nextC1 >= 0) && (nextC2 >= 0))
					{
						decoder.AddChar(ch);
						continue;
					}
				}
				else
				{
					//just add the char and continue
					decoder.AddChar(ch);
					continue;
				}
			}
			return decoder.GetString();
		}

		private static int HexCharToInt(char ch)
		{
			if ((ch >= '0') && (ch <= '9'))
			{
				return (ch - '0');
			}
			if ((ch >= 'a') && (ch <= 'f'))
			{
				return ((ch - 'a') + 10);
			}
			if ((ch >= 'A') && (ch <= 'F'))
			{
				return ((ch - 'A') + 10);
			}
			return -1;
		}
	}
}

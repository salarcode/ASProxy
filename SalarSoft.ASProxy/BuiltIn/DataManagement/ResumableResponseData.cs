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
// The Initial Developer of the Original Code is Salar.K.
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.K https://github.com/salarcode (original author)
//
//**************************************************************************

using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using SalarSoft.ASProxy;

namespace SalarSoft.ResumableDownload
{
	public struct ResumableResponseData
	{
		private string _fileHeaderName;
		private string _fileName;
		private bool _isDataFile;

		public bool IsDataFile { get { return _isDataFile; } }
		public string FileName { get { return _fileName; } }
		public string FileHeaderName
		{
			get { return _fileHeaderName; }
			set
			{
				if (string.IsNullOrEmpty(value))
					_fileHeaderName = value;
				else
					_fileHeaderName = value.Replace(' ', '_');
			}
		}

		/// <summary>
		/// Content total size in bytes
		/// </summary>
		public long DataLength;
		public string ContentType;
		public DateTime LastWriteTimeUTC;
		/// <summary>
		/// This unique code must keep the same as long as the file does not change.
		/// </summary>
		public string EntityTag;
		public Stream DataStream;

		public long RangeBegin;
		public long RangeEnd;

		/// <summary>
		/// Indicates if this is a range request 
		/// </summary>
		public bool RangeRequest;

		/// <summary>
		/// If true and if RangeRequest is true the specifed range values will apply to DataStream,
		/// otherwise the DataStream will send without change.
		/// </summary>
		public bool ApplyRangeToStream;

		/// <summary>
		/// Request http method
		/// </summary>
		public string HttpMethod;

		public ResumableResponseData(string fileName)
		{
			FileInfo info = new FileInfo(fileName);
			_isDataFile = true;
			_fileHeaderName = "";
			_fileName = fileName;
			DataLength = info.Length;
			LastWriteTimeUTC = info.LastWriteTimeUtc;
			ContentType = "application/octet-stream";
			EntityTag = fileName.GetHashCode().ToString();

			DataStream = File.OpenRead(fileName);
			ApplyRangeToStream = true;

			HttpMethod = "GET";
			RangeBegin = 0;
			RangeEnd = DataLength - 1;
			RangeRequest = false;
			FileHeaderName = UrlProvider.GetFileNameForHttpHeader(fileName);
		}

		public ResumableResponseData(Stream dataStream, string fileName)
		{
			_fileHeaderName = "";
			_fileName = null;
			if (dataStream.CanSeek)
				DataLength = dataStream.Length;
			else
				DataLength = -1;
			_isDataFile = false;
			LastWriteTimeUTC = DateTime.Now;
			ContentType = "application/octet-stream";
			EntityTag = fileName.GetHashCode().ToString();

			DataStream = dataStream;
			ApplyRangeToStream = true;

			HttpMethod = "GET";
			RangeBegin = 0;
			RangeEnd = DataLength - 1;
			RangeRequest = false;
			FileHeaderName = UrlProvider.GetFileNameForHttpHeader(fileName);
		}

		public ResumableResponseData(byte[] dataBytes, string fileName)
		{
			_fileHeaderName = "";
			_isDataFile = false;
			_fileName = null;
			DataLength = dataBytes.Length;
			LastWriteTimeUTC = DateTime.Now;
			ContentType = "application/octet-stream";
			EntityTag = fileName.GetHashCode().ToString();

			DataStream = new MemoryStream(dataBytes);
			ApplyRangeToStream = true;

			HttpMethod = "GET";
			RangeBegin = 0;
			RangeEnd = DataLength - 1;
			RangeRequest = false;
			FileHeaderName = UrlProvider.GetFileNameForHttpHeader(fileName);
		}


		public static MemoryStream ReadToBuffer(Stream backEndDataStream)
		{
			MemoryStream result = new MemoryStream();
			// Declare variables
			int maxBlockReadFromWeb = 5120;
			int readed = -1;
			byte[] buffer = new byte[maxBlockReadFromWeb];

			if (backEndDataStream.CanSeek)
				backEndDataStream.Seek(0, SeekOrigin.Begin);

			// Read the stream and write it into memory
			while ((int)(readed = backEndDataStream.Read(buffer, 0, maxBlockReadFromWeb)) > 0)
			{
				result.Write(buffer, 0, readed);
			}

			result.Seek(0, SeekOrigin.Begin);
			return result;
		}
	}
}

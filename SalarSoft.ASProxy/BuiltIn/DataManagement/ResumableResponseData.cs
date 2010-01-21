using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;

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
            FileHeaderName = Path.GetFileName(fileName);
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
            FileHeaderName = Path.GetFileName(fileName);
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
            FileHeaderName = Path.GetFileName(fileName);
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

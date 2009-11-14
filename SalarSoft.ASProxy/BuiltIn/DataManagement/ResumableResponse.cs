//#define dotNET20sp1

using System;
using System.IO;
using System.Web;

namespace SalarSoft.ResumableDownload
{
	/// <summary>
	/// Original class name is "ZIPHandler" in VB.Net
	/// http://www.devx.com
	/// Optimization and convertion by SalarSoft
	/// </summary>
	public class ResumableResponse : IDisposable
	{
		public enum ResponseState
		{
			None,

			/// <summary>
			/// In Progress: File is locked, and download 
			/// is currently in progress
			/// </summary>
			InProgress,

			/// <summary>
			/// Broken: File is locked, download was in
			/// progress, but was cancelled 
			/// </summary>
			Broken,

			/// <summary>
			/// Finished: File is locked, download
			/// was completed
			/// </summary>
			Finished
		}

		#region Header consts
		const string MULTIPART_BOUNDARY = "<q1w2e3r4t5y6u7i8o9p0>";
		const string MULTIPART_CONTENTTYPE = "multipart/byteranges; boundary=" + MULTIPART_BOUNDARY;
		const string HTTP_HEADER_Content_Disposition = "Content-Disposition";
		const string HTTP_HEADER_ACCEPT_RANGES = "Accept-Ranges";
		const string HTTP_HEADER_ACCEPT_RANGES_BYTES = "bytes";
		const string HTTP_HEADER_CONTENT_TYPE = "Content-Type";
		const string HTTP_HEADER_CONTENT_RANGE = "Content-Range";
		const string HTTP_HEADER_CONTENT_LENGTH = "Content-Length";
		const string HTTP_HEADER_ENTITY_TAG = "ETag";
		const string HTTP_HEADER_LAST_MODIFIED = "Last-Modified";
		const string HTTP_HEADER_RANGE = "Range";
		const string HTTP_HEADER_IF_RANGE = "If-Range";
		const string HTTP_HEADER_IF_MATCH = "If-Match";
		const string HTTP_HEADER_IF_NONE_MATCH = "If-None-Match";
		const string HTTP_HEADER_IF_MODIFIED_SINCE = "If-Modified-Since";
		const string HTTP_HEADER_IF_UNMODIFIED_SINCE = "If-Unmodified-Since";
		const string HTTP_HEADER_UNLESS_MODIFIED_SINCE = "Unless-Modified-Since";
		const string HTTP_METHOD_GET = "GET";
		const string HTTP_METHOD_HEAD = "HEAD";
		#endregion
		private ResumableResponseData _responseData;

		public ResumableResponse(ResumableResponseData responseData)
		{
			_responseData = responseData;
		}

		public void Dispose()
		{
			if (_responseData.DataStream != null)
				_responseData.DataStream.Dispose();
		}

		public ResponseState ProcessDownload(HttpResponse objResponse)
		{
			ResponseState resultState = ResponseState.None;

			// Long Arrays for Range values:
			// ...Begin() contains start positions for each requested Range
			long[] alRequestedRangesBegin = new long[1];
			alRequestedRangesBegin[0] = _responseData.RangeBegin;

			// ...End() contains end positions for each requested Range
			long[] alRequestedRangesEnd = new long[1];
			alRequestedRangesEnd[0] = _responseData.RangeEnd;

			// Response Header value: Content Length...
			int iResponseContentLength = 0;

			// The Stream we//re using to download the file in chunks...
			System.IO.Stream objStream;
			// Total Bytes to read (per requested range)
			int iBytesToRead;

			// Size of the Buffer for chunk-wise reading
			int iBufferSize = 5120;

			// The Buffer itself
			byte[] bBuffer = new byte[iBufferSize];
			// Amount of Bytes read
			int iLengthOfReadChunk = -1;

			// Indicates if the download was interrupted
			bool bDownloadBroken = false;

			// Indicates if this is a multipart range request
			bool bMultipart = false;

			// Loop counter used to iterate through the ranges
			int iLoop;

			// Content-Disposition value
			string Content_Disposition_File = "attachment; filename=" + _responseData.FileHeaderName;


			if (!(_responseData.HttpMethod.Equals(HTTP_METHOD_GET) ||
				_responseData.HttpMethod.Equals(HTTP_METHOD_HEAD)))
				// Currently, only the GET and HEAD methods 
				// are supported...
				objResponse.StatusCode = 501;  // Not implemented
			else
			{
				// Preliminary checks where successful... 
				if (_responseData.RangeRequest)
				{
					// This is a Range request... 

					// if the Range arrays contain more than one entry,
					// it even is a multipart range request...
					bMultipart = (alRequestedRangesBegin.GetUpperBound(0) > 0);

					// Loop through each Range to calculate the entire Response length
					for (iLoop = alRequestedRangesBegin.GetLowerBound(0); iLoop <= alRequestedRangesBegin.GetUpperBound(0); iLoop++)
					{
						// The length of the content (for this range)
						iResponseContentLength += Convert.ToInt32(alRequestedRangesEnd[iLoop] - alRequestedRangesBegin[iLoop]) + 1;

						if (bMultipart)
						{
							//
							iResponseContentLength += HTTP_HEADER_Content_Disposition.Length;
							// if this is a multipart range request, calculate 
							// the length of the intermediate headers to send
							iResponseContentLength += MULTIPART_BOUNDARY.Length;
							iResponseContentLength += _responseData.ContentType.Length;
							iResponseContentLength += alRequestedRangesBegin[iLoop].ToString().Length;
							iResponseContentLength += alRequestedRangesEnd[iLoop].ToString().Length;

							// DataLength = Total size
							iResponseContentLength += _responseData.DataLength.ToString().Length;

							// 49 is the length of line break and other 
							// needed characters in one multipart header
							iResponseContentLength += 49;
						}

					}

					if (bMultipart)
					{
						// if this is a multipart range request,  
						// we must also calculate the length of 
						// the last intermediate header we must send
						iResponseContentLength += MULTIPART_BOUNDARY.Length;
						// 8 is the length of dash and line break characters
						iResponseContentLength += 8;
					}
					else
					{
						// This is no multipart range request, so
						// we must indicate the response Range of 
						// in the initial HTTP Header
						// DataLength = Total size
						objResponse.AppendHeader(HTTP_HEADER_CONTENT_RANGE, "bytes " +
							alRequestedRangesBegin[0].ToString() + "-" +
							alRequestedRangesEnd[0].ToString() + "/" +
							_responseData.DataLength.ToString());
					}

					// Range response 
					objResponse.StatusCode = 206; // Partial Response

				}
				else
				{
					// This is not a Range request, or the requested Range entity ID
					// does not match the current entity ID, so start a new download

					// Indicate the file's complete size as content length
					// DataLength = Total size
					iResponseContentLength = Convert.ToInt32(_responseData.DataLength);

					// Return a normal OK status...
					objResponse.StatusCode = 200;
				}


				// Write file name into the Response
				objResponse.AppendHeader(HTTP_HEADER_Content_Disposition, Content_Disposition_File);

				// Write the content length into the Response
				objResponse.AppendHeader(HTTP_HEADER_CONTENT_LENGTH, iResponseContentLength.ToString());

				// Write the Last-Modified Date into the Response
				objResponse.AppendHeader(HTTP_HEADER_LAST_MODIFIED, _responseData.LastWriteTimeUTC.ToString("r"));

				// Tell the client software that we accept Range request
				objResponse.AppendHeader(HTTP_HEADER_ACCEPT_RANGES, HTTP_HEADER_ACCEPT_RANGES_BYTES);

				// Write the file//s Entity Tag into the Response (in quotes!)
				objResponse.AppendHeader(HTTP_HEADER_ENTITY_TAG, "\"" + _responseData.EntityTag + "\"");


				// Write the Content Type into the Response
				if (bMultipart)
					// Multipart messages have this special Type.
					// In this case, the file//s actual mime type is
					// written into the Response at a later time...
					objResponse.ContentType = MULTIPART_CONTENTTYPE;
				else
					// Single part messages have the files content type...
					objResponse.ContentType = _responseData.ContentType;


				if (_responseData.HttpMethod.Equals(HTTP_METHOD_HEAD))
				{
					// Only the HEAD was requested, so we can quit the Response right here... 
				}
				else
				{
					// Flush the HEAD information to the client...
					objResponse.Flush();

					// Download is in progress...
					resultState = ResponseState.InProgress;

					// The steram
					objStream = _responseData.DataStream;

					if (!_responseData.ApplyRangeToStream)
					{
						// first range only
						iLoop = 0;

						// Calculate the total amount of bytes for first range only
						iBytesToRead = Convert.ToInt32(alRequestedRangesEnd[iLoop] - alRequestedRangesBegin[iLoop]) + 1;

						// sending header for multipart request
						if (bMultipart)
						{
							// if this is a multipart response, we must add 
							// certain headers before streaming the content:

							// The multipart boundary
							objResponse.Output.WriteLine("--" + MULTIPART_BOUNDARY);

							// The mime type of this part of the content 
							objResponse.Output.WriteLine(HTTP_HEADER_CONTENT_TYPE + ": " + _responseData.ContentType);

							// The actual range
							// // DataLength = Total size
							objResponse.Output.WriteLine(HTTP_HEADER_CONTENT_RANGE + ": bytes " +
								alRequestedRangesBegin[iLoop].ToString() + "-" +
								alRequestedRangesEnd[iLoop].ToString() + "/" +
								_responseData.DataLength.ToString());

							// Indicating the end of the intermediate headers
							objResponse.Output.WriteLine();
						}

						// flush the data

						// Declare variables
						int readed = -1;

#if dotNET20sp1
						if (_responseData.IsDataFile)
						{
							// send file content directly
							objResponse.TransmitFile(_responseData.FileName);
						}
						else
#endif
						// Get the response stream for reading
						// Read the stream and write it into memory
						while ((int)(readed = objStream.Read(bBuffer, 0, bBuffer.Length)) > 0)
						{
							if (objResponse.IsClientConnected)
							{
								// write to response
								objResponse.OutputStream.Write(bBuffer, 0, readed);

								// send response
								objResponse.Flush();
							}
							else
							{
								bDownloadBroken = true;
								break;
							}
						}

						// In Multipart responses, mark the end of the part 
						if (bMultipart)
							objResponse.Output.WriteLine();

						// No need to proceed to the next part if the 
						// client was disconnected
						if (bDownloadBroken)
						{
							//break;
						}
						// done!
					}
					else
					{
						// Now, for each requested range, stream the chunks to the client:
						for (iLoop = alRequestedRangesBegin.GetLowerBound(0); iLoop <= alRequestedRangesBegin.GetUpperBound(0); iLoop++)
						{
							// Move the stream to the desired start position...
							objStream.Seek(alRequestedRangesBegin[iLoop], SeekOrigin.Begin);

							// Calculate the total amount of bytes for this range
							iBytesToRead = Convert.ToInt32(alRequestedRangesEnd[iLoop] - alRequestedRangesBegin[iLoop]) + 1;

							if (bMultipart)
							{
								// if this is a multipart response, we must add 
								// certain headers before streaming the content:

								// The multipart boundary
								objResponse.Output.WriteLine("--" + MULTIPART_BOUNDARY);

								// The mime type of this part of the content 
								objResponse.Output.WriteLine(HTTP_HEADER_CONTENT_TYPE + ": " + _responseData.ContentType);

								// The actual range
								// // DataLength = Total size
								objResponse.Output.WriteLine(HTTP_HEADER_CONTENT_RANGE + ": bytes " +
									alRequestedRangesBegin[iLoop].ToString() + "-" +
									alRequestedRangesEnd[iLoop].ToString() + "/" +
									_responseData.DataLength.ToString());

								/*objResponse.AppendHeader(HTTP_HEADER_CONTENT_RANGE,": bytes " +
									alRequestedRangesBegin[iLoop].ToString() + "-" +
									alRequestedRangesend[iLoop].ToString() + "/" +
									objFile.Length.ToString());
								*/
								// Indicating the end of the intermediate headers
								objResponse.Output.WriteLine();

							}

#if dotNET20sp1
							if (_responseData.IsDataFile)
							{
								// send file content directly
								objResponse.TransmitFile(_responseData.FileName, alRequestedRangesBegin[iLoop], iBytesToRead);
							}
							else
#endif
							// Now stream the range to the client...
							while (iBytesToRead > 0)
							{

								if (objResponse.IsClientConnected)
								{
									// Read a chunk of bytes from the stream
									iLengthOfReadChunk = objStream.Read(bBuffer, 0, Math.Min(bBuffer.Length, iBytesToRead));

									// Write the data to the current output stream.
									objResponse.OutputStream.Write(bBuffer, 0, iLengthOfReadChunk);

									// Flush the data to the HTML output.
									objResponse.Flush();

									// Clear the buffer
									//bBuffer=new byte[iBufferSize];

									// Reduce BytesToRead
									iBytesToRead -= iLengthOfReadChunk;
								}
								else
								{
									// The client was or has disconneceted from the server... stop downstreaming...
									iBytesToRead = -1;
									bDownloadBroken = true;
								}
							}

							// In Multipart responses, mark the end of the part 
							if (bMultipart)
								objResponse.Output.WriteLine();

							// No need to proceed to the next part if the 
							// client was disconnected
							if (bDownloadBroken)
								break;
						}
					}
					// At this point, the response was finished or cancelled... 

					if (bDownloadBroken)
						// Download is broken...
						resultState = ResponseState.Broken;
					else
					{
						if (bMultipart)
						{
							// In multipart responses, close the response once more with 
							// the boundary and line breaks
							objResponse.Output.WriteLine("--" + MULTIPART_BOUNDARY + "--");
							objResponse.Output.WriteLine();
						}

						// The download was finished
						resultState = ResponseState.Finished;
					}

					// the data stream now can be closed
					objStream.Close();
				}
			}

			//====== return download state ======
			return resultState;
		}
	}
}
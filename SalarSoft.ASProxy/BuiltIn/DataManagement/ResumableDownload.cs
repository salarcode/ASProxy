using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;

namespace SalarSoft.ResumableDownload
{
	/// <summary>
	/// Original class name is "ZIPHandler" in VB.Net
	/// http://www.devx.com
	/// 
	/// Optimization and convertion by SalarSoft
	/// </summary>
	public class ResumableDownload:IDisposable
	{
		const string MULTIPART_BOUNDARY  = "<q1w2e3r4t5y6u7i8o9p0>";
		const string MULTIPART_CONTENTTYPE  = "multipart/byteranges; boundary=" + MULTIPART_BOUNDARY;

		//Response.AddHeader("Content-Disposition","attachment; filename=" + Path.GetFileName(fUrl));
		const string HTTP_HEADER_Content_Disposition  = "Content-Disposition";

		const string HTTP_HEADER_ACCEPT_RANGES  = "Accept-Ranges";
		const string HTTP_HEADER_ACCEPT_RANGES_BYTES  = "bytes";
		const string HTTP_HEADER_CONTENT_TYPE  = "Content-Type";
		const string HTTP_HEADER_CONTENT_RANGE  = "Content-Range";
		const string HTTP_HEADER_CONTENT_LENGTH  = "Content-Length";
		const string HTTP_HEADER_ENTITY_TAG  = "ETag";
		const string HTTP_HEADER_LAST_MODIFIED  = "Last-Modified";
		const string HTTP_HEADER_RANGE  = "Range";
		const string HTTP_HEADER_IF_RANGE  = "If-Range";
		const string HTTP_HEADER_IF_MATCH  = "If-Match";
		const string HTTP_HEADER_IF_NONE_MATCH  = "If-None-Match";
		const string HTTP_HEADER_IF_MODIFIED_SINCE  = "If-Modified-Since";
		const string HTTP_HEADER_IF_UNMODIFIED_SINCE  = "If-Unmodified-Since";
		const string HTTP_HEADER_UNLESS_MODIFIED_SINCE  = "Unless-Modified-Since";

		const string HTTP_METHOD_GET  = "GET";
		const string HTTP_METHOD_HEAD  = "HEAD";

		private string fContentType="application/octet-stream";

		public string ContentType
		{
			get{return fContentType;}
			set{fContentType=value;}
		}

		public void Dispose()
		{
		}

		public void ClearResponseData()
		{
			HttpResponse objResponse = HttpContext.Current.Response;
			objResponse.ClearContent();
            SalarSoft.ASProxy.Common.ClearHeadersButSaveEncoding(objResponse);
		}
		public DownloadState ProcessDownload(string UrlAddress,string filename)
		{
			byte[] pageData;
			//m_objFile = new System.IO.FileInfo(sPath);
			try
			{
				pageData=WebDataProvider.GetUrlByteData(UrlAddress);
			}
			catch
			{
				pageData=new byte[0];
			}
			return ProcessDownload(pageData,UrlAddress,filename);
		}

		public DownloadState ProcessDownload(byte[] pageData,string UrlAddress,string filename)
		{
    
			HttpContext objContext=HttpContext.Current;
			// The Response object from the Context
			HttpResponse objResponse = objContext.Response;
			// The Request object from the Context
			HttpRequest objRequest =objContext.Request;

			// File information object...
			UrlInformation objFile;

			// Long Arrays for Range values:
			// ...Begin() contains start positions for each requested Range
			long[] alRequestedRangesBegin=new long[1];
			// ...End() contains end positions for each requested Range
			long[] alRequestedRangesend=new long[1];

			// Response Header value: Content Length...
			int iResponseContentLength=0;

			// The Stream we//re using to download the file in chunks...
			System.IO.Stream objStream ;
			// Total Bytes to read (per requested range)
			int iBytesToRead;
			// Size of the Buffer for chunk-wise reading
			int iBufferSize = 25000;
			// The Buffer itself
			byte[] bBuffer=new byte[iBufferSize];
			// Amount of Bytes read
			int iLengthOfReadChunk=-1;

			// Indicates if the download was interrupted
			bool bDownloadBroken=false;

			// Indicates if this is a range request 
			bool bIsRangeRequest=false;
			// Indicates if this is a multipart range request
			bool bMultipart=false;

			// Loop counter used to iterate through the ranges
			int iLoop;

			filename = filename.Replace(' ', '-');
			// Content-Disposition value
			string Content_Disposition_File = "attachment; filename=" + filename + "";

			// ToDo - your code here (Determine which file is requested)
			// Using objRequest, determine which file is requested to
			// be downloaded, and open objFile with that file:
			// Example:
			// objFile = New Download.FileInformation(<Full path to the requested file>)
			//objFile = new Download.FileInformation(objContext.Server.MapPath("~/download.zip"));
			objFile = new UrlInformation(pageData,UrlAddress);			
			objFile.ContentType=this.ContentType;

			// Clear the current output content from the buffer
			//objResponse.Clear();

			if(!(objRequest.HttpMethod.Equals(HTTP_METHOD_GET) || 
				objRequest.HttpMethod.Equals(HTTP_METHOD_HEAD)))
				// Currently, only the GET and HEAD methods 
				// are supported...
				objResponse.StatusCode = 501;  // Not implemented

			else if (! objFile.Exists)
				// The requested file could not be retrieved...
				objResponse.StatusCode = 404;  // Not found

			else if( objFile.Length > Int32.MaxValue)
				// The file size is too large... 
				objResponse.StatusCode = 413;  // Request Entity Too Large

			else if (! ParseRequestHeaderRange(objRequest, ref alRequestedRangesBegin,ref alRequestedRangesend, 
				objFile.Length,ref bIsRangeRequest))
				// The Range request contained bad entries
				objResponse.StatusCode = 400;  // Bad Request

			else if(!CheckIfModifiedSince(objRequest, objFile))
				// The entity is still unmodified...
				objResponse.StatusCode = 304;  // Not Modified

			else if (!CheckIfUnmodifiedSince(objRequest, objFile))
				// The entity was modified since the requested date... 
				objResponse.StatusCode = 412;  // Precondition failed

			else if(!CheckIfMatch(objRequest, objFile))
				// The entity does not match the request... 
				objResponse.StatusCode = 412;  // Precondition failed

			else if(! CheckIfNoneMatch(objRequest, objResponse, objFile) )
			{
				// The entity does match the none-match request, the response 
				// code was set inside the CheckifNoneMatch function
			}
			else
			{
				// Preliminary checks where successful... 

				if (bIsRangeRequest && CheckIfRange(objRequest, objFile))
				{
					// This is a Range request... 

					// if the Range arrays contain more than one entry,
					// it even is a multipart range request...
					bMultipart = (alRequestedRangesBegin.GetUpperBound(0) > 0);

					// Loop through each Range to calculate the entire Response length
					for (iLoop = alRequestedRangesBegin.GetLowerBound(0) ;iLoop<= alRequestedRangesBegin.GetUpperBound(0);iLoop++)
					{
						// The length of the content (for this range)
						iResponseContentLength += Convert.ToInt32(alRequestedRangesend[iLoop] - alRequestedRangesBegin[iLoop]) + 1;

						if( bMultipart)
						{
							//
							iResponseContentLength += HTTP_HEADER_Content_Disposition.Length;
							// if this is a multipart range request, calculate 
							// the length of the intermediate headers to send
							iResponseContentLength += MULTIPART_BOUNDARY.Length;
							iResponseContentLength += objFile.ContentType.Length;
							iResponseContentLength += alRequestedRangesBegin[iLoop].ToString().Length;
							iResponseContentLength += alRequestedRangesend[iLoop].ToString().Length;
							iResponseContentLength += objFile.Length.ToString().Length;
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
						objResponse.AppendHeader(HTTP_HEADER_CONTENT_RANGE, "bytes " + 
							alRequestedRangesBegin[0].ToString() + "-" + 
							alRequestedRangesend[0].ToString() + "/" + 
							objFile.Length.ToString());
					}

					// Range response 
					objResponse.StatusCode = 206; // Partial Response

				}
				else
				{
					// This is not a Range request, or the requested Range entity ID
					// does not match the current entity ID, so start a new download

					// Indicate the file//s complete size as content length
					iResponseContentLength = Convert.ToInt32(objFile.Length);

					// Return a normal OK status...
					objResponse.StatusCode = 200;
				}


				// Write file name into the Response
				objResponse.AppendHeader(HTTP_HEADER_Content_Disposition, Content_Disposition_File);

				// Write the content length into the Response
				objResponse.AppendHeader(HTTP_HEADER_CONTENT_LENGTH, iResponseContentLength.ToString());

				// Write the Last-Modified Date into the Response
				objResponse.AppendHeader(HTTP_HEADER_LAST_MODIFIED, objFile.LastWriteTimeUTC.ToString("r"));

				// Tell the client software that we accept Range request
				objResponse.AppendHeader(HTTP_HEADER_ACCEPT_RANGES, HTTP_HEADER_ACCEPT_RANGES_BYTES);

				// Write the file//s Entity Tag into the Response (in quotes!)
				objResponse.AppendHeader(HTTP_HEADER_ENTITY_TAG, "\"" + objFile.EntityTag + "\"");


				// Write the Content Type into the Response
				if (bMultipart)
					// Multipart messages have this special Type.
					// In this case, the file//s actual mime type is
					// written into the Response at a later time...
					objResponse.ContentType = MULTIPART_CONTENTTYPE;
				else
					// Single part messages have the files content type...
					objResponse.ContentType = objFile.ContentType;


				if (objRequest.HttpMethod.Equals(HTTP_METHOD_HEAD))
				{
					// Only the HEAD was requested, so we can quit the Response right here... 
				}
				else
				{

					// Flush the HEAD information to the client...
					objResponse.Flush();

					// Download is in progress...
					objFile.State = DownloadState.fsDownloadInProgress;

					// Open the file as filestream
					/*objStream = new FileStream(objFile.FullName, FileMode.Open, 
						FileAccess.Read,
						FileShare.Read);*/
					objStream=objFile.DataStream;

					// Now, for each requested range, stream the chunks to the client:
					for (iLoop = alRequestedRangesBegin.GetLowerBound(0) ;iLoop<= alRequestedRangesBegin.GetUpperBound(0);iLoop++)
					{

						// Move the stream to the desired start position...
						objStream.Seek(alRequestedRangesBegin[iLoop], SeekOrigin.Begin);

						// Calculate the total amount of bytes for this range
						iBytesToRead = Convert.ToInt32(alRequestedRangesend[iLoop] - alRequestedRangesBegin[iLoop]) + 1;

						if (bMultipart)
						{
							// if this is a multipart response, we must add 
							// certain headers before streaming the content:

							// The multipart boundary
							objResponse.Output.WriteLine("--" + MULTIPART_BOUNDARY);
							//objResponse.AppendHeader("--",MULTIPART_BOUNDARY);

							// The mime type of this part of the content 
							objResponse.Output.WriteLine(HTTP_HEADER_CONTENT_TYPE + ": " + objFile.ContentType);
							//objResponse.AppendHeader(HTTP_HEADER_CONTENT_TYPE,objFile.ContentType);

							// The actual range
							objResponse.Output.WriteLine(HTTP_HEADER_CONTENT_RANGE + ": bytes " +
								alRequestedRangesBegin[iLoop].ToString() + "-" +
								alRequestedRangesend[iLoop].ToString() + "/" +
								objFile.Length.ToString());
								
							/*objResponse.AppendHeader(HTTP_HEADER_CONTENT_RANGE,": bytes " +
								alRequestedRangesBegin[iLoop].ToString() + "-" +
								alRequestedRangesend[iLoop].ToString() + "/" +
								objFile.Length.ToString());
							*/
							// Indicating the end of the intermediate headers
							objResponse.Output.WriteLine();

						}

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
						if( bMultipart) objResponse.Output.WriteLine();

						// No need to proceed to the next part if the 
						// client was disconnected
						if( bDownloadBroken)  break;
					}

					// At this point, the response was finished or cancelled... 

					if (bDownloadBroken)
						// Download is broken...
						objFile.State = DownloadState.fsDownloadBroken;

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
						objFile.State = DownloadState.fsDownloadFinished;
					}
					objStream.Close();
				}
			}
			//objResponse.End();

			//====== return download state ======
			return objFile.State;
		}

		bool CheckIfRange(HttpRequest objRequest ,UrlInformation objFile ) 
		{
			string sRequestHeaderIfRange;

			// Checks the If-Range header if it was sent with the request.
			//
			// returns true if the header value matches the file//s entity tag,
			//              or if no header was sent,
			// returns false if a header was sent, but does not match the file.


			// Retrieve If-Range Header value from Request (objFile.EntityTag if none is indicated)
			sRequestHeaderIfRange = RetrieveHeader(objRequest, HTTP_HEADER_IF_RANGE, objFile.EntityTag);

			// If the requested file entity matches the current
			// file entity, return true
			return sRequestHeaderIfRange.Equals(objFile.EntityTag);
		}

		bool CheckIfMatch(HttpRequest objRequest,UrlInformation objFile )
		{
			string sRequestHeaderIfMatch;
			string[] sEntityIDs;
			bool breturn=false;

			// Checks the If-Match header if it was sent with the request.
			//
			// returns true if one of the header values matches the file//s entity tag,
			//              or if no header was sent,
			// returns false if a header was sent, but does not match the file.


			// Retrieve If-Match Header value from Request (*, meaning any, if none is indicated)
			sRequestHeaderIfMatch = RetrieveHeader(objRequest, HTTP_HEADER_IF_MATCH, "*");

			if( sRequestHeaderIfMatch.Equals("*"))
				// The server may perform the request as if the
				// If-Match header does not exists...
				breturn = true;

			else
			{
				// One or more Match IDs where sent by the client software...
				sEntityIDs = sRequestHeaderIfMatch.Replace("bytes=", "").Split(",".ToCharArray());

				// Loop through all entity IDs, finding one 
				// which matches the current file's ID will
				// be enough to satisfy the If-Match
				for(int iLoop = sEntityIDs.GetLowerBound(0) ;iLoop<= sEntityIDs.GetUpperBound(0);iLoop++)
				{
					if (sEntityIDs[iLoop].Trim().Equals(objFile.EntityTag))
						breturn = true;
				}
			}
			// return the result...
			return breturn;
		}

		bool CheckIfNoneMatch(HttpRequest objRequest ,HttpResponse objResponse ,UrlInformation objFile)
		{  
			string sRequestHeaderIfNoneMatch;
			string[] sEntityIDs;
			bool breturn=true;
			string sreturn="";
			// Checks the If-None-Match header if it was sent with the request.
			//
			// returns true if one of the header values matches the file//s entity tag,
			//              or if "*" was sent,
			// returns false if a header was sent, but does not match the file, or
			//               if no header was sent.

			// Retrieve If-None-Match Header value from Request (*, meaning any, if none is indicated)
			sRequestHeaderIfNoneMatch = RetrieveHeader(objRequest, HTTP_HEADER_IF_NONE_MATCH, String.Empty);

			if (sRequestHeaderIfNoneMatch.Equals(String.Empty)) 
				// Perform the request normally...
				breturn = true;
			else
			{
				if( sRequestHeaderIfNoneMatch.Equals("*") )
				{
					// The server must not perform the request 
					objResponse.StatusCode = 412;  // Precondition failed
					breturn = false;
				}
				else
				{
					// One or more Match IDs where sent by the client software...
					sEntityIDs = sRequestHeaderIfNoneMatch.Replace("bytes=", "").Split(",".ToCharArray());

					// Loop through all entity IDs, finding one which 
					// does not match the current file//s ID will be
					// enough to satisfy the If-None-Match
					for (int iLoop= sEntityIDs.GetLowerBound(0);iLoop<= sEntityIDs.GetUpperBound(0);iLoop++)
					{
						if( sEntityIDs[iLoop].Trim().Equals(objFile.EntityTag))
						{
							sreturn = sEntityIDs[iLoop];
							breturn = false;
						}
					}

					if (! breturn)
					{
						// One of the requested entities matches the current file//s tag,
						objResponse.AppendHeader("ETag", sreturn);
						objResponse.StatusCode = 304 ; // Not Modified
					}
				}
			}
			// return the result...
			return breturn;
		}

		bool CheckIfModifiedSince(HttpRequest objRequest,UrlInformation objFile)
		{
			string sDate;
			DateTime dDate;
			bool breturn;

			// Checks the If-Modified header if it was sent with the request.
			//
			// returns true, if the file was modified since the 
			//               indicated date (RFC 1123 format), or
			//               if no header was sent,
			// returns false, if the file was not modified since
			//                the indicated date


			// Retrieve If-Modified-Since Header value from Request (Empty if none is indicated)
			sDate = RetrieveHeader(objRequest, HTTP_HEADER_IF_MODIFIED_SINCE, string.Empty);

			if( sDate.Equals(String.Empty))
				// No If-Modified-Since date was indicated, 
				// so just give this as true 
				breturn = true;

			else
			{
				try
				{
					// ... to parse the indicated sDate to a datetime value
					dDate = DateTime.Parse(sDate);
					// return true if the file was modified since or at the indicated date...
					breturn = (objFile.LastWriteTimeUTC >= DateTime.Parse(sDate));
				}
				catch
				{
					// Converting the indicated date value failed, return false 
					breturn = false;
				}
			}
			return breturn;
		}

		bool CheckIfUnmodifiedSince(HttpRequest objRequest ,UrlInformation objFile)
		{
			string sDate;
			DateTime dDate;
			bool breturn;


			// Checks the If-Unmodified or Unless-Modified-Since header, if 
			// one of them was sent with the request.
			//
			// returns true, if the file was not modified since the 
			//               indicated date (RFC 1123 format), or
			//               if no header was sent,
			// returns false, if the file was modified since the indicated date


			// Retrieve If-Unmodified-Since Header value from Request (Empty if none is indicated)
			sDate = RetrieveHeader(objRequest, HTTP_HEADER_IF_UNMODIFIED_SINCE, String.Empty);

			if (sDate.Equals(String.Empty))
				// If-Unmodified-Since was not sent, check Unless-Modified-Since... 
				sDate = RetrieveHeader(objRequest, HTTP_HEADER_UNLESS_MODIFIED_SINCE, String.Empty);


			if(sDate.Equals(String.Empty))
				// No date was indicated, 
				// so just give this as true 
				breturn = true;

			else
			{
				try
				{
					// ... to parse the indicated sDate to a datetime value
					dDate = DateTime.Parse(sDate);
					// return true if the file was not modified since the indicated date...
					breturn = objFile.LastWriteTimeUTC < DateTime.Parse(sDate);
				}
				catch (Exception)
				{
					// Converting the indicated date value failed, return false 
					breturn = false;
				}
			}
			return breturn;
		}

		bool ParseRequestHeaderRange(HttpRequest objRequest , ref long[] lBegin, ref long[] lEnd , long lMax , ref bool bRangeRequest)
		{
			bool bValidRanges;
			string sSource;
			int iLoop;
			string[] sRanges;

			// Parses the Range header from the Request (if there is one)
			// returns true, if the Range header was valid, or if there was no 
			//               Range header at all (meaning that the whole 
			//               file was requested)
			// returns false, if the Range header asked for unsatisfieable 
			//                ranges

			// Retrieve Range Header value from Request (Empty if none is indicated)
			sSource = RetrieveHeader(objRequest, HTTP_HEADER_RANGE, String.Empty);

			if (sSource.Equals(String.Empty)) 
			{
				// No Range was requested, return the entire file range...

				lBegin=new long[1];
				//ReDim lBegin(0);
				lEnd=new long[1];
				//ReDim lEnd(0);

				lBegin[0] = 0;
				lEnd[0] = lMax - 1;

				// A valid range is returned
				bValidRanges = true;
				// no Range request
				bRangeRequest = false;
			}
			else
			{
				// A Range was requested... 

				// Preset value...
				bValidRanges = true;

				// return true for the bRange parameter, telling the caller
				// that the Request is indeed a Range request...
				bRangeRequest = true;

				// Remove "bytes=" from the beginning, and split the remaining 
				// string by comma characters
				sRanges = sSource.Replace("bytes=", "").Split(",".ToCharArray());
				lBegin=new long[sRanges.GetUpperBound(0)+1];
				//ReDim lBegin(sRanges.GetUpperBound(0));
				lEnd=new long[sRanges.GetUpperBound(0)+1];
				//ReDim lEnd(sRanges.GetUpperBound(0));

				// Check each found Range request for consistency
				for (iLoop = sRanges.GetLowerBound(0) ;iLoop<=sRanges.GetUpperBound(0);iLoop++)
				{

					// Split this range request by the dash character, 
					// sRange(0) contains the requested begin-value,
					// sRange(1) contains the requested end-value...
					string[] sRange = sRanges[iLoop].Split("-".ToCharArray());

					// Determine the end of the requested range
					if (sRange[1].Equals(String.Empty))
						// No end was specified, take the entire range
						lEnd[iLoop] = lMax - 1;
					else
						// An end was specified...
						lEnd[iLoop] = long.Parse(sRange[1]);

					// Determine the begin of the requested range
					if (sRange[0].Equals(String.Empty) )
					{
						// No begin was specified, which means that
						// the end value indicated to return the last n
						// bytes of the file:

						// Calculate the begin
						lBegin[iLoop] = lMax - 1 - lEnd[iLoop];
						// ... to the end of the file...
						lEnd[iLoop] = lMax - 1;
					}
					else
						// A normal begin value was indicated...
						lBegin[iLoop] = long.Parse(sRange[0]);


					// Check if the requested range values are valid, 
					// return false if they are not.
					//
					// Note:
					// Do not clean invalid values up by fitting them into
					// valid parameters using Math.Min and Math.Max, because
					// some download clients (like Go!Zilla) might send invalid 
					// (e.g. too large) range requests to determine the file limits!

					// Begin and end must not exceed the file size
					if ((lBegin[iLoop] > (lMax - 1)) || (lEnd[iLoop] > (lMax - 1)))
						bValidRanges = false;

					// Begin and end cannot be < 0
					if	((lBegin[iLoop] < 0) || (lEnd[iLoop] < 0) )
						bValidRanges = false;

					// End must be larger or equal to begin value
					if	(lEnd[iLoop] < lBegin[iLoop] )
						// The requested Range is invalid...
						bValidRanges = false;

				}

			}
			return bValidRanges;
		}

		string RetrieveHeader(HttpRequest objRequest, string sHeader,string sDefault)
		{
			string sreturn;

			// Retrieves the indicated Header//s value from the Request,
			// if the header was not sent, sDefault is returned.
			//
			// If the value contains quote characters, they are removed.

			sreturn = objRequest.Headers[sHeader];

			if ((sreturn ==null) || (sreturn.Equals(string.Empty)))
				// The Header wos not found in the Request, 
				// return the indicated default value...
				return sDefault;

			else
				// return the found header value, stripped of any quote characters...
				return sreturn.Replace("\"", "");

		}


		string GenerateHash(System.IO.Stream objStream, long lBegin ,long lEnd)
		{
			byte[] bByte=new byte[Convert.ToInt32(lEnd)];

			objStream.Read(bByte, Convert.ToInt32(lBegin), Convert.ToInt32(lEnd - lBegin) + 1);

			//Instantiate an MD5 Provider object
			MD5CryptoServiceProvider Md5 =new System.Security.Cryptography.MD5CryptoServiceProvider();

			//Compute the hash value from the source
			byte[] ByteHash = Md5.ComputeHash(bByte);

			//And convert it to String format for return
			return Convert.ToBase64String(ByteHash);
		}

    }

	public class UrlInformation
	{

		DownloadState m_nState;
		DateTime fCreatTime;
		string fUrlPath="";
		string m_ContentType="application/octet-stream";
		MemoryStream fStream;
		//System.IO.FileInfo m_objFile;

		public UrlInformation(string sUrlPath)
		{
			byte[] data;
			//m_objFile = new System.IO.FileInfo(sPath);
			fUrlPath=sUrlPath;
			fCreatTime=DateTime.Now;
			try
			{
				data=WebDataProvider.GetUrlByteData(sUrlPath);
			}
			catch
			{
				data=new byte[0];
			}
			fStream=new MemoryStream(data);
		}

		public UrlInformation(byte[] webPageData,string sUrlPath)
		{
			fUrlPath=sUrlPath;
			fCreatTime=DateTime.Now;
			if(webPageData==null)
				webPageData=new byte[0];
			fStream=new MemoryStream(webPageData);
		}

		public bool Exists
		{
			get
			{
				//return m_objFile.Exists;
				return fStream.Length>0;
			}
		}

		public string FullName
		{
			get{return fUrlPath;/*m_objFile.FullName;*/}
		}

		public DateTime LastWriteTimeUTC
		{
			get{return fCreatTime.ToUniversalTime();}
		}
		public long Length
		{
			get{return fStream.Length;}
		}

		public string ContentType
		{
			get
			{
				// This article shows a list of MIME types for IIS:
				// (Appendix A: Default MIME Type Associations for IIS)
				// http://www.microsoft.com/technet/prodtechnol/isa/2004/plan/mimetypes.mspx

				// If you do not know the correct mime type for
				// your document, please use "application/octet-stream".

				// Returns ZIP MIME type
				//return "application/x-zip-compressed";
				return m_ContentType;
			}
			set
			{
				m_ContentType=value;
			}
		}
		public string EntityTag
		{
			get
			{
				// Please note, that this unique code must keep
				// the same as long as the file does not change. 
				// If the file DOES change or is edited, however,
				// the code MUST change.
				return fUrlPath.GetHashCode().ToString();//"MyExampleFileID";
			}
		}
		public Stream DataStream
		{
			get
			{
				return this.fStream;
			}
		}
		public virtual DownloadState State
		{
			get
			{
				return m_nState;
			}
			set
			{
				m_nState = value;
				// ToDo - optional
				// At this point, you could delete the file automatically. 
				// If the state is set to Finished, your might not need
				// the file anymore:
				//
				// If nState = DownloadState.fsDownloadFinished Then
				//   Clear()
				// Else
				//   Save()
				// End If

				Save();
			}
		}

		void  Clear()
		{
			// Delete the source file and "clear" the file state...
			if (State == DownloadState.fsDownloadBroken || State == DownloadState.fsDownloadInProgress) 
			{  // Do not allow deleting if the file download is in progress 
			}
			else
			{
				//m_objFile.Delete();
				State = DownloadState.fsClear;
			}
		}

		void  Save()
		{
			// Do not use the Session or Application or Cache to
			// store this information, it must be independent from
			// Application, Session or Cache states!
			//
			// If you do not create files dynamically, 
			// you do not need to save the state, of course.
		}
	}

	public class WebDataProvider
	{
		
		public static string GetUrlData(string url)
		{
			System.Net.WebClient urlfile=new System.Net.WebClient();
			byte[] data=urlfile.DownloadData(url);//====== Get data from internet ======
			return System.Text.Encoding.UTF8.GetString(data);
		}

		public static byte[] GetUrlByteData(string url)
		{
			System.Net.WebClient urlfile=new System.Net.WebClient();
			return urlfile.DownloadData(url);//====== Get data from internet ======
		}
	}

	public enum DownloadState
	{
		/// Clear: No download in progress, 
		/// the file can be manipulated
		fsClear = 1,

		/// Locked: A dynamically created file must
		/// not be changed
		fsLocked = 2,

		/// In Progress: File is locked, and download 
		/// is currently in progress
		fsDownloadInProgress = 6,

		/// Broken: File is locked, download was in
		/// progress, but was cancelled 
		fsDownloadBroken = 10,

		/// Finished: File is locked, download
		/// was completed
		fsDownloadFinished = 18
	};
}
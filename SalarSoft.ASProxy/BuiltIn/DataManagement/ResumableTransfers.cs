using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Net;

namespace SalarSoft.ResumableDownload
{
	/// <summary>
	/// </summary>
	public class ResumableTransfers
	{
		const string HTTP_HEADER_CONTENT_RANGE = "Content-Range";
		const string HTTP_HEADER_CONTENT_LENGTH = "Content-Length";
		const string HTTP_HEADER_IF_RANGE = "If-Range";
		const string HTTP_HEADER_IF_MATCH = "If-Match";
		const string HTTP_HEADER_IF_NONE_MATCH = "If-None-Match";
		const string HTTP_HEADER_IF_MODIFIED_SINCE = "If-Modified-Since";
		const string HTTP_HEADER_IF_UNMODIFIED_SINCE = "If-Unmodified-Since";
		const string HTTP_HEADER_UNLESS_MODIFIED_SINCE = "Unless-Modified-Since";
		const string HTTP_HEADER_RANGE = "Range";

		/// <summary>
		/// Detects if response supports download resuming
		/// </summary>
		public static bool IsPartialContentSupported(WebResponse webResponse)
		{
			if (webResponse is FtpWebResponse)
				return false;
			else
			{
				bool acceptRanges = String.Compare(webResponse.Headers["Accept-Ranges"], "bytes", true) == 0;
				return acceptRanges;
			}
		}

		public static bool ParseResponseHeaderRange(WebResponse webResponse,
			out int rangeBegin,
			out long rangeEnd,
			out long rangeLength,
			out long dataLength,
			out bool rangeResponse)
		{

			// Initial values
			rangeBegin = 0;
			rangeEnd = webResponse.ContentLength;
			rangeLength = rangeEnd;
			dataLength = webResponse.ContentLength;
			rangeResponse = false;

			if (webResponse is HttpWebResponse)
			{
				// only partial content ollowed range responses
				if ((webResponse as HttpWebResponse).StatusCode != HttpStatusCode.PartialContent)
					return true;

				string contentRange = RetrieveHeader(webResponse.Headers[HTTP_HEADER_CONTENT_RANGE], String.Empty);
				string contentLength = RetrieveHeader(webResponse.Headers[HTTP_HEADER_CONTENT_LENGTH], String.Empty);

				if (contentRange.Length == 0 || contentLength.Length == 0)
				{
					rangeResponse = false;
					return true;
				}
				else
				{
					// the header whould be like this
					//HTTP/1.1 206 OK
					//Content-Range: bytes 500001-54216827/54216828
					//Content-Length: 53716827
					//Content-Type: application/x-zip-compressed
					//Last-Modified: Fri, 24 Jul 2009 21:45:22 GMT
					//ETag: "mTkR3PTZIofQXrAY3ZkNzA=="
					//Accept-Ranges: bytes
					//Binary content of DancingHampsters.zip from byte 500,001 onwards...

					rangeResponse = true;

					string[] rangeParts = contentRange.Replace("bytes ", "").Split(new char[] { '/' });
					if (rangeParts.Length == 2)
					{
						string[] dataRange = rangeParts[0].Split(new char[] { '-' });

						rangeBegin = Convert.ToInt32(dataRange[0]);
						rangeEnd = Convert.ToInt64(dataRange[1]);

						// and the content length
						dataLength = Convert.ToInt64(rangeParts[1]);

						// the range length is the content-length
						rangeLength = Convert.ToInt64(contentLength);

						// NOW VALIDATING RANGES 

						// range is not valid
						if (rangeLength > dataLength)
							return false;

						// range is not valid
						if (rangeBegin > rangeEnd)
							return false;

						// range can not be larger than total data size
						if (rangeEnd > dataLength)
							return false;

						rangeResponse = true;

						// the header is parsed
						return true;
					}
					else
					{
						rangeResponse = false;
						// invalid header!
						return false;
					}
				}
			}
			else
			{
				rangeResponse = false;
				return true;
			}
		}

		/// <summary>
		/// Parses user request for range specifed parameters
		/// </summary>
		/// <param name="objRequest">The request object</param>
		/// <param name="lBegin">Range start</param>
		/// <param name="lEnd">Range end</param>
		/// <param name="lMax">Total content length</param>
		/// <param name="rangeRequest">Is the request range specified</param>
		/// <returns>Returns false if the requested ranges are invalid, otherwise returns true </returns>
		public static void ParseRequestHeaderRange(HttpRequest request, out int lBegin, out long lEnd, out bool rangeRequest)
		{
			string sSource;
			int iLoop;
			string[] sRanges;
			lBegin = 0;
			lEnd = -1;

			// Parses the Range header from the Request (if there is one)
			// returns true, if the Range header was valid, or if there was no 
			//               Range header at all (meaning that the whole 
			//               file was requested)
			// returns false, if the Range header asked for unsatisfieable 
			//                ranges

			// Retrieve Range Header value from Request (Empty if none is indicated)
			sSource = RetrieveHeader(request, HTTP_HEADER_RANGE, String.Empty);

			if (sSource.Equals(String.Empty))
			{
				// No Range was requested, return the entire file range...

				lBegin = 0;
				lEnd = -1;

				// no Range request
				rangeRequest = false;
			}
			else
			{
				// A Range was requested... 

				// return true for the bRange parameter, telling the caller
				// that the Request is indeed a Range request...
				rangeRequest = true;

				// Remove "bytes=" from the beginning, and split the remaining 
				// string by comma characters
				sRanges = sSource.Replace("bytes=", "").Split(",".ToCharArray());

				// Check each found Range request for consistency
				if (sRanges.Length > 0)
				{
					iLoop = 0;

					// Split this range request by the dash character, 
					// sRange(0) contains the requested begin-value,
					// sRange(1) contains the requested end-value...
					string[] sRange = sRanges[iLoop].Split("-".ToCharArray());

					// Determine the end of the requested range
					if (sRange[1].Equals(String.Empty))
						// No end was specified, take the entire range
						lEnd = -1;
					else
						// An end was specified...
						lEnd = long.Parse(sRange[1]);

					// Determine the begin of the requested range
					if (sRange[0].Equals(String.Empty))
					{
						// No begin was specified, which means that
						// the end value indicated to return the last n
						// bytes of the file:

						// Calculate the begin
						lBegin = -1;

						// ... to the end of the file...
						lEnd = -1;
					}
					else
						// A normal begin value was indicated...
						lBegin = int.Parse(sRange[0]);


					// Check if the requested range values are valid, 
					// return false if they are not.
					//
					// Note:
					// Do not clean invalid values up by fitting them into
					// valid parameters using Math.Min and Math.Max, because
					// some download clients (like Go!Zilla) might send invalid 
					// (e.g. too large) range requests to determine the file limits!

					// Begin and end must not exceed the file size
					//if ((lBegin > (lMax - 1)) || (lEnd[iLoop] > (lMax - 1)))
					//    validRanges = false;

					//// Begin and end cannot be < 0
					//if ((lBegin < 0) || (lEnd[iLoop] < 0))
					//    validRanges = false;

					//// End must be larger or equal to begin value
					//if (lEnd < lBegin)
					//    // The requested Range is invalid...
					//    validRanges = false;

				}
			}
			//return validRanges;
		}

		/// <summary>
		/// Parses user request for range specifed parameters
		/// </summary>
		/// <param name="objRequest">The request object</param>
		/// <param name="lBegin">Range start</param>
		/// <param name="lEnd">Range end</param>
		/// <param name="lMax">Total content length</param>
		/// <param name="rangeRequest">Is the request range specified</param>
		/// <returns>Returns false if the requested ranges are invalid, otherwise returns true </returns>
		public static bool ParseRequestHeaderRange(HttpRequest request, out long[] lBegin, out long[] lEnd, long lMax, out bool rangeRequest)
		{
			bool validRanges;
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
			sSource = RetrieveHeader(request, HTTP_HEADER_RANGE, String.Empty);

			if (sSource.Equals(String.Empty))
			{
				// No Range was requested, return the entire file range...

				lBegin = new long[1];
				lEnd = new long[1];

				lBegin[0] = 0;
				lEnd[0] = lMax - 1;

				// A valid range is returned
				validRanges = true;
				// no Range request
				rangeRequest = false;
			}
			else
			{
				// A Range was requested... 

				// Preset value...
				validRanges = true;

				// return true for the bRange parameter, telling the caller
				// that the Request is indeed a Range request...
				rangeRequest = true;

				// Remove "bytes=" from the beginning, and split the remaining 
				// string by comma characters
				sRanges = sSource.Replace("bytes=", "").Split(",".ToCharArray());
				lBegin = new long[sRanges.GetUpperBound(0) + 1];
				lEnd = new long[sRanges.GetUpperBound(0) + 1];

				// Check each found Range request for consistency
				for (iLoop = sRanges.GetLowerBound(0); iLoop <= sRanges.GetUpperBound(0); iLoop++)
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
					if (sRange[0].Equals(String.Empty))
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
						validRanges = false;

					// Begin and end cannot be < 0
					if ((lBegin[iLoop] < 0) || (lEnd[iLoop] < 0))
						validRanges = false;

					// End must be larger or equal to begin value
					if (lEnd[iLoop] < lBegin[iLoop])
						// The requested Range is invalid...
						validRanges = false;

				}
			}
			return validRanges;
		}


		public static bool ValidatePartialRequest(HttpRequest request, ResumableResponseData responseData, out string matchedETag, ref int statusCode)
		{
			const string HTTP_METHOD_GET = "GET";
			const string HTTP_METHOD_HEAD = "HEAD";
			matchedETag = null;

			if (!(responseData.HttpMethod.Equals(HTTP_METHOD_GET) ||
				responseData.HttpMethod.Equals(HTTP_METHOD_HEAD)))
			{
				// Currently, only the GET and HEAD methods 
				// are supported...
				statusCode = 501;  // Not implemented
				return false;
			}
			else if (!CheckIfModifiedSince(request, responseData))
			{
				// The entity is still unmodified...
				statusCode = 304;  // Not Modified
				return false;
			}
			else if (!CheckIfUnmodifiedSince(request, responseData))
			{
				// The entity was modified since the requested date... 
				statusCode = 412;  // Precondition failed
				return false;
			}
			else if (!CheckIfMatch(request, responseData))
			{
				// The entity does not match the request... 
				statusCode = 412;  // Precondition failed
				return false;
			}
			else if (!CheckIfNoneMatch(request, ref statusCode, out matchedETag, responseData))
			{
				// The entity does match the none-match request, the response 
				// code was set inside the CheckifNoneMatch function

				//matchedETag

				// valid but content is not required
				return false;
			}
			else
			{
				//valid
				return true;
			}
		}

		private static bool CheckIfNoneMatch(HttpRequest request, ref int statusCode, out string matchedETag, ResumableResponseData responseData)
		{
			string sRequestHeaderIfNoneMatch;
			string[] sEntityIDs;
			bool breturn = true;
			string sreturn = "";
			matchedETag = "";
			// Checks the If-None-Match header if it was sent with the request.
			//
			// returns true if one of the header values matches the file//s entity tag,
			//              or if "*" was sent,
			// returns false if a header was sent, but does not match the file, or
			//               if no header was sent.

			// Retrieve If-None-Match Header value from Request (*, meaning any, if none is indicated)
			sRequestHeaderIfNoneMatch = RetrieveHeader(request, HTTP_HEADER_IF_NONE_MATCH, String.Empty);

			if (sRequestHeaderIfNoneMatch.Equals(String.Empty))
				// Perform the request normally...
				breturn = true;
			else
			{
				if (sRequestHeaderIfNoneMatch.Equals("*"))
				{
					// The server must not perform the request 
					statusCode = 412;  // Precondition failed
					breturn = false;
				}
				else
				{
					// One or more Match IDs where sent by the client software...
					sEntityIDs = sRequestHeaderIfNoneMatch.Replace("bytes=", "").Split(",".ToCharArray());

					// Loop through all entity IDs, finding one which 
					// does not match the current file//s ID will be
					// enough to satisfy the If-None-Match
					for (int iLoop = sEntityIDs.GetLowerBound(0); iLoop <= sEntityIDs.GetUpperBound(0); iLoop++)
					{
						if (sEntityIDs[iLoop].Trim().Equals(responseData.EntityTag))
						{
							sreturn = sEntityIDs[iLoop];
							breturn = false;
						}
					}

					if (!breturn)
					{
						// One of the requested entities matches the current file's tag,
						//objResponse.AppendHeader("ETag", sreturn);
						matchedETag = sreturn;
						statusCode = 304; // Not Modified
					}
				}
			}
			// return the result...
			return breturn;
		}

		private static bool CheckIfMatch(HttpRequest request, ResumableResponseData responseData)
		{
			string sRequestHeaderIfMatch;
			string[] sEntityIDs;
			bool breturn = false;

			// Checks the If-Match header if it was sent with the request.
			//
			// returns true if one of the header values matches the file//s entity tag,
			//              or if no header was sent,
			// returns false if a header was sent, but does not match the file.


			// Retrieve If-Match Header value from Request (*, meaning any, if none is indicated)
			sRequestHeaderIfMatch = RetrieveHeader(request, HTTP_HEADER_IF_MATCH, "*");

			if (sRequestHeaderIfMatch.Equals("*"))
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
				for (int iLoop = sEntityIDs.GetLowerBound(0); iLoop <= sEntityIDs.GetUpperBound(0); iLoop++)
				{
					if (sEntityIDs[iLoop].Trim().Equals(responseData.EntityTag))
						breturn = true;
				}
			}
			// return the result...
			return breturn;
		}

		private static bool CheckIfUnmodifiedSince(HttpRequest request, ResumableResponseData responseData)
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
			sDate = RetrieveHeader(request, HTTP_HEADER_IF_UNMODIFIED_SINCE, String.Empty);

			if (sDate.Equals(String.Empty))
				// If-Unmodified-Since was not sent, check Unless-Modified-Since... 
				sDate = RetrieveHeader(request, HTTP_HEADER_UNLESS_MODIFIED_SINCE, String.Empty);


			if (sDate.Equals(String.Empty))
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
					breturn = responseData.LastWriteTimeUTC < DateTime.Parse(sDate);
				}
				catch (Exception)
				{
					// Converting the indicated date value failed, return false 
					breturn = false;
				}
			}
			return breturn;
		}

		private static bool CheckIfModifiedSince(HttpRequest request, ResumableResponseData responseData)
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
			sDate = RetrieveHeader(request, HTTP_HEADER_IF_MODIFIED_SINCE, string.Empty);

			if (sDate.Equals(String.Empty))
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
					breturn = (responseData.LastWriteTimeUTC >= DateTime.Parse(sDate));
				}
				catch
				{
					// Converting the indicated date value failed, return false 
					breturn = false;
				}
			}
			return breturn;
		}

		private static string RetrieveHeader(HttpRequest objRequest, string headerName, string sDefault)
		{
			string sreturn;

			// Retrieves the indicated Header//s value from the Request,
			// if the header was not sent, sDefault is returned.
			//
			// If the value contains quote characters, they are removed.

			sreturn = objRequest.Headers[headerName];

			if ((sreturn == null) || (sreturn.Equals(string.Empty)))
				// The Header wos not found in the Request, 
				// return the indicated default value...
				return sDefault;

			else
				// return the found header value, stripped of any quote characters...
				return sreturn.Replace("\"", "");
		}

		static string RetrieveHeader(string headerString, string sDefault)
		{
			string sreturn;

			// Retrieves the indicated Header//s value from the Request,
			// if the header was not sent, sDefault is returned.
			//
			// If the value contains quote characters, they are removed.

			sreturn = headerString;

			if ((sreturn == null) || (sreturn.Equals(string.Empty)))
				// The Header wos not found in the Request, 
				// return the indicated default value...
				return sDefault;

			else
				// return the found header value, stripped of any quote characters...
				return sreturn.Replace("\"", "");
		}

	}
}
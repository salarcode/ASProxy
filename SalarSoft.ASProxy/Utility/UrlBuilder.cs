using System;

namespace SalarSoft.ASProxy
{
	/// <summary>
	/// Functions for URL manipulation
	/// </summary>
    public class UrlBuilder
    {
		/// <summary>
		/// Combines Url paths
		/// </summary>
        public static string CombinePaths(string path, string file)
        {
            const char urlSep = '/';
            const char dirSep = '\\';

            if (file.Length == 0)
                return path;
            if (path.Length == 0)
                return file;

			// remove seperator from the file start
            if (file[0] == urlSep || file[0] == dirSep)
                file = file.Substring(1);

			// remove seperator from the path end
			if (path[path.Length - 1] == urlSep || path[path.Length - 1] == dirSep)
                path = path.Substring(0, path.Length - 1);

			// join
            return path + urlSep + file;
        }

		/// <summary>
		/// Appends bunch of queries to the url
		/// </summary>
        public static string AppendAntoherQueries(string url, string queriesCollection)
        {
            if (string.IsNullOrEmpty(queriesCollection))
                return url;

			// remove question mark
            if (queriesCollection[0] == '?')
                queriesCollection = queriesCollection.Remove(0, 1);

			// question mark position
			int qMark = url.IndexOf("?", 0);

			// if url doesn't has queries
			if (qMark == -1)
            {
                return url + "?" + queriesCollection;
            }
            else
            {
				// Oh! there is question mark at the end of url already!
				if (qMark == url.Length - 1)
					return url + queriesCollection;


				// This can be a mistake but, if the url has "&" mark at its end
				// don't insert that mark again
                if (url[url.Length - 1] == '&')
					return url + queriesCollection;
				
                return url + "&" + queriesCollection;
            }
        }

		/// <summary>
		/// Changes value of specified query in url with new value. If the specifed query doesn't found nothing will change.
		/// </summary>
        public static string ReplaceUrlQuery(string url, string query, string newval)
        {
            string result = url;
			
			// locate query assign posistion
            int pos1 = result.IndexOf(query + "=", 0);
            if (pos1 == -1)
                return result;

			// after the assign
			pos1 += (query).Length + 1;

			// locate next query start
            int pos2 = result.IndexOf("&", pos1);

			// if there is nothing
            if (pos2 == -1)
            {
				// test for bookmark
                pos2 = result.IndexOf("#", pos1);
                
				// there is nothing the end is length of the url
				if (pos2 == -1)
                    pos2 = result.Length;
            }
			// remove old value
            result = result.Remove(pos1, pos2 - pos1);

			// new value
            return result.Insert(pos1, newval);
        }

		/// <summary>
		/// Inserts specifed query to the url. The query will be the first query of url.
		/// </summary>
        public static string AddUrlQuery(string url, string query, string val)
        {
			string queryAssign = query + "=";

            // If already exists
			if (url.IndexOf(queryAssign, 0) > -1)
                return ReplaceUrlQuery(url, query, val);

            // Full query
			string fullquery = queryAssign + val;
            int place = -1;

            // Check if there is any query before
            place = url.IndexOf('?', 0);
            if (place > -1)
                return url.Insert(place + 1, fullquery + "&");

            // Check if there is bookmark before
            place = url.IndexOf('#', 0);
            if (place > -1)
                return url.Insert(place, fullquery + "?");

            return url + "?" + fullquery;
        }

		/// <summary>
		/// Inserts specifed query to the url. The query will be the last query of url.
		/// </summary>
		public static string AddUrlQueryToEnd(string url, string query, string val)
        {
            // If already exists
            if (url.IndexOf(query + "=", 0) > -1)
                return ReplaceUrlQuery(url, query, val);

            // full query
            string fullquery = query + "=" + val;
            int place = -1;

            // Check if there is any query before
            place = url.IndexOf('?', 0);
            if (place > -1)
            {
                // Check if there is bookmark before
                int bookPlace = url.IndexOf('#', 0);
                if (bookPlace > -1)
                {
                    place = bookPlace;
                    return url.Insert(bookPlace, "&" + fullquery);
                }
                else
                    return url + "&" + fullquery;
            }

            // Check if there is bookmark before
            place = url.IndexOf('#', 0);
            if (place > -1)
                return url.Insert(place, fullquery + "?");

            return url + "?" + fullquery;
        }

		/// <summary>
		/// Removes the specifed query from url
		/// </summary>
        public static string RemoveQuery(string url, string query)
        {
            int place, place2;

			// query assign location
			place = url.IndexOf(query + "=", 0);
			if (place == -1)
                return url;
            
			// the next query if there is
			place2 = url.IndexOf('&', place);
            if (place2 == -1)
            {
				// the bookmark if ther is any
                place2 = url.IndexOf('#', place);
                if (place2 == -1)
                    place2 = url.Length;
            }

			// if there is other queries
            if (place2 != url.Length)
                if (url[place2] == '&' && url[place - 1] == '?')
                {
                    url = url.Insert(place2 + 1, "?");
                    place2++;
                }

			// this will cause to remove martk too
            place--;

			// remove it!
            return url.Remove(place, place2 - place);
        }

		/// <summary>
		/// Removes bookmark part of the url.
		/// </summary>
        public static string RemoveUrlBookmark(string url, out string bookmark)
        {
            bookmark = string.Empty;
            int markpos = url.IndexOf('#');
            if (markpos == -1)
                return url;
            bookmark = url.Substring(markpos, url.Length - markpos);
            return url.Substring(0, markpos);
        }

		/// <summary>
		/// Removes all queries from url
		/// </summary>
        public static string ClearQuery(string url)
        {
			int place = url.IndexOf("?", 0);
            if (place == -1)
            {
				place = url.IndexOf("#", 0);
                if (place == -1)
					return url;
            }
			return url.Substring(0, place);
        }
    }
}
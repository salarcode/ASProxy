using System;

namespace SalarSoft.ASProxy
{
	public class UrlBuilder
	{
		public static string CombinePaths(string path, string file)
		{
			const char urlSep = '/';
			const char dirSep = '/';

			if (file.Length == 0)
				return path;
			else if (path.Length == 0)
				return file;

			if (file[0] == urlSep || file[0] == dirSep)
				file = file.Substring(1);

			if (path[path.Length - 1] == urlSep || path[path.Length - 1] == dirSep)
				path = path.Substring(0, path.Length - 1);

			return path + urlSep + file;
		}

		public static string AppendAntoherQueries(string url, string queriesCollection)
		{
			if (queriesCollection.Length == 0)
				return url;

			if (queriesCollection[0] == '?')
				queriesCollection = queriesCollection.Remove(0, 1);

			if (url.IndexOf("?", 0) == -1)
			{
				return url + "?" + queriesCollection;
			}
			else
			{
				if (url[url.Length - 1] == '&')
					url = url.Remove(url.Length - 1, 1);
				return url + "&" + queriesCollection;
			}
		}
		public static string ReplaceUrlQuery(string url, string query, string newval)
		{
			string result = url;
			int pos1 = result.IndexOf(query + "=", 0);
			if (pos1 == -1)
				return result;
			pos1 += (query + "=").Length;
			int pos2 = result.IndexOf("&", pos1);
			if (pos2 == -1)
			{
				pos2 = result.IndexOf("#", pos1);
				if (pos2 == -1)
					pos2 = result.Length;
			}
			result = result.Remove(pos1, pos2 - pos1);
			result = result.Insert(pos1, newval);
			return result;
		}

		public static string AddUrlQuery(string url, string query, string val)
		{
            // If already exists
            if (url.IndexOf(query + "=", 0) > -1)
				return ReplaceUrlQuery(url, query, val);
			
            // Full query
            string fullquery = query + "=" + val;
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

		public static string RemoveQuery(string url, string query)
		{
			string fullquery = query + "=";
			string result = "";
			int place = -1, place2 = -1;
			place = url.IndexOf(fullquery, 0);
			if (place == -1)
				return url;
			place2 = url.IndexOf('&', place);
			if (place2 == -1)
			{
				place2 = url.IndexOf('#', place);
				if (place2 == -1)
					place2 = url.Length;
			}

			if (place2 != url.Length)
				if (url[place2] == '&' && url[place - 1] == '?')
				{
					url = url.Insert(place2 + 1, "?");
					place2++;
				}

			place--;
			result = url.Remove(place, place2 - place);
			return result;
		}
		public static string ClearQuery(string url)
		{
			string result = url;
			int place = -1;
			place = result.IndexOf("?", 0);
			if (place == -1)
			{
				place = result.IndexOf("#", 0);
				if (place == -1)
					return result;
			}
			result = result.Substring(0, place);
			return result;
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SalarSoft.ASProxy
{
	/// <author>
	/// Dave Transom
	/// http://www.singular.co.nz/blog/archive/2008/07/06/finding-preferred-accept-encoding-header-in-csharp.aspx
	/// </author>
	/// <summary>
	/// Represents a weighted value (or quality value) from an http header e.g. gzip=0.9; deflate; x-gzip=0.5;
	/// </summary>
	/// <remarks>
	/// accept-encoding spec:
	///    http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
	/// </remarks>
	/// <example>
	/// Accept:          text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
	/// Accept-Encoding: gzip,deflate
	/// Accept-Charset:  ISO-8859-1,utf-8;q=0.7,*;q=0.7
	/// Accept-Language: en-us,en;q=0.5
	/// </example>
	[DebuggerDisplay("QValue[{Name}, {Weight}]")]
	public struct QValue : IComparable<QValue>
	{
		static char[] delimiters = { ';', '=' };
		const float defaultWeight = 1;

		#region Fields

		string _name;
		float _weight;
		int _ordinal;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new QValue by parsing the given value
		/// for name and weight (qvalue)
		/// </summary>
		/// <param name="value">The value to be parsed e.g. gzip=0.3</param>
		public QValue(string value)
			: this(value, 0)
		{ }

		/// <summary>
		/// Creates a new QValue by parsing the given value
		/// for name and weight (qvalue) and assigns the given
		/// ordinal
		/// </summary>
		/// <param name="value">The value to be parsed e.g. gzip=0.3</param>
		/// <param name="ordinal">The ordinal/index where the item
		/// was found in the original list.</param>
		public QValue(string value, int ordinal)
		{
			_name = null;
			_weight = 0;
			_ordinal = ordinal;

			ParseInternal(ref this, value);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the value part
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// The weighting (or qvalue, quality value) of the encoding
		/// </summary>
		public float Weight
		{
			get { return _weight; }
		}

		/// <summary>
		/// Whether the value can be accepted
		/// i.e. it's weight is greater than zero
		/// </summary>
		public bool CanAccept
		{
			get { return _weight > 0; }
		}

		/// <summary>
		/// Whether the value is empty (i.e. has no name)
		/// </summary>
		public bool IsEmpty
		{
			get { return string.IsNullOrEmpty(_name); }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Parses the given string for name and
		/// weigth (qvalue)
		/// </summary>
		/// <param name="value">The string to parse</param>
		public static QValue Parse(string value)
		{
			QValue item = new QValue();
			ParseInternal(ref item, value);
			return item;
		}

		/// <summary>
		/// Parses the given string for name and
		/// weigth (qvalue)
		/// </summary>
		/// <param name="value">The string to parse</param>
		/// <param name="ordinal">The order of item in sequence</param>
		/// <returns></returns>
		public static QValue Parse(string value, int ordinal)
		{
			QValue item = Parse(value);
			item._ordinal = ordinal;
			return item;
		}

		/// <summary>
		/// Parses the given string for name and
		/// weigth (qvalue)
		/// </summary>
		/// <param name="value">The string to parse</param>
		static void ParseInternal(ref QValue target, string value)
		{
			string[] parts = value.Split(delimiters, 3);
			if (parts.Length > 0)
			{
				target._name = parts[0].Trim();
				target._weight = defaultWeight;
			}

			if (parts.Length == 3)
			{
				float.TryParse(parts[2], out target._weight);
			}
		}

		#endregion

		#region IComparable<QValue> Members

		/// <summary>
		/// Compares this instance to another QValue by
		/// comparing first weights, then ordinals.
		/// </summary>
		/// <param name="other">The QValue to compare</param>
		/// <returns></returns>
		public int CompareTo(QValue other)
		{
			int value = _weight.CompareTo(other._weight);
			if (value == 0)
			{
				int ord = -_ordinal;
				value = ord.CompareTo(-other._ordinal);
			}
			return value;
		}

		#endregion

		#region CompareByWeight

		/// <summary>
		/// Compares two QValues in ascending order.
		/// </summary>
		/// <param name="x">The first QValue</param>
		/// <param name="y">The second QValue</param>
		/// <returns></returns>
		public static int CompareByWeightAsc(QValue x, QValue y)
		{
			return x.CompareTo(y);
		}

		/// <summary>
		/// Compares two QValues in descending order.
		/// </summary>
		/// <param name="x">The first QValue</param>
		/// <param name="y">The second QValue</param>
		/// <returns></returns>
		public static int CompareByWeightDesc(QValue x, QValue y)
		{
			return -x.CompareTo(y);
		}

		#endregion

	}

	/// <summary>
	/// Provides a collection for working with qvalue http headers
	/// </summary>
	/// <remarks>
	/// accept-encoding spec:
	///    http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
	/// </remarks>
	[DebuggerDisplay("QValue[{Count}, {AcceptWildcard}]")]
	public sealed class QValueList : List<QValue>
	{
		static char[] delimiters = { ',' };

		#region Fields

		bool _acceptWildcard;
		bool _autoSort;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new instance of an QValueList list from
		/// the given string of comma delimited values
		/// </summary>
		/// <param name="values">The raw string of qvalues to load</param>
		public QValueList(string values)
			: this(null == values ? new string[0] : values.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
		{ }

		/// <summary>
		/// Creates a new instance of an QValueList from
		/// the given string array of qvalues
		/// </summary>
		/// <param name="values">The array of qvalue strings
		/// i.e. name(;q=[0-9\.]+)?</param>
		/// <remarks>
		/// Should AcceptWildcard include */* as well?
		/// What about other wildcard forms?
		/// </remarks>
		public QValueList(string[] values)
		{
			int ordinal = -1;
			foreach (string value in values)
			{
				QValue qvalue = QValue.Parse(value.Trim(), ++ordinal);
				if (qvalue.Name.Equals("*")) // wildcard
					_acceptWildcard = qvalue.CanAccept;
				Add(qvalue);
			}

			/// this list should be sorted by weight for
			/// methods like FindPreferred to work correctly
			DefaultSort();
			_autoSort = true;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Whether or not the wildcarded encoding is available and allowed
		/// </summary>
		public bool AcceptWildcard
		{
			get { return _acceptWildcard; }
		}

		/// <summary>
		/// Whether, after an add operation, the list should be resorted
		/// </summary>
		public bool AutoSort
		{
			get { return _autoSort; }
			set { _autoSort = value; }
		}

		/// <summary>
		/// Synonym for FindPreferred
		/// </summary>
		/// <param name="candidates">The preferred order in which to return an encoding</param>
		/// <returns>An QValue based on weight, or null</returns>
		public QValue this[params string[] candidates]
		{
			get { return FindPreferred(candidates); }
		}

		#endregion

		#region Add

		/// <summary>
		/// Adds an item to the list, then applies sorting
		/// if AutoSort is enabled.
		/// </summary>
		/// <param name="item">The item to add</param>
		public new void Add(QValue item)
		{
			base.Add(item);

			applyAutoSort();
		}

		#endregion

		#region AddRange

		/// <summary>
		/// Adds a range of items to the list, then applies sorting
		/// if AutoSort is enabled.
		/// </summary>
		/// <param name="collection">The items to add</param>
		public new void AddRange(IEnumerable<QValue> collection)
		{
			bool state = _autoSort;
			_autoSort = false;

			base.AddRange(collection);

			_autoSort = state;
			applyAutoSort();
		}

		#endregion

		#region Find

		/// <summary>
		/// Finds the first QValue with the given name (case-insensitive)
		/// </summary>
		/// <param name="name">The name of the QValue to search for</param>
		/// <returns></returns>
		public QValue Find(string name)
		{
			Predicate<QValue> criteria = delegate(QValue item) { return item.Name.Equals(name, StringComparison.OrdinalIgnoreCase); };
			return Find(criteria);
		}

		#endregion

		#region FindHighestWeight

		/// <summary>
		/// Returns the first match found from the given candidates
		/// </summary>
		/// <param name="candidates">The list of QValue names to find</param>
		/// <returns>The first QValue match to be found</returns>
		/// <remarks>Loops from the first item in the list to the last and finds
		/// the first candidate - the list must be sorted for weight prior to
		/// calling this method.</remarks>
		public QValue FindHighestWeight(params string[] candidates)
		{
			Predicate<QValue> criteria = delegate(QValue item)
			{
				return isCandidate(item.Name, candidates);
			};
			return Find(criteria);
		}

		#endregion

		#region FindPreferred

		/// <summary>
		/// Returns the first match found from the given candidates that is accepted
		/// </summary>
		/// <param name="candidates">The list of names to find</param>
		/// <returns>The first QValue match to be found</returns>
		/// <remarks>Loops from the first item in the list to the last and finds the
		/// first candidate that can be accepted - the list must be sorted for weight
		/// prior to calling this method.</remarks>
		public QValue FindPreferred(params string[] candidates)
		{
			Predicate<QValue> criteria = delegate(QValue item)
			{
				return isCandidate(item.Name, candidates) && item.CanAccept;
			};
			return Find(criteria);
		}

		#endregion

		#region DefaultSort

		/// <summary>
		/// Sorts the list comparing by weight in
		/// descending order
		/// </summary>
		public void DefaultSort()
		{
			Sort(QValue.CompareByWeightDesc);
		}

		#endregion

		#region applyAutoSort

		/// <summary>
		/// Applies the default sorting method if
		/// the autosort field is currently enabled
		/// </summary>
		void applyAutoSort()
		{
			if (_autoSort)
				DefaultSort();
		}

		#endregion

		#region isCandidate

		/// <summary>
		/// Determines if the given item contained within the applied array
		/// (case-insensitive)
		/// </summary>
		/// <param name="item">The string to search for</param>
		/// <param name="candidates">The array to search in</param>
		/// <returns></returns>
		static bool isCandidate(string item, params string[] candidates)
		{
			foreach (string candidate in candidates)
			{
				if (candidate.Equals(item, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}

		#endregion

	}
}
